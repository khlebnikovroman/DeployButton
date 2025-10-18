@echo off
cd /d "%~dp0"

sc create DeployButtonService ^
    binPath= "%~dp0DeployButtonService.exe" ^
    start= auto ^
    DisplayName= "Deploy Button Service"

sc description DeployButtonService "Запускает деплой по сигналу с Arduino"

net start DeployButtonService

echo Служба установлена и запущена.
pause