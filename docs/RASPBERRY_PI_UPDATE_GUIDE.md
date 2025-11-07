# Uppdateringsguide för Raspberry Pi

Denna guide beskriver hur du uppdaterar din befintliga Privatekonomi-installation på Raspberry Pi till den senaste versionen.

## 🚀 Snabbstart - Automatisk uppdatering

**Enklaste sättet:** Använd det automatiserade uppdateringsskriptet:

```bash
cd ~/Privatekonomi
./raspberry-pi-update.sh
```

Uppdateringsskriptet hanterar automatiskt:
- ✅ Stoppar körande tjänster
- ✅ Skapar backup av databas och konfiguration
- ✅ Hämtar senaste ändringar från GitHub
- ✅ Återställer NuGet-paket
- ✅ Bygger uppdaterad applikation
- ✅ Publicerar nya ARM64-binärer (valfritt)
- ✅ Uppdaterar systemd-tjänst om nödvändigt
- ✅ Startar om tjänster
- ✅ Verifierar att uppdateringen lyckades

### Kommandoradsalternativ

```bash
# Full uppdatering (rekommenderat)
./raspberry-pi-update.sh

# Uppdatera utan att publicera om (snabbare)
./raspberry-pi-update.sh --no-publish

# Visa hjälp
./raspberry-pi-update.sh --help
```

## 📋 Vad händer under uppdateringen?

### 1. Kontroll av installation
Skriptet verifierar att Privatekonomi är installerat i rätt katalog.

### 2. Stoppa tjänster
- Stoppar systemd-tjänst (om installerad)
- Avslutar eventuella körande Privatekonomi-processer

### 3. Backup
Automatisk backup skapas innan uppdatering:
- **SQLite-databas**: `~/privatekonomi-backups/pre_update_YYYYMMDD_HHMMSS.db`
- **JSON-filer**: `~/privatekonomi-backups/pre_update_YYYYMMDD_HHMMSS_json.tar.gz`
- **Konfigurationsfiler**: `~/privatekonomi-backups/config_backup_YYYYMMDD_HHMMSS/`

### 4. Uppdatera repository
- Hämtar senaste ändringar från GitHub
- Visar vad som har ändrats
- Frågar om bekräftelse innan uppdatering
- Sparar eventuella lokala ändringar automatiskt

### 5. Bygga applikation
- Återställer NuGet-paket
- Rensar tidigare byggen
- Bygger hela lösningen i Release-läge

### 6. Publicera (valfritt)
- Skapar backup av gammal publicerad version
- Publicerar nya ARM64-optimerade binärer
- Kopierar konfigurationsfiler

### 7. Uppdatera systemd-tjänst
- Kontrollerar om tjänsten behöver uppdateras
- Uppdaterar till publicerade binärer om tillgängligt

### 8. Starta om tjänster
- Laddar om systemd-konfiguration
- Startar applikationen
- Verifierar att tjänsten körs

### 9. Verifiering
- Kontrollerar .NET SDK
- Verifierar nuvarande version
- Kontrollerar att portar lyssnar

## 📦 Manuell uppdatering

Om du föredrar att uppdatera manuellt:

### 1. Stoppa applikationen

**Med systemd:**
```bash
sudo systemctl stop privatekonomi
```

**Manuellt startad:**
```bash
# Tryck Ctrl+C i terminalen där appen körs
# Eller:
pkill -f "Privatekonomi"
```

### 2. Backup av data (rekommenderat)

**SQLite:**
```bash
mkdir -p ~/privatekonomi-backups
cp ~/privatekonomi-data/privatekonomi.db ~/privatekonomi-backups/backup_$(date +%Y%m%d_%H%M%S).db
```

**JsonFile:**
```bash
mkdir -p ~/privatekonomi-backups
tar -czf ~/privatekonomi-backups/backup_$(date +%Y%m%d_%H%M%S).tar.gz -C ~/privatekonomi-data .
```

### 3. Uppdatera kod

```bash
cd ~/Privatekonomi

# Spara eventuella lokala ändringar
git stash

# Hämta senaste version
git pull origin main

# Återställ lokala ändringar om du har några
git stash pop  # Endast om du gjorde git stash
```

### 4. Bygga applikation

```bash
cd ~/Privatekonomi

# Återställ paket
dotnet restore

# Rensa gamla byggen
dotnet clean --configuration Release

# Bygg nya versionen
dotnet build --configuration Release
```

### 5. Publicera om (valfritt, för bättre prestanda)

