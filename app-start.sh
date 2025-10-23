#!/bin/bash

# ============================================================================
# Privatekonomi Codespace Environment Setup & Startup Script
# ============================================================================
# 
# This script automates the complete setup of the development environment
# for the Privatekonomi project in GitHub Codespaces and starts the application.
#
# Summary of setup performed:
# 1. Install .NET 9 SDK (required for this project)
# 2. Install .NET Aspire workload
# 3. Configure PATH for .NET tools
# 4. Restore project dependencies
# 5. Clean and rebuild solution
# 6. Install Entity Framework CLI tools
# 7. Configure HTTPS development certificates
# 8. Verify installation and test readiness
# 9. Start Aspire Dashboard with all services
#
# Created: October 23, 2025
# Based on: GitHub Copilot chat session for Codespace prerequisites
# ============================================================================

set -e  # Exit on any error

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Logging functions
log_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

log_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

log_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

log_section() {
    echo -e "\n${BLUE}========================================${NC}"
    echo -e "${BLUE} $1${NC}"
    echo -e "${BLUE}========================================${NC}\n"
}

# Check if running in Codespace
check_codespace() {
    if [[ -z "${CODESPACES}" ]]; then
        log_warning "This script is designed for GitHub Codespaces but can work in other Ubuntu environments"
    else
        log_info "Running in GitHub Codespaces environment"
    fi
}

# Step 1: Install .NET 9 SDK
install_dotnet_9() {
    log_section "Installing .NET 9 SDK"
    
    log_info "Checking current .NET installation..."
    if command -v dotnet &> /dev/null; then
        log_info "Current .NET version: $(dotnet --version)"
        log_info "Installed SDKs:"
        dotnet --list-sdks
    else
        log_warning ".NET not found in PATH"
    fi
    
    log_info "Installing .NET 9 SDK using Microsoft's installation script..."
    curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --version latest --channel 9.0
    
    log_info "Configuring PATH for .NET..."
    export PATH="$HOME/.dotnet:$PATH"
    echo 'export PATH="$HOME/.dotnet:$PATH"' >> ~/.bashrc
    
    log_info "Verifying .NET 9 installation..."
    dotnet --list-sdks
    log_success ".NET 9 SDK installation completed"
}

# Step 2: Install Aspire workload
install_aspire_workload() {
    log_section "Installing .NET Aspire Workload"
    
    log_info "Checking if Aspire workload is already installed..."
    if dotnet workload list | grep -q "aspire"; then
        log_success "Aspire workload is already installed"
    else
        log_info "Installing Aspire workload..."
        dotnet workload install aspire
    fi
    
    log_info "Verifying Aspire workload installation..."
    dotnet workload list
    log_success "Aspire workload installation completed"
}

# Step 3: Restore and build project
setup_project() {
    log_section "Setting up Project Dependencies"
    
    log_info "Restoring project dependencies..."
    dotnet restore Privatekonomi.sln
    
    log_info "Cleaning previous build artifacts..."
    dotnet clean Privatekonomi.sln
    
    log_info "Building solution..."
    dotnet build Privatekonomi.sln
    
    log_success "Project setup completed"
}

# Step 4: Install Entity Framework CLI tools
install_ef_tools() {
    log_section "Installing Entity Framework CLI Tools"
    
    log_info "Checking if Entity Framework tools are already installed..."
    if dotnet tool list --global | grep -q "dotnet-ef"; then
        log_success "Entity Framework tools are already installed"
    else
        log_info "Installing Entity Framework global tools..."
        dotnet tool install --global dotnet-ef
    fi
    
    log_info "Verifying Entity Framework tools installation..."
    if dotnet tool list --global | grep -q "dotnet-ef"; then
        log_success "Entity Framework tools installed successfully"
        # Use || true to prevent script from stopping if ef command has issues
        dotnet ef --version || log_warning "EF version check had issues but tool is installed"
    else
        log_error "Failed to install Entity Framework tools"
        return 1
    fi
    log_success "Entity Framework tools installation completed"
}

# Step 5: Configure HTTPS development certificates
configure_dev_certs() {
    log_section "Configuring HTTPS Development Certificates"
    
    log_info "Cleaning any existing development certificates..."
    dotnet dev-certs https --clean || log_warning "Failed to clean existing certificates (may not exist)"
    
    log_info "Generating new HTTPS development certificate..."
    dotnet dev-certs https --trust || log_warning "Failed to trust certificate (may require manual trust)"
    
    log_info "Verifying HTTPS certificate installation..."
    if dotnet dev-certs https --check; then
        log_success "HTTPS development certificate is properly configured"
    else
        log_warning "HTTPS certificate verification failed - applications may have SSL issues"
    fi
    
    log_success "HTTPS development certificates configuration completed"
}

