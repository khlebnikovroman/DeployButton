# build-client.ps1

$ErrorActionPreference = "Stop"

$RootDir = "$PSScriptRoot"
$ClientAppDir = "$RootDir\ClientApp"
$BuildOutputDir = "$RootDir\build\client"

if (-not (Test-Path $ClientAppDir)) {
    throw "ClientApp directory not found: $ClientAppDir"
}

Push-Location $ClientAppDir

try {
    # Check if Node.js is available
    $nodeVersion = node --version 2>$null
    if (-not $nodeVersion) {
        throw "Node.js is not installed or not in PATH"
    }
    Write-Host "Using Node.js: $nodeVersion"

    # Ensure dependencies are installed
    if (-not (Test-Path "node_modules")) {
        Write-Host "Installing npm dependencies..."
        npm ci
    }

    # Build Angular app for production
    Write-Host "Building Angular app (production mode)..."
    npx ng build --output-path="$BuildOutputDir" --configuration=production

    Write-Host "Success: Angular app built to $BuildOutputDir"
}
finally {
    Pop-Location
}