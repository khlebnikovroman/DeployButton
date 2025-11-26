using DeployButton.Api.Abstractions;
using DeployButton.Api.Configs;
using Microsoft.Extensions.Options;

namespace DeployButton.Api;

public class TeamCityClientFactory : ITeamCityClientFactory
{
    public ITeamCityClient Create(TeamCityConfig config)
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
    private readonly ITeamCityClientFactory _clientFactory;
    private ITeamCityClient _teamCityClient;

    private int _isHandling = 0;
    private CancellationTokenSource _cts = new();

    public TeamCityDeployHandler(
        IOptionsMonitor<AppSettings> options,
        ILogger<TeamCityDeployHandler> logger,
        ITeamCityClientFactory clientFactory)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
        options.OnChange(ApplySettings);
        ApplySettings(options.CurrentValue);
    }

    private void ApplySettings(AppSettings settings)
    {
        _cts.Cancel();
        _cts = new CancellationTokenSource();
        _teamCityClient?.Dispose();
        _teamCityClient = _clientFactory.Create(settings.TeamCity);
    }

    public async Task<(DeployResult, Task<BuildResult>?)> TriggerAsync()
    {
        if (Interlocked.CompareExchange(ref _isHandling, 1, 0) == 1)
        {
            _logger.LogWarning("Деплой уже запущен — пропускаем");
            return (DeployResult.AlreadyBuilding, null);
        }

        try
        {
            var config = _options.CurrentValue.TeamCity;
            if (string.IsNullOrWhiteSpace(config.BaseUrl) || string.IsNullOrWhiteSpace(config.BuildConfigurationId))
            {
                _logger.LogError("TeamCity: не указаны BaseUrl или BuildConfigurationId");
                return (DeployResult.Failed, null);
            }

            if (await _teamCityClient.IsBuildQueuedOrRunningAsync())
            {
                _logger.LogWarning("Сборка уже в очереди или выполняется");
                return (DeployResult.AlreadyBuilding, null);
            }

            var buildId = await _teamCityClient.TriggerBuildAsync();
            _logger.LogInformation($"✅ Сборка {buildId} запущена в TeamCity");
            return (DeployResult.Queued, MonitorBuildStatusAsync(buildId));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 Ошибка при запуске деплоя");
        }
        finally
        {
            Interlocked.Exchange(ref _isHandling, 0);
        }
        return (DeployResult.Failed, null);
    }

    private Task<BuildResult> MonitorBuildStatusAsync(string buildId)
    {
        var tcs = new TaskCompletionSource<BuildResult>();
        var timeout = TimeSpan.FromMinutes(60);
        var tokenSource = new CancellationTokenSource(timeout);
        _ = Task.Run(async () =>
        {
            while (!tokenSource.Token.IsCancellationRequested && !_cts.IsCancellationRequested)
            {
                try
                {
                    var status = await _teamCityClient.GetBuildStatusAsync(buildId);
                    if (status == "SUCCESS")
                    {
                        _logger.LogInformation("✅ Сборка {BuildId} завершена успешно", buildId);
                        tcs.SetResult(BuildResult.Success);
                        return;
                    }
                    else if (status == "FAILURE" || status == "ERROR")
                    {
                        _logger.LogWarning("❌ Сборка {BuildId} завершена с ошибкой", buildId);
                        tcs.SetResult(BuildResult.Failed);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Ошибка при мониторинге сборки {BuildId}", buildId);
                    tcs.SetResult(BuildResult.Failed);
                }

                await Task.Delay(TimeSpan.FromSeconds(5), tokenSource.Token);
            }
        }, tokenSource.Token);
        
        return tcs.Task;
    }

    public void Dispose()
    {
        _cts.Cancel();
    }
}