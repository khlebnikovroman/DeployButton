using DeployButton.Api.Configs;

namespace DeployButton.Api;

public interface ITeamCityClientFactory
{
    ITeamCityClient Create(TeamCityConfig config);
}