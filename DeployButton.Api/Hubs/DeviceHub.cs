using DeployButton.Api.Abstractions;
using DeployButton.Api.Configs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;

namespace DeployButton.Api.Hubs;

public class DeviceHub : Hub
{
    private readonly IDeviceStateProvider _deviceStateProvider;
    private readonly IOptionsMonitor<AppSettings> _options;

    public DeviceHub(
        IDeviceStateProvider deviceStateProvider,
        IOptionsMonitor<AppSettings> options)
    {
        _deviceStateProvider = deviceStateProvider;
        _options = options;
    }

    public override async Task OnConnectedAsync()
    {
        // Отправляем текущее состояние устройства
        await Clients.Caller.SendAsync("DeviceStateChanged", _deviceStateProvider.CurrentState);

        // Отправляем текущую конфигурацию (опционально)
        await Clients.Caller.SendAsync("ConfigUpdated", _options.CurrentValue);

        await base.OnConnectedAsync();
    }
}