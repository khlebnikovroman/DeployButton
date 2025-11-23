// Models/AppSettings.cs
namespace DeployButton.Api.Configs;

public class AppSettings
{
    public SerialPortConfig SerialPort { get; set; } = new();
    public AudioConfig Audio { get; set; } = new();
    public TeamCityConfig TeamCity { get; set; } = new();
}

public class SerialPortConfig
{
    public string PortName { get; set; } = "auto";
    public int BaudRate { get; set; } = 9600;
}

public class AudioConfig
{
    public int Volume { get; set; } = 15;
    public string DeployStart { get; set; } = "0001";
    public string BuildSuccess { get; set; } = "0002";
    public string BuildFail { get; set; } = "0003";
}

public class TeamCityConfig
{
    public string BaseUrl { get; set; } = "";
    public string BuildConfigurationId { get; set; } = "";
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
}