```bash
cd ~/Privatekonomi

# Backup gammal publicerad version
mv publish publish.backup.$(date +%Y%m%d_%H%M%S)

# Publicera AppHost
dotnet publish src/Privatekonomi.AppHost/Privatekonomi.AppHost.csproj \
    --runtime linux-arm64 \
    --self-contained \
    --configuration Release \
    -o publish/AppHost

# Publicera Web
dotnet publish src/Privatekonomi.Web/Privatekonomi.Web.csproj \
    --runtime linux-arm64 \
    --self-contained \
    --configuration Release \
    -o publish/Web

# Publicera API
dotnet publish src/Privatekonomi.Api/Privatekonomi.Api.csproj \
    --runtime linux-arm64 \
    --self-contained \
    --configuration Release \
    -o publish/Api

# Kopiera konfigurationsfiler
cp src/Privatekonomi.AppHost/appsettings.Production.json publish/AppHost/
cp src/Privatekonomi.Web/appsettings.Production.json publish/Web/
cp src/Privatekonomi.Api/appsettings.Production.json publish/Api/
```

### 6. Starta applikationen

**Med systemd:**
```bash
sudo systemctl daemon-reload  # Om du ändrat service-filen
sudo systemctl start privatekonomi
sudo systemctl status privatekonomi
```

**Manuellt:**
```bash
cd ~/Privatekonomi
./raspberry-pi-start.sh
```

## 🔍 Kontrollera version

För att se vilken version du kör:

```bash
cd ~/Privatekonomi
git log -1 --oneline
git rev-parse --short HEAD
```

För att se vad som är nytt:

```bash
cd ~/Privatekonomi
git log --oneline --decorate -10
```

## 🆕 Uppdateringsfrekvens

### Rekommenderad frekvens
- **Säkerhetsuppdateringar**: Installera omedelbart
- **Nya funktioner**: Var 1-2 månad
- **Buggfixar**: Enligt behov

### Hålla dig informerad
1. Följ repository på GitHub: https://github.com/pownas/Privatekonomi
2. Prenumerera på "Releases" för notifikationer
3. Läs GitHub Releases för ändringshistorik

## ⚙️ Uppdatera konfiguration

Om nya konfigurationsalternativ har lagts till:

### 1. Kontrollera exempel-filer

```bash
cd ~/Privatekonomi
ls -la *.example.json
```

### 2. Jämför med din konfiguration

```bash
# För Web
diff src/Privatekonomi.Web/appsettings.Production.json appsettings.RaspberryPi.example.json

# För API
diff src/Privatekonomi.Api/appsettings.Production.json appsettings.RaspberryPi.example.json
```

### 3. Uppdatera om nödvändigt

Redigera dina `appsettings.Production.json`-filer med nya inställningar.

## 🔧 Felsökning

### Uppdateringen misslyckades

**Problem: Git-konflikt**
```bash
cd ~/Privatekonomi
git status  # Se vilka filer som har konflikt
git stash   # Spara dina ändringar
git pull origin main  # Försök igen
```

**Problem: Build-fel**
```bash
# Rensa allt och börja om
cd ~/Privatekonomi
dotnet clean
rm -rf bin/ obj/
dotnet restore
dotnet build --configuration Release
```

**Problem: Tjänsten startar inte**
```bash
# Kontrollera loggar
journalctl -u privatekonomi -n 50

# Kontrollera service-fil
sudo systemctl cat privatekonomi

# Försök starta manuellt för att se fel
cd ~/Privatekonomi/src/Privatekonomi.AppHost
ASPNETCORE_ENVIRONMENT=Production dotnet run
```

### Databas-migrering

Om uppdateringen inkluderar databasändringar:

```bash
cd ~/Privatekonomi/src/Privatekonomi.Core

# Kontrollera pending migrations
dotnet ef migrations list

# Applicera migrations
dotnet ef database update
```

### Portar lyssnar inte

```bash
# Kontrollera vilka portar som lyssnar
ss -lntp | grep '17127\|5274\|5277'

# Kontrollera om något annat använder portarna
sudo lsof -i :17127
sudo lsof -i :5274
sudo lsof -i :5277

# Starta om tjänsten
sudo systemctl restart privatekonomi
```

### Återställa från backup

**SQLite:**
```bash
# Stoppa applikationen
sudo systemctl stop privatekonomi

# Återställ databas
cp ~/privatekonomi-backups/backup_YYYYMMDD_HHMMSS.db ~/privatekonomi-data/privatekonomi.db

# Starta applikationen
sudo systemctl start privatekonomi
```

**JsonFile:**
```bash
# Stoppa applikationen
sudo systemctl stop privatekonomi

# Återställ JSON-filer
rm -rf ~/privatekonomi-data/*
tar -xzf ~/privatekonomi-backups/backup_YYYYMMDD_HHMMSS.tar.gz -C ~/privatekonomi-data

# Starta applikationen
sudo systemctl start privatekonomi
```

