using System.Net.Http.Headers;
using System.Text;
using DeployButton.Api.Configs;
using DeployButton.Api.Services;

namespace DeployButton.Api;

public class TeamCityClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly ITeamCityAuthenticationService _authService;
    private readonly TeamCityConfig _config;

    public TeamCityClient(HttpClient httpClient, ITeamCityAuthenticationService authService, TeamCityConfig config)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    public async Task<bool> IsBuildQueuedOrRunningAsync()
    {
        _authService.SetAuthentication(_httpClient, _config);
        
        var url = $"{_config.BaseUrl}/httpAuth/app/rest/builds?locator=buildType:{_config.BuildConfigurationId},state:queued,running,count:1";
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                return response.StatusCode == System.Net.HttpStatusCode.NotFound ? false : false;

            var xml = await response.Content.ReadAsStringAsync();
            var doc = new System.Xml.XmlDocument();
            doc.LoadXml(xml);

            var countAttr = doc.DocumentElement?.Attributes["count"];
            return countAttr != null && int.TryParse(countAttr.Value, out var count) && count > 0;
        }
        catch
        {
            return false;
        }
    }

    public async Task TriggerBuildAsync()
    {
        _authService.SetAuthentication(_httpClient, _config);
        
        var triggerUrl = $"{_config.BaseUrl}/httpAuth/action.html?add2Queue={_config.BuildConfigurationId}";
        var response = await _httpClient.PostAsync(triggerUrl, null);
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"HTTP {response.StatusCode}: {response.ReasonPhrase}");
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}