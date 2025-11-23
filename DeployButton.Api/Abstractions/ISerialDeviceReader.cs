namespace DeployButton.Api.Abstractions;

public interface ISerialDeviceReader
{
    event Action<string>? OnCommandReceived;
    bool IsConnected { get; }
}