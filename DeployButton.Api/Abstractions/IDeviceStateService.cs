using DeployButton.Api.Models;

namespace DeployButton.Api.Abstractions;

public interface IDeviceStateService
{
    DeviceState CurrentState { get; }
    void UpdateState(DeviceState state);
}

public interface IDeviceMonitorService
{
    Task StartAsync(CancellationToken ct);
    Task StopAsync(CancellationToken ct);
}