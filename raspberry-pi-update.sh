#!/bin/bash

# ============================================================================
# Privatekonomi Raspberry Pi Update Script
# ============================================================================
# 
# This script automates the update process for an existing Privatekonomi
# installation on Raspberry Pi OS.
#
# Summary of update operations:
# 1. Stop running services
# 2. Backup current data
# 3. Pull latest changes from repository
# 4. Restore NuGet packages
# 5. Build updated application
# 6. Optionally publish new ARM64 binaries
# 7. Restart services
# 8. Verify update success
#
# Created: January 7, 2025
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
INSTALL_DIR="$HOME/Privatekonomi"
DATA_DIR="$HOME/privatekonomi-data"
BACKUP_DIR="$HOME/privatekonomi-backups"
SERVICE_NAME="privatekonomi"

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

# Check if Privatekonomi is installed
check_installation() {
    log_section "Kontrollerar befintlig installation"
    
    if [ ! -d "$INSTALL_DIR" ]; then
        log_error "Privatekonomi är inte installerat i $INSTALL_DIR"
        echo ""
        echo "Kör först installationsskriptet:"
        echo "  curl -sSL https://raw.githubusercontent.com/pownas/Privatekonomi/main/raspberry-pi-install.sh | bash"
        exit 1
    fi
    
    if [ ! -f "$INSTALL_DIR/Privatekonomi.sln" ]; then
        log_error "Ogiltig installation i $INSTALL_DIR"
        exit 1
    fi
    
    log_success "Befintlig installation hittad i $INSTALL_DIR"
}

# Stop running services
stop_services() {
    log_section "Stoppar tjänster"
    
    # Check if systemd service exists
    if systemctl is-active --quiet "$SERVICE_NAME" 2>/dev/null; then
        log_info "Stoppar systemd-tjänst: $SERVICE_NAME"
        sudo systemctl stop "$SERVICE_NAME"
        log_success "Tjänst stoppad"
    else
        log_info "Ingen systemd-tjänst aktiv"
    fi
    
    # Kill any running dotnet processes
    if pgrep -f "Privatekonomi" > /dev/null; then
        log_warning "Hittade körande Privatekonomi-processer, stoppar dem..."
        pkill -f "Privatekonomi" || true
        sleep 2
        
        # Force kill if still running
        if pgrep -f "Privatekonomi" > /dev/null; then
            log_warning "Tvångsstoppar kvarvarande processer..."
            pkill -9 -f "Privatekonomi" || true
            sleep 1
        fi
        
        log_success "Processer stoppade"
    fi
}

# Create backup before update
create_backup() {
    log_section "Skapar backup innan uppdatering"
    
    mkdir -p "$BACKUP_DIR"
    
    local timestamp=$(date +%Y%m%d_%H%M%S)
    local backup_name="pre_update_$timestamp"
    
    # Backup SQLite database if exists
    if [ -f "$DATA_DIR/privatekonomi.db" ]; then
        log_info "Backup av SQLite-databas..."
        cp "$DATA_DIR/privatekonomi.db" "$BACKUP_DIR/${backup_name}.db"
        log_success "SQLite backup: $BACKUP_DIR/${backup_name}.db"
    fi
    
    # Backup JSON files if exist
    if [ -d "$DATA_DIR" ] && [ -n "$(find "$DATA_DIR" -maxdepth 1 -name '*.json' -print -quit 2>/dev/null)" ]; then
        log_info "Backup av JSON-filer..."
        tar -czf "$BACKUP_DIR/${backup_name}_json.tar.gz" -C "$DATA_DIR" . 2>/dev/null || true
        log_success "JSON backup: $BACKUP_DIR/${backup_name}_json.tar.gz"
    fi
    
    # Backup configuration files
    if [ -d "$INSTALL_DIR/src" ]; then
        log_info "Backup av konfigurationsfiler..."
        mkdir -p "$BACKUP_DIR/config_backup_$timestamp"
        
        find "$INSTALL_DIR/src" -name "appsettings.Production.json" -exec cp --parents {} "$BACKUP_DIR/config_backup_$timestamp/" \; 2>/dev/null || true
        
        log_success "Konfiguration backup: $BACKUP_DIR/config_backup_$timestamp/"
    fi
    
    log_success "Backup slutförd framgångsrikt"
}

