# ============================================================================
# Privatekonomi Local Application Startup Script (PowerShell)
# ============================================================================
# 
# This script starts the Privatekonomi application using .NET Aspire Dashboard.
# 
# Prerequisites:
# - Run .\local-app-install.ps1 first to set up the development environment
# 
# What this script does:
# 1. Check prerequisites are installed
# 2. Navigate to AppHost directory
# 3. Start Aspire Dashboard with all services
#
# Created: October 23, 2025
# ============================================================================

$ErrorActionPreference = "Stop"

# Logging functions with colors
function Write-LogInfo {
    param([string]$Message)
    Write-Host "[INFO] $Message" -ForegroundColor Blue
}

function Write-LogSuccess {
    param([string]$Message)
    Write-Host "[SUCCESS] $Message" -ForegroundColor Green
}

function Write-LogWarning {
    param([string]$Message)
    Write-Host "[WARNING] $Message" -ForegroundColor Yellow
}

function Write-LogError {
    param([string]$Message)
    Write-Host "[ERROR] $Message" -ForegroundColor Red
}

function Write-LogSection {
    param([string]$Title)
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Blue
    Write-Host " $Title" -ForegroundColor Blue
    Write-Host "========================================" -ForegroundColor Blue
    Write-Host ""
}

# Check prerequisites
function Test-Prerequisites {
    Write-LogSection "Checking Prerequisites"
    
    # Check if .NET is installed and accessible
    try {
        $dotnetVersion = dotnet --version
        Write-LogInfo "✅ .NET SDK version: $dotnetVersion"
    }
    catch {
        Write-LogError ".NET SDK is not installed or not in PATH"
        Write-LogWarning "Please run '.\local-app-install.ps1' first to set up the environment"
        throw "Missing .NET SDK"
    }
    
    # Check if .NET 9 is available
    $sdks = dotnet --list-sdks | Out-String
    if (-not ($sdks -match "9\.")) {
        Write-LogError ".NET 9 SDK is not installed"
        Write-LogWarning "Please run '.\local-app-install.ps1' first to install .NET 9"
        throw "Missing .NET 9 SDK"
    }
    Write-LogInfo "✅ .NET 9 SDK is available"
    
    # Check if Aspire workload is installed
    $workloadList = dotnet workload list | Out-String
    if (-not ($workloadList -match "aspire")) {
        Write-LogError "Aspire workload is not installed"
        Write-LogWarning "Please run '.\local-app-install.ps1' first to install Aspire workload"
        throw "Missing Aspire workload"
    }
    Write-LogInfo "✅ Aspire workload is installed"
    
    # Check if solution exists
    if (-not (Test-Path "Privatekonomi.sln")) {
        Write-LogError "Privatekonomi.sln not found"
        Write-LogWarning "Make sure you're in the project root directory"
        throw "Solution file not found"
    }
    Write-LogInfo "✅ Project solution found"
    
    # Check if AppHost directory exists
    if (-not (Test-Path "src\Privatekonomi.AppHost")) {
        Write-LogError "AppHost directory not found at src\Privatekonomi.AppHost"
        throw "AppHost directory not found"
    }
    Write-LogInfo "✅ AppHost directory found"
    
    Write-LogSuccess "All prerequisites are satisfied"
}

# Quick build check
function Test-QuickBuild {
    Write-LogSection "Quick Build Check"
    
    Write-LogInfo "Performing quick build verification..."
    try {
        $buildResult = dotnet build Privatekonomi.sln --verbosity minimal --no-restore 2>&1
        if ($LASTEXITCODE -eq 0) {
            Write-LogSuccess "Project builds successfully"
        }
        else {
            Write-LogWarning "Build issues detected - attempting to restore and rebuild..."
            dotnet restore Privatekonomi.sln
            dotnet build Privatekonomi.sln
            Write-LogSuccess "Project restored and built successfully"
        }
    }
    catch {
        Write-LogWarning "Build issues detected - attempting to restore and rebuild..."
        try {
            dotnet restore Privatekonomi.sln
            dotnet build Privatekonomi.sln
            Write-LogSuccess "Project restored and built successfully"
        }
        catch {
            Write-LogError "Failed to build project: $($_.Exception.Message)"
            throw
        }
    }
}

