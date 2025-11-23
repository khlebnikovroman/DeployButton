// Services/DeviceStateProvider.cs

using DeployButton.Api.Abstractions;
using DeployButton.Api.Models;

namespace DeployButton.Api.Services;

public class DeviceStateProvider : IDeviceStateProvider
{
    private DeviceState _state = new();
    private readonly object _lock = new();

    public DeviceState CurrentState
    {
        get { lock (_lock) return _state; }
    }

    public void UpdateState(DeviceState state)
    {
        lock (_lock) _state = state;
    }
}