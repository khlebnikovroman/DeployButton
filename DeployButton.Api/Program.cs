using DeployButton.Api.Abstractions;
using DeployButton.Api.Abstractions.TeamCity;
using DeployButton.Api.Adapters;
using DeployButton.Api.Configs;
using DeployButton.Api.Controllers;
using DeployButton.Api.Factories;
using DeployButton.Api.Hubs;
using DeployButton.Api.Services;
using DeployButton.Api.Services.TeamCity;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;

namespace DeployButton.Api;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Configuration
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        // === 2. Сервисы DI ===
        builder.Services.AddSignalR();
        builder.Services.AddSingleton<IConfigProvider<AppSettings>>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<FileConfigProvider<AppSettings>>>();
            return new FileConfigProvider<AppSettings>("appsettings.settings.json", logger);
        });

        builder.Services.AddSingleton<IDeviceEventPublisher, DeviceEventPublisher>();
        builder.Services.AddSingleton<IDeviceStateProvider, DeviceStateProvider>();
        builder.Services.AddSingleton<IDeployTrigger, TeamCityDeployHandler>();
        builder.Services.AddSingleton<ISerialDeviceAdapterFactory, SerialDeviceAdapterFactory>();
        builder.Services.AddSingleton<ITeamCityClientFactory, TeamCityClientFactory>();
        builder.Services.AddSingleton<ISoundPlayer, SerialSoundPlayer>();
        builder.Services.AddSingleton<DeviceMonitorService>();
        builder.Services.AddSingleton<DeviceMonitorHostedService>();
        builder.Services.AddSingleton<ISerialDeviceAdapterProvider>(x => x.GetService<DeviceMonitorService>());
        builder.Services.AddHostedService<DeviceMonitorHostedService>();
        builder.Services.AddSingleton<IDeviceSubscriber, DeviceSubscriber>();
        builder.Services.AddSingleton<IAudioConfigService, AudioConfigService>();
        
        // Web API
        builder.Services.AddControllers();

        // Windows Service (только если не в интерактивном режиме)
        if (!Environment.UserInteractive)
        {
            builder.Services.AddWindowsService(options =>
            {
                options.ServiceName = "DeployButton Service";
            });
        }
        else
        {
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
            var fileProvider = new PhysicalFileProvider(wwwrootPath);
            var contentTypeProvider = new FileExtensionContentTypeProvider();
            contentTypeProvider.Mappings[".hdr"] = "image/vnd.radiance";
            contentTypeProvider.Mappings[".glb"] = "model/gltf-binary";

            var staticFileOptions = new StaticFileOptions
            {
                FileProvider = fileProvider,
                ContentTypeProvider = contentTypeProvider
            };

            app.UseDefaultFiles();
            app.UseStaticFiles(staticFileOptions);

            logger.LogInformation($"SPA: serving static files from {wwwrootPath}");
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
        if (app.Environment.IsDevelopment())
        {
            app.UseCors(policy => policy
                .AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod());
        }
        app.MapHub<DeviceHub>("hubs/deviceHub");
        app.MapControllers();
        app.MapFallbackToFile("/index.html"); // для Angular маршрутов

        // === 5. Запуск ===
        logger.LogInformation("Запуск DeployButton...");
        app.Services.GetService<IDeviceSubscriber>().Subscribe();
        await app.RunAsync();
    }
}