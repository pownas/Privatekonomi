#!/bin/bash

# Raspberry Pi Felsökningsskript för Nätverksåtkomst
# Kör detta på din Raspberry Pi för att diagnostisera anslutningsproblem

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

echo -e "${BLUE}╔════════════════════════════════════════════════════════╗${NC}"
echo -e "${BLUE}║     Privatekonomi Raspberry Pi Felsökning             ║${NC}"
echo -e "${BLUE}╚════════════════════════════════════════════════════════╝${NC}"
echo ""

# 1. Kontrollera IP-adresser
echo -e "${BLUE}[1/8]${NC} Kontrollerar IP-adresser..."
echo "Din Raspberry Pi IP-adress(er):"
hostname -I
MY_IP=$(hostname -I | awk '{print $1}')
echo -e "${GREEN}Primär IP: $MY_IP${NC}"
echo ""

# 2. Kontrollera om tjänster körs
echo -e "${BLUE}[2/8]${NC} Kontrollerar om .NET-tjänster körs..."
if pgrep -f "dotnet" > /dev/null; then
    echo -e "${GREEN}✓ .NET-processer körs${NC}"
    ps aux | grep -E "[d]otnet" | head -5
else
    echo -e "${RED}✗ Inga .NET-processer hittades${NC}"
    echo -e "${YELLOW}Starta applikationen med: ./raspberry-pi-start.sh${NC}"
fi
echo ""

# 3. Kontrollera vilka portar som lyssnar
echo -e "${BLUE}[3/8]${NC} Kontrollerar vilka portar som lyssnar..."
echo "Portar som applikationen ska använda: 17127, 5274, 5277"
echo ""

# Kontrollera varje port
for port in 17127 5274 5277; do
    if ss -lntp 2>/dev/null | grep -q ":$port "; then
        BIND_ADDR=$(ss -lntp 2>/dev/null | grep ":$port " | awk '{print $4}' | head -1)
        if echo "$BIND_ADDR" | grep -q "0.0.0.0:$port\|*:$port"; then
            echo -e "${GREEN}✓ Port $port lyssnar på ALLA nätverksinterfaces ($BIND_ADDR)${NC}"
        elif echo "$BIND_ADDR" | grep -q "127.0.0.1:$port\|localhost:$port"; then
            echo -e "${RED}✗ Port $port lyssnar ENDAST på localhost ($BIND_ADDR)${NC}"
            echo -e "${YELLOW}  Detta är problemet! Porten är inte tillgänglig från nätverket.${NC}"
        else
            echo -e "${YELLOW}⚠ Port $port lyssnar på: $BIND_ADDR${NC}"
        fi
    else
        echo -e "${RED}✗ Port $port lyssnar INTE${NC}"
    fi
done
echo ""

# 4. Kontrollera miljövariabler
echo -e "${BLUE}[4/8]${NC} Kontrollerar miljövariabler..."
if pgrep -f "dotnet" > /dev/null; then
    PID=$(pgrep -f "Privatekonomi.AppHost" | head -1)
    if [ -n "$PID" ]; then
        echo "Miljövariabler för process $PID:"
        cat /proc/$PID/environ 2>/dev/null | tr '\0' '\n' | grep -E "PRIVATEKONOMI|ASPNETCORE|DOTNET_DASHBOARD" || echo "Kunde inte läsa miljövariabler"
    fi
else
    echo -e "${YELLOW}Ingen process körs, kan inte kontrollera miljövariabler${NC}"
fi
echo ""

# 5. Kontrollera brandvägg (UFW)
echo -e "${BLUE}[5/8]${NC} Kontrollerar brandvägg (UFW)..."
if command -v ufw &> /dev/null; then
    if sudo ufw status 2>/dev/null | grep -q "Status: active"; then
        echo -e "${YELLOW}⚠ UFW brandvägg är aktiverad${NC}"
        echo "Brandväggsregler:"
        sudo ufw status numbered | grep -E "17127|5274|5277|ALLOW"
        
        # Kontrollera om våra portar är öppna
        for port in 17127 5274 5277; do
            if sudo ufw status | grep -q "$port"; then
                echo -e "${GREEN}✓ Port $port är öppen i brandväggen${NC}"
            else
                echo -e "${RED}✗ Port $port är INTE öppen i brandväggen${NC}"
                echo -e "${YELLOW}  Lägg till med: sudo ufw allow $port/tcp${NC}"
            fi
        done
    else
        echo -e "${GREEN}✓ UFW brandvägg är inaktiverad${NC}"
    fi
