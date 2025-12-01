using DeployButton.Api.Configs;

namespace DeployButton.Api.Abstractions.TeamCity;

public interface ITeamCityClientFactory
{
    ITeamCityClient Create(TeamCityConfig config);
}