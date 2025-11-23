using DeployButton.Api.Abstractions;
using DeployButton.Api.Adapters;
using DeployButton.Api.Configs;
using DeployButton.Api.Factories;
using DeployButton.Api.Services;
using Microsoft.OpenApi;

namespace DeployButton.Api;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Configuration
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddJsonFile("appsettings.settings.json", optional: false, reloadOnChange: true)
            .Build();

        // === 2. Сервисы DI ===
        builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

        builder.Services.AddSingleton<IDeviceStateProvider, DeviceStateProvider>();
        builder.Services.AddSingleton<IDeployTrigger, TeamCityDeployHandler>(); // Legacy - will be removed
        builder.Services.AddScoped<ITeamCityService, TeamCityService>();
        builder.Services.AddScoped<IBuildStatusMonitor, BuildStatusMonitor>();
        builder.Services.AddScoped<IDeploymentService, TeamCityDeploymentService>();
        builder.Services.AddSingleton<IDeploymentServiceFactory, DeploymentServiceFactory>();
        builder.Services.AddSingleton<ISerialDeviceAdapterFactory, SerialDeviceAdapterFactory>();
        builder.Services.AddSingleton<ISoundPlayer, SerialSoundPlayer>();
        builder.Services.AddSingleton<DeviceCommandHandler>();
        builder.Services.AddSingleton<DeviceMonitorService>();
        builder.Services.AddSingleton<RefactoredDeviceMonitorService>();
        builder.Services.AddHostedService<RefactoredDeviceMonitorService>();

        // Web API
        builder.Services.AddControllers();

        // Windows Service (только если не в интерактивном режиме)
        if (!Environment.UserInteractive)
        {
            builder.Services.AddWindowsService(options =>
            {
                options.ServiceName = "DeployButton Service";
            });
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo()
                {
                    Title = "DeployButton API",
                    Version = "v1",
                    Description = "API для управления настройками DeployButton Service"
                });
            });
        }

        // === 3. Построение приложения ===
        var app = builder.Build();

        // === 4. Настройка HTTP-пайплайна ===
        var logger = app.Services.GetRequiredService<ILogger<Program>>();

        var wwwrootPath = Path.Combine(app.Environment.ContentRootPath, "wwwroot");
        if (Directory.Exists(wwwrootPath))
        {
            app.UseDefaultFiles();
            app.UseStaticFiles();
            logger.LogInformation($"SPA: раздача из {wwwrootPath}");
        }
        else
        {
            logger.LogWarning($"Папка wwwroot не найдена. UI недоступен.");
        }
        
        if (Environment.UserInteractive)
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "DeployButton v1");
                options.RoutePrefix = "swagger"; // UI будет на /swagger
            });
        }
        
        app.MapControllers();
        app.MapFallbackToFile("/index.html"); // для Angular маршрутов

        // === 5. Запуск ===
        logger.LogInformation("Запуск DeployButton...");
        
        await app.RunAsync();
    }
}