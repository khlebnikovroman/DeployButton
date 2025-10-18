@echo off
chcp 65001 >nul
cd /d "%~dp0"

net stop DeployButtonService
sc delete DeployButtonService

echo Служба удалена.
pause