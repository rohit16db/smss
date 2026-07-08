<#
.SYNOPSIS
    Compiles, publishes, and packages the SMS application for deployment on IIS.
.DESCRIPTION
    This script builds the frontend (with target backend API URL) and publishes the .NET backend.
    It compiles both parts, grabs SQL migration scripts, and packages them into 'sms-deployment.zip'.
.PARAMETER ApiUrl
    The absolute URL of the backend API as it will be accessed by clients/browsers (e.g., http://192.168.1.100:5208/api or http://api.schoolapi.local/api).
#>

[CmdletBinding()]
param (
    [Parameter(Mandatory = $false)]
    [string]$ApiUrl = "http://localhost:5208/api"
)

$ErrorActionPreference = "Stop"

# Determine base directory robustly
$scriptDir = if ($PSScriptRoot) { $PSScriptRoot } else { $pwd.Path }

Write-Host "==============================================" -ForegroundColor Cyan
Write-Host " SMS IIS PUBLISH & PACKAGING TOOL             " -ForegroundColor Cyan
Write-Host "==============================================" -ForegroundColor Cyan
Write-Host "Target API URL: $ApiUrl" -ForegroundColor Yellow
Write-Host ""

# 1. Prerequisite Checks
Write-Host "[1/6] Verifying prerequisites..." -ForegroundColor Cyan
if ($null -eq (Get-Command "node" -ErrorAction SilentlyContinue)) {
    Write-Error "Node.js is not installed or not in PATH. Please install Node.js."
}
if ($null -eq (Get-Command "dotnet" -ErrorAction SilentlyContinue)) {
    Write-Error ".NET SDK is not installed or not in PATH. Please install .NET SDK 10.0+."
}
Write-Host "[OK] Node.js and .NET SDK verified." -ForegroundColor Green

# Create publish staging folder
$stagingPath = Join-Path $scriptDir "publish-staging"
if (Test-Path $stagingPath) {
    Write-Host "Cleaning up old staging directory..." -ForegroundColor Gray
    Remove-Item $stagingPath -Recurse -Force
}
New-Item -ItemType Directory -Path $stagingPath | Out-Null

# 2. Build Frontend
Write-Host ""
Write-Host "[2/6] Building Frontend..." -ForegroundColor Cyan
$frontendDir = Join-Path $scriptDir "frontend"
$envFile = Join-Path $frontendDir ".env"
$envBackupFile = Join-Path $frontendDir ".env.publish_backup"

# Backup existing .env file
if (Test-Path $envFile) {
    Copy-Item $envFile $envBackupFile -Force
}

# Write custom VITE_API_URL to .env for the build
Write-Host "Setting frontend VITE_API_URL to '$ApiUrl'..." -ForegroundColor Gray
"VITE_API_URL=$ApiUrl" | Out-File $envFile -Encoding utf8 -Force

try {
    Push-Location $frontendDir
    Write-Host "Installing npm dependencies..." -ForegroundColor Gray
    npm install
    
    Write-Host "Running frontend build..." -ForegroundColor Gray
    npm run build
}
finally {
    Pop-Location
    # Restore original .env file
    if (Test-Path $envBackupFile) {
        Copy-Item $envBackupFile $envFile -Force
        Remove-Item $envBackupFile -Force
    } else {
        if (Test-Path $envFile) { Remove-Item $envFile -Force }
    }
}

Write-Host "Copying frontend build artifacts to staging..." -ForegroundColor Gray
$frontendStaging = Join-Path $stagingPath "frontend"
Copy-Item (Join-Path $frontendDir "dist") $frontendStaging -Recurse -Force
Write-Host "[OK] Frontend compiled successfully." -ForegroundColor Green

# 3. Publish Backend
Write-Host ""
Write-Host "[3/6] Publishing Backend API..." -ForegroundColor Cyan
$backendProj = Join-Path $scriptDir "backend\src\SMS.API"
$backendStaging = Join-Path $stagingPath "api"

dotnet publish $backendProj -c Release -o $backendStaging
Write-Host "[OK] Backend API published successfully." -ForegroundColor Green

# 4. Gather Database Scripts
Write-Host ""
Write-Host "[4/6] Copying database migration scripts..." -ForegroundColor Cyan
$dbScriptsStaging = Join-Path $stagingPath "db-scripts"
New-Item -ItemType Directory -Path $dbScriptsStaging | Out-Null

$backendDir = Join-Path $scriptDir "backend"
Get-ChildItem -Path $backendDir -Filter "*.sql" | ForEach-Object {
    Copy-Item $_.FullName $dbScriptsStaging -Force
    Write-Host "Copied: $($_.Name)" -ForegroundColor Gray
}
Write-Host "[OK] DB Scripts gathered." -ForegroundColor Green

# 5. Create Deployment Archive
Write-Host ""
Write-Host "[5/6] Creating deployment ZIP archive..." -ForegroundColor Cyan
$zipFile = Join-Path $scriptDir "sms-deployment.zip"
if (Test-Path $zipFile) {
    Remove-Item $zipFile -Force
}

Compress-Archive -Path "$stagingPath\*" -DestinationPath $zipFile -Force
Write-Host "[OK] Package created: sms-deployment.zip" -ForegroundColor Green

# 6. Cleanup Staging
Write-Host ""
Write-Host "[6/6] Cleaning up staging directory..." -ForegroundColor Cyan
if (Test-Path $stagingPath) {
    Remove-Item $stagingPath -Recurse -Force -ErrorAction SilentlyContinue
}
Write-Host "[OK] Staging directory cleaned." -ForegroundColor Green

Write-Host ""
Write-Host "==========================================================" -ForegroundColor Green
Write-Host " SUCCESS! Publication Package Ready                       " -ForegroundColor Green
Write-Host "==========================================================" -ForegroundColor Green
Write-Host "Zip Archive Location: $zipFile" -ForegroundColor Yellow
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Cyan
Write-Host "1. Copy 'sms-deployment.zip' to the destination machine."
Write-Host "2. Extract it to a folder like C:\SMS."
Write-Host "3. Follow the instructions to host backend and frontend in IIS."
Write-Host "==========================================================" -ForegroundColor Green
