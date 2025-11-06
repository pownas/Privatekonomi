#!/bin/bash

# ============================================================================
# Privatekonomi Raspberry Pi Installation Script
# ============================================================================
# 
# This script automates the complete setup of the Privatekonomi application
# on Raspberry Pi OS.
#
# Summary of setup performed:
# 1. Check system requirements and dependencies
# 2. Install/verify .NET 9 SDK
# 3. Configure PATH for .NET tools
# 4. Clone or update Privatekonomi repository 
# 5. Install Entity Framework CLI tools
# 6. Configure HTTPS development certificates
# 7. Build and prepare the application
# 8. Set up systemd service (optional)
# 9. Configure firewall if needed
# 10. Verify installation and provide usage instructions
#
# Created: November 6, 2025
# For: Raspberry Pi OS (Debian-based)
# ============================================================================

set -e  # Exit on any error

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
PURPLE='\033[0;35m'
NC='\033[0m' # No Color

# Configuration
REPO_URL="https://github.com/pownas/Privatekonomi.git"
INSTALL_DIR="$HOME/Privatekonomi"
DATA_DIR="$HOME/privatekonomi-data"
BACKUP_DIR="$HOME/privatekonomi-backups"
SERVICE_NAME="privatekonomi"
DEFAULT_PORT="17127"
WEB_PORT="5274"
API_PORT="5277"

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
    echo -e ""
    echo -e "${PURPLE}===================================================${NC}"
    echo -e "${PURPLE} $1${NC}"
    echo -e "${PURPLE}===================================================${NC}"
}

# Check if running on Raspberry Pi
check_raspberry_pi() {
    log_section "Kontrollerar Raspberry Pi-miljö"
    
    if [ ! -f /proc/device-tree/model ]; then
        log_warning "Kan inte verifiera Raspberry Pi-modell"
    else
        local model=$(cat /proc/device-tree/model 2>/dev/null || echo "Okänd")
        log_info "Raspberry Pi-modell: $model"
    fi
    
    # Check OS
    if [ -f /etc/os-release ]; then
        local os_info=$(grep PRETTY_NAME /etc/os-release | cut -d'"' -f2)
        log_info "Operativsystem: $os_info"
    fi
    
    # Check architecture
    local arch=$(uname -m)
    log_info "Arkitektur: $arch"
    
    if [[ "$arch" != "aarch64" && "$arch" != "armv7l" && "$arch" != "armv6l" ]]; then
        log_warning "Okänd arkitektur: $arch. Fortsätter ändå..."
    fi
}

# Check system requirements
check_system_requirements() {
    log_section "Kontrollerar systemkrav"
    
    # Check available memory
    local mem_kb=$(grep MemTotal /proc/meminfo | awk '{print $2}')
    local mem_mb=$((mem_kb / 1024))
    log_info "Tillgängligt minne: ${mem_mb}MB"
    
    if [ $mem_mb -lt 1024 ]; then
        log_warning "Lågt minne ($mem_mb MB). Rekommenderat: minst 1GB för bästa prestanda"
    fi
    
    # Check available disk space
    local disk_space=$(df -h . | awk 'NR==2 {print $4}')
    log_info "Tillgängligt diskutrymme: $disk_space"
    
    # Check if git is installed
    if ! command -v git &> /dev/null; then
        log_info "Installerar git..."
        sudo apt update
        sudo apt install -y git
    fi
    
    # Check if curl is installed
    if ! command -v curl &> /dev/null; then
        log_info "Installerar curl..."
        sudo apt install -y curl
    fi
    
    log_success "Systemkrav kontrollerade"
}

# Create NuGet config if missing
create_nuget_config() {
    log_section "Konfigurerar NuGet"
    
    local nuget_dir="$HOME/.nuget/NuGet"
    local nuget_config="$nuget_dir/NuGet.Config"
    
    if [ -f "$nuget_config" ]; then
        log_info "NuGet.Config finns redan"
        return 0
    fi
    
    log_info "Skapar NuGet.Config..."
    mkdir -p "$nuget_dir"
    
    cat > "$nuget_config" << 'EOF'
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
</configuration>
EOF
    
    log_success "NuGet.Config skapad"
}