**Konfiguration:**
```bash
# Återställ konfigurationsfiler
cp -r ~/privatekonomi-backups/config_backup_YYYYMMDD_HHMMSS/home/*/Privatekonomi/src/*/appsettings.Production.json \
      ~/Privatekonomi/src/
```

## 🔄 Automatiska uppdateringar

### Schemalagd uppdatering med cron

**Varning:** Automatiska uppdateringar kan orsaka driftstopp. Använd med försiktighet.

```bash
# Öppna crontab
crontab -e

# Lägg till (uppdatera varje söndag kl 03:00)
0 3 * * 0 cd ~/Privatekonomi && ./raspberry-pi-update.sh --no-publish >> ~/privatekonomi-update.log 2>&1
```

**Rekommendation:** Använd `--no-publish` för automatiska uppdateringar för att minimera driftstopp.

### E-postnotifikationer vid uppdatering

```bash
# Installera mailutils om det inte finns
sudo apt install mailutils

# Modifiera cron-jobb
0 3 * * 0 cd ~/Privatekonomi && ./raspberry-pi-update.sh --no-publish 2>&1 | mail -s "Privatekonomi Update" din@email.com
```

## 📊 Efter uppdatering

### Verifiera att allt fungerar

1. **Öppna webbläsaren**
   ```
   http://[raspberry-pi-ip]:5274
   ```

2. **Kontrollera Dashboard**
   - Logga in
   - Verifiera att data finns kvar
   - Kontrollera nya funktioner

3. **Testa kritiska funktioner**
   - Skapa transaktion
   - Uppdatera budget
   - Exportera data

4. **Kontrollera prestanda**
   ```bash
   # Systemresurser
   htop
   
   # Loggar för fel
   journalctl -u privatekonomi -n 100 | grep -i "error\|exception"
   ```

### Rapportera problem

Om du stöter på problem efter uppdatering:

1. **Samla information**
   ```bash
   # Version
   cd ~/Privatekonomi && git rev-parse --short HEAD
   
   # Loggar
   journalctl -u privatekonomi -n 100 > ~/privatekonomi-logs.txt
   
   # Systeminfo
   uname -a > ~/system-info.txt
   dotnet --info >> ~/system-info.txt
   ```

2. **Skapa GitHub Issue**
   - Gå till: https://github.com/pownas/Privatekonomi/issues
   - Beskriv problemet
   - Bifoga loggfiler och systeminformation
   - Inkludera steg för att återskapa problemet

## 📚 Ytterligare resurser

- **Installation**: [RASPBERRY_PI_GUIDE.md](RASPBERRY_PI_GUIDE.md)
- **Första installation**: [RASPBERRY_PI_första_installationen.md](RASPBERRY_PI_första_installationen.md)
- **Nginx & SSL**: [RASPBERRY_PI_NGINX_SSL.md](RASPBERRY_PI_NGINX_SSL.md)
- **Testguide**: [RASPBERRY_PI_INSTALL_TEST.md](RASPBERRY_PI_INSTALL_TEST.md)
- **Backup & Återställning**: [STORAGE_GUIDE.md](STORAGE_GUIDE.md)

## ✅ Uppdateringschecklista

Använd denna checklista för varje uppdatering:

- [ ] Läs release notes/CHANGELOG
- [ ] Backup av databas skapad
- [ ] Backup av konfiguration skapad
- [ ] Stoppa tjänster
- [ ] Hämta senaste kod från GitHub
- [ ] Återställ NuGet-paket
- [ ] Bygg applikation
- [ ] Publicera om (valfritt)
- [ ] Applicera databas-migrationer (om nödvändigt)
- [ ] Uppdatera konfiguration (om nödvändigt)
- [ ] Starta tjänster
- [ ] Verifiera att applikationen fungerar
- [ ] Testa kritiska funktioner
- [ ] Kontrollera loggar för fel
- [ ] Dokumentera eventuella problem

## 🎉 Sammanfattning

Med det automatiska uppdateringsskriptet är det enkelt att hålla din Privatekonomi-installation uppdaterad:

```bash
cd ~/Privatekonomi
./raspberry-pi-update.sh
```

För manuell kontroll över processen, följ den manuella uppdateringsguiden ovan.

**Viktigt att komma ihåg:**
- ✅ Alltid skapa backup innan uppdatering
- ✅ Läs release notes för viktiga ändringar
- ✅ Testa applikationen efter uppdatering
- ✅ Håll systemet uppdaterat för säkerhet och nya funktioner

Lycka till med uppdateringen! 🚀
