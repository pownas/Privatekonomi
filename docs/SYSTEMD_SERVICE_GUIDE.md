# Systemd Service Setup för Privatekonomi

Guide för att konfigurera Privatekonomi som en systemd-service på Linux-servern, vilket möjliggör automatisk start vid systemomstart och enkel hantering av applikationen.

## Fördelar med Systemd Service

- ✅ Automatisk start vid server-omstart
- ✅ Automatisk omstart vid applikationskrasch
- ✅ Enkel hantering med `systemctl`-kommandon
- ✅ Centraliserad loggning via `journalctl`
- ✅ Säker körning som icke-root användare

## Installation

### Steg 1: Skapa Service-fil

Skapa en ny systemd service-fil:

```bash
sudo nano /etc/systemd/system/privatekonomi.service
```

Lägg in följande innehåll (justera sökvägar efter din miljö):

```ini
[Unit]
Description=Privatekonomi - Personal Finance Application
After=network.target
Documentation=https://github.com/pownas/Privatekonomi

[Service]
Type=notify
# Användare som kör applikationen (byt till din användare)
User=www-data
Group=www-data

# Arbetskatalog där applikationen ligger
WorkingDirectory=/var/www/privatekonomi

# Kommando för att starta applikationen
ExecStart=/var/www/privatekonomi/Privatekonomi.Web

# Environment variabler
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false
Environment=ASPNETCORE_URLS=http://0.0.0.0:5000

# Starta om automatiskt vid fel
Restart=always
RestartSec=10

# Timeout-inställningar
TimeoutStopSec=30

# Säkerhetsinställningar
NoNewPrivileges=true
PrivateTmp=true

# Logging
SyslogIdentifier=privatekonomi
StandardOutput=journal
StandardError=journal

[Install]
WantedBy=multi-user.target
```

### Steg 2: Justera Konfiguration

#### För HTTPS med specifik port:
```ini
Environment=ASPNETCORE_URLS=https://0.0.0.0:5001;http://0.0.0.0:5000
```

#### Med SSL-certifikat:
```ini
Environment=ASPNETCORE_Kestrel__Certificates__Default__Path=/etc/ssl/certs/privatekonomi.pfx
Environment=ASPNETCORE_Kestrel__Certificates__Default__Password=your_certificate_password
```

#### Annan SQLite-databas sökväg:
```ini
Environment=ConnectionStrings__DefaultConnection=Data Source=/opt/privatekonomi/data/privatekonomi.db
```

### Steg 3: Sätt Behörigheter

Säkerställ att användaren har rätt behörigheter:

```bash
# Skapa användare om den inte finns
sudo useradd -r -s /bin/false www-data

# Sätt ägare på applikationskatalogen
sudo chown -R www-data:www-data /var/www/privatekonomi

# Sätt körbehörighet på executables
sudo chmod +x /var/www/privatekonomi/Privatekonomi.Web

# Skapa data-katalog för SQLite (om behövs)
sudo mkdir -p /var/www/privatekonomi/data
sudo chown -R www-data:www-data /var/www/privatekonomi/data
```

### Steg 4: Aktivera och Starta Servicen

```bash
# Ladda om systemd efter ändringar
sudo systemctl daemon-reload

# Aktivera service (startar automatiskt vid boot)
sudo systemctl enable privatekonomi

# Starta servicen
sudo systemctl start privatekonomi

# Kontrollera status
sudo systemctl status privatekonomi
```

## Hantera Servicen

### Status och Information

```bash
# Visa status
sudo systemctl status privatekonomi

# Visa om servicen är aktiv
sudo systemctl is-active privatekonomi

# Visa om servicen är enabled (startar vid boot)
sudo systemctl is-enabled privatekonomi
```

### Starta, Stoppa och Starta om

```bash
# Starta servicen
sudo systemctl start privatekonomi

# Stoppa servicen
sudo systemctl stop privatekonomi

# Starta om servicen
sudo systemctl restart privatekonomi

# Ladda om konfiguration utan att stoppa
sudo systemctl reload privatekonomi
```

### Aktivera/Inaktivera Auto-start

```bash
# Aktivera auto-start vid boot
sudo systemctl enable privatekonomi

# Inaktivera auto-start
sudo systemctl disable privatekonomi
```