# Update repository
update_repository() {
    log_section "Uppdaterar Privatekonomi från GitHub"
    
    cd "$INSTALL_DIR"
    
    # Check current branch
    local current_branch=$(git branch --show-current)
    log_info "Nuvarande branch: $current_branch"
    
    # Stash any local changes
    if ! git diff-index --quiet HEAD --; then
        log_warning "Lokala ändringar detekterade, sparar dem tillfälligt..."
        git stash push -m "Auto-stash before update $(date +%Y%m%d_%H%M%S)"
    fi
    
    # Fetch latest changes
    log_info "Hämtar senaste ändringar från GitHub..."
    git fetch origin
    
    # Get current and remote commit
    local local_commit=$(git rev-parse HEAD)
    local remote_commit=$(git rev-parse origin/$current_branch)
    
    if [ "$local_commit" = "$remote_commit" ]; then
        log_success "Du har redan den senaste versionen ($(git rev-parse --short HEAD))"
        
        read -p "Vill du fortsätta uppdateringen ändå? (y/n): " -n 1 -r
        echo
        if [[ ! $REPLY =~ ^[Yy]$ ]]; then
            log_info "Uppdatering avbruten"
            exit 0
        fi
    else
        log_info "Nya ändringar tillgängliga"
        log_info "  Nuvarande: $(git rev-parse --short $local_commit)"
        log_info "  Senaste: $(git rev-parse --short $remote_commit)"
        
        # Show what will be updated
        echo ""
        log_info "Ändringar som kommer att hämtas:"
        git log --oneline --decorate --color $local_commit..$remote_commit | head -10
        echo ""
        
        read -p "Vill du fortsätta med uppdateringen? (y/n): " -n 1 -r
        echo
        if [[ ! $REPLY =~ ^[Yy]$ ]]; then
            log_info "Uppdatering avbruten"
            exit 0
        fi
    fi
    
    # Pull latest changes
    log_info "Hämtar och applicerar ändringar..."
    git pull origin $current_branch
    
    log_success "Repository uppdaterat framgångsrikt"
}

# Restore packages and build
build_application() {
    log_section "Bygger uppdaterad applikation"
    
    cd "$INSTALL_DIR"
    
    # Ensure .NET is in PATH
    if [ -d "$HOME/.dotnet" ]; then
        export PATH="$PATH:$HOME/.dotnet"
        export DOTNET_ROOT="$HOME/.dotnet"
    fi
    
    # Restore NuGet packages
    log_info "Återställer NuGet-paket..."
    dotnet restore
    
    # Clean previous build
    log_info "Rensar tidigare byggen..."
    dotnet clean --configuration Release
    
    # Build solution
    log_info "Bygger lösningen..."
    dotnet build --configuration Release
    
    log_success "Applikation byggd framgångsrikt"
}

# Publish application (optional)
publish_application() {
    if [ "$SKIP_PUBLISH" = true ]; then
        log_info "Hoppar över publicering (--no-publish)"
        return 0
    fi
    
    log_section "Publicerar uppdaterad applikation"
    
    # Check if published directory exists
    if [ ! -d "$INSTALL_DIR/publish" ]; then
        log_info "Ingen publicerad version finns. Hoppar över publicering."
        log_info "Om du vill använda publicerade binärer, kör: ./raspberry-pi-install.sh"
        return 0
    fi
    
    read -p "Vill du publicera om applikationen för bättre prestanda? (y/n): " -n 1 -r
    echo
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        log_info "Hoppar över publicering"
        return 0
    fi
    
    cd "$INSTALL_DIR"
    
    local publish_dir="$INSTALL_DIR/publish"
    
    # Backup old published version
    if [ -d "$publish_dir" ]; then
        log_info "Backup av befintlig publicerad version..."
        local backup_publish="$publish_dir.backup.$(date +%Y%m%d_%H%M%S)"
        mv "$publish_dir" "$backup_publish"
        log_info "Gammal version sparad i: $backup_publish"
    fi
    
    log_info "Publicerar för linux-arm64 med self-contained..."
    
    # Publish AppHost
    log_info "Publicerar Privatekonomi.AppHost..."
    dotnet publish src/Privatekonomi.AppHost/Privatekonomi.AppHost.csproj \
        --runtime linux-arm64 \
        --self-contained \
        --configuration Release \
        -o "$publish_dir/AppHost" \
        /p:PublishTrimmed=false \
        /p:PublishSingleFile=false
    
    # Publish Web
    log_info "Publicerar Privatekonomi.Web..."
    dotnet publish src/Privatekonomi.Web/Privatekonomi.Web.csproj \
        --runtime linux-arm64 \
        --self-contained \
        --configuration Release \
        -o "$publish_dir/Web" \
        /p:PublishTrimmed=false \
        /p:PublishSingleFile=false
    
    # Publish API
    log_info "Publicerar Privatekonomi.Api..."
    dotnet publish src/Privatekonomi.Api/Privatekonomi.Api.csproj \
        --runtime linux-arm64 \
        --self-contained \
        --configuration Release \
        -o "$publish_dir/Api" \
        /p:PublishTrimmed=false \
        /p:PublishSingleFile=false
    
    # Copy configuration files
    log_info "Kopierar konfigurationsfiler..."
    
    if [ -f "src/Privatekonomi.AppHost/appsettings.Production.json" ]; then
        cp "src/Privatekonomi.AppHost/appsettings.Production.json" "$publish_dir/AppHost/"
    fi
    
    if [ -f "src/Privatekonomi.Web/appsettings.Production.json" ]; then
        cp "src/Privatekonomi.Web/appsettings.Production.json" "$publish_dir/Web/"
    fi
    
    if [ -f "src/Privatekonomi.Api/appsettings.Production.json" ]; then
        cp "src/Privatekonomi.Api/appsettings.Production.json" "$publish_dir/Api/"
    fi
    
    log_success "Applikation publicerad framgångsrikt"
}

