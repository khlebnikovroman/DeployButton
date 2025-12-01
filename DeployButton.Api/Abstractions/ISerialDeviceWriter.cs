namespace DeployButton.Api.Abstractions;

public interface ISerialDeviceWriter
{
    Task SendCommandAsync(string command);
}
public interface ISerialDeviceAdapter: ISerialDeviceWriter, ISerialDeviceReader, IDisposable;