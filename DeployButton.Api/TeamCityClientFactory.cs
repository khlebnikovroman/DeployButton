using System.Net.Http.Headers;
using System.Text;
using System.Xml;
using DeployButton.Api.Abstractions;
using DeployButton.Api.Configs;
using Microsoft.Extensions.Options;

namespace DeployButton.Api;

public class TeamCityClientFactory : ITeamCityClientFactory
{
    public TeamCityClient Create(TeamCityConfig config)
    {
        var handler = new HttpClientHandler { UseCookies = false };
        var httpClient = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(15) };
        return new TeamCityClient(httpClient, config);
    }
}

public class TeamCityDeployHandler : IDeployTrigger, IDisposable
{
    private readonly IOptionsMonitor<AppSettings> _options;
    private readonly ILogger<TeamCityDeployHandler> _logger;
    private readonly HttpClient _httpClient;

    private int _isHandling = 0;
    private readonly CancellationTokenSource _cts = new();

    public TeamCityDeployHandler(
        IOptionsMonitor<AppSettings> options,
        ILogger<TeamCityDeployHandler> logger)
    {
        _options = options;
        _logger = logger;

        var handler = new HttpClientHandler { UseCookies = false };
        _httpClient = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(15) };

        UpdateAuth();
    }

    private void UpdateAuth()
    {
        var config = _options.CurrentValue.TeamCity;
        if (!string.IsNullOrEmpty(config.Username) && !string.IsNullOrEmpty(config.Password))
        {
            var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{config.Username}:{config.Password}"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
        }
    }

    public async Task TriggerAsync()
    {
        if (Interlocked.CompareExchange(ref _isHandling, 1, 0) == 1)
        {
            _logger.LogWarning("Деплой уже запущен — пропускаем");
            return;
        }

        try
        {
            var config = _options.CurrentValue.TeamCity;
            if (string.IsNullOrWhiteSpace(config.BaseUrl) || string.IsNullOrWhiteSpace(config.BuildConfigurationId))
            {
                _logger.LogError("TeamCity: не указаны BaseUrl или BuildConfigurationId");
                return;
            }

            UpdateAuth();

            // Проверяем, не запущена ли уже сборка
            if (await IsBuildQueuedOrRunningAsync(config))
            {
                _logger.LogWarning("Сборка уже в очереди или выполняется");
                return;
            }

            // Запускаем сборку
            await TriggerBuildAsync(config);
            _logger.LogInformation("✅ Сборка запущена в TeamCity");

            // Получаем ID последней запущенной сборки
            var buildId = await GetLastBuildIdAsync(config);
            if (buildId != null)
            {
                // Мониторим статус
                await MonitorBuildStatusAsync(buildId, config);
            }
            else
            {
                _logger.LogWarning("Не удалось получить ID сборки — мониторинг недоступен");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 Ошибка при запуске деплоя");
        }
        finally
        {
            Interlocked.Exchange(ref _isHandling, 0);
        }
    }

    private async Task<bool> IsBuildQueuedOrRunningAsync(TeamCityConfig config)
    {
        return await IsBuildStateActiveAsync(config, "queued") ||
               await IsBuildStateActiveAsync(config, "running");
    }

    private async Task<bool> IsBuildStateActiveAsync(TeamCityConfig config, string state)
    {
        var url = $"{config.BaseUrl}/httpAuth/app/rest/builds?locator=buildType:{config.BuildConfigurationId},state:{state},count:1";
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode) return false;

            var xml = await response.Content.ReadAsStringAsync();
            var doc = new XmlDocument();
            doc.LoadXml(xml);
            var count = doc.DocumentElement?.Attributes["count"]?.Value;
            return int.TryParse(count, out var c) && c > 0;
        }
        catch
        {
            return false;
        }
    }

    private async Task TriggerBuildAsync(TeamCityConfig config)
    {
        var url = $"{config.BaseUrl}/httpAuth/action.html?add2Queue={config.BuildConfigurationId}";
        var response = await _httpClient.PostAsync(url, null);
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"HTTP {response.StatusCode}: {response.ReasonPhrase}");
    }

    private async Task<string?> GetLastBuildIdAsync(TeamCityConfig config)
    {
        var url = $"{config.BaseUrl}/httpAuth/app/rest/builds?locator=buildType:{config.BuildConfigurationId},count:1";
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode) return null;

            var xml = await response.Content.ReadAsStringAsync();
            var doc = new XmlDocument();
            doc.LoadXml(xml);
            var build = doc.DocumentElement?.FirstChild;
            return build?.Attributes?["id"]?.Value;
        }
        catch
        {
            return null;
        }
    }

    private async Task MonitorBuildStatusAsync(string buildId, TeamCityConfig config)
    {
        const int maxAttempts = 120; // 10 минут при 5-секундных интервалах
        var attempt = 0;

        while (attempt++ < maxAttempts && !_cts.Token.IsCancellationRequested)
        {
            try
            {
                var status = await GetBuildStatusAsync(buildId);
                if (status == "SUCCESS")
                {
                    _logger.LogInformation("✅ Сборка {BuildId} завершена успешно", buildId);
                    return;
                }
                else if (status == "FAILURE" || status == "ERROR")
                {
                    _logger.LogWarning("❌ Сборка {BuildId} завершена с ошибкой", buildId);
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Ошибка при мониторинге сборки {BuildId}", buildId);
            }

            await Task.Delay(TimeSpan.FromSeconds(5), _cts.Token);
        }

        _logger.LogWarning("Таймаут мониторинга сборки {BuildId}", buildId);
    }

    private async Task<string?> GetBuildStatusAsync(string buildId)
    {
        var url = $"http://192.168.1.210:8111/httpAuth/app/rest/builds/id:{buildId}";
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode) return null;

            var xml = await response.Content.ReadAsStringAsync();
            var doc = new XmlDocument();
            doc.LoadXml(xml);
            return doc.DocumentElement?.Attributes?["status"]?.Value;
        }
        catch
        {
            return null;
        }
    }

    public void Dispose()
    {
        _cts.Cancel();
        _httpClient?.Dispose();
    }
}