# Update systemd service if needed
update_systemd_service() {
    log_section "Uppdaterar systemd-tjänst (om den finns)"
    
    if [ ! -f "/etc/systemd/system/$SERVICE_NAME.service" ]; then
        log_info "Ingen systemd-tjänst installerad"
        return 0
    fi
    
    log_info "Systemd-tjänst finns redan installerad"
    
    # Check if using published binaries
    local use_published=false
    if [ -d "$INSTALL_DIR/publish/AppHost" ] && [ -f "$INSTALL_DIR/publish/AppHost/Privatekonomi.AppHost" ]; then
        use_published=true
    fi
    
    # Read current service to check configuration
    local current_exec=$(grep "^ExecStart=" "/etc/systemd/system/$SERVICE_NAME.service" | head -1)
    
    if [ "$use_published" = true ] && [[ "$current_exec" == *"dotnet run"* ]]; then
        log_warning "Tjänsten använder 'dotnet run' men publicerade binärer finns"
        
        read -p "Vill du uppdatera tjänsten att använda publicerade binärer? (y/n): " -n 1 -r
        echo
        if [[ $REPLY =~ ^[Yy]$ ]]; then
            log_info "Uppdaterar systemd-tjänst..."
            
            local user=$(whoami)
            
            sudo tee "/etc/systemd/system/$SERVICE_NAME.service" > /dev/null << EOF
[Unit]
Description=Privatekonomi Personal Finance Application
After=network.target

[Service]
Type=notify
User=$user
Group=$user
WorkingDirectory=$INSTALL_DIR/publish/AppHost
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=PRIVATEKONOMI_ENVIRONMENT=RaspberryPi
Environment=PRIVATEKONOMI_STORAGE_PROVIDER=Sqlite
Environment=PRIVATEKONOMI_RASPBERRY_PI=true
Environment=DOTNET_DASHBOARD_URLS=http://0.0.0.0:17127
Environment=DOTNET_ROOT=$HOME/.dotnet
ExecStart=$INSTALL_DIR/publish/AppHost/Privatekonomi.AppHost
Restart=always
RestartSec=10
SyslogIdentifier=$SERVICE_NAME

[Install]
WantedBy=multi-user.target
EOF
            
            sudo systemctl daemon-reload
            log_success "Systemd-tjänst uppdaterad"
        fi
    fi
    
    log_success "Systemd-tjänst kontrollerad"
}

# Restart services
restart_services() {
    log_section "Startar om tjänster"
    
    # Check if systemd service exists
    if [ -f "/etc/systemd/system/$SERVICE_NAME.service" ]; then
        log_info "Startar systemd-tjänst: $SERVICE_NAME"
        sudo systemctl daemon-reload
        sudo systemctl start "$SERVICE_NAME"
        
        # Wait a moment for service to start
        sleep 3
        
        # Check service status
        if systemctl is-active --quiet "$SERVICE_NAME"; then
            log_success "Tjänst startad framgångsrikt"
        else
            log_error "Tjänsten kunde inte startas"
            log_info "Kontrollera status med: sudo systemctl status $SERVICE_NAME"
            log_info "Visa loggar med: journalctl -u $SERVICE_NAME -n 50"
            return 1
        fi
    else
        log_info "Ingen systemd-tjänst installerad"
        log_info "Du kan starta applikationen manuellt med:"
        echo "  cd $INSTALL_DIR"
        echo "  ./raspberry-pi-start.sh"
    fi
}

