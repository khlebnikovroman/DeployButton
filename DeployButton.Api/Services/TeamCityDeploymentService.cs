using DeployButton.Api.Abstractions;
using DeployButton.Api.Configs;
using Microsoft.Extensions.Options;

namespace DeployButton.Api.Services;

public class TeamCityDeploymentService : IDeploymentService, IDisposable
{
    private readonly ITeamCityService _teamCityService;
    private readonly IBuildStatusMonitor _buildStatusMonitor;
    private readonly ISoundPlayer _soundPlayer;
    private readonly IOptionsMonitor<AppSettings> _options;
    private readonly ILogger<TeamCityDeploymentService> _logger;
    
    private int _isHandling = 0;
    private readonly CancellationTokenSource _cts = new();

    public TeamCityDeploymentService(
        ITeamCityService teamCityService,
        IBuildStatusMonitor buildStatusMonitor,
        ISoundPlayer soundPlayer,
        IOptionsMonitor<AppSettings> options,
        ILogger<TeamCityDeploymentService> logger)
    {
        _teamCityService = teamCityService ?? throw new ArgumentNullException(nameof(teamCityService));
        _buildStatusMonitor = buildStatusMonitor ?? throw new ArgumentNullException(nameof(buildStatusMonitor));
        _soundPlayer = soundPlayer ?? throw new ArgumentNullException(nameof(soundPlayer));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<bool> TriggerDeploymentAsync(CancellationToken cancellationToken = default)
    {
        if (Interlocked.CompareExchange(ref _isHandling, 1, 0) == 1)
        {
            _logger.LogWarning("Deployment already in progress - skipping");
            return false;
        }

        try
        {
            var config = _options.CurrentValue.TeamCity;
            if (string.IsNullOrWhiteSpace(config.BaseUrl) || string.IsNullOrWhiteSpace(config.BuildConfigurationId))
            {
                _logger.LogError("TeamCity: BaseUrl or BuildConfigurationId not specified");
                await _soundPlayer.PlaySoundAsync(_options.CurrentValue.Audio.BuildFail);
                return false;
            }

            // Check if build is already queued or running
            if (await _teamCityService.IsBuildQueuedOrRunningAsync(config, cancellationToken))
            {
                _logger.LogWarning("Build already queued or running");
                await _soundPlayer.PlaySoundAsync(_options.CurrentValue.Audio.BuildFail);
                return false;
            }

            // Trigger the build
            var buildId = await _teamCityService.TriggerBuildAsync(config, cancellationToken);
            if (buildId == null)
            {
                _logger.LogError("Failed to trigger build in TeamCity");
                await _soundPlayer.PlaySoundAsync(_options.CurrentValue.Audio.BuildFail);
                return false;
            }

            _logger.LogInformation("✅ Build {BuildId} triggered in TeamCity", buildId);
            await _soundPlayer.PlaySoundAsync(_options.CurrentValue.Audio.DeployStart);

            // Monitor the build status
            var success = await _buildStatusMonitor.MonitorBuildAsync(buildId, config, cancellationToken);
            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 Error triggering deployment");
            await _soundPlayer.PlaySoundAsync(_options.CurrentValue.Audio.BuildFail);
            return false;
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