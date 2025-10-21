#!/bin/bash
# Startup script for Privatekonomi application in Codespaces
# This script starts the .NET Aspire Dashboard which orchestrates all services

set -e

echo "🚀 Starting Privatekonomi with .NET Aspire Dashboard..."
echo ""

# Check if .NET is installed
if ! command -v dotnet &> /dev/null; then
    echo "❌ Error: .NET SDK is not installed"
    echo "Please install .NET 9 SDK: https://dotnet.microsoft.com/download/dotnet/9.0"
    exit 1
fi

# Check .NET version
DOTNET_VERSION=$(dotnet --version)
echo "✅ .NET SDK version: $DOTNET_VERSION"

# Check if Aspire workload is installed
if ! dotnet workload list | grep -q "aspire"; then
    echo "⚠️  Aspire workload is not installed"
    echo "Installing Aspire workload..."
    dotnet workload install aspire
    if [ $? -ne 0 ]; then
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
