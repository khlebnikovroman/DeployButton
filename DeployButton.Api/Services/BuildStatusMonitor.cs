using DeployButton.Api.Abstractions;
using DeployButton.Api.Configs;
using Microsoft.Extensions.Options;

namespace DeployButton.Api.Services;

public class BuildStatusMonitor : IBuildStatusMonitor
{
    private readonly ITeamCityService _teamCityService;
    private readonly ISoundPlayer _soundPlayer;
    private readonly IOptionsMonitor<AppSettings> _options;
    private readonly ILogger<BuildStatusMonitor> _logger;

    public BuildStatusMonitor(
        ITeamCityService teamCityService, 
        ISoundPlayer soundPlayer,
        IOptionsMonitor<AppSettings> options,
        ILogger<BuildStatusMonitor> logger)
    {
        _teamCityService = teamCityService ?? throw new ArgumentNullException(nameof(teamCityService));
        _soundPlayer = soundPlayer ?? throw new ArgumentNullException(nameof(soundPlayer));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<bool> MonitorBuildAsync(string buildId, TeamCityConfig config, CancellationToken cancellationToken = default)
    {
        const int maxAttempts = 120; // 10 minutes with 5-second intervals
        var attempt = 0;

        while (attempt++ < maxAttempts && !cancellationToken.IsCancellationRequested)
        {
            try
            {
                var status = await _teamCityService.GetBuildStatusAsync(buildId, config, cancellationToken);
                if (status == "SUCCESS")
                {
                    _logger.LogInformation("✅ Build {BuildId} completed successfully", buildId);
                    await _soundPlayer.PlaySoundAsync(_options.CurrentValue.Audio.BuildSuccess);
                    return true;
                }
                else if (status == "FAILURE" || status == "ERROR")
                {
                    _logger.LogWarning("❌ Build {BuildId} completed with error", buildId);
                    await _soundPlayer.PlaySoundAsync(_options.CurrentValue.Audio.BuildFail);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error monitoring build {BuildId}", buildId);
            }

            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
        }

        _logger.LogWarning("Build monitoring timeout for build {BuildId}", buildId);
        await _soundPlayer.PlaySoundAsync(_options.CurrentValue.Audio.BuildFail);
        return false;
    }
}