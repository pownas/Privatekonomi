# ============================================================================
# Privatekonomi Local Environment Setup Script (PowerShell)
# ============================================================================
# 
# This script automates the complete setup of the development environment
# for the Privatekonomi project on Windows/PowerShell.
#
# Summary of setup performed:
# 1. Install .NET 9 SDK (required for this project)
# 2. Install .NET Aspire workload
# 3. Restore project dependencies
# 4. Clean and rebuild solution
# 5. Install Entity Framework CLI tools
# 6. Configure HTTPS development certificates
# 7. Verify installation and test readiness
#
# Created: October 23, 2025
# Based on: GitHub Copilot chat session for local development prerequisites
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

# Check if running with appropriate permissions
function Test-AdminPrivileges {
    Write-LogSection "Checking Administrator Privileges"
    
    $currentUser = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = New-Object Security.Principal.WindowsPrincipal($currentUser)
    $isAdmin = $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
    
    if (-not $isAdmin) {
        Write-LogWarning "This script is not running as Administrator"
        Write-LogWarning "Some operations (like installing workloads) may require elevated privileges"
        Write-LogWarning "Consider running PowerShell as Administrator if you encounter issues"
    } else {
        Write-LogInfo "Running with Administrator privileges"
    }
}

# Step 1: Install .NET 9 SDK
function Install-DotNet9 {
    Write-LogSection "Installing .NET 9 SDK"
    
    Write-LogInfo "Checking current .NET installation..."
    try {
        $dotnetVersion = dotnet --version
        Write-LogInfo "Current .NET version: $dotnetVersion"
        Write-LogInfo "Installed SDKs:"
        dotnet --list-sdks
        
        # Check if .NET 9 is already installed
        $sdks = dotnet --list-sdks | Out-String
        if ($sdks -match "9\.") {
            Write-LogSuccess ".NET 9 SDK is already installed"
            return
        }
    }
    catch {
        Write-LogWarning ".NET not found in PATH"
    }
    
    Write-LogInfo "Downloading and installing .NET 9 SDK..."
    try {
        # Download the .NET install script
        $installScript = Invoke-WebRequest -Uri "https://dot.net/v1/dotnet-install.ps1" -UseBasicParsing
        $scriptPath = "$env:TEMP\dotnet-install.ps1"
        $installScript.Content | Out-File -FilePath $scriptPath -Encoding UTF8
        
        # Run the installer
        & $scriptPath -Channel 9.0 -InstallDir "$env:LOCALAPPDATA\Microsoft\dotnet"
        
        # Add to PATH for current session
        $dotnetPath = "$env:LOCALAPPDATA\Microsoft\dotnet"
        if ($env:PATH -notlike "*$dotnetPath*") {
            $env:PATH = "$dotnetPath;$env:PATH"
        }
        
        # Add to user PATH permanently
        $userPath = [Environment]::GetEnvironmentVariable("PATH", "User")
        if ($userPath -notlike "*$dotnetPath*") {
            [Environment]::SetEnvironmentVariable("PATH", "$dotnetPath;$userPath", "User")
        }
        
        Write-LogInfo "Verifying .NET 9 installation..."
        dotnet --list-sdks
        Write-LogSuccess ".NET 9 SDK installation completed"
    }
    catch {
        Write-LogError "Failed to install .NET 9 SDK: $($_.Exception.Message)"
        Write-LogWarning "Please manually download and install .NET 9 SDK from: https://dotnet.microsoft.com/download/dotnet/9.0"
        throw
    }
}

# Step 2: Install Aspire workload
function Install-AspireWorkload {
    Write-LogSection "Installing .NET Aspire Workload"
    
    Write-LogInfo "Checking if Aspire workload is already installed..."
    $workloadList = dotnet workload list | Out-String
    if ($workloadList -match "aspire") {
        Write-LogSuccess "Aspire workload is already installed"
    }
    else {
        Write-LogInfo "Installing Aspire workload..."
        try {
            dotnet workload install aspire
            Write-LogSuccess "Aspire workload installed successfully"
        }
        catch {
            Write-LogError "Failed to install Aspire workload: $($_.Exception.Message)"
            throw
        }
    }
    
    Write-LogInfo "Verifying Aspire workload installation..."
    dotnet workload list
    Write-LogSuccess "Aspire workload installation completed"
}

