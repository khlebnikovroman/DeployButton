using DeployButton.Api.Models;

namespace DeployButton.Api.Abstractions;

public interface IDeviceStateProvider
{
    DeviceState CurrentState { get; }
    void UpdateState(DeviceState state);
}