else
    echo -e "${GREEN}✓ UFW är inte installerat${NC}"
fi
echo ""

# 6. Kontrollera appsettings.Production.json
echo -e "${BLUE}[6/8]${NC} Kontrollerar konfigurationsfiler..."

# Kontrollera Web appsettings
WEB_CONFIG="$HOME/Privatekonomi/src/Privatekonomi.Web/appsettings.Production.json"
if [ -f "$WEB_CONFIG" ]; then
    echo "Web appsettings.Production.json:"
    grep -A1 '"Urls"' "$WEB_CONFIG" || echo "Ingen Urls-konfiguration hittades"
else
    echo -e "${RED}✗ $WEB_CONFIG finns inte${NC}"
fi
echo ""

# Kontrollera API appsettings
API_CONFIG="$HOME/Privatekonomi/src/Privatekonomi.Api/appsettings.Production.json"
if [ -f "$API_CONFIG" ]; then
    echo "API appsettings.Production.json:"
    grep -A1 '"Urls"' "$API_CONFIG" || echo "Ingen Urls-konfiguration hittades"
else
    echo -e "${RED}✗ $API_CONFIG finns inte${NC}"
fi
echo ""

# 7. Testa lokal åtkomst
echo -e "${BLUE}[7/8]${NC} Testar lokal åtkomst..."
for port in 17127 5274 5277; do
    if curl -s -o /dev/null -w "%{http_code}" --connect-timeout 2 "http://localhost:$port" > /dev/null 2>&1; then
        echo -e "${GREEN}✓ Port $port svarar lokalt${NC}"
    else
        echo -e "${RED}✗ Port $port svarar INTE lokalt${NC}"
    fi
done
echo ""

# 8. Testa nätverksåtkomst från Pi själv
echo -e "${BLUE}[8/8]${NC} Testar nätverksåtkomst (från Pi till sig själv)..."
for port in 17127 5274 5277; do
    if curl -s -o /dev/null -w "%{http_code}" --connect-timeout 2 "http://$MY_IP:$port" > /dev/null 2>&1; then
        echo -e "${GREEN}✓ Port $port är nåbar via nätverks-IP ($MY_IP:$port)${NC}"
    else
        echo -e "${RED}✗ Port $port är INTE nåbar via nätverks-IP ($MY_IP:$port)${NC}"
    fi
done
echo ""

# Sammanfattning och rekommendationer
echo -e "${BLUE}╔════════════════════════════════════════════════════════╗${NC}"
echo -e "${BLUE}║     Sammanfattning och Rekommendationer              ║${NC}"
echo -e "${BLUE}╚════════════════════════════════════════════════════════╝${NC}"
echo ""

echo -e "${YELLOW}Vanliga problem och lösningar:${NC}"
echo ""
echo "1. Om tjänster inte körs:"
echo "   ./raspberry-pi-start.sh"
echo ""
echo "2. Om portar lyssnar på 127.0.0.1 istället för 0.0.0.0:"
echo "   - Kontrollera att PRIVATEKONOMI_RASPBERRY_PI=true är satt"
echo "   - Kör om installationen: ./raspberry-pi-install.sh"
echo ""
echo "3. Om brandväggen blockerar:"
echo "   sudo ufw allow 17127/tcp"
echo "   sudo ufw allow 5274/tcp"
echo "   sudo ufw allow 5277/tcp"
echo "   sudo ufw reload"
echo ""
echo "4. Om appsettings.Production.json saknas eller är fel:"
echo "   Kör om installationen: ./raspberry-pi-install.sh"
echo ""
echo "5. Testa åtkomst från annan enhet:"
echo "   http://$MY_IP:17127  (Aspire Dashboard)"
echo "   http://$MY_IP:5274   (Web App)"
echo "   http://$MY_IP:5277   (API)"
echo ""
echo -e "${BLUE}För mer information, se:${NC}"
echo "  docs/RASPBERRY_PI_första_installationen.md"
echo ""
