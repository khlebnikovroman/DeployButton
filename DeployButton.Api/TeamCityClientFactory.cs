using System.Net.Http.Headers;
using System.Text;
using System.Xml;
using DeployButton.Api.Abstractions;
using DeployButton.Api.Configs;
using DeployButton.Api.Services;
using Microsoft.Extensions.Options;

namespace DeployButton.Api;

public class TeamCityClientFactory : ITeamCityClientFactory
{
    public TeamCityClient Create(TeamCityConfig config)
    {
        var handler = new HttpClientHandler { UseCookies = false };
        var httpClient = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(15) };
        return new TeamCityClient(httpClient, config);
    }
}


public class TeamCityDeployHandler : IDeployTrigger, IDisposable
{
    private readonly IOptionsMonitor<AppSettings> _options;
    private readonly ISoundPlayer _soundPlayer;
    private readonly IDeploymentService _deploymentService;
    private readonly ILogger<TeamCityDeployHandler> _logger;

    private int _isHandling = 0;
    private readonly CancellationTokenSource _cts = new();

    public TeamCityDeployHandler(
        IOptionsMonitor<AppSettings> options,
        ISoundPlayer soundPlayer,
        IDeploymentService deploymentService,
        ILogger<TeamCityDeployHandler> logger)
    {
        _options = options;
        _soundPlayer = soundPlayer;
        _deploymentService = deploymentService;
        _logger = logger;
    }

    public async Task TriggerAsync()
    {
        // Delegate to the new deployment service
        await _deploymentService.TriggerDeploymentAsync();
    }

    public void Dispose()
    {
        _cts.Cancel();
    }
}
