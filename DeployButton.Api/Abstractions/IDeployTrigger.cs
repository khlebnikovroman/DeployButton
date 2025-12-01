namespace DeployButton.Api.Abstractions;

public interface IDeployTrigger
{
    Task<(DeployResult deployResult, Task<BuildResult>? buildTask)> TriggerAsync();
}
public enum DeployResult
{
    Queued,
    AlreadyBuilding,
    Failed,
}

public enum BuildResult
{
    Success,
    Failed,
}