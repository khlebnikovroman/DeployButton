namespace DeployButton.Api.Hubs;

public interface IDeviceEventPublisher
{
    Task PublishDeviceStateChangedAsync();
    Task PublishDeployTriggeredAsync();
    Task PublishBuildStatusAsync(string status);
}