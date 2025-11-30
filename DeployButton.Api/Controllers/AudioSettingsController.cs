using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using DeployButton.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace DeployButton.Api.Controllers;
public class AudioConfigDto
{
    [Range(0, 30)]
    public int Volume { get; set; } = 15;

    // Словарь: тип события → ID звука (может быть пустой строкой = выключено)
    public Dictionary<ButtonSoundEventType, string> Sounds { get; set; } = new()
    {
        { ButtonSoundEventType.BuildQueued, "0002" },
        { ButtonSoundEventType.BuildNotQueued, "" },
        { ButtonSoundEventType.BuildSucceeded, "0003" },
        { ButtonSoundEventType.BuildFailed, "0004" }
    };
}
public class SoundDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}
[ApiController]
[Route("api/[controller]")]
public class AudioSettingsController : ControllerBase
{
    private readonly IAudioConfigService _audioConfigService;
    private readonly ILogger<AudioSettingsController> _logger;

    public AudioSettingsController(IAudioConfigService audioConfigService, ILogger<AudioSettingsController> logger)
    {
        _audioConfigService = audioConfigService;
        _logger = logger;
    }

    [HttpGet("config")]
    public async Task<IActionResult> GetConfig()
    {
        try
        {
            var config = await _audioConfigService.GetConfigAsync();
            return Ok(config);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при загрузке конфигурации");
            return StatusCode(500, "Не удалось загрузить настройки");
        }
    }

    [HttpPost("config")]
    public async Task<IActionResult> SaveConfig([FromBody] AudioConfigDto config)
    {
        if (config == null || !ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            await _audioConfigService.SaveConfigAsync(config);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при сохранении конфигурации");
            return StatusCode(500, "Не удалось сохранить настройки");
        }
    }

    [HttpGet("sounds")]
    public IActionResult GetSounds()
    {
        var soundsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "sounds");
        if (!Directory.Exists(soundsFolder))
            return Ok(Array.Empty<SoundDto>());

        var files = Directory.GetFiles(soundsFolder)
            .Where(f => f.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase) ||
                        f.EndsWith(".wav", StringComparison.OrdinalIgnoreCase))
            .Select(Path.GetFileName)
            .Where(f => f is not null)
            .ToArray();

        var sounds = files.Select(file => new SoundDto
        {
            Id = Path.GetFileNameWithoutExtension(file!),
            Name = Path.GetFileNameWithoutExtension(file!),
            Url = $"/sounds/{file}"
        }).ToList();

        return Ok(sounds);
    }
}

// Services/IAudioConfigService.cs
public interface IAudioConfigService
{
    Task<AudioConfigDto> GetConfigAsync();
    Task SaveConfigAsync(AudioConfigDto config);
}

public class AudioConfigService : IAudioConfigService
{
    private readonly string _configFilePath;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly ILogger<AudioConfigService> _logger;

    public AudioConfigService(IHostEnvironment environment, ILogger<AudioConfigService> logger)
    {
        _logger = logger;
        _configFilePath = Path.Combine(environment.ContentRootPath, "audio-config.json");
    }

    public async Task<AudioConfigDto> GetConfigAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            if (!File.Exists(_configFilePath))
            {
                _logger.LogInformation("Файл конфигурации не найден. Создаём по умолчанию.");
                var defaultConfig = new AudioConfigDto();
                await SaveConfigInternalAsync(defaultConfig);
                return defaultConfig;
            }

            var json = await File.ReadAllTextAsync(_configFilePath);
            var config = JsonSerializer.Deserialize<AudioConfigDto>(json, 
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) 
                ?? new AudioConfigDto();

            return config;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при чтении конфигурации из {Path}", _configFilePath);
            return new AudioConfigDto(); // fallback
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task SaveConfigAsync(AudioConfigDto config)
    {
        if (config == null) throw new ArgumentNullException(nameof(config));

        await _semaphore.WaitAsync();
        try
        {
            await SaveConfigInternalAsync(config);
            _logger.LogInformation("Конфигурация звука сохранена в {Path}", _configFilePath);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task SaveConfigInternalAsync(AudioConfigDto config)
    {
        var json = JsonSerializer.Serialize(config, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        await File.WriteAllTextAsync(_configFilePath, json);
    }
}