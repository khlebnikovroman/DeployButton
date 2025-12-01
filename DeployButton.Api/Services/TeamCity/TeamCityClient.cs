using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using DeployButton.Api.Abstractions.TeamCity;
using DeployButton.Api.Configs;

namespace DeployButton.Api.Services.TeamCity;

public class TeamCityClient : ITeamCityClient
{
    private readonly HttpClient _httpClient;
    private readonly TeamCityConfig _config;

    public TeamCityClient(HttpClient httpClient, TeamCityConfig config)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _config = config ?? throw new ArgumentNullException(nameof(config));

        var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_config.Username}:{_config.Password}"));
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<bool> IsBuildQueuedOrRunningAsync()
    {
        var tasks = new[]
        {
            IsBuildStateActiveAsync("queued"),
            IsBuildStateActiveAsync("running")
        };

        await Task.WhenAll(tasks);
        return tasks.Any(t => t.Result);
    }

    private async Task<bool> IsBuildStateActiveAsync(string state)
    {
        var url = $"{_config.BaseUrl}/httpAuth/app/rest/builds?locator=buildType:{_config.BuildConfigurationId},state:{state},count:1";

        try
        {
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return false;

            using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
            var count = doc.RootElement.GetProperty("count").GetInt32();
            return count > 0;
        }
        catch
        {
            return false;
        }
    }

    public async Task<string> TriggerBuildAsync()
    {
        var url = $"{_config.BaseUrl}/httpAuth/app/rest/buildQueue";
        var payload = new
        {
            buildType = new
            {
                id = _config.BuildConfigurationId
            }
        };

        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = false });
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(url, content);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"HTTP {response.StatusCode}: {response.ReasonPhrase}. Body: {error}");
        }

        using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        var id = doc.RootElement.GetProperty("id").GetRawText();
        return id;
    }
    
    public async Task<string?> GetBuildStatusAsync(string buildId)
    {
        var url = $"{_config.BaseUrl}/httpAuth/app/rest/builds/id:{buildId}";
        try
        {
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return null;

            using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
            return doc.RootElement.TryGetProperty("status", out var statusEl) ? statusEl.GetString() : null;
        }
        catch
        {
            return null;
        }
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}