using DeployButton.Api.Abstractions;
using DeployButton.Api.Abstractions.TeamCity;
using DeployButton.Api.Configs;
using DeployButton.Api.Enums;

namespace DeployButton.Api.Services.TeamCity;

public class TeamCityDeployHandler : IDeployTrigger, IDisposable
{
    private readonly ILogger<TeamCityDeployHandler> _logger;
    private readonly IConfigProvider<AppSettings> _configProvider;
    private readonly ITeamCityClientFactory _clientFactory;
    private ITeamCityClient? _teamCityClient;
    private TeamCityConfig _config;
    private int _isHandling = 0;
    private CancellationTokenSource _cts = new();

    public TeamCityDeployHandler(
        ILogger<TeamCityDeployHandler> logger,
        IConfigProvider<AppSettings> configProvider,
        ITeamCityClientFactory clientFactory)
    {
        _logger = logger;
        _configProvider = configProvider;
        _clientFactory = clientFactory;
        ApplySettings(_configProvider.Current);
        _configProvider.OnChange += ApplySettings;
    }

    
    private void ApplySettings(AppSettings settings)
    {
        _cts.Cancel();
        _cts = new CancellationTokenSource();
        _config = settings.TeamCity;
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
            var config = _config;
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
        _configProvider.OnChange -= ApplySettings;
    }
}