using System;
using System.IO;
using System.IO.Ports;
using System.Net;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DeployButton
{
    static class Program
    {
        static void Main()
        {
            // Проверяем: запущено ли как служба или вручную (для отладки)
            if (Environment.UserInteractive)
            {
                var service = new DeployButtonService();
                service.Start();
                System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite);
            }
            else
            {
                // Запуск как настоящая служба Windows
                ServiceBase.Run(new DeployButtonService());
            }
        }
    }
    public partial class DeployButtonService : ServiceBase
    {
        private SerialPort _serialPort;
        private TeamCityClient _teamCityClient;

        public DeployButtonService()
        {
            // InitializeComponent();
            ServiceName = "DeployButtonService";
        }

        public void Start()
        {
            OnStart(null);
        }
        protected override void OnStart(string[] args)
        {
            try
            {
                var config = LoadConfig();
                if (config?.TeamCity == null)
                {
                    Log.Error("Ошибка: не загружена конфигурация TeamCity.");
                    return;
                }
                
                _teamCityClient = new TeamCityClient(config.TeamCity);
                
                _serialPort = new SerialPort(config.SerialPort.PortName, config.SerialPort.BaudRate);
                _serialPort.DataReceived += OnDataReceived;
                _serialPort.Open();

                Log.Info($"Слушаю порт {_serialPort.PortName}...");
            }
            catch (Exception ex)
            {
                Log.Error($"Ошибка при запуске: {ex}");
            }
        }

        protected override void OnStop()
        {
            _serialPort?.Close();
            Log.Info("Служба остановлена.");
        }

        private int _isHandlingDeploy = 0;
        private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                if (_serialPort == null || !_serialPort.IsOpen) return;
                var data = _serialPort.ReadLine().Trim();
                if (data == "DEPLOY")
                {
                    Log.Info("Получена команда: DEPLOY");

                    if (Interlocked.CompareExchange(ref _isHandlingDeploy, 1, 0) == 1)
                    {
                        Log.Warning("Пропускаем команду: обработка уже идёт.");
                        return;
                    }
                    
                    HandleDeployCommandAsync().ContinueWith(task =>
                    {
                        Interlocked.Exchange(ref _isHandlingDeploy, 0);
                    });
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Ошибка при обработке: {ex.Message}");
            }
        }

        private async Task HandleDeployCommandAsync()
        {
            Log.Info("Получена команда: DEPLOY. Загружаем конфигурацию...");

            var config = LoadConfig();
            if (config?.TeamCity == null)
            {
                Log.Error("❌ Ошибка: не удалось загрузить конфигурацию TeamCity.");
                return;
            }

            _teamCityClient.UpdateConfig(config.TeamCity);

            try
            {
                var isBusy = await _teamCityClient.IsBuildQueuedOrRunningAsync();
                if (isBusy)
                {
                    Log.Warning("⚠️ Сборка уже в очереди или выполняется — новый запуск отменён.");
                    return;
                }

                await _teamCityClient.TriggerBuildAsync();
                Log.Info("✅ Новая сборка успешно запущена!");
            }
            catch (Exception ex)
            {
                Log.Error($"💥 Ошибка при работе с TeamCity: {ex.Message}");
            }
        }
        
        private AppSettings LoadConfig()
        {
            try
            {
                var json = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json"));
                return JsonConvert.DeserializeObject<AppSettings>(json);
            }
            catch
            {
                return null;
            }
        }
    }
}