# build-sounds.ps1

$ErrorActionPreference = "Stop"

$RootDir = "$PSScriptRoot"
$BuildDir = "$RootDir\build\mp3formatter"
$FormatterExe = "$BuildDir\Mp3Formatter.exe"
$RawSoundsDir = "$RootDir\raw_sounds"
$NormalizedDir = "$RootDir\normalized_sounds"
$ClientSounds = "$RootDir\ClientApp\public\sounds"
$ApiSounds = "$RootDir\DeployButton.Api\wwwroot\sounds"

# 1. Build Mp3Formatter as a single-file self-contained executable if it does not exist
if (-not (Test-Path $FormatterExe)) {
    Write-Host "Building Mp3Formatter..." -ForegroundColor Cyan
    if (-not (Test-Path $BuildDir)) {
        New-Item -ItemType Directory -Path $BuildDir | Out-Null
    }

    dotnet publish "$RootDir\Mp3Formatter\Mp3Formatter.csproj" `
        -c Release `
        -r win-x64 `
        --self-contained true `
        -p:PublishSingleFile=true `
        -p:IncludeNativeLibrariesForSelfExtract=true `
        -p:PublishTrimmed=false `
        -o "$BuildDir"

    if (-not (Test-Path $FormatterExe)) {
        throw "Failed to build Mp3Formatter.exe"
    }
    Write-Host "Success: Mp3Formatter built at $FormatterExe" -ForegroundColor Green
} else {
    Write-Host "Mp3Formatter already exists; skipping rebuild." -ForegroundColor Gray
}

# 2. Verify source sounds directory exists and contains MP3 files
if (-not (Test-Path $RawSoundsDir)) {
    throw "Source directory 'raw_sounds' not found at: $RawSoundsDir"
}
$Mp3Count = (Get-ChildItem $RawSoundsDir -Filter "*.mp3" -ErrorAction SilentlyContinue | Measure-Object).Count
if ($Mp3Count -eq 0) {
    throw "No MP3 files found in raw_sounds directory"
}

# 3. Clean and recreate normalized_sounds directory
if (Test-Path $NormalizedDir) {
    Remove-Item $NormalizedDir -Recurse -Force
}
New-Item -ItemType Directory -Path $NormalizedDir | Out-Null

# 4. Run normalization
Write-Host "Running audio normalization..." -ForegroundColor Cyan
& $FormatterExe --input "$RawSoundsDir" --output "$NormalizedDir"

if ($LASTEXITCODE -ne 0) {
    throw "Mp3Formatter exited with error code $LASTEXITCODE"
}

Write-Host "Success: Audio normalization completed" -ForegroundColor Green

# 5. Copy normalized sounds to ClientApp and DeployButton.Api
@($ClientSounds, $ApiSounds) | ForEach-Object {
    $target = $_
    if (Test-Path $target) {
        Remove-Item $target -Recurse -Force
    }
    New-Item -ItemType Directory -Path $target | Out-Null
    Copy-Item "$NormalizedDir\*" -Destination $target -Recurse
    Write-Host "Copied normalized sounds to: $target" -ForegroundColor Green
}

Write-Host "`nAll sounds processed and deployed successfully." -ForegroundColor Green