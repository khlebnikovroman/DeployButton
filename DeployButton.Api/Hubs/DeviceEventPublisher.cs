using DeployButton.Api.Abstractions;
using Microsoft.AspNetCore.SignalR;

namespace DeployButton.Api.Hubs;

public class DeviceEventPublisher : IDeviceEventPublisher
{
    private readonly IHubContext<DeviceHub> _hubContext;
    private readonly IDeviceStateProvider _deviceStateProvider;

    public DeviceEventPublisher(
        IHubContext<DeviceHub> hubContext,
        IDeviceStateProvider deviceStateProvider)
    {
        _hubContext = hubContext;
        _deviceStateProvider = deviceStateProvider;
    }

    public Task PublishDeviceStateChangedAsync()
    {
        return _hubContext.Clients.All.SendAsync("DeviceStateChanged", _deviceStateProvider.CurrentState);
    }

    public Task PublishDeployTriggeredAsync()
    {
        return _hubContext.Clients.All.SendAsync("DeployTriggered");
    }

    public Task PublishBuildStatusAsync(string status)
    {
        return _hubContext.Clients.All.SendAsync("BuildStatusChanged", status);
    }
}