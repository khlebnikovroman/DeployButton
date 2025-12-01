@echo off
setlocal

set "SCRIPT_DIR=%~dp0"
set "SERVICE_NAME=DeployButtonService"
set "EXE_PATH=%SCRIPT_DIR%app\DeployButton.Api.exe"
set "DESKTOP=%USERPROFILE%\Desktop"
set "SHORTCUT_PATH=%DESKTOP%\Deploy Button.url"

:: Check if running as administrator
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo Error: This script must be run as Administrator.
    pause
    exit /b 1
)

:: Install service using sc.exe
echo Installing service '%SERVICE_NAME%'...
sc create "%SERVICE_NAME%" binPath="%EXE_PATH%" start=auto DisplayName="Deploy Button API Service"
if %errorlevel% neq 0 (
    echo Failed to create service.
    pause
    exit /b 1
)

sc description "%SERVICE_NAME%" "Backend service for Deploy Button application"
sc start "%SERVICE_NAME%"
if %errorlevel% neq 0 (
    echo Warning: Service created but failed to start.
)

:: Create desktop shortcut (.url file)
echo Creating desktop shortcut...
echo [InternetShortcut] > "%SHORTCUT_PATH%"
echo URL=http://localhost:5000 >> "%SHORTCUT_PATH%"
echo IconIndex=0 >> "%SHORTCUT_PATH%"

echo Success: Service installed and shortcut created.
pause