namespace DeployButton.Api.Abstractions;

public interface ISoundPlayer
{
    Task PlaySoundAsync(string soundId);
    Task SetVolumeAsync(int volume);
}