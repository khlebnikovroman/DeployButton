# DeployButton Application

This application provides a physical button deployment system that integrates with CI/CD platforms like TeamCity. The application has been refactored to follow SOLID principles and maintain a clean, modular architecture.

## Architecture Overview

The application follows a layered architecture with clear separation of concerns:

- **API Layer** (`DeployButton.Api`): Contains all business logic, services, and API endpoints
- **Main Project** (`DeployButton`): Simple console application that starts the API application

## SOLID Principles Implementation

### 1. Single Responsibility Principle (SRP)
- `SerialDeviceAdapter`: Handles only serial port communication
- `DeviceCommandHandler`: Processes only device commands
- `TeamCityDeploymentService`: Manages only TeamCity deployment logic
- `BuildStatusMonitor`: Monitors only build status

### 2. Open/Closed Principle (OCP)
- Interfaces like `IDeploymentService` allow for new CI/CD integrations without modifying existing code
- `DeploymentServiceFactory` enables adding new deployment services

### 3. Liskov Substitution Principle (LSP)
- All implementations properly implement their interfaces
- Services can be substituted without breaking the application

### 4. Interface Segregation Principle (ISP)
- Focused interfaces like `ISerialDeviceReader`, `ISerialDeviceWriter`, `ISoundPlayer`
- Each interface has a specific purpose

### 5. Dependency Inversion Principle (DIP)
- High-level modules depend on abstractions, not concretions
- Dependency injection used throughout the application

## Key Improvements to Serial Port Management

1. **Improved Serial Communication**:
   - Better error handling and disposal
   - Thread-safe operations with proper locking
   - More robust data reading using `ReadExisting()` instead of `ReadLine()`
   - Proper resource management

2. **Simplified Dependencies**:
   - Removed unnecessary dependencies from main project
   - Clear project structure with main project referencing API project
   - Centralized configuration management

3. **Enhanced Reliability**:
   - Proper disposal patterns
   - Better connection management
   - Improved ping/verification mechanism

## Project Structure

```
DeployButton/                 # Main console application (just starts API)
├── DeployButton.csproj       # References DeployButton.Api
└── Program.cs                # Simple startup

DeployButton.Api/             # Core application with services
├── Abstractions/             # Interfaces
├── Adapters/                 # Serial port adapter
├── Configs/                  # Configuration models
├── Controllers/              # API controllers
├── Factories/                # Object factories
├── Models/                   # Data models
├── Services/                 # Business logic services
└── Program.cs                # Main application startup
```

## How to Run

1. **Development**: Run the `DeployButton.Api` project directly to get Swagger UI
2. **Production**: Run the `DeployButton` project as a Windows service

The application will automatically detect and connect to the serial device, monitor for button presses, and trigger deployments as configured.