using System.Net.Http.Headers;
using System.Text;
using System.Xml;
using DeployButton.Api.Configs;

namespace DeployButton.Api.Services;

public interface ITeamCityBuildService
{
    Task<bool> IsBuildQueuedOrRunningAsync(TeamCityConfig config);
    Task TriggerBuildAsync(TeamCityConfig config);
    Task<string?> GetLastBuildIdAsync(TeamCityConfig config);
    Task<string?> GetBuildStatusAsync(string buildId, TeamCityConfig config);
}

public class TeamCityBuildService : ITeamCityBuildService
{
    private readonly HttpClient _httpClient;
    private readonly ITeamCityAuthenticationService _authService;

    public TeamCityBuildService(HttpClient httpClient, ITeamCityAuthenticationService authService)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
    }

    public async Task<bool> IsBuildQueuedOrRunningAsync(TeamCityConfig config)
    {
        if (await IsBuildStateActiveAsync(config, "queued")) return true;
        if (await IsBuildStateActiveAsync(config, "running")) return true;
        return false;
    }

    private async Task<bool> IsBuildStateActiveAsync(TeamCityConfig config, string state)
    {
        var url = $"{config.BaseUrl}/httpAuth/app/rest/builds?locator=buildType:{config.BuildConfigurationId},state:{state},count:1";
        try
        {
            _authService.SetAuthentication(_httpClient, config);
            
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                return response.StatusCode == System.Net.HttpStatusCode.NotFound ? false : false;

            var xml = await response.Content.ReadAsStringAsync();
            var doc = new XmlDocument();
            doc.LoadXml(xml);

            var countAttr = doc.DocumentElement?.Attributes["count"];
            return countAttr != null && int.TryParse(countAttr.Value, out var count) && count > 0;
        }
        catch
        {
            return false;
        }
    }

    public async Task TriggerBuildAsync(TeamCityConfig config)
    {
        _authService.SetAuthentication(_httpClient, config);
        
        var triggerUrl = $"{config.BaseUrl}/httpAuth/action.html?add2Queue={config.BuildConfigurationId}";
        var response = await _httpClient.PostAsync(triggerUrl, null);
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"HTTP {response.StatusCode}: {response.ReasonPhrase}");
    }

    public async Task<string?> GetLastBuildIdAsync(TeamCityConfig config)
    {
        var url = $"{config.BaseUrl}/httpAuth/app/rest/builds?locator=buildType:{config.BuildConfigurationId},count:1";
        try
        {
            _authService.SetAuthentication(_httpClient, config);
            
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode) return null;

            var xml = await response.Content.ReadAsStringAsync();
            var doc = new XmlDocument();
            doc.LoadXml(xml);
            var build = doc.DocumentElement?.FirstChild;
            return build?.Attributes?["id"]?.Value;
        }
        catch
        {
            return null;
        }
    }

    public async Task<string?> GetBuildStatusAsync(string buildId, TeamCityConfig config)
    {
        var url = $"{config.BaseUrl}/httpAuth/app/rest/builds/id:{buildId}";
        try
        {
            _authService.SetAuthentication(_httpClient, config);
            
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode) return null;

            var xml = await response.Content.ReadAsStringAsync();
            var doc = new XmlDocument();
            doc.LoadXml(xml);
            return doc.DocumentElement?.Attributes?["status"]?.Value;
        }
        catch
        {
            return null;
        }
    }
}