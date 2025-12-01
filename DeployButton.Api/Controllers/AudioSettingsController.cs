using System.ComponentModel.DataAnnotations;
using DeployButton.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Mp3Formatter;

namespace DeployButton.Api.Controllers;

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
            .Where(f => f.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase))
            // .Select(Path.GetFileName)
            .Where(f => f is not null)
            .ToArray();
        
        var sounds = files.Select(file =>
        {
            var fileName = Path.GetFileName(file);
            var filenameWithoutExtension = Path.GetFileNameWithoutExtension(file);
            var name = filenameWithoutExtension;
            try
            {
                name = Mp3MetadataReader.GetOriginalFilename(file!);
            }
            catch { }
            return new SoundDto
            {
                Id = filenameWithoutExtension,
                Name = name,
                Url = $"/sounds/{fileName}"
            };
        }).ToList();

        return Ok(sounds);
    }
}