## Logghantering

### Visa Loggar

```bash
# Visa senaste loggarna
sudo journalctl -u privatekonomi -n 50

# Följ loggar i realtid
sudo journalctl -u privatekonomi -f

# Visa loggar från specifik tid
sudo journalctl -u privatekonomi --since "2025-01-09 14:00"

# Visa loggar mellan tidsperioder
sudo journalctl -u privatekonomi --since "2025-01-09" --until "2025-01-10"

# Visa endast fel
sudo journalctl -u privatekonomi -p err

# Exportera loggar till fil
sudo journalctl -u privatekonomi --since today > /tmp/privatekonomi-logs.txt
```

### Logg-nivåer

- `emerg`: Emergency (0)
- `alert`: Alert (1)
- `crit`: Critical (2)
- `err`: Error (3)
- `warning`: Warning (4)
- `notice`: Notice (5)
- `info`: Info (6)
- `debug`: Debug (7)

```bash
# Visa bara kritiska fel
sudo journalctl -u privatekonomi -p crit

# Visa warnings och högre
sudo journalctl -u privatekonomi -p warning
```

## Avancerad Konfiguration

### Extra Säkerhetsinställningar

Lägg till i `[Service]`-sektionen för ökat säkerhet:

```ini
# Begränsa filsystem-åtkomst
ProtectSystem=strict
ProtectHome=true
ReadWritePaths=/var/www/privatekonomi/data

# Nätverksbegränsningar
PrivateNetwork=false
RestrictAddressFamilies=AF_INET AF_INET6

# System call filtering
SystemCallFilter=@system-service
SystemCallErrorNumber=EPERM

# Begränsa resurser
MemoryLimit=512M
CPUQuota=50%
TasksMax=100

# Capability restrictions
CapabilityBoundingSet=
AmbientCapabilities=
```

### Resource Limits

```ini
[Service]
# Minnesgräns
MemoryMax=1G
MemoryHigh=800M

# CPU-begränsning (50% av en core)
CPUQuota=50%

# Antal trådar/processer
TasksMax=50

# Filstorlek-begränsningar
LimitNOFILE=4096
```

### Miljövariabler från Fil

Skapa en miljövariabel-fil:

```bash
sudo nano /etc/privatekonomi/environment
```

Innehåll:
```
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://0.0.0.0:5000
ConnectionStrings__DefaultConnection=Data Source=/var/www/privatekonomi/data/privatekonomi.db
```

Referera till den i service-filen:
```ini
[Service]
EnvironmentFile=/etc/privatekonomi/environment
```

## Felsökning

### Servicen Startar Inte

1. **Kontrollera status och loggar:**
   ```bash
   sudo systemctl status privatekonomi
   sudo journalctl -u privatekonomi -n 100 --no-pager
   ```

2. **Verifiera filbehörigheter:**
   ```bash
   ls -la /var/www/privatekonomi/
   ls -la /var/www/privatekonomi/Privatekonomi.Web
   ```

3. **Testa starta manuellt:**
   ```bash
   cd /var/www/privatekonomi/
   sudo -u www-data ./Privatekonomi.Web
   ```

4. **Kontrollera systemd-syntax:**
   ```bash
   sudo systemd-analyze verify privatekonomi.service
   ```

### Port Redan Används

```bash
# Hitta vad som använder port 5000
sudo lsof -i :5000
sudo netstat -tlnp | grep 5000

# Döda processen (om nödvändigt)
sudo kill -9 <PID>
```

### Behörighetsproblem

```bash
# Rätta ägare
sudo chown -R www-data:www-data /var/www/privatekonomi

# Rätta behörigheter
sudo chmod -R 755 /var/www/privatekonomi
sudo chmod +x /var/www/privatekonomi/Privatekonomi.Web

# För databas-filen
sudo chmod 644 /var/www/privatekonomi/data/privatekonomi.db
```

### .NET Runtime Saknas

```bash
# Kontrollera installerade runtimes
dotnet --list-runtimes

# Installera .NET 9 Runtime (Ubuntu/Debian)
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt-get update
sudo apt-get install -y aspnetcore-runtime-9.0
```

## Nginx Reverse Proxy (Valfritt)

