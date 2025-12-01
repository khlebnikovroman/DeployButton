# build-all.ps1

$ErrorActionPreference = "Stop"

$RootDir = "$PSScriptRoot"
$BuildDir = "$RootDir\build"
$BundleDir = "$BuildDir\deploy_button_bundled"
$ScriptDir = "$RootDir\scripts"

# Step 1: Build components
Write-Host "Building Mp3Formatter..."
& "$RootDir\build-sounds.ps1" -ErrorAction Stop

Write-Host "Building Angular client..."
& "$RootDir\build-client.ps1" -ErrorAction Stop

Write-Host "Building API..."
& "$RootDir\build-api.ps1" -ErrorAction Stop

# Step 2: Clean and create bundle structure
if (Test-Path $BundleDir) {
    Remove-Item $BundleDir -Recurse -Force
}
New-Item -ItemType Directory -Path "$BundleDir\app" | Out-Null
New-Item -ItemType Directory -Path "$BundleDir\mp3Formatter" | Out-Null
New-Item -ItemType Directory -Path "$BundleDir\raw_sounds" | Out-Null

# Step 3: Copy API executable and required files
Copy-Item "$BuildDir\api\*" -Destination "$BundleDir\app" -Recurse

# Step 4: Copy Angular client into wwwroot
$WwwRoot = "$BundleDir\app\wwwroot"
if (-not (Test-Path $WwwRoot)) {
    New-Item -ItemType Directory -Path $WwwRoot | Out-Null
}
Copy-Item "$BuildDir\client\browser\*" -Destination $WwwRoot -Recurse -Force

# Step 5: Copy normalized sounds into wwwroot/sounds (with overwrite)
$SoundsTarget = "$WwwRoot\sounds"
if (-not (Test-Path $SoundsTarget)) {
    New-Item -ItemType Directory -Path $SoundsTarget | Out-Null
}
Copy-Item "$RootDir\normalized_sounds\*" -Destination $SoundsTarget -Recurse -Force

# Step 6: Copy Mp3Formatter tool
Copy-Item "$BuildDir\mp3formatter\*" -Destination "$BundleDir\mp3Formatter" -Recurse

# Step 7: Copy raw_sounds for potential reprocessing
Copy-Item "$RootDir\raw_sounds\*" -Destination "$BundleDir\raw_sounds" -Recurse

# Step 8: Copy update-audio.ps1 into bundle root
Copy-Item "$ScriptDir\update-audio.ps1" -Destination "$BundleDir\" -ErrorAction SilentlyContinue
# Step 8: Copy update-audio.ps1 into bundle root
Copy-Item "$ScriptDir\install-service.bat" -Destination "$BundleDir\" -ErrorAction SilentlyContinue
# Step 8: Copy update-audio.ps1 into bundle root
Copy-Item "$ScriptDir\uninstall-service.bat" -Destination "$BundleDir\" -ErrorAction SilentlyContinue

Write-Host "Success: Full bundle created at $BundleDir"
Write-Host "To update audio later, run: .\deploy_button_bundled\update-audio.ps1"