# Step 3: Restore and build project
function Setup-Project {
    Write-LogSection "Setting up Project Dependencies"
    
    if (-not (Test-Path "Privatekonomi.sln")) {
        Write-LogError "Privatekonomi.sln not found in current directory"
        Write-LogError "Please run this script from the project root directory"
        throw "Solution file not found"
    }
    
    Write-LogInfo "Restoring project dependencies..."
    try {
        dotnet restore Privatekonomi.sln
        
        Write-LogInfo "Cleaning previous build artifacts..."
        dotnet clean Privatekonomi.sln
        
        Write-LogInfo "Building solution..."
        dotnet build Privatekonomi.sln
        
        Write-LogSuccess "Project setup completed"
    }
    catch {
        Write-LogError "Failed to restore/build project: $($_.Exception.Message)"
        throw
    }
}

# Step 4: Install Entity Framework CLI tools
function Install-EFTools {
    Write-LogSection "Installing Entity Framework CLI Tools"
    
    Write-LogInfo "Checking if Entity Framework tools are already installed..."
    $globalTools = dotnet tool list --global | Out-String
    if ($globalTools -match "dotnet-ef") {
        Write-LogSuccess "Entity Framework tools are already installed"
    }
    else {
        Write-LogInfo "Installing Entity Framework global tools..."
        try {
            dotnet tool install --global dotnet-ef
        }
        catch {
            Write-LogError "Failed to install Entity Framework tools: $($_.Exception.Message)"
            throw
        }
    }
    
    Write-LogInfo "Verifying Entity Framework tools installation..."
    if ((dotnet tool list --global | Out-String) -match "dotnet-ef") {
        Write-LogSuccess "Entity Framework tools installed successfully"
        try {
            dotnet ef --version
        }
        catch {
            Write-LogWarning "EF version check had issues but tool is installed"
        }
    }
    else {
        Write-LogError "Failed to verify Entity Framework tools installation"
        throw "EF tools verification failed"
    }
    Write-LogSuccess "Entity Framework tools installation completed"
}

# Step 5: Configure HTTPS development certificates
function Configure-DevCerts {
    Write-LogSection "Configuring HTTPS Development Certificates"
    
    Write-LogInfo "Cleaning any existing development certificates..."
    try {
        dotnet dev-certs https --clean
    }
    catch {
        Write-LogWarning "Failed to clean existing certificates (may not exist)"
    }
    
    Write-LogInfo "Generating new HTTPS development certificate..."
    try {
        dotnet dev-certs https --trust
    }
    catch {
        Write-LogWarning "Failed to trust certificate (may require manual trust)"
    }
    
    Write-LogInfo "Verifying HTTPS certificate installation..."
    try {
        $certCheck = dotnet dev-certs https --check
        if ($LASTEXITCODE -eq 0) {
            Write-LogSuccess "HTTPS development certificate is properly configured"
        }
        else {
            Write-LogWarning "HTTPS certificate verification failed - applications may have SSL issues"
        }
    }
    catch {
        Write-LogWarning "HTTPS certificate verification failed - applications may have SSL issues"
    }
    
    Write-LogSuccess "HTTPS development certificates configuration completed"
}