# Install .NET 9 SDK
install_dotnet_9() {
    log_section "Installerar .NET 9 SDK"
    
    if command -v dotnet &> /dev/null; then
        local current_version=$(dotnet --version 2>/dev/null || echo "Okänd")
        log_info "Befintlig .NET-version: $current_version"
        
        if [[ "$current_version" == 9.* ]]; then
            log_success ".NET 9 SDK är redan installerat"
            return 0
        fi
    fi
    
    log_info "Laddar ner och installerar .NET 9 SDK..."
    
    # Download and run the install script
    curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 9.0
    
    # Add to PATH for current session
    export PATH="$PATH:$HOME/.dotnet"
    
    # Add to bash profile
    if ! grep -q ".dotnet" ~/.bashrc; then
        echo "" >> ~/.bashrc
        echo "# Add .NET to PATH" >> ~/.bashrc
        echo 'export PATH="$PATH:$HOME/.dotnet"' >> ~/.bashrc
        log_info ".NET har lagts till i ~/.bashrc"
    fi
    
    # Verify installation
    if [ -f "$HOME/.dotnet/dotnet" ]; then
        local version=$($HOME/.dotnet/dotnet --version)
        log_success ".NET 9 SDK installerat: version $version"
    else
        log_error "Misslyckades med att installera .NET 9 SDK"
        exit 1
    fi
}

# Setup project
setup_project() {
    log_section "Konfigurerar Privatekonomi-projekt"
    
    if [ -d "$INSTALL_DIR" ]; then
        log_info "Katalogen $INSTALL_DIR finns redan"
        read -p "Vill du uppdatera befintlig installation? (y/n): " -n 1 -r
        echo
        if [[ $REPLY =~ ^[Yy]$ ]]; then
            log_info "Uppdaterar befintlig installation..."
            cd "$INSTALL_DIR"
            git fetch origin
            git reset --hard origin/main
        else
            log_info "Använder befintlig installation"
            cd "$INSTALL_DIR"
        fi
    else
        log_info "Klonar Privatekonomi repository..."
        git clone "$REPO_URL" "$INSTALL_DIR"
        cd "$INSTALL_DIR"
    fi
    
    # Make scripts executable
    chmod +x app-start.sh raspberry-pi-start.sh 2>/dev/null || true
    
    log_info "Återställer NuGet-paket..."
    dotnet restore
    
    log_info "Bygger lösningen..."
    dotnet build --configuration Release
    
    log_success "Projekt konfigurerat framgångsrikt"
}

