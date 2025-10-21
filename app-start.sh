#!/bin/bash
# Startup script for Privatekonomi application in Codespaces
# This script starts the .NET Aspire Dashboard which orchestrates all services

echo "🚀 Starting Privatekonomi with .NET Aspire Dashboard..."
echo ""

# Check if .NET is installed
if ! command -v dotnet &> /dev/null; then
    echo "⚠️  .NET SDK is not installed"
    echo "Installing .NET 9 SDK..."
    
    # Install .NET SDK
    if ! bash <(curl -sSL https://dot.net/v1/dotnet-install.sh) --channel 9.0 --install-dir "$HOME/.dotnet"; then
        echo "❌ Failed to install .NET SDK"
        exit 1
    fi
    
    # Add to current session
    export DOTNET_ROOT="$HOME/.dotnet"
    export PATH="$HOME/.dotnet:$PATH"
    
    echo "✅ .NET SDK installed successfully"
fi

# Check .NET version
DOTNET_VERSION=$(dotnet --version)
echo "✅ .NET SDK version: $DOTNET_VERSION"

# Check if Aspire workload is installed
if ! dotnet workload list | grep -q "aspire"; then
    echo "⚠️  Aspire workload is not installed"
    echo "Installing Aspire workload..."
    if ! dotnet workload install aspire; then
        echo "❌ Failed to install Aspire workload"
        exit 1
    fi
    echo "✅ Aspire workload installed successfully"
fi

# Navigate to AppHost directory
cd "$(dirname "$0")/src/Privatekonomi.AppHost"

echo ""
echo "📦 Starting Aspire Dashboard..."
echo "The dashboard will open automatically in your browser."
echo ""
echo "Services that will be available:"
echo "  - Aspire Dashboard: View logs, traces, and metrics"
echo "  - Web Application: Blazor Server UI"
echo "  - API: REST API with Swagger documentation"
echo ""
echo "Press Ctrl+C to stop all services"
echo ""

# Run the AppHost
dotnet run
