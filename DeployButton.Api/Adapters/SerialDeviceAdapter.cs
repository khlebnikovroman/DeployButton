using System.IO.Ports;
using System.Text;
using DeployButton.Api.Abstractions;

namespace DeployButton.Api.Adapters;

public class SerialDeviceAdapter : ISerialDeviceReader, ISerialDeviceWriter, IDisposable
{
    private readonly SerialPort _port;
    private readonly ILogger<SerialDeviceAdapter> _logger;
    private bool _isConnected;

    public event Action<string>? OnCommandReceived;
    public bool IsConnected => _isConnected && _port.IsOpen;
    public SerialPort Port => _port;

    public SerialDeviceAdapter(SerialPort port, ILogger<SerialDeviceAdapter> logger)
    {
        _port = port;
        _logger = logger;
        _port.DataReceived += OnDataReceived;
    }

    private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        try
        {
            var data = _port.ReadLine().Trim();
            if (!string.IsNullOrEmpty(data))
            {
                OnCommandReceived?.Invoke(data);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Ошибка при чтении данных");
        }
    }

    public async Task SendCommandAsync(string command)
    {
        if (!_port.IsOpen) return;
        await _port.BaseStream.WriteAsync(Encoding.UTF8.GetBytes($"{command}\n"));
        _logger.LogDebug("→ {Command}", command);
    }

    public async Task<bool> PingAsync(CancellationToken ct = default)
    {
        var tcs = new TaskCompletionSource<string>();
        using var reg = ct.Register(() => tcs.TrySetCanceled());
        
        void OnPong(string r) { if (r == "PONG") tcs.TrySetResult(r); }
        OnCommandReceived += OnPong;
        
        try
        {
            await SendCommandAsync("PING");
            var delay = Task.Delay(2000, ct);
            var winner = await Task.WhenAny(tcs.Task, delay);
            return tcs.Task.IsCompletedSuccessfully;
        }
        finally
        {
            OnCommandReceived -= OnPong;
        }
    }

    public void Connect()
    {
        if (!_port.IsOpen)
        {
            _port.Open();
            _isConnected = true;
        }
    }

    public void Disconnect()
    {
        if (_port.IsOpen)
        {
            _port.Close();
            _isConnected = false;
        }
    }

    public void Dispose()
    {
        _port?.Dispose();
    }
}