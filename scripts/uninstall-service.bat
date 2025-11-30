@echo off
setlocal

set "SERVICE_NAME=DeployButtonService"
set "DESKTOP=%USERPROFILE%\Desktop"
set "SHORTCUT_PATH=%DESKTOP%\Deploy Button.url"

:: Check if running as administrator
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo Error: This script must be run as Administrator.
    pause
    exit /b 1
)

:: Stop and delete service
echo Stopping service '%SERVICE_NAME%'...
sc stop "%SERVICE_NAME%" >nul 2>&1

echo Deleting service '%SERVICE_NAME%'...
sc delete "%SERVICE_NAME%"
if %errorlevel% neq 0 (
    echo Warning: Failed to delete service (may not exist).
)

:: Remove desktop shortcut
if exist "%SHORTCUT_PATH%" (
    echo Deleting desktop shortcut...
    del "%SHORTCUT_PATH%"
)

echo Success: Service removed and shortcut deleted.
pause