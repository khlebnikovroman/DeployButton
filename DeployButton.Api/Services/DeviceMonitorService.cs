using System.IO.Ports;
using DeployButton.Api.Abstractions;
using DeployButton.Api.Adapters;
using DeployButton.Api.Configs;
using DeployButton.Api.Factories;
using DeployButton.Api.Models;
using Microsoft.Extensions.Options;

namespace DeployButton.Api.Services;

public class DeviceMonitorHostedService : IHostedService
{
    private readonly DeviceMonitorService _deviceMonitorService;

    public DeviceMonitorHostedService(DeviceMonitorService deviceMonitorService)
    {
        _deviceMonitorService = deviceMonitorService;
    }
    public Task StartAsync(CancellationToken cancellationToken)
    {
        return _deviceMonitorService.StartAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return _deviceMonitorService.StopAsync(cancellationToken);
    }
}
public class DeviceMonitorService : ISerialDeviceAdapterProvider, IHostedService, IDisposable 
{
    private readonly IDeviceStateProvider _stateProvider;
    private readonly ISerialDeviceAdapterFactory _adapterFactory;
    private readonly ILogger<DeviceMonitorService> _logger;
    private readonly IConfigProvider<AppSettings> _configProvider;

    private CancellationTokenSource? _cts;
    private readonly Lock _adapterLock = new();

    public DeviceMonitorService(
        IDeviceStateProvider stateProvider,
        ISerialDeviceAdapterFactory adapterFactory,
        ILogger<DeviceMonitorService> logger,
        IConfigProvider<AppSettings> configProvider)
    {
        _stateProvider = stateProvider;
        _adapterFactory = adapterFactory;
        _logger = logger;
        _configProvider = configProvider;
    }

    private SerialDeviceAdapter? CurrentAdapter
    {
        get { lock (_adapterLock) return field; }
        set { 
            lock (_adapterLock)
            {
                field = value;
                OnAdapterChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public async Task StartAsync(CancellationToken ct)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        _ = MonitorLoop(_cts.Token);
    }

    private async Task MonitorLoop(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            var config = _configProvider.Current;
            var availablePorts = SerialPort.GetPortNames();

            SerialDeviceAdapter? adapter = null;

            if (!string.Equals(config.SerialPort.PortName, "auto", StringComparison.OrdinalIgnoreCase) &&
                availablePorts.Contains(config.SerialPort.PortName, StringComparer.OrdinalIgnoreCase))
            {
                adapter = await TryCreateAndVerifyAsync(config.SerialPort.PortName, config.SerialPort.BaudRate, ct);
            }

            if (adapter == null)
            {
                foreach (var portName in availablePorts)
                {
                    if (ct.IsCancellationRequested) break;
                    adapter = await TryCreateAndVerifyAsync(portName, config.SerialPort.BaudRate, ct);
                    if (adapter != null) break;
                }
            }

            _stateProvider.UpdateState(new DeviceState
            {
                IsConnected = adapter != null,
                PortName = adapter?.Port.PortName,
                BaudRate = adapter?.Port.BaudRate,
            });

            if (adapter != null)
            {
                CurrentAdapter = adapter;
                _logger.LogInformation("Устройство подключено к {Port}", adapter.Port.PortName);

                await WaitForDisconnection(adapter, ct);
                _stateProvider.UpdateState(new DeviceState
                {
                    IsConnected = false,
                    PortName = null,
                    BaudRate = null,
                });
                CurrentAdapter = null;
                adapter.Dispose();
            }
            else
            {
                await Task.Delay(TimeSpan.FromSeconds(3), ct);
            }
        }
    }

    private async Task<SerialDeviceAdapter?> TryCreateAndVerifyAsync(string portName, int baudRate, CancellationToken ct)
    {
        try
        {
            var adapter = _adapterFactory.Create(portName, baudRate);
            adapter.Connect();
            if (await adapter.PingAsync(ct))
                return adapter;

            adapter.Disconnect();
            adapter.Dispose();
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Не удалось подключиться к {Port}", portName);
            return null;
        }
    }

    private async Task WaitForDisconnection(SerialDeviceAdapter adapter, CancellationToken ct)
    {
        while (!ct.IsCancellationRequested && adapter.IsConnected)
        {
            await Task.Delay(1000, ct);
        }
    }
    

    public async Task StopAsync(CancellationToken ct)
    {
        _cts?.Cancel();
        var adapter = CurrentAdapter;
        if (adapter != null)
        {
            adapter.Disconnect();
            adapter.Dispose();
            CurrentAdapter = null;
        }
        await Task.Delay(100, ct);
    }

    public void Dispose()
    {
        _cts?.Dispose();
        CurrentAdapter?.Dispose();
    }

    public event EventHandler? OnAdapterChanged;
    public ISerialDeviceAdapter? GetAdapter() => CurrentAdapter;
}