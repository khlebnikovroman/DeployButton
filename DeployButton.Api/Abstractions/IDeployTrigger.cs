using DeployButton.Api.Enums;

namespace DeployButton.Api.Abstractions;

public interface IDeployTrigger
{
    Task<(DeployResult deployResult, Task<BuildResult>? buildTask)> TriggerAsync();
}