För att köra Privatekonomi bakom Nginx:

### Nginx Konfiguration

```bash
sudo nano /etc/nginx/sites-available/privatekonomi
```

```nginx
server {
    listen 80;
    server_name privatekonomi.example.com;

    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

Aktivera konfigurationen:
```bash
sudo ln -s /etc/nginx/sites-available/privatekonomi /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl restart nginx
```

### SSL med Let's Encrypt

```bash
# Installera certbot
sudo apt-get install certbot python3-certbot-nginx

# Skaffa certifikat
sudo certbot --nginx -d privatekonomi.example.com

# Auto-renewal är aktiverad som standard
sudo certbot renew --dry-run
```

## Automatisk Update-script

Skapa ett script för att uppdatera applikationen:

```bash
sudo nano /usr/local/bin/update-privatekonomi.sh
```

```bash
#!/bin/bash
set -e

BACKUP_DIR="/var/backups/privatekonomi"
APP_DIR="/var/www/privatekonomi"
DATE=$(date +%Y%m%d-%H%M%S)

echo "=== Privatekonomi Update Script ==="
echo "Started: $(date)"

# Skapa backup
echo "Creating backup..."
mkdir -p "$BACKUP_DIR"
sudo -u www-data cp "$APP_DIR/privatekonomi.db" "$BACKUP_DIR/privatekonomi-$DATE.db"

# Stoppa service
echo "Stopping service..."
sudo systemctl stop privatekonomi

# Ladda ner nya filer (från GitHub Release eller SFTP)
echo "Downloading new version..."
# wget eller lftp kommando här

# Extrahera och ersätt filer
# tar -xzf privatekonomi-new.tar.gz -C $APP_DIR

# Rätta behörigheter
echo "Setting permissions..."
sudo chown -R www-data:www-data "$APP_DIR"
sudo chmod +x "$APP_DIR/Privatekonomi.Web"

# Starta service
echo "Starting service..."
sudo systemctl start privatekonomi

# Vänta och kontrollera status
sleep 5
sudo systemctl status privatekonomi

echo "Update completed: $(date)"
```

Gör scriptet körbart:
```bash
sudo chmod +x /usr/local/bin/update-privatekonomi.sh
```

## Monitorering

### Health Check Script

```bash
#!/bin/bash
# /usr/local/bin/privatekonomi-health-check.sh

URL="http://localhost:5000"
RESPONSE=$(curl -s -o /dev/null -w "%{http_code}" $URL)

if [ $RESPONSE -eq 200 ]; then
    echo "OK: Privatekonomi is running"
    exit 0
else
    echo "ERROR: Privatekonomi is not responding (HTTP $RESPONSE)"
    exit 1
fi
```

### Cron Job för Health Monitoring

```bash
# Lägg till i crontab
crontab -e

# Kontrollera varje 5:e minut
*/5 * * * * /usr/local/bin/privatekonomi-health-check.sh || systemctl restart privatekonomi
```

## Prestanda-tuning

### För Produktionsmiljö

```ini
[Service]
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=COMPlus_EnableDiagnostics=0
Environment=DOTNET_EnableEventLog=0
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false

# Garbage collection optimization
Environment=COMPlus_gcServer=1
Environment=COMPlus_GCHeapCount=2
```

## Säkerhetsrekommendationer

1. **Kör aldrig som root:**
   ```ini
   User=www-data
   Group=www-data
   ```

2. **Begränsa filsystemåtkomst:**
   ```ini
   ProtectSystem=strict
   ProtectHome=true
   ReadWritePaths=/var/www/privatekonomi/data
   ```

3. **Använd HTTPS:**
   - Konfigurera SSL-certifikat
   - Använd Nginx/Apache som reverse proxy

4. **Regelbundna backups:**
   - Automatisera med cron
   - Testa återställning regelbundet

5. **Uppdatera regelbundet:**
   - .NET Runtime
   - Systempaket
   - Applikationen själv

## Referens

- [systemd Documentation](https://www.freedesktop.org/software/systemd/man/)
- [ASP.NET Core Hosting](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/)
- [Nginx Configuration](https://nginx.org/en/docs/)

---

**Version:** 1.0.0  
**Senast uppdaterad:** 2025-11-09
