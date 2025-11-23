using DeployButton.Api.Abstractions;
using DeployButton.Api.Configs;
using Microsoft.Extensions.Options;

namespace DeployButton.Api.Services;

public interface IDeployService
{
    Task<bool> CanDeployAsync(TeamCityConfig config);
    Task TriggerDeployAsync(TeamCityConfig config);
}

public class DeployService : IDeployService
{
    private readonly ITeamCityBuildService _buildService;
    private readonly ILogger<DeployService> _logger;

    public DeployService(ITeamCityBuildService buildService, ILogger<DeployService> logger)
    {
        _buildService = buildService;
        _logger = logger;
    }

    public async Task<bool> CanDeployAsync(TeamCityConfig config)
    {
        try
        {
            return !await _buildService.IsBuildQueuedOrRunningAsync(config);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking deploy availability");
            return false;
        }
    }

    public async Task TriggerDeployAsync(TeamCityConfig config)
    {
        await _buildService.TriggerBuildAsync(config);
    }
}