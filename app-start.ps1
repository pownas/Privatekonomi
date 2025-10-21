# Startup script for Privatekonomi application in Codespaces
# This script starts the .NET Aspire Dashboard which orchestrates all services

$ErrorActionPreference = "Stop"

Write-Host "🚀 Starting Privatekonomi with .NET Aspire Dashboard..." -ForegroundColor Green
Write-Host ""

# Check if .NET is installed
try {
    $dotnetVersion = dotnet --version
    Write-Host "✅ .NET SDK version: $dotnetVersion" -ForegroundColor Green
}
catch {
    Write-Host "❌ Error: .NET SDK is not installed" -ForegroundColor Red
    Write-Host "Please install .NET 9 SDK: https://dotnet.microsoft.com/download/dotnet/9.0" -ForegroundColor Yellow
    exit 1
}

# Check if Aspire workload is installed
$workloadList = dotnet workload list | Out-String
if (-not ($workloadList -match "aspire")) {
    Write-Host "⚠️  Aspire workload is not installed" -ForegroundColor Yellow
    Write-Host "Installing Aspire workload..." -ForegroundColor Yellow
    dotnet workload install aspire
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Failed to install Aspire workload" -ForegroundColor Red
        exit 1
    }
    Write-Host "✅ Aspire workload installed successfully" -ForegroundColor Green
}

# Navigate to AppHost directory
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location "$scriptPath/src/Privatekonomi.AppHost"

Write-Host ""
Write-Host "📦 Starting Aspire Dashboard..." -ForegroundColor Cyan
Write-Host "The dashboard will open automatically in your browser." -ForegroundColor Cyan
Write-Host ""
Write-Host "Services that will be available:" -ForegroundColor Cyan
Write-Host "  - Aspire Dashboard: View logs, traces, and metrics"
Write-Host "  - Web Application: Blazor Server UI"
Write-Host "  - API: REST API with Swagger documentation"
Write-Host ""
Write-Host "Press Ctrl+C to stop all services" -ForegroundColor Yellow
Write-Host ""

# Run the AppHost
dotnet run
