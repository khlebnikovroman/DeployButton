using System.ComponentModel.DataAnnotations;
using DeployButton.Api.Services;

namespace DeployButton.Api.DTOs;

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