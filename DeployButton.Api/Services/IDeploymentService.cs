using DeployButton.Api.Abstractions;

namespace DeployButton.Services;

/// <summary>
/// Interface for deployment service that handles triggering deployments
/// </summary>
public interface IDeploymentService
{
    /// <summary>
    /// Triggers a deployment asynchronously
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deployment was successfully triggered, false otherwise</returns>
    Task<bool> TriggerDeploymentAsync(CancellationToken cancellationToken = default);
}