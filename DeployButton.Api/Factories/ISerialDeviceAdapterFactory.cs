using DeployButton.Api.Adapters;

namespace DeployButton.Api.Factories;

public interface ISerialDeviceAdapterFactory
{
    SerialDeviceAdapter Create(string portName, int baudRate);
}