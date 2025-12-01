using System.Text;
using System.Text.Json;

namespace DeployButton.Api.Configs;

public interface IConfigProvider<T>
{
    T Current { get; }
    event Action<T>? OnChange;
    T Reload();
    Task SaveAsync(T newConfig);
}

public class FileConfigProvider<T> : IConfigProvider<T>, IDisposable
{
    private readonly string _configFilePath;
    private readonly ILogger<FileConfigProvider<T>> _logger;
    private readonly FileSystemWatcher _watcher;
    private readonly SemaphoreSlim _writeLock = new(1, 1);
    private readonly JsonSerializerOptions _jsonOptions;
    private T _current;

    public event Action<T>? OnChange;

    public T Current => _current;

    public FileConfigProvider(
        string configFileName,
        ILogger<FileConfigProvider<T>> logger,
        JsonSerializerOptions? jsonOptions = null)
    {
        _logger = logger;
        _jsonOptions = jsonOptions ?? new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };

        _configFilePath = Path.Combine(AppContext.BaseDirectory, configFileName);

        if (!File.Exists(_configFilePath))
            throw new FileNotFoundException($"Файл конфигурации не найден: {_configFilePath}");

        _current = LoadConfig();

        _watcher = new FileSystemWatcher(Path.GetDirectoryName(_configFilePath)!, Path.GetFileName(_configFilePath))
        {
            NotifyFilter = NotifyFilters.LastWrite,
            EnableRaisingEvents = true
        };
        _watcher.Changed += OnFileChanged;
    }

    private T LoadConfig()
    {
        var json = File.ReadAllText(_configFilePath, Encoding.UTF8);
        return JsonSerializer.Deserialize<T>(json, _jsonOptions)
               ?? throw new InvalidOperationException($"Не удалось десериализовать конфигурацию типа {typeof(T).Name}.");
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        try
        {
            var newConfig = LoadConfig();
            _current = newConfig;
            OnChange?.Invoke(newConfig);
            _logger.LogInformation("Конфигурация типа {Type} перезагружена из файла {File}.", 
                typeof(T).Name, _configFilePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при перезагрузке конфигурации типа {Type}.", typeof(T).Name);
        }
    }

    public T Reload()
    {
        _current = LoadConfig();
        return _current;
    }

    public async Task SaveAsync(T newConfig)
    {
        if (newConfig == null)
            throw new ArgumentNullException(nameof(newConfig));

        await _writeLock.WaitAsync();
        try
        {
            var json = JsonSerializer.Serialize(newConfig, _jsonOptions);
            var tempPath = _configFilePath + ".tmp";
            await File.WriteAllTextAsync(tempPath, json, Encoding.UTF8);
            File.Replace(tempPath, _configFilePath, null);
            
            _current = newConfig;
            OnChange?.Invoke(newConfig);
            _logger.LogInformation("Конфигурация типа {Type} успешно сохранена в {File}.", 
                typeof(T).Name, _configFilePath);
        }
        finally
        {
            _writeLock.Release();
        }
    }

    public void Dispose()
    {
        _watcher?.Dispose();
        _writeLock?.Dispose();
    }
}           