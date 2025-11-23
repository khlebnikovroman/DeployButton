namespace DeployButton.Api.Services;

public class DeploymentServiceFactory : IDeploymentServiceFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DeploymentServiceFactory> _logger;

    public DeploymentServiceFactory(IServiceProvider serviceProvider, ILogger<DeploymentServiceFactory> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public IDeploymentService GetDeploymentService(string deploymentType)
    {
        return deploymentType?.ToLowerInvariant() switch
        {
            "teamcity" => _serviceProvider.GetRequiredService<TeamCityDeploymentService>(),
            _ => throw new ArgumentException($"Unknown deployment type: {deploymentType}", nameof(deploymentType))
        };
    }

    public IEnumerable<string> GetAvailableDeploymentTypes()
    {
        return new[] { "teamcity" };
    }
}