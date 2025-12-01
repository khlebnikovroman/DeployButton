using System.Text.Json;
using DeployButton.Api.Configs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DeployButton.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConfigController : ControllerBase
{
    private readonly ILogger<ConfigController> _logger;

    public ConfigController(IConfigProvider<AppSettings> configProvider,
        ILogger<ConfigController> logger)
    {
        _logger = logger;
        _configProvider = configProvider;
    }

    private readonly IConfigProvider<AppSettings> _configProvider;

    [HttpGet]
    public IActionResult Get()
    {
        return Ok(_configProvider.Current);
    }

    [HttpPost]
    public async Task<IActionResult> Update([FromBody] AppSettings? newConfig)
    {
        if (newConfig == null)
            return BadRequest("Конфигурация не предоставлена.");

        if (string.IsNullOrWhiteSpace(newConfig.TeamCity?.BaseUrl) ||
            string.IsNullOrWhiteSpace(newConfig.TeamCity?.BuildConfigurationId))
        {
            return BadRequest("BaseUrl и BuildConfigurationId обязательны.");
        }

        try
        {
            await _configProvider.SaveAsync(newConfig);
            return Ok(new { message = "Конфигурация успешно сохранена." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при сохранении конфигурации");
            return StatusCode(500, new { error = "Не удалось сохранить файл конфигурации." });
        }
    }
}