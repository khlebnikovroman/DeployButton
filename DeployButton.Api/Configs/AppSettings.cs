// Models/AppSettings.cs
namespace DeployButton.Api.Configs;

public class AppSettings
{
    public SerialPortConfig SerialPort { get; set; } = new();
    public TeamCityConfig TeamCity { get; set; } = new();
}

public class SerialPortConfig
{
    public string PortName { get; set; } = "auto";
    public int BaudRate { get; set; } = 115200;
}

public class TeamCityConfig
{
    public string BaseUrl { get; set; } = "";
    public string BuildConfigurationId { get; set; } = "";
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
}