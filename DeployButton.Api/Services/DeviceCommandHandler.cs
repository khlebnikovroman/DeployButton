using DeployButton.Api.Abstractions;

namespace DeployButton.Services;

/// <summary>
/// Handles device commands received from the serial device
/// </summary>
public class DeviceCommandHandler
{
    private readonly IDeploymentService _deploymentService;
    private readonly ILogger<DeviceCommandHandler> _logger;

    public DeviceCommandHandler(IDeploymentService deploymentService, ILogger<DeviceCommandHandler> logger)
    {
        _deploymentService = deploymentService ?? throw new ArgumentNullException(nameof(deploymentService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task HandleCommandAsync(string command)
    {
        switch (command)
        {
            case "DEPLOY":
                _logger.LogInformation("Received DEPLOY command");
                await _deploymentService.TriggerDeploymentAsync();
                break;
            default:
                _logger.LogDebug("Received unknown command: {Command}", command);
                break;
        }
    }
}