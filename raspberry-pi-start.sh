#!/bin/bash

# Raspberry Pi Aspire Startup Script
# Kör Privatekonomi med Aspire på Raspberry Pi

echo "Startar Privatekonomi Aspire AppHost på Raspberry Pi..."

# Sätt miljövariabler för Raspberry Pi
export PRIVATEKONOMI_RASPBERRY_PI=true
export ASPNETCORE_ENVIRONMENT=Production
export PRIVATEKONOMI_ENVIRONMENT=RaspberryPi
export PRIVATEKONOMI_STORAGE_PROVIDER=Sqlite

# Alternativ 1: Använd miljövariabel för URL
export ASPNETCORE_URLS="http://0.0.0.0:17127"

# Navigera till AppHost-katalogen
cd "$(dirname "$0")/src/Privatekonomi.AppHost"

echo "Miljövariabler:"
echo "  PRIVATEKONOMI_RASPBERRY_PI: $PRIVATEKONOMI_RASPBERRY_PI"
echo "  ASPNETCORE_ENVIRONMENT: $ASPNETCORE_ENVIRONMENT"
echo "  PRIVATEKONOMI_ENVIRONMENT: $PRIVATEKONOMI_ENVIRONMENT"
echo "  PRIVATEKONOMI_STORAGE_PROVIDER: $PRIVATEKONOMI_STORAGE_PROVIDER"
echo "  ASPNETCORE_URLS: $ASPNETCORE_URLS"
echo ""

echo "Startar applikationen..."
echo "Aspire Dashboard kommer att vara tillgänglig på: http://[raspberry-pi-ip]:17127"
echo "Tryck Ctrl+C för att stoppa"
echo ""

# Starta applikationen
dotnet run --configuration Release