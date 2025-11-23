// Factories/SerialDeviceAdapterFactory.cs

using DeployButton.Api.Adapters;

namespace DeployButton.Api.Factories;

public class SerialDeviceAdapterFactory : ISerialDeviceAdapterFactory
{
    private readonly ILogger<SerialDeviceAdapter> _logger;

    public SerialDeviceAdapterFactory(ILogger<SerialDeviceAdapter> logger)
    {
        _logger = logger;
    }

    public SerialDeviceAdapter Create(string portName, int baudRate)
    {
        var port = new System.IO.Ports.SerialPort(portName, baudRate)
        {
            DtrEnable = true,
            RtsEnable = true,
            ReadTimeout = 1000,
            WriteTimeout = 1000
        };
        return new SerialDeviceAdapter(port, _logger);
    }
}