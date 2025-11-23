using DeployButton.Api.Configs;

namespace DeployButton.Services;

/// <summary>
/// Interface for monitoring build status
/// </summary>
public interface IBuildStatusMonitor
{
    /// <summary>
    /// Monitors a build until completion or timeout
    /// </summary>
    /// <param name="buildId">ID of the build to monitor</param>
    /// <param name="config">TeamCity configuration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if build completed successfully, false otherwise</returns>
    Task<bool> MonitorBuildAsync(string buildId, TeamCityConfig config, CancellationToken cancellationToken = default);
}