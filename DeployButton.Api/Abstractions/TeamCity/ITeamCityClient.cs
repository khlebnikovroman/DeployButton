namespace DeployButton.Api.Abstractions.TeamCity;

public interface ITeamCityClient : IDisposable
{
    Task<bool> IsBuildQueuedOrRunningAsync();
    Task<string> TriggerBuildAsync();
    Task<string?> GetBuildStatusAsync(string buildId);
}