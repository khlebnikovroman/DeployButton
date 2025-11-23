using Microsoft.AspNetCore.Mvc;
using DeployButton.Api.Abstractions;
using DeployButton.Api.Services;

namespace DeployButton.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DeployController : ControllerBase
{
    private readonly IDeployTrigger _deployTrigger;
    private readonly IDeviceStateService _deviceStateService;
    private readonly ILogger<DeployController> _logger;

    public DeployController(
        IDeployTrigger deployTrigger, 
        IDeviceStateService deviceStateService,
        ILogger<DeployController> logger)
    {
        _deployTrigger = deployTrigger;
        _deviceStateService = deviceStateService;
        _logger = logger;
    }

    [HttpPost("trigger")]
    public async Task<IActionResult> TriggerDeploy()
    {
        try
        {
            await _deployTrigger.TriggerAsync();
            return Ok(new { message = "Deploy triggered successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error triggering deploy");
            return StatusCode(500, new { error = "Failed to trigger deploy" });
        }
    }

    [HttpGet("status")]
    public IActionResult GetStatus()
    {
        var state = _deviceStateService.CurrentState;
        return Ok(new { 
            isConnected = state.IsConnected,
            portName = state.PortName,
            baudRate = state.BaudRate,
            availablePorts = state.AvailablePorts
        });
    }
}