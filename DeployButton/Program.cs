using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using DeployButton.Api;
using DeployButton.Api.Abstractions;
using DeployButton.Api.Adapters;
using DeployButton.Api.Factories;
using DeployButton.Api.Configs;
using DeployButton.Api.Services;
using DeployButton.Services;

var builder = Host.CreateApplicationBuilder(args);

// Configuration
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// Options configuration
builder.Services.Configure<AppSettings>(builder.Configuration);

// HTTP client for TeamCity
builder.Services.AddHttpClient();

// Services
builder.Services.AddSingleton<IDeviceStateService, DeviceStateProvider>();
builder.Services.AddScoped<IDeviceStateProvider>(provider => 
    provider.GetRequiredService<IDeviceStateService>() as DeviceStateProvider);
builder.Services.AddSingleton<ISoundPlayer, SerialSoundPlayer>();
builder.Services.AddSingleton<ISerialDeviceAdapterFactory, SerialDeviceAdapterFactory>();

// TeamCity related services
builder.Services.AddSingleton<ITeamCityAuthenticationService, TeamCityAuthenticationService>();
builder.Services.AddSingleton<ITeamCityBuildService, TeamCityBuildService>();
builder.Services.AddSingleton<ITeamCityClientFactory, TeamCityClientFactory>();
builder.Services.AddSingleton<IDeployService, DeployService>();
builder.Services.AddSingleton<IBuildMonitoringService, BuildMonitoringService>();
builder.Services.AddSingleton<IDeployTrigger, TeamCityDeployHandler>();

// Hosted services
builder.Services.AddHostedService<DeviceMonitorService>();

// Определяем, запущено ли как служба
bool isService = !Environment.UserInteractive;

// Настройка службы Windows (если нужно)
if (isService)
{
    builder.Services.AddWindowsService(options =>
    {
        options.ServiceName = "DeployButton Service";
    });
}

// Добавляем ASP.NET Core
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();

// Запуск
await app.RunAsync();