using DeployButton.Api.Abstractions;

namespace DeployButton.Api.Services;

public class SerialSoundPlayer : ISoundPlayer
{
    private readonly ISerialDeviceWriter _writer;
    private readonly ILogger<SerialSoundPlayer> _logger;

    public SerialSoundPlayer(ISerialDeviceWriter writer, ILogger<SerialSoundPlayer> logger)
    {
        _writer = writer;
        _logger = logger;
    }

    public async Task PlaySoundAsync(string soundId)
    {
        if (int.TryParse(soundId, out var num) && num is >= 1 and <= 255)
        {
            await _writer.SendCommandAsync($"PLAYSOUND {soundId}");
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
            await _writer.SendCommandAsync($"SETVOLUME {volume}");
        }
    }
}