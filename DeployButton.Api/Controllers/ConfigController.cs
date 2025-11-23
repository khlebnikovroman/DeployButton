using System.Text.Json;
using DeployButton.Api.Configs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DeployButton.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConfigController(
    IOptions<AppSettings> currentOptions,
    ILogger<ConfigController> logger)
    : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(currentOptions.Value);
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
            // Сохраняем в тот же файл, что и читаем: appsettings.settings.json рядом с .exe
            var configPath = Path.Combine(AppContext.BaseDirectory, "appsettings.settings.json");

            var json = JsonSerializer.Serialize(newConfig, new JsonSerializerOptions
            {
                WriteIndented = true,
                // Убедитесь, что enum'ы и null-значения сериализуются корректно
            });

            await System.IO.File.WriteAllTextAsync(configPath, json);

            logger.LogInformation("Конфигурация сохранена в {Path}", configPath);

            // Благодаря reloadOnChange: true в Program.cs,
            // IOptionsMonitor<AppSettings> автоматически обновит значение
            // и вызовет OnChange у подписчиков (в DeployButtonHostedService)

            return Ok(new { message = "Конфигурация успешно сохранена." });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при сохранении конфигурации");
            return StatusCode(500, new { error = "Не удалось сохранить файл конфигурации." });
        }
    }
}