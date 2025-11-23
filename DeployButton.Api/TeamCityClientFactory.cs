using System.Net.Http.Headers;
using System.Text;
using System.Xml;
using DeployButton.Api.Abstractions;
using DeployButton.Api.Configs;
using DeployButton.Api.Services;
using Microsoft.Extensions.Options;

namespace DeployButton.Api;

public class TeamCityClientFactory : ITeamCityClientFactory
{
    private readonly ITeamCityAuthenticationService _authService;

    public TeamCityClientFactory(ITeamCityAuthenticationService authService)
    {
        _authService = authService;
    }

    public TeamCityClient Create(TeamCityConfig config)
    {
        var handler = new HttpClientHandler { UseCookies = false };
        var httpClient = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(15) };
        return new TeamCityClient(httpClient, _authService, config);
    }
}

public class TeamCityDeployHandler : IDeployTrigger, IDisposable
{
    private readonly IOptionsMonitor<AppSettings> _options;
    private readonly ISoundPlayer _soundPlayer;
    private readonly ILogger<TeamCityDeployHandler> _logger;
    private readonly IDeployService _deployService;
    private readonly ITeamCityBuildService _buildService;
    private readonly IBuildMonitoringService _monitoringService;

    private int _isHandling = 0;
    private readonly CancellationTokenSource _cts = new();

    public TeamCityDeployHandler(
        IOptionsMonitor<AppSettings> options,
        ISoundPlayer soundPlayer,
        ILogger<TeamCityDeployHandler> logger,
        IDeployService deployService,
        ITeamCityBuildService buildService,
        IBuildMonitoringService monitoringService)
    {
        _options = options;
        _soundPlayer = soundPlayer;
        _logger = logger;
        _deployService = deployService;
        _buildService = buildService;
        _monitoringService = monitoringService;
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
                await _soundPlayer.PlaySoundAsync(_options.CurrentValue.Audio.BuildFail);
                return;
            }

            // Проверяем, можно ли запустить деплой
            if (!await _deployService.CanDeployAsync(config))
            {
                _logger.LogWarning("Сборка уже в очереди или выполняется");
                await _soundPlayer.PlaySoundAsync(_options.CurrentValue.Audio.BuildFail);
                return;
            }

            // Запускаем сборку
            await _deployService.TriggerDeployAsync(config);
            _logger.LogInformation("✅ Сборка запущена в TeamCity");
            await _soundPlayer.PlaySoundAsync(_options.CurrentValue.Audio.DeployStart);

            // Получаем ID последней запущенной сборки
            var buildId = await _buildService.GetLastBuildIdAsync(config);
            if (buildId != null)
            {
                // Мониторим статус
                await _monitoringService.MonitorBuildAsync(buildId, config, _soundPlayer, _options.CurrentValue);
            }
            else
            {
                _logger.LogWarning("Не удалось получить ID сборки — мониторинг недоступен");
                await _soundPlayer.PlaySoundAsync(_options.CurrentValue.Audio.BuildSuccess);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 Ошибка при запуске деплоя");
            await _soundPlayer.PlaySoundAsync(_options.CurrentValue.Audio.BuildFail);
        }
        finally
        {
            Interlocked.Exchange(ref _isHandling, 0);
        }
    }

    public void Dispose()
    {
        _cts.Cancel();
    }
}