# Start application
function Start-Application {
    Write-LogSection "Starting Privatekonomi Application"
    
    Write-LogInfo "Navigating to AppHost directory..."
    $originalLocation = Get-Location
    
    try {
        Set-Location "src\Privatekonomi.AppHost"
        
        Write-Host ""
        Write-Host "🚀 Starting Privatekonomi with .NET Aspire Dashboard..." -ForegroundColor Green
        Write-Host "The dashboard will open automatically in your browser." -ForegroundColor Blue
        Write-Host ""
        Write-Host "Services that will be available:" -ForegroundColor Yellow
        Write-Host "  • " -NoNewline
        Write-Host "Aspire Dashboard:" -NoNewline -ForegroundColor Green
        Write-Host " View logs, traces, and metrics"
        Write-Host "  • " -NoNewline  
        Write-Host "Web Application:" -NoNewline -ForegroundColor Green
        Write-Host " Blazor Server UI"
        Write-Host "  • " -NoNewline
        Write-Host "API:" -NoNewline -ForegroundColor Green
        Write-Host " REST API with Swagger documentation"
        Write-Host ""
        Write-Host "💡 Useful Aspire Dashboard features:" -ForegroundColor Blue
        Write-Host "  • " -NoNewline
        Write-Host "Structured Logs:" -NoNewline -ForegroundColor Yellow
        Write-Host " Real-time application logging"
        Write-Host "  • " -NoNewline
        Write-Host "Distributed Tracing:" -NoNewline -ForegroundColor Yellow
        Write-Host " Request flow across services"
        Write-Host "  • " -NoNewline
        Write-Host "Metrics:" -NoNewline -ForegroundColor Yellow
        Write-Host " Performance and health monitoring"
        Write-Host "  • " -NoNewline
        Write-Host "Resources:" -NoNewline -ForegroundColor Yellow
        Write-Host " Service status and configuration"
        Write-Host ""
        Write-Host "Press Ctrl+C to stop all services" -ForegroundColor Red
        Write-Host ""
        
        # Run the AppHost
        dotnet run
    }
    finally {
        Set-Location $originalLocation
    }
}

# Display help information
function Show-Help {
    Write-Host "Privatekonomi Application Startup Script" -ForegroundColor Blue
    Write-Host ""
    Write-Host "Usage:" -ForegroundColor Yellow
    Write-Host "  .\local-app-start.ps1              Start the application"
    Write-Host "  .\local-app-start.ps1 -Help        Show this help message"
    Write-Host ""
    Write-Host "Prerequisites:" -ForegroundColor Yellow
    Write-Host "  Run " -NoNewline
    Write-Host ".\local-app-install.ps1" -NoNewline -ForegroundColor Green
    Write-Host " first to set up the development environment"
    Write-Host ""
    Write-Host "What this script does:" -ForegroundColor Yellow
    Write-Host "  1. Checks that prerequisites are installed (.NET 9, Aspire workload)"
    Write-Host "  2. Verifies the project builds successfully"
    Write-Host "  3. Starts the Aspire Dashboard with all services"
    Write-Host ""
    Write-Host "Troubleshooting:" -ForegroundColor Yellow
    Write-Host "  • If you get 'command not found' errors, run " -NoNewline
    Write-Host ".\local-app-install.ps1" -ForegroundColor Green
    Write-Host "  • If services fail to start, check the Aspire Dashboard logs"
    Write-Host "  • For HTTPS certificate issues, run " -NoNewline
    Write-Host "dotnet dev-certs https --trust" -ForegroundColor Green
    Write-Host "  • Try running PowerShell as Administrator if you encounter permission issues"
}

# Main execution
function Main {
    param(
        [switch]$Help
    )
    
    # Check for help flag
    if ($Help) {
        Show-Help
        return
    }
    
    Write-LogSection "Privatekonomi Application Startup"
    
    try {
        Test-Prerequisites
        Test-QuickBuild
        Start-Application
    }
    catch {
        Write-LogError "Startup failed: $($_.Exception.Message)"
        Write-Host ""
        Write-Host "Common solutions:" -ForegroundColor Yellow
        Write-Host "  • Run '.\local-app-install.ps1' first"
        Write-Host "  • Check that you're in the project root directory"
        Write-Host "  • Ensure .NET 9 and Aspire workload are installed"
        Write-Host "  • Try running PowerShell as Administrator"
        Write-Host "  • Check for any build errors in the output above"
        exit 1
    }
}

# Handle parameters
param(
    [switch]$Help
)

# Run main function
Main -Help:$Help