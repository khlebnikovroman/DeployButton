using DeployButton.Api.Configs;

namespace DeployButton.Api.Services;

/// <summary>
/// Interface for TeamCity API communication
/// </summary>
public interface ITeamCityService
{
    /// <summary>
    /// Checks if a build is currently queued or running
    /// </summary>
    /// <param name="config">TeamCity configuration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if build is queued or running, false otherwise</returns>
    Task<bool> IsBuildQueuedOrRunningAsync(TeamCityConfig config, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Triggers a build in TeamCity
    /// </summary>
    /// <param name="config">TeamCity configuration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Build ID of the triggered build, or null if failed</returns>
    Task<string?> TriggerBuildAsync(TeamCityConfig config, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the status of a build
    /// </summary>
    /// <param name="buildId">ID of the build to check</param>
    /// <param name="config">TeamCity configuration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Status of the build (SUCCESS, FAILURE, ERROR, etc.)</returns>
    Task<string?> GetBuildStatusAsync(string buildId, TeamCityConfig config, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the ID of the last build for a configuration
    /// </summary>
    /// <param name="config">TeamCity configuration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>ID of the last build, or null if not found</returns>
    Task<string?> GetLastBuildIdAsync(TeamCityConfig config, CancellationToken cancellationToken = default);
}