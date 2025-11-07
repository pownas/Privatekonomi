#!/bin/bash

# Raspberry Pi Aspire Startup Script
# Kör Privatekonomi med Aspire på Raspberry Pi

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

# Function to stop existing processes and free ports
cleanup_processes() {
    log_info "Stoppar befintliga .NET-processer och frigör portar..."
    
    # Kill all dotnet processes
    if pgrep -f "dotnet" > /dev/null; then
        log_warning "Hittade körande .NET-processer, stoppar dem..."
        pkill -f "dotnet" || true
        sleep 2
        
        # Force kill if still running
        if pgrep -f "dotnet" > /dev/null; then
            log_warning "Tvångsstoppar kvarvarande .NET-processer..."
            pkill -9 -f "dotnet" || true
            sleep 1
        fi
    fi
    
    # Check for processes using Privatekonomi ports and kill them
    local ports=(17127 5274 5277)
    
    for port in "${ports[@]}"; do
        if command -v lsof &> /dev/null; then
            local pid=$(lsof -ti:$port 2>/dev/null || true)
            if [ -n "$pid" ]; then
                log_warning "Port $port används av process $pid, stoppar den..."
                kill $pid 2>/dev/null || true
                sleep 1
                
                # Force kill if still running
                if kill -0 $pid 2>/dev/null; then
                    log_warning "Tvångsstoppar process $pid på port $port..."
                    kill -9 $pid 2>/dev/null || true
                fi
            fi
        fi
    done
    
    log_success "Process-rensning slutförd"
}

echo ""
log_info "Startar Privatekonomi Aspire AppHost på Raspberry Pi..."
echo ""

# Cleanup before starting
cleanup_processes

# Lägg till .NET i PATH om det inte finns där
if [ -d "$HOME/.dotnet" ] && ! command -v dotnet &> /dev/null; then
    export PATH="$PATH:$HOME/.dotnet"
    export DOTNET_ROOT="$HOME/.dotnet"
fi

# Verifiera att dotnet finns
if ! command -v dotnet &> /dev/null; then
    log_error "dotnet hittades inte i PATH"
    echo "Kör först: ./raspberry-pi-install.sh"
    echo "Eller lägg till .NET manuellt i PATH:"
    echo "  export PATH=\"\$PATH:\$HOME/.dotnet\""
    exit 1
fi

# Sätt miljövariabler för Raspberry Pi
export PRIVATEKONOMI_RASPBERRY_PI=true
export ASPNETCORE_ENVIRONMENT=Production
export PRIVATEKONOMI_ENVIRONMENT=RaspberryPi
export PRIVATEKONOMI_STORAGE_PROVIDER=Sqlite

# Konfigurera Aspire Dashboard för att lyssna på alla nätverksinterfaces
export ASPNETCORE_URLS="http://0.0.0.0:17127"
export DOTNET_DASHBOARD_URLS="http://0.0.0.0:17127"

# Check if using published binaries or source
INSTALL_DIR="$HOME/Privatekonomi"
PUBLISH_DIR="$INSTALL_DIR/publish/AppHost"

if [ -d "$PUBLISH_DIR" ] && [ -f "$PUBLISH_DIR/Privatekonomi.AppHost" ]; then
    log_info "Hittade publicerade binärer, använder dem..."
    WORKING_DIR="$PUBLISH_DIR"
    USE_PUBLISHED=true
else
    log_info "Använder källkod med dotnet run..."
    WORKING_DIR="$(dirname "$0")/src/Privatekonomi.AppHost"
    USE_PUBLISHED=false
fi

# Navigera till rätt katalog
cd "$WORKING_DIR"

# Kontrollera att appsettings.Production.json finns
if [ ! -f "appsettings.Production.json" ]; then
    log_warning "appsettings.Production.json saknas för AppHost"
    echo "Aspire Dashboard kommer endast att lyssna på localhost"
fi

if [ "$USE_PUBLISHED" = false ]; then
    if [ ! -f "../Privatekonomi.Web/appsettings.Production.json" ] || [ ! -f "../Privatekonomi.Api/appsettings.Production.json" ]; then
        log_warning "appsettings.Production.json saknas för Web eller API"
        echo ""
        echo "Kör installationsskriptet igen för att skapa konfigurationsfiler:"
        echo "  ./raspberry-pi-install.sh"
        echo ""
    fi
fi

echo ""
log_info "Miljövariabler:"
echo "  PRIVATEKONOMI_RASPBERRY_PI: $PRIVATEKONOMI_RASPBERRY_PI"
echo "  ASPNETCORE_ENVIRONMENT: $ASPNETCORE_ENVIRONMENT"
echo "  PRIVATEKONOMI_ENVIRONMENT: $PRIVATEKONOMI_ENVIRONMENT"
echo "  PRIVATEKONOMI_STORAGE_PROVIDER: $PRIVATEKONOMI_STORAGE_PROVIDER"
echo "  ASPNETCORE_URLS: $ASPNETCORE_URLS"
echo "  DOTNET_DASHBOARD_URLS: $DOTNET_DASHBOARD_URLS"
echo ""

log_info "Använder .NET version: $(dotnet --version)"

if [ "$USE_PUBLISHED" = true ]; then
    log_success "Startar från publicerade binärer (snabbare uppstart)..."
    echo ""
    echo -e "${GREEN}🚀 Startar Privatekonomi...${NC}"
    echo ""
    echo -e "${YELLOW}Tjänster:${NC}"
    echo "  • Aspire Dashboard: http://[raspberry-pi-ip]:17127"
    echo "  • Web App: http://[raspberry-pi-ip]:5274"
    echo "  • API: http://[raspberry-pi-ip]:5277"
    echo ""
    echo -e "${RED}Tryck Ctrl+C för att stoppa${NC}"
    echo ""
    
    # Starta från publicerade binärer
    ./Privatekonomi.AppHost
else
    log_info "Startar från källkod med dotnet run..."
    echo ""
    echo -e "${GREEN}🚀 Startar Privatekonomi...${NC}"
    echo ""
    echo -e "${YELLOW}Tjänster:${NC}"
    echo "  • Aspire Dashboard: http://[raspberry-pi-ip]:17127"
    echo "  • Web App: http://[raspberry-pi-ip]:5274"
    echo "  • API: http://[raspberry-pi-ip]:5277"
    echo ""
    echo -e "${RED}Tryck Ctrl+C för att stoppa${NC}"
    echo ""
    
    # Starta applikationen
    dotnet run --configuration Release
fi