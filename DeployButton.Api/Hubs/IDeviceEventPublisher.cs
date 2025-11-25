namespace DeployButton.Api.Hubs;

public interface IDeviceEventPublisher
{
    Task PublishDeviceStateChangedAsync();
    Task ButtonPressed();
}