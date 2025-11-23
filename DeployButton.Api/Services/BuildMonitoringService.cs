using DeployButton.Api.Abstractions;
using DeployButton.Api.Configs;

namespace DeployButton.Api.Services;

public interface IBuildMonitoringService
{
    Task MonitorBuildAsync(string buildId, TeamCityConfig config, ISoundPlayer soundPlayer, AppSettings settings);
}

public class BuildMonitoringService : IBuildMonitoringService
{
    private readonly ITeamCityBuildService _buildService;
    private readonly ILogger<BuildMonitoringService> _logger;

    public BuildMonitoringService(ITeamCityBuildService buildService, ILogger<BuildMonitoringService> logger)
    {
        _buildService = buildService;
        _logger = logger;
    }

    public async Task MonitorBuildAsync(string buildId, TeamCityConfig config, ISoundPlayer soundPlayer, AppSettings settings)
    {
        const int maxAttempts = 120; // 10 минут при 5-секундных интервалах
        var attempt = 0;
        var cts = new CancellationTokenSource();

        try
        {
            while (attempt++ < maxAttempts && !cts.Token.IsCancellationRequested)
            {
                try
                {
                    var status = await _buildService.GetBuildStatusAsync(buildId, config);
                    if (status == "SUCCESS")
                    {
                        _logger.LogInformation("✅ Сборка {BuildId} завершена успешно", buildId);
                        await soundPlayer.PlaySoundAsync(settings.Audio.BuildSuccess);
                        return;
                    }
                    else if (status == "FAILURE" || status == "ERROR")
                    {
                        _logger.LogWarning("❌ Сборка {BuildId} завершена с ошибкой", buildId);
                        await soundPlayer.PlaySoundAsync(settings.Audio.BuildFail);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Ошибка при мониторинге сборки {BuildId}", buildId);
                }

                await Task.Delay(TimeSpan.FromSeconds(5), cts.Token);
            }

            _logger.LogWarning("Таймаут мониторинга сборки {BuildId}", buildId);
            await soundPlayer.PlaySoundAsync(settings.Audio.BuildFail);
        }
        finally
        {
            cts.Cancel();
        }
    }
}