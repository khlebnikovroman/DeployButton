namespace DeployButton.Api.Models;


public record DeviceState
{
    public bool IsConnected { get; init; }
    public string? PortName { get; init; }
    public int? BaudRate { get; init; }
}