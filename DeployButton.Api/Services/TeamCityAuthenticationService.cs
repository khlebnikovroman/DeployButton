using System.Net.Http.Headers;
using System.Text;
using DeployButton.Api.Configs;

namespace DeployButton.Api.Services;

public interface ITeamCityAuthenticationService
{
    void SetAuthentication(HttpClient httpClient, TeamCityConfig config);
}

public class TeamCityAuthenticationService : ITeamCityAuthenticationService
{
    public void SetAuthentication(HttpClient httpClient, TeamCityConfig config)
    {
        if (httpClient == null) throw new ArgumentNullException(nameof(httpClient));
        if (config == null) throw new ArgumentNullException(nameof(config));
        
        if (string.IsNullOrEmpty(config.Username) || string.IsNullOrEmpty(config.Password))
        {
            throw new InvalidOperationException("TeamCity credentials are not configured properly");
        }
        
        var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{config.Username}:{config.Password}"));
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
    }
}