# Step 6: Verify installation
function Test-Installation {
    Write-LogSection "Verifying Installation"
    
    Write-LogInfo "Checking .NET installation..."
    try {
        dotnet --version
        dotnet --list-sdks
        
        Write-LogInfo "Checking Aspire workload..."
        $workloadList = dotnet workload list | Out-String
        if ($workloadList -match "aspire") {
            Write-LogSuccess "Aspire workload is installed"
        }
        else {
            Write-LogWarning "Aspire workload not found"
        }
        
        Write-LogInfo "Checking Entity Framework tools..."
        $globalTools = dotnet tool list --global | Out-String
        if ($globalTools -match "dotnet-ef") {
            Write-LogSuccess "EF tools are installed"
        }
        else {
            Write-LogWarning "EF tools not found in global tools"
        }
        
        Write-LogInfo "Checking HTTPS development certificates..."
        try {
            $certCheck = dotnet dev-certs https --check --quiet
            if ($LASTEXITCODE -eq 0) {
                Write-LogSuccess "HTTPS development certificates are properly configured"
            }
            else {
                Write-LogWarning "HTTPS development certificates may need attention"
            }
        }
        catch {
            Write-LogWarning "HTTPS development certificates may need attention"
        }
        
        Write-LogInfo "Checking project build status..."
        dotnet build Privatekonomi.sln --verbosity quiet
        
        Write-LogSuccess "All verifications completed successfully!"
    }
    catch {
        Write-LogError "Installation verification failed: $($_.Exception.Message)"
        throw
    }
}

# Step 7: Display usage information
function Show-UsageInfo {
    Write-LogSection "Installation Complete - Usage Information"
    
    Write-Host "✅ Privatekonomi environment is ready!" -ForegroundColor Green
    Write-Host ""
    
    Write-Host "Next Steps:" -ForegroundColor Blue
    Write-Host "  Start Application:" -ForegroundColor Yellow
    Write-Host "    .\local-app-start.ps1"
    Write-Host ""
    Write-Host "Available commands:" -ForegroundColor Blue
    Write-Host "  Database Operations:" -ForegroundColor Yellow
    Write-Host "    cd src\Privatekonomi.Core"
    Write-Host "    dotnet ef migrations add <MigrationName>"
    Write-Host "    dotnet ef database update"
    Write-Host ""
    Write-Host "  Build and Test:" -ForegroundColor Yellow
    Write-Host "    dotnet build Privatekonomi.sln"
    Write-Host "    dotnet test"
    Write-Host ""
    Write-Host "Project Structure:" -ForegroundColor Blue
    Write-Host "  • Privatekonomi.Api - Backend API project" -ForegroundColor Yellow
    Write-Host "  • Privatekonomi.Web - Blazor frontend project" -ForegroundColor Yellow
    Write-Host "  • Privatekonomi.Core - Shared core library" -ForegroundColor Yellow
    Write-Host "  • Privatekonomi.AppHost - Aspire orchestration" -ForegroundColor Yellow
    Write-Host "  • Privatekonomi.ServiceDefaults - Shared service configurations" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Installed Tools:" -ForegroundColor Blue
    Write-Host "  • .NET 9 SDK"
    Write-Host "  • .NET Aspire workload"
    Write-Host "  • Entity Framework CLI tools"
    Write-Host "  • HTTPS development certificates (trusted)"
    Write-Host ""
    Write-Host "Ready to start coding! Run " -NoNewline -ForegroundColor Green
    Write-Host ".\local-app-start.ps1" -NoNewline -ForegroundColor Yellow
    Write-Host " to launch the application! 🚀" -ForegroundColor Green
}

# Main execution
function Main {
    Write-LogSection "Privatekonomi Local Environment Installation"
    Write-LogInfo "Starting automated environment setup..."
    
    try {
        Test-AdminPrivileges
        Install-DotNet9
        Install-AspireWorkload
        Setup-Project
        Install-EFTools
        Configure-DevCerts
        Test-Installation
        Show-UsageInfo
        
        Write-LogSuccess "Environment installation completed successfully!"
    }
    catch {
        Write-LogError "Installation failed: $($_.Exception.Message)"
        Write-Host ""
        Write-Host "Common solutions:" -ForegroundColor Yellow
        Write-Host "  • Run PowerShell as Administrator"
        Write-Host "  • Check your internet connection"
        Write-Host "  • Ensure you're in the project root directory"
        Write-Host "  • Try running individual steps manually"
        exit 1
    }
}

# Run main function
Main