# Create data directory and configuration
configure_storage() {
    log_section "Konfigurerar datalagring"
    
    # Create data directory
    mkdir -p "$DATA_DIR"
    log_info "Datakatalog skapad: $DATA_DIR"
    
    # Create backup directory
    mkdir -p "$BACKUP_DIR"
    log_info "Backup-katalog skapad: $BACKUP_DIR"
    
    # Check existing configuration
    local web_config="$INSTALL_DIR/src/Privatekonomi.Web/appsettings.Production.json"
    local existing_provider=""
    
    if [ -f "$web_config" ]; then
        # Try to extract existing provider from config
        existing_provider=$(grep -Po '"Provider":\s*"\K[^"]+' "$web_config" 2>/dev/null || echo "")
        if [ -n "$existing_provider" ]; then
            log_info "Befintlig lagringskonfiguration hittad: $existing_provider"
        fi
    fi
    
    # Ask user for storage provider (always ask to allow easy change)
    echo -e "${YELLOW}Välj lagringsalternativ:${NC}"
    echo "  1) SQLite (Rekommenderat - snabb, låg resursanvändning)"
    echo "  2) JsonFile (Enkel backup, automatisk sparning var 5:e minut)"
    
    if [ "$existing_provider" = "Sqlite" ]; then
        read -p "Ditt val (1/2) [1 - nuvarande]: " storage_choice
    elif [ "$existing_provider" = "JsonFile" ]; then
        read -p "Ditt val (1/2) [2 - nuvarande]: " storage_choice
    else
        read -p "Ditt val (1/2) [1]: " storage_choice
    fi
    
    storage_choice=${storage_choice:-1}
    
    local storage_provider
    local connection_string
    
    if [ "$storage_choice" = "2" ]; then
        storage_provider="JsonFile"
        connection_string="$DATA_DIR"
        log_info "Använder JsonFile-lagring"
    else
        storage_provider="Sqlite"
        connection_string="Data Source=$DATA_DIR/privatekonomi.db"
        log_info "Använder SQLite-lagring"
    fi
    
    # Create appsettings.Production.json for Web
    log_info "Skapar $web_config..."
    
    cat > "$web_config" << EOF
{
  "Storage": {
    "Provider": "$storage_provider",
    "ConnectionString": "$connection_string",
    "SeedTestData": false
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "Urls": "http://0.0.0.0:$WEB_PORT",
  "AllowedHosts": "*"
}
EOF
    
    # Create appsettings.Production.json for Api
    local api_config="$INSTALL_DIR/src/Privatekonomi.Api/appsettings.Production.json"
    log_info "Skapar $api_config..."
    
    cat > "$api_config" << EOF
{
  "Storage": {
    "Provider": "$storage_provider",
    "ConnectionString": "$connection_string",
    "SeedTestData": false
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "Urls": "http://0.0.0.0:$API_PORT",
  "AllowedHosts": "*"
}
EOF
    
    log_success "Lagringskonfiguration skapad"
}

# Install Entity Framework tools
install_ef_tools() {
    log_section "Installerar Entity Framework-verktyg"
    
    log_info "Installerar dotnet-ef globalt..."
    dotnet tool install --global dotnet-ef || dotnet tool update --global dotnet-ef
    
    # Add tools to PATH
    local tools_path="$HOME/.dotnet/tools"
    if [ -d "$tools_path" ] && ! echo "$PATH" | grep -q "$tools_path"; then
        export PATH="$PATH:$tools_path"
        
        if ! grep -q ".dotnet/tools" ~/.bashrc; then
            echo "" >> ~/.bashrc
            echo "# Add .NET Core SDK tools" >> ~/.bashrc
            echo 'export PATH="$PATH:$HOME/.dotnet/tools"' >> ~/.bashrc
            log_info "EF-verktyg har lagts till i PATH"
        fi
    fi
    
    log_success "Entity Framework-verktyg installerade"
}

# Configure development certificates
configure_dev_certs() {
    log_section "Konfigurerar utvecklingscertifikat"
    
    log_info "Rensar befintliga certifikat..."
    dotnet dev-certs https --clean || true
    
    log_info "Genererar nya utvecklingscertifikat..."
    dotnet dev-certs https --trust || {
        log_warning "Kunde inte lita på certifikat automatiskt (normalt på Linux)"
        log_info "Du kan ignorera HTTPS-varningar i utvecklingsmiljö"
    }
    
    log_success "Utvecklingscertifikat konfigurerade"
}

# Optimize swap for low memory systems
optimize_swap() {
    if [ "$SKIP_SWAP" = true ]; then
        log_info "Hoppar över swap-optimering (--no-swap)"
        return 0
    fi
    
    log_section "Optimerar swap-minne (valfritt)"
    
    local mem_kb=$(grep MemTotal /proc/meminfo | awk '{print $2}')
    local mem_mb=$((mem_kb / 1024))
    
    if [ $mem_mb -ge 4096 ]; then
        log_info "Tillräckligt med minne ($mem_mb MB), hoppar över swap-optimering"
        return 0
    fi
    
    # Check current swap size
    local current_swap_kb=$(grep SwapTotal /proc/meminfo | awk '{print $2}')
    local current_swap_mb=$((current_swap_kb / 1024))
    
    log_info "Nuvarande minne: ${mem_mb}MB"
    log_info "Nuvarande swap: ${current_swap_mb}MB"
    
    if [ $current_swap_mb -ge 2048 ]; then
        log_success "Swap är redan optimerat (${current_swap_mb}MB)"
        return 0
    fi
    
    log_warning "Lågt minne detekterat och swap är bara ${current_swap_mb}MB"
    read -p "Vill du öka swap-storleken till 2GB för bättre prestanda? (y/n): " -n 1 -r
    echo
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        return 0
    fi
    
    log_info "Ökar swap-storlek till 2GB..."
    
    # Check if dphys-swapfile is installed
    if ! command -v dphys-swapfile &> /dev/null; then
        log_info "Installerar dphys-swapfile..."
        sudo apt install -y dphys-swapfile
    fi
    
    sudo dphys-swapfile swapoff || true
    sudo sed -i 's/^CONF_SWAPSIZE=.*/CONF_SWAPSIZE=2048/' /etc/dphys-swapfile
    sudo dphys-swapfile setup
    sudo dphys-swapfile swapon
    
    log_success "Swap-storlek ökad till 2GB"
}

# Configure firewall (optional)
configure_firewall() {
    if [ "$SKIP_FIREWALL" = true ]; then
        log_info "Hoppar över brandväggskonfiguration (--no-firewall)"
        return 0
    fi
    
    log_section "Konfigurerar brandvägg (valfritt)"
    
    if ! command -v ufw &> /dev/null; then
        read -p "UFW brandvägg är inte installerad. Vill du installera den? (y/n): " -n 1 -r
        echo
        if [[ $REPLY =~ ^[Yy]$ ]]; then
            sudo apt install -y ufw
        else
            log_info "Hoppar över brandväggskonfiguration"
            return 0
        fi
    fi
    
    # Check if UFW is already configured with our ports
    local ufw_status=$(sudo ufw status 2>/dev/null || echo "inactive")
    local has_dashboard=$(echo "$ufw_status" | grep -q "$DEFAULT_PORT" && echo "yes" || echo "no")
    local has_web=$(echo "$ufw_status" | grep -q "$WEB_PORT" && echo "yes" || echo "no")
    local has_api=$(echo "$ufw_status" | grep -q "$API_PORT" && echo "yes" || echo "no")
    
    if [ "$has_dashboard" = "yes" ] && [ "$has_web" = "yes" ] && [ "$has_api" = "yes" ]; then
        log_success "UFW är redan konfigurerat med alla Privatekonomi-portar"
        sudo ufw status | grep -E "$DEFAULT_PORT|$WEB_PORT|$API_PORT"
        return 0
    fi
    
    if [ "$has_dashboard" = "yes" ] || [ "$has_web" = "yes" ] || [ "$has_api" = "yes" ]; then
        log_info "UFW är delvis konfigurerat. Saknade portar kommer att läggas till."
    fi
    
    read -p "Vill du konfigurera UFW-brandväggen? (y/n): " -n 1 -r
    echo
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        return 0
    fi
    
    log_info "Konfigurerar UFW-brandvägg..."
    
    # Allow SSH first to avoid lockout
    sudo ufw allow ssh
    
    # Allow Aspire Dashboard
    if [ "$has_dashboard" = "no" ]; then
        sudo ufw allow $DEFAULT_PORT/tcp comment "Privatekonomi Aspire Dashboard"
        log_info "Port $DEFAULT_PORT (Aspire Dashboard) öppnad"
    fi
    
    # Allow Web application
    if [ "$has_web" = "no" ]; then
        sudo ufw allow $WEB_PORT/tcp comment "Privatekonomi Web App"
        log_info "Port $WEB_PORT (Web App) öppnad"
    fi
    
    # Allow API
    if [ "$has_api" = "no" ]; then
        sudo ufw allow $API_PORT/tcp comment "Privatekonomi API"
        log_info "Port $API_PORT (API) öppnad"
    fi
    
    # Enable firewall
    sudo ufw --force enable
    
    log_success "Brandvägg konfigurerad"
    sudo ufw status
}

# Create systemd service (optional)
create_systemd_service() {
    if [ "$SKIP_SERVICE" = true ]; then
        log_info "Hoppar över systemd-tjänst (--no-service)"
        return 0
    fi
    
    log_section "Skapar systemd-tjänst (valfritt)"
    
    local service_file="/etc/systemd/system/${SERVICE_NAME}.service"
    
    # Check if service already exists
    if [ -f "$service_file" ]; then
        log_info "Systemd-tjänst '$SERVICE_NAME' finns redan"
        
        if systemctl is-enabled "$SERVICE_NAME" &>/dev/null; then
            log_success "Tjänsten är redan aktiverad"
            
            read -p "Vill du uppdatera tjänstekonfigurationen? (y/n): " -n 1 -r
            echo
            if [[ ! $REPLY =~ ^[Yy]$ ]]; then
                return 0
            fi
        fi
    else
        read -p "Vill du skapa en systemd-tjänst för att starta Privatekonomi automatiskt? (y/n): " -n 1 -r
        echo
        if [[ ! $REPLY =~ ^[Yy]$ ]]; then
            return 0
        fi
    fi
    
    local user=$(whoami)
    
    log_info "Skapar systemd-tjänst: $service_file"
    
    sudo tee "$service_file" > /dev/null << EOF
[Unit]
Description=Privatekonomi Personal Finance Application
After=network.target

[Service]
Type=notify
User=$user
Group=$user
WorkingDirectory=$INSTALL_DIR/src/Privatekonomi.AppHost
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=PRIVATEKONOMI_ENVIRONMENT=RaspberryPi
Environment=PRIVATEKONOMI_STORAGE_PROVIDER=Sqlite
Environment=PRIVATEKONOMI_RASPBERRY_PI=true
Environment=ASPNETCORE_URLS=http://0.0.0.0:$DEFAULT_PORT
Environment=DOTNET_DASHBOARD_URLS=http://0.0.0.0:$DEFAULT_PORT
Environment=DOTNET_ROOT=$HOME/.dotnet
ExecStart=$HOME/.dotnet/dotnet run --configuration Release
Restart=always
RestartSec=10
SyslogIdentifier=$SERVICE_NAME

[Install]
WantedBy=multi-user.target
EOF
    
    sudo systemctl daemon-reload
    sudo systemctl enable "$SERVICE_NAME"
    
    log_success "Systemd-tjänst '$SERVICE_NAME' skapad och aktiverad"
    log_info "Använd följande kommandon för att hantera tjänsten:"
    echo -e "  ${YELLOW}sudo systemctl start $SERVICE_NAME${NC}   # Starta tjänsten"
    echo -e "  ${YELLOW}sudo systemctl stop $SERVICE_NAME${NC}    # Stoppa tjänsten"
    echo -e "  ${YELLOW}sudo systemctl status $SERVICE_NAME${NC}  # Kontrollera status"
    echo -e "  ${YELLOW}journalctl -u $SERVICE_NAME -f${NC}       # Visa loggar"
}

# Create backup script and schedule
setup_backup() {
    if [ "$SKIP_BACKUP" = true ]; then
        log_info "Hoppar över backup-konfiguration (--no-backup)"
        return 0
    fi
    
    log_section "Konfigurerar automatiska backuper (valfritt)"
    
    local scripts_dir="$HOME/scripts"
    local backup_script="$scripts_dir/backup-privatekonomi.sh"
    
    # Check if backup script already exists
    if [ -f "$backup_script" ]; then
        log_info "Backup-script finns redan: $backup_script"
        
        # Check if cron job exists
        if crontab -l 2>/dev/null | grep -q "backup-privatekonomi.sh"; then
            log_success "Automatisk backup är redan schemalagd"
            crontab -l 2>/dev/null | grep "backup-privatekonomi.sh"
            
            read -p "Vill du uppdatera backup-skriptet? (y/n): " -n 1 -r
            echo
            if [[ ! $REPLY =~ ^[Yy]$ ]]; then
                return 0
            fi
        else
            log_info "Backup-script finns men är inte schemalagt"
        fi
    else
        read -p "Vill du skapa automatiska dagliga backuper? (y/n): " -n 1 -r
        echo
        if [[ ! $REPLY =~ ^[Yy]$ ]]; then
            return 0
        fi
    fi
    
    mkdir -p "$scripts_dir"
    
    log_info "Skapar backup-script: $backup_script"
    
    cat > "$backup_script" << 'EOFBACKUP'
#!/bin/bash

# Backup directory
BACKUP_DIR=~/privatekonomi-backups
DATA_DIR=~/privatekonomi-data
DATE=$(date +%Y%m%d_%H%M%S)

# Skapa backup directory om det inte finns
mkdir -p $BACKUP_DIR

# För SQLite
if [ -f "$DATA_DIR/privatekonomi.db" ]; then
    cp "$DATA_DIR/privatekonomi.db" "$BACKUP_DIR/privatekonomi_$DATE.db"
    echo "SQLite backup skapad: $BACKUP_DIR/privatekonomi_$DATE.db"
fi

# För JsonFile
if [ -d "$DATA_DIR" ] && [ "$(ls -A $DATA_DIR/*.json 2>/dev/null)" ]; then
    tar -czf "$BACKUP_DIR/privatekonomi_json_$DATE.tar.gz" -C "$DATA_DIR" .
    echo "JSON backup skapad: $BACKUP_DIR/privatekonomi_json_$DATE.tar.gz"
fi

# Ta bort backuper äldre än 750 dagar (ca 2 år)
find $BACKUP_DIR -name "privatekonomi_*" -type f -mtime +750 -delete

echo "Backup klar: $(date)"
EOFBACKUP
    
    chmod +x "$backup_script"
    log_success "Backup-script skapat"
    
    # Ask about cron schedule
    read -p "Vill du schemalägga dagliga backuper kl 02:00? (y/n): " -n 1 -r
    echo
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        log_info "Du kan manuellt köra backuper med: $backup_script"
        return 0
    fi
    
    # Add to crontab
    local cron_entry="0 2 * * * $backup_script >> $HOME/backup.log 2>&1"
    
    # Check if entry already exists
    if crontab -l 2>/dev/null | grep -q "backup-privatekonomi.sh"; then
        log_info "Cron-jobb finns redan"
    else
        (crontab -l 2>/dev/null; echo "$cron_entry") | crontab -
        log_success "Automatisk backup schemalagd för varje dag kl 02:00"
    fi
    
    log_info "Backup-loggar sparas i: $HOME/backup.log"
}

# Configure static IP (optional)
configure_static_ip() {
    if [ "$SKIP_STATIC_IP" = true ]; then
        log_info "Hoppar över statisk IP-konfiguration (--no-static-ip)"
        return 0
    fi
    
    log_section "Konfigurera statisk IP-adress (valfritt)"
    
    # Check if static IP is already configured
    if [ -f /etc/dhcpcd.conf ]; then
        if grep -q "^interface.*" /etc/dhcpcd.conf && grep -q "^static ip_address=" /etc/dhcpcd.conf; then
            log_info "Statisk IP verkar redan vara konfigurerad:"
            grep -A 3 "^interface" /etc/dhcpcd.conf | grep -E "^(interface|static)" | head -4
            
            read -p "Vill du ändra statisk IP-konfiguration? (y/n): " -n 1 -r
            echo
            if [[ ! $REPLY =~ ^[Yy]$ ]]; then
                return 0
            fi
        fi
    fi
    
    read -p "Vill du konfigurera en statisk IP-adress? (y/n): " -n 1 -r
    echo
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        return 0
    fi
    
    local current_ip=$(hostname -I | awk '{print $1}')
    log_info "Nuvarande IP-adress: $current_ip"
    
    # Detect interface
    local interface=$(ip route | grep default | awk '{print $5}' | head -n1)
    log_info "Nätverksgränssnitt: $interface"
    
    # Ask for IP settings
    read -p "Ange statisk IP-adress [$current_ip]: " static_ip
    static_ip=${static_ip:-$current_ip}
    
    read -p "Ange router/gateway [192.168.1.1]: " gateway
    gateway=${gateway:-192.168.1.1}
    
    read -p "Ange DNS-server [$gateway]: " dns
    dns=${dns:-$gateway}
    
    log_info "Konfigurerar statisk IP i /etc/dhcpcd.conf..."
    
    # Backup dhcpcd.conf
    sudo cp /etc/dhcpcd.conf /etc/dhcpcd.conf.backup
    
    # Add static IP configuration
    sudo tee -a /etc/dhcpcd.conf > /dev/null << EOF

# Static IP configuration added by Privatekonomi installer
interface $interface
static ip_address=$static_ip/24
static routers=$gateway
static domain_name_servers=$dns 8.8.8.8
EOF
    
    log_success "Statisk IP konfigurerad"
    log_warning "Du måste starta om Raspberry Pi för att ändringarna ska träda i kraft"
}

# Verify installation
verify_installation() {
    log_section "Verifierar installation"
    
    # Ensure .NET is in PATH
    if [ -d "$HOME/.dotnet" ]; then
        export PATH="$PATH:$HOME/.dotnet"
        export DOTNET_ROOT="$HOME/.dotnet"
    fi
    
    if [ -d "$HOME/.dotnet/tools" ]; then
        export PATH="$PATH:$HOME/.dotnet/tools"
    fi
    
    # Check .NET
    if command -v dotnet &> /dev/null; then
        local dotnet_version=$(dotnet --version)
        log_success ".NET SDK: $dotnet_version"
    else
        log_error ".NET SDK inte funnet"
        return 1
    fi
    
    # Check EF tools
    if command -v dotnet-ef &> /dev/null; then
        local ef_version=$(dotnet-ef --version 2>&1 | head -n1)
        log_success "Entity Framework CLI: $ef_version"
    else
        log_warning "Entity Framework CLI inte funnet i PATH"
        # Try direct path
        if [ -f "$HOME/.dotnet/tools/dotnet-ef" ]; then
            local ef_version=$("$HOME/.dotnet/tools/dotnet-ef" --version 2>&1 | head -n1)
            log_success "Entity Framework CLI: $ef_version"
        else
            log_error "Entity Framework CLI inte installerat"
            return 1
        fi
    fi
    
    # Check project
    if [ -f "$INSTALL_DIR/Privatekonomi.sln" ]; then
        log_success "Privatekonomi-projekt: Installerat i $INSTALL_DIR"
    else
        log_error "Privatekonomi-projekt inte funnet"
        return 1
    fi
    
    # Test build
    cd "$INSTALL_DIR"
    if dotnet build --configuration Release --verbosity quiet; then
        log_success "Projektbygge: Framgångsrik"
    else
        log_error "Projektbygge: Misslyckades"
        return 1
    fi
}

# Show usage information
show_usage_info() {
    log_section "Installation klar - Användningsinformation"
    
    local pi_ip=$(hostname -I | awk '{print $1}')
    
    echo -e ""
    echo -e "${GREEN}🎉 Privatekonomi har installerats framgångsrikt på din Raspberry Pi!${NC}"
    echo -e ""
    echo -e "${BLUE}Så här startar du applikationen:${NC}"
    echo -e ""
    echo -e "  ${YELLOW}Manuell start (rekommenderat för första gången):${NC}"
    echo -e "    cd $INSTALL_DIR"
    echo -e "    ./raspberry-pi-start.sh"
    echo -e ""
    echo -e "  ${YELLOW}Eller direkt med dotnet:${NC}"
    echo -e "    cd $INSTALL_DIR/src/Privatekonomi.AppHost"
    echo -e "    PRIVATEKONOMI_RASPBERRY_PI=true ASPNETCORE_URLS=http://0.0.0.0:$DEFAULT_PORT dotnet run"
    echo -e ""
    
    if systemctl is-enabled "$SERVICE_NAME" &>/dev/null; then
        echo -e "  ${YELLOW}Med systemd-tjänst:${NC}"
        echo -e "    sudo systemctl start $SERVICE_NAME"
        echo -e ""
    fi
    
    echo -e "${BLUE}Åtkomst till applikationen:${NC}"
    echo -e "  ${YELLOW}Lokalt (på Raspberry Pi):${NC}"
    echo -e "    http://localhost:$DEFAULT_PORT (Aspire Dashboard)"
    echo -e "    http://localhost:$WEB_PORT (Web App)"
    echo -e "    http://localhost:$API_PORT (API)"
    echo -e ""
    echo -e "  ${YELLOW}Från andra enheter på nätverket:${NC}"
    echo -e "    http://$pi_ip:$DEFAULT_PORT (Aspire Dashboard)"
    echo -e "    http://$pi_ip:$WEB_PORT (Web App)"
    echo -e "    http://$pi_ip:$API_PORT (API)"
    echo -e ""
    echo -e "${BLUE}Användbara kommandon:${NC}"
    echo -e "  ${YELLOW}Kontrollera portar:${NC}"
    echo -e "    ss -lntp | grep '$DEFAULT_PORT\\|$WEB_PORT\\|$API_PORT'"
    echo -e ""
    echo -e "  ${YELLOW}Visa IP-adress:${NC}"
    echo -e "    hostname -I"
    echo -e ""
    echo -e "  ${YELLOW}Uppdatera projekt:${NC}"
    echo -e "    cd $INSTALL_DIR"
    echo -e "    git pull origin main"
    echo -e "    dotnet build --configuration Release"
    echo -e ""
    echo -e "  ${YELLOW}Manuell backup:${NC}"
    echo -e "    ~/scripts/backup-privatekonomi.sh"
    echo -e ""
    echo -e "  ${YELLOW}Visa backup-loggar:${NC}"
    echo -e "    cat ~/backup.log"
    echo -e ""
    echo -e "${BLUE}Projektstruktur:${NC}"
    echo -e "  • ${YELLOW}Privatekonomi.Api${NC} - Backend API"
    echo -e "  • ${YELLOW}Privatekonomi.Web${NC} - Blazor frontend"
    echo -e "  • ${YELLOW}Privatekonomi.Core${NC} - Kärnbibliotek"
    echo -e "  • ${YELLOW}Privatekonomi.AppHost${NC} - Aspire-orkestrering"
    echo -e ""
    echo -e "${BLUE}Datakatalog:${NC}"
    echo -e "  • ${YELLOW}Data:${NC} $DATA_DIR"
    echo -e "  • ${YELLOW}Backuper:${NC} $BACKUP_DIR"
    echo -e ""
    echo -e "${BLUE}Nästa steg:${NC}"
    if systemctl is-enabled "$SERVICE_NAME" &>/dev/null; then
        echo -e "  1. ${GREEN}Applikationen startar automatiskt vid omstart${NC}"
        echo -e "  2. Första manuella start: ${YELLOW}sudo systemctl start $SERVICE_NAME${NC}"
    else
        echo -e "  1. Starta applikationen: ${YELLOW}cd $INSTALL_DIR && ./raspberry-pi-start.sh${NC}"
    fi
    echo -e "  3. Öppna webbläsare på http://$pi_ip:$WEB_PORT"
    echo -e "  4. Skapa ditt första användarkonto"
    echo -e "  5. Importera eller börja lägga till transaktioner"
    echo -e ""
    echo -e "${GREEN}Lycka till med din personliga ekonomi! 💰${NC}"
}

# Main execution
main() {
    log_section "Privatekonomi Raspberry Pi Installation"
    log_info "Startar automatisk installation för Raspberry Pi..."
    
    check_raspberry_pi
    check_system_requirements
    create_nuget_config
    install_dotnet_9
    setup_project
    configure_storage
    install_ef_tools
    configure_dev_certs
    optimize_swap
    configure_firewall
    create_systemd_service
    setup_backup
    configure_static_ip
    verify_installation
    show_usage_info
    
    log_success "Installation slutförd framgångsrikt!"
    echo -e ""
    echo -e "${GREEN}Starta applikationen med: ${YELLOW}cd $INSTALL_DIR && ./raspberry-pi-start.sh${NC}"
}

# Handle script arguments
SKIP_SERVICE=false
SKIP_FIREWALL=false
SKIP_BACKUP=false
SKIP_STATIC_IP=false
SKIP_SWAP=false

case "${1:-}" in
    --help|-h)
        echo "Privatekonomi Raspberry Pi Installation Script"
        echo ""
        echo "Användning: $0 [ALTERNATIV]"
        echo ""
        echo "Alternativ:"
        echo "  --help, -h         Visa denna hjälp"
        echo "  --no-service       Hoppa över skapande av systemd-tjänst"
        echo "  --no-firewall      Hoppa över brandväggskonfiguration"
        echo "  --no-backup        Hoppa över backup-konfiguration"
        echo "  --no-static-ip     Hoppa över statisk IP-konfiguration"
        echo "  --no-swap          Hoppa över swap-optimering"
        echo "  --skip-interactive Hoppa över alla interaktiva frågor (använd standardvärden)"
        echo ""
        echo "Exempel:"
        echo "  $0                    # Full interaktiv installation"
        echo "  $0 --no-service       # Installera utan systemd-tjänst"
        echo "  $0 --skip-interactive # Automatisk installation utan frågor"
        echo ""
        exit 0
        ;;
    --no-service)
        SKIP_SERVICE=true
        ;;
    --no-firewall)
        SKIP_FIREWALL=true
        ;;
    --no-backup)
        SKIP_BACKUP=true
        ;;
    --no-static-ip)
        SKIP_STATIC_IP=true
        ;;
    --no-swap)
        SKIP_SWAP=true
        ;;
    --skip-interactive)
        SKIP_SERVICE=false  # Create service by default in non-interactive mode
        SKIP_FIREWALL=true
        SKIP_BACKUP=false   # Create backup script by default
        SKIP_STATIC_IP=true
        SKIP_SWAP=true
        ;;
esac

# Run main function
main "$@"