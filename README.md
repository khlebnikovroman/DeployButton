# DeployButton - Refactored Application

This is a .NET application that provides a physical button to trigger TeamCity deployments. The application has been refactored to follow SOLID principles and improve maintainability.

## Architecture Overview

The application follows a layered architecture with clear separation of concerns:

- **Controllers**: Handle HTTP requests and responses
- **Services**: Business logic and application-specific operations
- **Adapters**: Interface with external systems (serial devices, HTTP clients)
- **Abstractions**: Define contracts and interfaces
- **Factories**: Create instances of complex objects

## SOLID Principles Implemented

### 1. Single Responsibility Principle (SRP)
Each class has a single, well-defined responsibility:
- `TeamCityAuthenticationService`: Handles authentication concerns
- `TeamCityBuildService`: Manages build operations
- `DeployService`: Manages deployment logic
- `BuildMonitoringService`: Monitors build status
- `DeviceMonitorService`: Manages serial device connections

### 2. Open/Closed Principle (OCP)
The system is open for extension but closed for modification:
- New authentication methods can be added by implementing `ITeamCityAuthenticationService`
- New deployment targets can be added by implementing `IDeployService`
- New monitoring strategies can be added by implementing `IBuildMonitoringService`

### 3. Liskov Substitution Principle (LSP)
All implementations properly implement their interfaces, ensuring that derived classes can be substituted for their base classes without affecting the correctness of the program.

### 4. Interface Segregation Principle (ISP)
Interfaces are focused and specific:
- `IDeviceStateService`: Handles device state management
- `IDeviceMonitorService`: Handles device monitoring
- `ITeamCityBuildService`: Handles build operations
- `IDeployService`: Handles deployment logic

### 5. Dependency Inversion Principle (DIP)
High-level modules depend on abstractions, not concrete implementations:
- Services depend on interfaces rather than concrete classes
- Dependency injection is used throughout the application
- Configuration is externalized

## Key Components

### TeamCity Integration
- `TeamCityAuthenticationService`: Handles authentication with TeamCity
- `TeamCityBuildService`: Manages build operations (trigger, status check, etc.)
- `DeployService`: Coordinates deployment operations
- `BuildMonitoringService`: Monitors build status and reports results

### Device Management
- `DeviceMonitorService`: Automatically detects and connects to serial devices
- `SerialDeviceAdapter`: Provides communication with Arduino devices
- `IDeviceStateService`: Manages current device state

### API Endpoints
- `POST /api/deploy/trigger`: Triggers a new deployment
- `GET /api/deploy/status`: Gets current device status

## Configuration

The application uses `appsettings.json` for configuration:

```json
{
  "TeamCity": {
    "BaseUrl": "http://your-teamcity-server",
    "Username": "your-username",
    "Password": "your-password",
    "BuildConfigurationId": "your-build-config-id"
  },
  "SerialPort": {
    "PortName": "auto",
    "BaudRate": 9600
  },
  "Audio": {
    "DeployStart": "path/to/deploy-start.wav",
    "BuildSuccess": "path/to/build-success.wav",
    "BuildFail": "path/to/build-fail.wav"
  }
}
```

## Running the Application

The application can run as either a console application or a Windows service:

```bash
# Run as console application
dotnet run

# Install as Windows service
dotnet publish -r win-x64 --self-contained
# Then run install-service.bat
```

## Testing

To run the application with the Arduino button:
1. Connect the Arduino device via USB
2. Ensure the Arduino sketch is uploaded
3. Configure TeamCity settings in appsettings.json
4. Run the application
5. Press the physical button to trigger a deployment

## Benefits of Refactoring

1. **Improved Testability**: Each service can be unit tested independently
2. **Better Maintainability**: Changes to one concern don't affect others
3. **Enhanced Extensibility**: New features can be added with minimal changes
4. **Clearer Architecture**: Responsibilities are well-separated
5. **Reduced Coupling**: Components depend on abstractions