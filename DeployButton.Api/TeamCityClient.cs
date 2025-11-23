using System.Net.Http.Headers;
using System.Text;
using System.Xml;
using DeployButton.Api.Configs;

namespace DeployButton.Api;

public class TeamCityClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly TeamCityConfig _config;

    public TeamCityClient(HttpClient httpClient, TeamCityConfig config)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _config = config ?? throw new ArgumentNullException(nameof(config));

        var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_config.Username}:{_config.Password}"));
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
    }

    public async Task<bool> IsBuildQueuedOrRunningAsync()
    {
        var baseUrl = _config.BaseUrl;
        var buildTypeId = _config.BuildConfigurationId;

        if (await IsBuildStateActiveAsync(baseUrl, buildTypeId, "queued")) return true;
        if (await IsBuildStateActiveAsync(baseUrl, buildTypeId, "running")) return true;
        return false;
    }

    private async Task<bool> IsBuildStateActiveAsync(string baseUrl, string buildTypeId, string state)
    {
        var url = $"{baseUrl}/httpAuth/app/rest/builds?locator=buildType:{buildTypeId},state:{state},count:1";
        try
        {
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

    public async Task TriggerBuildAsync()
    {
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