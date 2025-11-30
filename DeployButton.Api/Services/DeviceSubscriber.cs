using DeployButton.Api.Abstractions;
using DeployButton.Api.Controllers;
using DeployButton.Api.Hubs;

namespace DeployButton.Api.Services;

public class DeviceSubscriber : IDeviceSubscriber
{
    private readonly ISerialDeviceAdapterProvider _adapterProvider;
    private ISerialDeviceAdapter? _adapter;
    private readonly ILogger<DeviceSubscriber> _logger;
    private readonly IDeployTrigger _deployTrigger;
    private readonly ISoundPlayer _soundPlayer;
    private readonly IDeviceEventPublisher _eventPublisher;
    private readonly IAudioConfigService _audioConfigService;

    public DeviceSubscriber(
        ISerialDeviceAdapterProvider adapterProvider,
        ILogger<DeviceSubscriber> logger,
        IDeployTrigger deployTrigger,
        ISoundPlayer soundPlayer,
        IDeviceEventPublisher eventPublisher,
        IAudioConfigService audioConfigService)
    {
        _adapterProvider = adapterProvider;
        _logger = logger;
        _deployTrigger = deployTrigger;
        _soundPlayer = soundPlayer;
        _eventPublisher = eventPublisher;
        _audioConfigService = audioConfigService;
    }
    
    public void Subscribe()
    {
        _adapterProvider.OnAdapterChanged += AdapterProviderOnOnAdapterChanged;
    }

    private void AdapterProviderOnOnAdapterChanged(object? sender, EventArgs e)
    {
        _adapter?.Dispose();
        _adapter = _adapterProvider.GetAdapter();
        _eventPublisher.PublishDeviceStateChangedAsync();
        if (_adapter != null)
        {
            _adapter.OnCommandReceived += AdapterOnOnCommandReceived;
        }
    }

    private void AdapterOnOnCommandReceived(string command)
    {
        _logger.LogInformation("Received command: {Command}", command);
        if (command.StartsWith("BUTTONPRESS"))
        {
            _ = ButtonPressCommandReceived(command);
        }
        if (command.StartsWith("BUTTONRELEASE"))
        {
            _ = DeployCommandReceived(command);
        }
    }

    private async Task ButtonPressCommandReceived(string command)
    {
        _ = _eventPublisher.ButtonPressed();
    }

    private async Task DeployCommandReceived(string command)
    {
        _ = _eventPublisher.ButtonReleased();
        var config = await _audioConfigService.GetConfigAsync();
        _ = _soundPlayer.SetVolumeAsync(config.Volume);
        var triggerResult = await _deployTrigger.TriggerAsync();
        switch (triggerResult.deployResult)
        {
            case DeployResult.Queued:
                await PlaySound(ButtonSoundEventType.BuildQueued, config);
                break;
            case DeployResult.AlreadyBuilding:
            case DeployResult.Failed:
                await PlaySound(ButtonSoundEventType.BuildNotQueued, config);
                break;
        }

        if (triggerResult.buildTask != null)
        {
            _ = Task.Run(async () =>
            {
                var buildResult = await triggerResult.buildTask;
                switch (buildResult)
                {
                    case BuildResult.Success:
                        await PlaySound(ButtonSoundEventType.BuildSucceeded, config);
                        break;
                    case BuildResult.Failed:
                        await PlaySound(ButtonSoundEventType.BuildFailed, config);
                        break;
                }
            });
        }
    }
    
    private async Task PlaySound(ButtonSoundEventType eventType, AudioConfigDto audioConfig)
    {
        var id = audioConfig.Sounds[eventType];
        await PlaySoundById(id);
    }

    private async Task PlaySoundById(string soundId)
    {
        await _soundPlayer.PlaySoundAsync(soundId);
    }
}
public enum ButtonSoundEventType
{
    ButtonPressed,
    BuildQueued,
    BuildNotQueued,
    BuildSucceeded,
    BuildFailed,
}