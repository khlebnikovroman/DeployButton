namespace DeployButton.Services;

/// <summary>
/// Factory interface for creating deployment services
/// </summary>
public interface IDeploymentServiceFactory
{
    /// <summary>
    /// Gets a deployment service by type
    /// </summary>
    /// <param name="deploymentType">Type of deployment service to get</param>
    /// <returns>Deployment service instance</returns>
    IDeploymentService GetDeploymentService(string deploymentType);
    
    /// <summary>
    /// Gets all available deployment services
    /// </summary>
    /// <returns>Collection of deployment service names</returns>
    IEnumerable<string> GetAvailableDeploymentTypes();
}