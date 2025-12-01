using System.Text.Json;
using Microsoft.Extensions.Hosting;

namespace DeployButton.Api.Services;

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