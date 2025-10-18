@echo off
cd /d "%~dp0"

net stop DeployButtonService
sc delete DeployButtonService

echo Служба удалена.
pause