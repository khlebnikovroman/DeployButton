using DeployButton.Api.Configs;

namespace DeployButton.Api;

public interface ITeamCityClientFactory
{
    TeamCityClient Create(TeamCityConfig config);
}