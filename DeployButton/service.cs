// using System.IO.Ports;
//
// namespace DeployButton;
//
// public class DeployButtonHostedService : IHostedService, IDisposable
// {
//     private SerialPort _serialPort;
//     private TeamCityClient _teamCityClient;
//     private readonly ILogger<DeployButtonHostedService> _logger;
//     private readonly IConfiguration _configuration;
//
//     public DeployButtonHostedService(ILogger<DeployButtonHostedService> logger, IConfiguration configuration)
//     {
//         _logger = logger;
//         _configuration = configuration;
//     }
//
//     public Task StartAsync(CancellationToken ct)
//     {
//         try
//         {
//             var config = LoadConfig();
//             if (config?.TeamCity == null)
//             {
//                 _logger.LogError("Конфигурация TeamCity не загружена.");
//                 return Task.CompletedTask;
//             }
//
//             _teamCityClient = new TeamCityClient(config.TeamCity);
//             _serialPort = new SerialPort(config.SerialPort.PortName, config.SerialPort.BaudRate);
//             _serialPort.DataReceived += OnDataReceived;
//             _serialPort.Open();
//
//             _logger.LogInformation($"Слушаю порт {_serialPort.PortName}");
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "Ошибка при запуске службы");
//         }
//
//         return Task.CompletedTask;
//     }
//
//     public Task StopAsync(CancellationToken ct)
//     {
//         _serialPort?.Close();
//         _logger.LogInformation("Служба остановлена.");
//         return Task.CompletedTask;
//     }
//
//     public void Dispose()
//     {
//         _serialPort?.Dispose();
//     }
//
//     // ... остальной код (OnDataReceived, HandleDeployCommandAsync, LoadConfig) остаётся почти без изменений
// }