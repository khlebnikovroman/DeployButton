using DeployButton.Api.Abstractions;
using DeployButton.Api.Abstractions.TeamCity;
using DeployButton.Api.Configs;
using DeployButton.Api.Services.TeamCity;

namespace DeployButton.Api.Factories;

public class TeamCityClientFactory : ITeamCityClientFactory
{
    public ITeamCityClient Create(TeamCityConfig config)
    {
        var handler = new HttpClientHandler { UseCookies = false };
        var httpClient = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(15) };
        return new TeamCityClient(httpClient, config);
    }
}