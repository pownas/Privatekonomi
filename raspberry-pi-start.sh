#!/bin/bash

# Raspberry Pi Aspire Startup Script
# Kör Privatekonomi med Aspire på Raspberry Pi

echo "Startar Privatekonomi Aspire AppHost på Raspberry Pi..."

# Lägg till .NET i PATH om det inte finns där
if [ -d "$HOME/.dotnet" ] && ! command -v dotnet &> /dev/null; then
    export PATH="$PATH:$HOME/.dotnet"
    export DOTNET_ROOT="$HOME/.dotnet"
fi

# Verifiera att dotnet finns
if ! command -v dotnet &> /dev/null; then
    echo "❌ Fel: dotnet hittades inte i PATH"
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

# Navigera till AppHost-katalogen
cd "$(dirname "$0")/src/Privatekonomi.AppHost"

# Kontrollera att appsettings.Production.json finns
if [ ! -f "appsettings.Production.json" ]; then
    echo "⚠️  Varning: appsettings.Production.json saknas för AppHost"
    echo "Aspire Dashboard kommer endast att lyssna på localhost"
fi

if [ ! -f "../Privatekonomi.Web/appsettings.Production.json" ] || [ ! -f "../Privatekonomi.Api/appsettings.Production.json" ]; then
    echo "⚠️  Varning: appsettings.Production.json saknas för Web eller API"
    echo ""
    echo "Kör installationsskriptet igen för att skapa konfigurationsfiler:"
    echo "  ./raspberry-pi-install.sh"
    echo ""
fi

echo "Miljövariabler:"
echo "  PRIVATEKONOMI_RASPBERRY_PI: $PRIVATEKONOMI_RASPBERRY_PI"
echo "  ASPNETCORE_ENVIRONMENT: $ASPNETCORE_ENVIRONMENT"
echo "  PRIVATEKONOMI_ENVIRONMENT: $PRIVATEKONOMI_ENVIRONMENT"
echo "  PRIVATEKONOMI_STORAGE_PROVIDER: $PRIVATEKONOMI_STORAGE_PROVIDER"
echo "  ASPNETCORE_URLS: $ASPNETCORE_URLS"
echo "  DOTNET_DASHBOARD_URLS: $DOTNET_DASHBOARD_URLS"
echo ""

echo "Startar applikationen..."
echo "Använder .NET version: $(dotnet --version)"
echo "Aspire Dashboard kommer att vara tillgänglig på: http://[raspberry-pi-ip]:17127"
echo "Tryck Ctrl+C för att stoppa"
echo ""

# Starta applikationen
dotnet run --configuration Release