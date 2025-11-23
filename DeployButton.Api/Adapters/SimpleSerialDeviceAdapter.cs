using System.IO.Ports;
using DeployButton.Api.Abstractions;

namespace DeployButton.Api.Adapters;

/// <summary>
/// Simplified serial device adapter that follows SOLID principles with minimal dependencies
/// </summary>
public class SimpleSerialDeviceAdapter : ISerialDeviceReader, ISerialDeviceWriter, IDisposable
{
    private readonly SerialPort _port;
    private readonly ILogger<SimpleSerialDeviceAdapter> _logger;
    private readonly object _lock = new object();
    private bool _isConnected;
    private bool _disposed = false;

    public event Action<string>? OnCommandReceived;
    public bool IsConnected => _isConnected && _port?.IsOpen == true;

    public SimpleSerialDeviceAdapter(string portName, int baudRate, ILogger<SimpleSerialDeviceAdapter> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        try
        {
            _port = new SerialPort(portName, baudRate)
            {
                DtrEnable = true,
                RtsEnable = true,
                ReadTimeout = 1000,
                WriteTimeout = 1000
            };
            
            _port.DataReceived += OnDataReceived;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating serial port {PortName}", portName);
            throw;
        }
    }

    private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        if (_disposed) return;

        try
        {
            // Read all available data
            var data = _port.ReadExisting().Trim();
            if (!string.IsNullOrEmpty(data))
            {
                // Split by newlines and process each command
                var commands = data.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var command in commands)
                {
                    var trimmedCommand = command.Trim();
                    if (!string.IsNullOrEmpty(trimmedCommand))
                    {
                        _logger.LogDebug("← {Command}", trimmedCommand);
                        OnCommandReceived?.Invoke(trimmedCommand);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error reading data from serial port");
        }
    }

    public async Task SendCommandAsync(string command)
    {
        if (_disposed) return;
        if (string.IsNullOrEmpty(command)) return;
        
        lock (_lock)
        {
            if (!_port.IsOpen) return;
            
            try
            {
                _port.WriteLine(command);
                _logger.LogDebug("→ {Command}", command);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending command to serial port");
                throw;
            }
        }
    }

    public async Task<bool> PingAsync(CancellationToken ct = default)
    {
        if (_disposed) return false;
        
        var tcs = new TaskCompletionSource<bool>();
        string? receivedResponse = null;
        void OnPong(string response) 
        { 
            if (response == "PONG") 
            { 
                receivedResponse = response; 
                tcs.TrySetResult(true); 
            } 
        }
        
        OnCommandReceived += OnPong;
        
        try
        {
            await SendCommandAsync("PING");
            
            using var delayCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            delayCts.CancelAfter(2000);
            
            using var reg = delayCts.Token.Register(() => tcs.TrySetCanceled());
            
            await Task.WhenAny(tcs.Task, Task.Delay(2000, ct));
            return receivedResponse == "PONG";
        }
        catch
        {
            return false;
        }
        finally
        {
            OnCommandReceived -= OnPong;
        }
    }

    public void Connect()
    {
        if (_disposed) return;
        
        lock (_lock)
        {
            if (!_port.IsOpen)
            {
                try
                {
                    _port.Open();
                    _isConnected = true;
                    _logger.LogInformation("Serial port {PortName} connected", _port.PortName);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to connect to serial port {PortName}", _port.PortName);
                    throw;
                }
            }
        }
    }

    public void Disconnect()
    {
        if (_disposed) return;
        
        lock (_lock)
        {
            if (_port.IsOpen)
            {
                try
                {
                    _port.Close();
                    _isConnected = false;
                    _logger.LogInformation("Serial port {PortName} disconnected", _port.PortName);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error disconnecting from serial port {PortName}", _port.PortName);
                }
            }
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        
        _disposed = true;
        
        try
        {
            OnCommandReceived = null;
            _port?.DataReceived -= OnDataReceived;
            Disconnect();
            _port?.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error during serial port disposal");
        }
    }
}