# Verify update
verify_update() {
    log_section "Verifierar uppdatering"
    
    cd "$INSTALL_DIR"
    
    # Check .NET
    if command -v dotnet &> /dev/null; then
        local dotnet_version=$(dotnet --version)
        log_success ".NET SDK: $dotnet_version"
    else
        log_error ".NET SDK inte funnet"
        return 1
    fi
    
    # Check current commit
    local current_commit=$(git rev-parse --short HEAD)
    log_success "Nuvarande version: $current_commit"
    
    # Check if service is running (if installed)
    if systemctl is-active --quiet "$SERVICE_NAME" 2>/dev/null; then
        log_success "Systemd-tjänst körs"
        
        # Check if ports are listening
        sleep 2
        if ss -lnt | grep -q ":17127 "; then
            log_success "Aspire Dashboard lyssnar på port 17127"
        else
            log_warning "Aspire Dashboard lyssnar inte på port 17127"
        fi
        
        if ss -lnt | grep -q ":5274 "; then
            log_success "Web App lyssnar på port 5274"
        else
            log_warning "Web App lyssnar inte på port 5274"
        fi
    else
        log_info "Tjänst körs inte (normalt om systemd-tjänst inte är installerad)"
    fi
    
    log_success "Verifiering slutförd"
}

# Show post-update information
show_post_update_info() {
    log_section "Uppdatering slutförd"
    
    local pi_ip=$(hostname -I | awk '{print $1}')
    local current_commit=$(git rev-parse --short HEAD 2>/dev/null || echo "okänd")
    
    echo -e ""
    echo -e "${GREEN}🎉 Privatekonomi har uppdaterats framgångsrikt!${NC}"
    echo -e ""
    echo -e "${BLUE}Version information:${NC}"
    echo -e "  Commit: $current_commit"
    echo -e "  Plats: $INSTALL_DIR"
    echo -e ""
    echo -e "${BLUE}Åtkomst till applikationen:${NC}"
    echo -e "  ${YELLOW}Lokalt:${NC}"
    echo -e "    http://localhost:17127 (Aspire Dashboard)"
    echo -e "    http://localhost:5274 (Web App)"
    echo -e ""
    echo -e "  ${YELLOW}Från andra enheter:${NC}"
    echo -e "    http://$pi_ip:17127 (Aspire Dashboard)"
    echo -e "    http://$pi_ip:5274 (Web App)"
    echo -e ""
    
    if systemctl is-active --quiet "$SERVICE_NAME" 2>/dev/null; then
        echo -e "${GREEN}✅ Tjänsten körs redan${NC}"
        echo -e ""
        echo -e "${BLUE}Användbara kommandon:${NC}"
        echo -e "  ${YELLOW}Visa status:${NC}    sudo systemctl status $SERVICE_NAME"
        echo -e "  ${YELLOW}Starta om:${NC}      sudo systemctl restart $SERVICE_NAME"
        echo -e "  ${YELLOW}Stoppa:${NC}         sudo systemctl stop $SERVICE_NAME"
        echo -e "  ${YELLOW}Visa loggar:${NC}    journalctl -u $SERVICE_NAME -f"
    else
        echo -e "${BLUE}Starta applikationen:${NC}"
        echo -e "  cd $INSTALL_DIR"
        echo -e "  ./raspberry-pi-start.sh"
        echo -e ""
        echo -e "  ${YELLOW}eller med systemd (om installerat):${NC}"
        echo -e "  sudo systemctl start $SERVICE_NAME"
    fi
    
    echo -e ""
    echo -e "${BLUE}Backup-information:${NC}"
    echo -e "  Backuper sparas i: $BACKUP_DIR"
    echo -e "  Senaste backup: $(ls -t $BACKUP_DIR | head -1 2>/dev/null || echo 'ingen')"
    echo -e ""
    echo -e "${GREEN}Allt klart! 💰${NC}"
}

# Main execution
main() {
    log_section "Privatekonomi Raspberry Pi Uppdatering"
    log_info "Startar uppdatering av befintlig installation..."
    
    check_installation
    stop_services
    create_backup
    update_repository
    build_application
    publish_application
    update_systemd_service
    restart_services
    verify_update
    show_post_update_info
    
    log_success "Uppdatering slutförd framgångsrikt!"
}

# Handle script arguments
SKIP_PUBLISH=false

case "${1:-}" in
    --help|-h)
        echo "Privatekonomi Raspberry Pi Update Script"
        echo ""
        echo "Användning: $0 [ALTERNATIV]"
        echo ""
        echo "Alternativ:"
        echo "  --help, -h        Visa denna hjälp"
        echo "  --no-publish      Hoppa över publicering (snabbare)"
        echo ""
        echo "Exempel:"
        echo "  $0                # Full uppdatering med publicering"
        echo "  $0 --no-publish   # Uppdatera utan att publicera om"
        echo ""
        exit 0
        ;;
    --no-publish)
        SKIP_PUBLISH=true
        ;;
esac

# Run main function
main "$@"
