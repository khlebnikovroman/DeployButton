using System.ComponentModel.DataAnnotations;

namespace DeployButton.Api.Services;

public interface IAudioConfigService
{
    Task<AudioConfigDto> GetConfigAsync();
    Task SaveConfigAsync(AudioConfigDto config);
}

public class AudioConfigDto
{
    [Range(0, 30)]
    public int Volume { get; set; } = 15;

    // Словарь: тип события → ID звука (может быть пустой строкой = выключено)
    public Dictionary<ButtonSoundEventType, string> Sounds { get; set; } = new()
    {
        { ButtonSoundEventType.BuildQueued, "0003" },
        { ButtonSoundEventType.BuildNotQueued, "0014" },
        { ButtonSoundEventType.BuildSucceeded, "0024" },
        { ButtonSoundEventType.BuildFailed, "0012" }
    };
}

public class SoundDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}

public enum ButtonSoundEventType
{
    ButtonPressed,
    BuildQueued,
    BuildNotQueued,
    BuildSucceeded,
    BuildFailed,
}