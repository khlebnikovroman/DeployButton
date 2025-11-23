using System.Net.Http.Headers;
using System.Xml;
using DeployButton.Api.Configs;
using Microsoft.Extensions.Options;

namespace DeployButton.Api.Services;

public class TeamCityService : ITeamCityService, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly IOptionsMonitor<AppSettings> _options;
    private readonly ILogger<TeamCityService> _logger;

    public TeamCityService(HttpClient httpClient, IOptionsMonitor<AppSettings> options, ILogger<TeamCityService> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<bool> IsBuildQueuedOrRunningAsync(TeamCityConfig config, CancellationToken cancellationToken = default)
    {
        return await IsBuildStateActiveAsync(config, "queued", cancellationToken) ||
               await IsBuildStateActiveAsync(config, "running", cancellationToken);
    }

    private async Task<bool> IsBuildStateActiveAsync(TeamCityConfig config, string state, CancellationToken cancellationToken)
    {
        var url = $"{config.BaseUrl}/httpAuth/app/rest/builds?locator=buildType:{config.BuildConfigurationId},state:{state},count:1";
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode) 
                return false;

            var xml = await response.Content.ReadAsStringAsync(cancellationToken);
            var doc = new XmlDocument();
            doc.LoadXml(xml);
            var count = doc.DocumentElement?.Attributes["count"]?.Value;
            return int.TryParse(count, out var c) && c > 0;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error checking build state {State}", state);
            return false;
        }
    }

    public async Task<string?> TriggerBuildAsync(TeamCityConfig config, CancellationToken cancellationToken = default)
    {
        var url = $"{config.BaseUrl}/httpAuth/action.html?add2Queue={config.BuildConfigurationId}";
        var response = await _httpClient.PostAsync(url, null, cancellationToken);
        
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"HTTP {response.StatusCode}: {response.ReasonPhrase}");

        // Return the build ID if available in response, otherwise null
        return await GetLastBuildIdAsync(config, cancellationToken);
    }

    public async Task<string?> GetBuildStatusAsync(string buildId, TeamCityConfig config, CancellationToken cancellationToken = default)
    {
        var url = $"{config.BaseUrl}/httpAuth/app/rest/builds/id:{buildId}";
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
            
            using var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode) 
                return null;

            var xml = await response.Content.ReadAsStringAsync(cancellationToken);
            var doc = new XmlDocument();
            doc.LoadXml(xml);
            return doc.DocumentElement?.Attributes?["status"]?.Value;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error getting build status for build {BuildId}", buildId);
            return null;
        }
    }

    public async Task<string?> GetLastBuildIdAsync(TeamCityConfig config, CancellationToken cancellationToken = default)
    {
        var url = $"{config.BaseUrl}/httpAuth/app/rest/builds?locator=buildType:{config.BuildConfigurationId},count:1";
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
            
            using var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode) 
                return null;

            var xml = await response.Content.ReadAsStringAsync(cancellationToken);
            var doc = new XmlDocument();
            doc.LoadXml(xml);
            var build = doc.DocumentElement?.FirstChild;
            return build?.Attributes?["id"]?.Value;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error getting last build ID");
            return null;
        }
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}