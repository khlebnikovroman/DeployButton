using DeployButton.Api.Abstractions;

namespace DeployButton.Api.Services;

public class SerialSoundPlayer : ISoundPlayer
{
    private readonly ISerialDeviceAdapterProvider _adapterProvider;
    private readonly ILogger<SerialSoundPlayer> _logger;

    public SerialSoundPlayer(ISerialDeviceAdapterProvider adapterProvider, ILogger<SerialSoundPlayer> logger)
    {
        _adapterProvider = adapterProvider;
        _logger = logger;
    }

    public async Task PlaySoundAsync(string soundId)
    {
        if (int.TryParse(soundId, out var num) && num is >= 1 and <= 255)
        {
            await _adapterProvider.GetAdapter().SendCommandAsync($"PLAYSOUND {soundId}");
        }
        else
        {
            _logger.LogWarning("Некорректный ID звука: {SoundId}", soundId);
        }
    }

    public async Task SetVolumeAsync(int volume)
    {
        if (volume is >= 0 and <= 30)
        {
            await _adapterProvider.GetAdapter().SendCommandAsync($"SETVOLUME {volume}");
        }
    }
}