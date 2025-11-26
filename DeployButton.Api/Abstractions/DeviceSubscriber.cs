using DeployButton.Api.Hubs;

namespace DeployButton.Api.Abstractions;

public class DeviceSubscriber : IDeviceSubscriber
{
    private readonly ISerialDeviceAdapterProvider _adapterProvider;
    private ISerialDeviceAdapter? _adapter;
    private readonly ILogger<DeviceSubscriber> _logger;
    private readonly IDeployTrigger _deployTrigger;
    private readonly ISoundPlayer _soundPlayer;
    private readonly IDeviceEventPublisher _eventPublisher;
    public DeviceSubscriber(
        ISerialDeviceAdapterProvider adapterProvider,
        ILogger<DeviceSubscriber> logger,
        IDeployTrigger deployTrigger,
        ISoundPlayer soundPlayer,
        IDeviceEventPublisher eventPublisher)
    {
        _adapterProvider = adapterProvider;
        _logger = logger;
        _deployTrigger = deployTrigger;
        _soundPlayer = soundPlayer;
        _eventPublisher = eventPublisher;
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
        if (command.StartsWith("DEPLOY"))
        {
            DeployCommandReceived(command);
        }
    }


    private async Task DeployCommandReceived(string command)
    {
        _ = _eventPublisher.ButtonPressed();

        var triggerResult = await _deployTrigger.TriggerAsync();
        switch (triggerResult.deployResult)
        {
            case DeployResult.Queued:
                await PlaySound(ButtonSoundEventType.BuildQueued);
                break;
            case DeployResult.AlreadyBuilding:
            case DeployResult.Failed:
                await PlaySound(ButtonSoundEventType.BuildNotQueued);
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
                        await PlaySound(ButtonSoundEventType.BuildSucceeded);
                        break;
                    case BuildResult.Failed:
                        await PlaySound(ButtonSoundEventType.BuildFailed);
                        break;
                }
            });
        }
    }
    
    private async Task PlaySound(ButtonSoundEventType eventType)
    {
        var id = eventType switch
        {
            ButtonSoundEventType.ButtonPressed => "6",
            ButtonSoundEventType.BuildQueued => "3",
            ButtonSoundEventType.BuildNotQueued => "5", // 5, 14
            ButtonSoundEventType.BuildSucceeded => "6", // 6
            ButtonSoundEventType.BuildFailed => "9",
            _ => "6"
        };

        await PlaySoundById(id);
    }

    private async Task PlaySoundById(string soundId)
    {
        var soundLevel = 20;
        // await _soundPlayer.SetVolumeAsync(soundLevel);
        await _soundPlayer.PlaySoundAsync(soundId);
    }
    
    private enum ButtonSoundEventType
    {
        ButtonPressed,
        BuildQueued,
        BuildNotQueued,
        BuildSucceeded,
        BuildFailed,
    }
}