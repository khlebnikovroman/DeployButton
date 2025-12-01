# update-audio.ps1
# Place this in the root of the bundled folder

$ErrorActionPreference = "Stop"

$BundleRoot = "$PSScriptRoot"
$FormatterExe = "$BundleRoot\mp3Formatter\Mp3Formatter.exe"
$RawSounds = "$BundleRoot\raw_sounds"
$TempNormalized = "$BundleRoot\_temp_normalized"
$WwwRootSounds = "$BundleRoot\app\wwwroot\sounds"

if (-not (Test-Path $FormatterExe)) {
    throw "Mp3Formatter.exe not found in mp3Formatter folder"
}
if (-not (Test-Path $RawSounds)) {
    throw "raw_sounds folder not found"
}
if ((Get-ChildItem $RawSounds -Filter "*.mp3" -ErrorAction SilentlyContinue | Measure-Object).Count -eq 0) {
    throw "No MP3 files in raw_sounds"
}

# Clean temp output
if (Test-Path $TempNormalized) {
    Remove-Item $TempNormalized -Recurse -Force
}
New-Item -ItemType Directory -Path $TempNormalized | Out-Null

# Run formatter
Write-Host "Normalizing audio from raw_sounds..."
& $FormatterExe --input "$RawSounds" --output "$TempNormalized"
if ($LASTEXITCODE -ne 0) {
    throw "Mp3Formatter failed"
}

# Replace sounds in wwwroot
if (Test-Path $WwwRootSounds) {
    Remove-Item $WwwRootSounds -Recurse -Force
}
New-Item -ItemType Directory -Path $WwwRootSounds | Out-Null
Copy-Item "$TempNormalized\*" -Destination $WwwRootSounds -Recurse

# Cleanup
Remove-Item $TempNormalized -Recurse -Force

Write-Host "Success: Audio updated in app\wwwroot\sounds"