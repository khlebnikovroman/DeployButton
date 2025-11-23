using DeployButton.Api;
using DeployButton.Api.Abstractions;
using DeployButton.Api.Adapters;
using DeployButton.Api.Configs;
using DeployButton.Api.Factories;
using DeployButton.Api.Models;
using Microsoft.Extensions.Options;

namespace DeployButton.Services;

public class RefactoredDeviceMonitorService : IHostedService, IDisposable
{
    private readonly IOptionsMonitor<AppSettings> _options;
    private readonly IDeviceStateProvider _stateProvider;
    private readonly ISerialDeviceAdapterFactory _adapterFactory;
    private readonly DeviceCommandHandler _commandHandler;
    private readonly ILogger<RefactoredDeviceMonitorService> _logger;

    private CancellationTokenSource? _cts;
    private readonly object _adapterLock = new();

    public RefactoredDeviceMonitorService(
        IOptionsMonitor<AppSettings> options,
        IDeviceStateProvider stateProvider,
        ISerialDeviceAdapterFactory adapterFactory,
        DeviceCommandHandler commandHandler,
        ILogger<RefactoredDeviceMonitorService> logger)
    {
        _options = options;
        _stateProvider = stateProvider;
        _adapterFactory = adapterFactory;
        _commandHandler = commandHandler;
        _logger = logger;
    }

    private SerialDeviceAdapter? CurrentAdapter
    {
        get { lock (_adapterLock) return field; }
        set { lock (_adapterLock) field = value; }
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
            var config = _options.CurrentValue;
            var availablePorts = System.IO.Ports.SerialPort.GetPortNames();

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
                AvailablePorts = availablePorts
            });

            if (adapter != null)
            {
                CurrentAdapter = adapter;
                adapter.OnCommandReceived += OnDeviceCommand;
                _logger.LogInformation("Device connected to {Port}", adapter.Port.PortName);

                await WaitForDisconnection(adapter, ct);
                adapter.OnCommandReceived -= OnDeviceCommand;
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
            _logger.LogDebug(ex, "Failed to connect to {Port}", portName);
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

    private async void OnDeviceCommand(string command)
    {
        await _commandHandler.HandleCommandAsync(command);
    }

    public async Task StopAsync(CancellationToken ct)
    {
        _cts?.Cancel();
        var adapter = CurrentAdapter;
        if (adapter != null)
        {
            adapter.OnCommandReceived -= OnDeviceCommand;
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
}