namespace DeployButton.Api.Abstractions;

public interface IDeployTrigger
{
    Task TriggerAsync();
}