# Step 6: Make scripts executable
setup_scripts() {
    log_section "Setting up Project Scripts"
    
    log_info "Making project scripts executable..."
    chmod +x ./app-start.sh
    chmod +x ./app-start.ps1 2>/dev/null || true
    
    log_success "Project scripts are now executable"
}

# Step 7: Verify installation
verify_installation() {
    log_section "Verifying Installation"
    
    log_info "Checking .NET installation..."
    dotnet --version
    dotnet --list-sdks
    
    log_info "Checking Aspire workload..."
    dotnet workload list | grep aspire || log_warning "Aspire workload not found"
    
    log_info "Checking Entity Framework tools..."
    dotnet tool list --global | grep dotnet-ef || log_warning "EF tools not found in global tools"
    
    log_info "Checking HTTPS development certificates..."
    if dotnet dev-certs https --check --quiet; then
        log_success "HTTPS development certificates are properly configured"
    else
        log_warning "HTTPS development certificates may need attention"
    fi
    
    log_info "Checking project build status..."
    dotnet build Privatekonomi.sln --verbosity quiet
    
    log_success "All verifications completed successfully!"
}

# Step 8: Start application
start_application() {
    log_section "Starting Privatekonomi Application"
    
    log_info "Navigating to AppHost directory..."
    cd "src/Privatekonomi.AppHost"
    
    echo ""
    echo -e "${GREEN}� Starting Privatekonomi with .NET Aspire Dashboard...${NC}"
    echo -e "${BLUE}The dashboard will open automatically in your browser.${NC}"
    echo ""
    echo -e "${YELLOW}Services that will be available:${NC}"
    echo -e "  • ${GREEN}Aspire Dashboard:${NC} View logs, traces, and metrics"
    echo -e "  • ${GREEN}Web Application:${NC} Blazor Server UI"
    echo -e "  • ${GREEN}API:${NC} REST API with Swagger documentation"
    echo ""
    echo -e "${RED}Press Ctrl+C to stop all services${NC}"
    echo ""
    
    # Run the AppHost
    dotnet run
}

# Step 9: Display usage information (for when not starting immediately)
show_usage_info() {
    log_section "Setup Complete - Usage Information"
    
    echo -e "${GREEN}✅ Privatekonomi environment is ready!${NC}\n"
    
    echo -e "${BLUE}Available commands:${NC}"
    echo -e "  ${YELLOW}Start Application:${NC}"
    echo -e "    ./app-start.sh"
    echo -e ""
    echo -e "  ${YELLOW}Database Operations:${NC}"
    echo -e "    cd src/Privatekonomi.Core"
    echo -e "    dotnet ef migrations add <MigrationName>"
    echo -e "    dotnet ef database update"
    echo -e ""
    echo -e "  ${YELLOW}Build and Test:${NC}"
    echo -e "    dotnet build Privatekonomi.sln"
    echo -e "    dotnet test"
    echo -e ""
    echo -e "${BLUE}Project Structure:${NC}"
    echo -e "  • ${YELLOW}Privatekonomi.Api${NC} - Backend API project"
    echo -e "  • ${YELLOW}Privatekonomi.Web${NC} - Blazor frontend project"
    echo -e "  • ${YELLOW}Privatekonomi.Core${NC} - Shared core library"
    echo -e "  • ${YELLOW}Privatekonomi.AppHost${NC} - Aspire orchestration"
    echo -e "  • ${YELLOW}Privatekonomi.ServiceDefaults${NC} - Shared service configurations"
    echo -e ""
    echo -e "${BLUE}Installed Tools:${NC}"
    echo -e "  • .NET 9 SDK"
    echo -e "  • .NET Aspire workload"
    echo -e "  • Entity Framework CLI tools"
    echo -e "  • HTTPS development certificates (trusted)"
    echo -e ""
    echo -e "${GREEN}Happy coding! 🚀${NC}"
}

# Main execution
main() {
    log_section "Privatekonomi Codespace Setup"
    log_info "Starting automated environment setup..."
    
    check_codespace
    install_dotnet_9
    install_aspire_workload
    setup_project
    install_ef_tools
    configure_dev_certs
    setup_scripts
    verify_installation
    
    # Check if --setup-only flag is provided
    if [[ "$1" == "--setup-only" ]]; then
        show_usage_info
        log_success "Setup completed! Run './app-start.sh' to start the application."
    else
        start_application
    fi
}

# Run main function
main "$@"
