using DeployButton.Api.Abstractions;
using DeployButton.Api.Configs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;

namespace DeployButton.Api.Hubs;

public class DeviceHub : Hub
{
    private readonly IDeviceStateProvider _deviceStateProvider;
    private readonly IConfigProvider<AppSettings> _configProvider;

    public DeviceHub(
        IDeviceStateProvider deviceStateProvider,
        IConfigProvider<AppSettings> configProvider)
    {
        _deviceStateProvider = deviceStateProvider;
        _configProvider = configProvider;
    }

    public override async Task OnConnectedAsync()
    {
        // Отправляем текущее состояние устройства
        await Clients.Caller.SendAsync("DeviceStateChanged", _deviceStateProvider.CurrentState);

        // Отправляем текущую конфигурацию (опционально)
        await Clients.Caller.SendAsync("ConfigUpdated", _configProvider.Current);

        await base.OnConnectedAsync();
    }
}