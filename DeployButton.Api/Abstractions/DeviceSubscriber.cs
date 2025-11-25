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
        var rnd = new Random();
        await _soundPlayer.SetVolumeAsync(20);
        // await _soundPlayer.PlaySoundAsync(rnd.Next(1, 15).ToString());
        await _soundPlayer.PlaySoundAsync(6.ToString());
    }
}