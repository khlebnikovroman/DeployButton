namespace DeployButton.Api.Abstractions;

public interface ISerialDeviceAdapterProvider
{
    event EventHandler OnAdapterChanged;
    ISerialDeviceAdapter? GetAdapter();
}