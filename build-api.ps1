# build-api.ps1

$ErrorActionPreference = "Stop"

$RootDir = "$PSScriptRoot"
$ProjectPath = "$RootDir\DeployButton.Api\DeployButton.Api.csproj"
$OutputDir = "$RootDir\build\api"
$ExecutablePath = "$OutputDir\DeployButton.Api.exe"

if (-not (Test-Path $ProjectPath)) {
    throw "API project file not found: $ProjectPath"
}

Write-Host "Publishing DeployButton.Api as a single-file executable..."

# Ensure output directory exists
if (Test-Path $OutputDir) {
    Remove-Item $OutputDir -Recurse -Force
}
New-Item -ItemType Directory -Path $OutputDir | Out-Null

# Publish as self-contained, single-file, trimmed (optional), for Windows x64
dotnet publish $ProjectPath `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:PublishTrimmed=false `
    -o "$OutputDir"

if (-not (Test-Path $ExecutablePath)) {
    throw "Failed to produce executable at $ExecutablePath"
}

Write-Host "Success: API published to $ExecutablePath"