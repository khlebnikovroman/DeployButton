using DeployButton.Api.Abstractions;
using DeployButton.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace DeployButton.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DeviceController : ControllerBase
{
    private readonly IDeviceStateProvider _deviceStateProvider;

    public DeviceController(IDeviceStateProvider deviceStateProvider)
    {
        _deviceStateProvider = deviceStateProvider;
    }

    [HttpGet]
    public DeviceState GetState()
    {
        return _deviceStateProvider.CurrentState;
    }
}