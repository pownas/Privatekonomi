# Implementeringssammanfattning: Raspberry Pi Production Deployment

## Översikt

Denna implementation lägger till professionella produktionsfunktioner för Raspberry Pi-installation, baserat på moderna best practices för .NET-hosting på Linux-servrar.

## Datum
2025-11-06

## Implementerade funktioner

### 1. Self-Contained Publish för ARM64

**Funktion:** `publish_application()`

**Vad den gör:**
- Publicerar alla tre projekt (AppHost, Web, Api) med `--runtime linux-arm64 --self-contained`
- Skapar optimerade binärer som inkluderar alla .NET-beroenden
- Kopierar appsettings.Production.json till publicerade kataloger
- Sparar publicerade filer i `~/Privatekonomi/publish/`

**Fördelar:**
- ✅ Snabbare uppstart (ingen JIT-kompilering)
- ✅ Lägre minnesanvändning
- ✅ Ingen .NET runtime-installation krävs vid runtime
- ✅ Optimerad för ARM64-arkitektur (Raspberry Pi 3/4/5)
- ✅ Enklare deployment (alla beroenden inkluderade)

**Kommando:**
```bash
./raspberry-pi-install.sh --no-publish  # Hoppa över publicering (använd dotnet run)
```

### 2. Nginx Reverse Proxy

**Funktion:** `configure_nginx()`

**Vad den gör:**
- Installerar Nginx
- Skapar konfigurationsfil i `/etc/nginx/sites-available/privatekonomi`
- Konfigurerar reverse proxy för:
  - Web App (/) → localhost:5274
  - API (/api/) → localhost:5277
  - Aspire Dashboard (/aspire/) → localhost:17127
  - Health check (/health)
- Aktiverar sajt med symbolisk länk
- Startar och aktiverar Nginx-tjänsten
- Öppnar brandväggsportar 80/443

**Fördelar:**
- ✅ Enkel åtkomst via http://[domain-or-ip]/ (ingen portspecifikation)
- ✅ Centraliserad säkerhetshantering
- ✅ HTTP/2-stöd
- ✅ Säkerhetsheaders (X-Frame-Options, CSP, etc.)
- ✅ Blazor SignalR-optimering
- ✅ Kan kombineras med SSL/HTTPS

**Kommando:**
```bash
./raspberry-pi-install.sh --no-nginx  # Hoppa över Nginx-installation
```

### 3. SSL/HTTPS-konfiguration

**Funktioner:** `configure_ssl()`, `configure_letsencrypt()`, `configure_selfsigned()`

**Alternativ 1: Let's Encrypt (Produktionsmiljö)**
- Installerar Certbot
- Hämtar gratis SSL-certifikat
- Konfigurerar automatisk förnyelse via systemd timer
- Uppdaterar Nginx-konfiguration med SSL-inställningar
- Aktiverar HTTPS-redirect från HTTP

**Krav:**
- Registrerat domännamn
- DNS A-record pekar på Raspberry Pi IP
- Port 80 och 443 öppna i router/brandvägg

**Alternativ 2: Self-Signed Certificate (Lokal användning)**
- Genererar OpenSSL self-signed certifikat
- Giltigt i 365 dagar
- Konfigurerar Nginx för HTTPS
- Aktiverar HSTS och starka SSL-protokoll
- Perfekt för privat hemmanätverk

**Kommando:**
```bash
./raspberry-pi-install.sh --no-ssl            # Hoppa över SSL
./raspberry-pi-install.sh --configure-ssl     # Konfigurera endast SSL
```

### 4. Uppdaterad systemd-tjänst

**Förbättring:** Automatisk detektion av publicerade binärer

**Logik:**
- Om `~/Privatekonomi/publish/AppHost/Privatekonomi.AppHost` finns:
  - Använd publicerade binärer direkt (snabbast)
- Annars:
  - Använd `dotnet run --configuration Release` (utveckling)

**Fördelar:**
- ✅ Flexibel mellan development och production
- ✅ Automatisk optimering baserat på publiceringsmetod
- ✅ Samma tjänst för båda scenarierna

### 5. Brandvägg med HTTP/HTTPS-stöd

**Förbättring:** Automatisk detektion och öppning av HTTP/HTTPS-portar

**Logik:**
- Öppnar alltid Aspire (17127), Web (5274), API (5277) som tidigare
- Om Nginx är installerat och konfigurerat:
  - Öppnar också port 80 (HTTP)
  - Öppnar också port 443 (HTTPS)

### 6. Nya kommandoradsalternativ

**Tillagda flaggor:**
- `--no-publish`: Hoppa över publicering (använd dotnet run)
- `--no-nginx`: Hoppa över Nginx-installation
- `--no-ssl`: Hoppa över SSL-konfiguration
- `--configure-ssl`: Kör endast SSL-konfiguration (för befintlig installation)

**Uppdaterad `--skip-interactive`:**
```bash
SKIP_PUBLISH=false   # Publicera i non-interactive mode
SKIP_NGINX=false     # Installera Nginx i non-interactive mode
SKIP_SSL=true        # Hoppa över SSL (kräver manuell input)
```

## Nya filer

### 1. `/workspaces/Privatekonomi/docs/RASPBERRY_PI_NGINX_SSL.md`

**Innehåll:**
- Komplett guide för Nginx och SSL
- Steg-för-steg-instruktioner för Let's Encrypt
- Steg-för-steg-instruktioner för self-signed certifikat
- Felsökningsguide
- Säkerhetsbästa praxis
- Prestandaoptimering
- Exempel på Nginx-konfigurationer

## Uppdaterade filer

### 1. `/workspaces/Privatekonomi/raspberry-pi-install.sh`

**Ändringar:**
- Nya funktioner: `publish_application()`, `configure_nginx()`, `configure_ssl()`, `configure_letsencrypt()`, `configure_selfsigned()`
- Uppdaterad `configure_firewall()`: HTTP/HTTPS-portar
- Uppdaterad `create_systemd_service()`: Automatisk detektion av publicerade binärer
- Uppdaterad `main()`: Anropar nya funktioner i rätt ordning
- Nya globala variabler: `SKIP_PUBLISH`, `SKIP_NGINX`, `SKIP_SSL`
- Uppdaterad help-text och case-statement för nya flaggor

**Funktionsordning i main():**
```bash
check_raspberry_pi
check_system_requirements
create_nuget_config
install_dotnet_9
setup_project
publish_application          # NYTT
configure_storage
install_ef_tools
configure_dev_certs
optimize_swap
configure_nginx              # NYTT
configure_ssl                # NYTT
configure_firewall
create_systemd_service
setup_backup
configure_static_ip
verify_installation
show_usage_info
```

### 2. `/workspaces/Privatekonomi/docs/RASPBERRY_PI_första_installationen.md`

**Ändringar:**
- Uppdaterad checklista med nya funktioner (publish, Nginx, SSL)
- Nya tabeller för åtkomst med/utan Nginx
- Förklaring av SSL-alternativ (Let's Encrypt vs Self-Signed)
- Information om publicerade binärer
- Nya kommandoradsexempel
- Kontrollkommandon för Nginx och SSL

### 3. `/workspaces/Privatekonomi/README.md`

**Ändringar:**
- Uppdaterad "Raspberry Pi Installation (Automatisk)"-sektion
- Nya checklistor med publish, Nginx, och SSL
- Nya kommandoradsexempel
- Länk till nya RASPBERRY_PI_NGINX_SSL.md

## Användningsexempel

### Scenario 1: Full produktion med Let's Encrypt

```bash
# Interaktiv installation
./raspberry-pi-install.sh

# Under installation:
# - Välj SQLite storage
# - Installera Nginx: y
# - Konfigurera SSL: y
# - Välj Let's Encrypt (1)
# - Ange domännamn: privatekonomi.example.com
# - Ange e-post: user@example.com

# Resultat:
# - Publicerade ARM64-binärer
# - Nginx reverse proxy
# - HTTPS med Let's Encrypt
# - Automatisk certifikatförnyelse
# - Systemd-tjänst
# - Åtkomst: https://privatekonomi.example.com
```

### Scenario 2: Lokal utveckling med self-signed

```bash
./raspberry-pi-install.sh

# Under installation:
# - Välj JsonFile storage
# - Installera Nginx: y
# - Konfigurera SSL: y
# - Välj Self-Signed (2)

# Resultat:
# - Publicerade ARM64-binärer
# - Nginx reverse proxy
# - HTTPS med self-signed certifikat (acceptera i webbläsare)
# - Åtkomst: https://192.168.1.100
```

### Scenario 3: Snabb utveckling utan extras

```bash
./raspberry-pi-install.sh --no-publish --no-nginx --no-ssl

# Resultat:
# - Bygger med "dotnet build" (inte publish)
# - Ingen Nginx
# - Ingen SSL
# - Direktåtkomst via portar
# - Åtkomst: http://192.168.1.100:5274
```

### Scenario 4: Lägg till SSL till befintlig installation

```bash
# Om du redan kört installationen utan SSL
./raspberry-pi-install.sh --configure-ssl

# Välj Let's Encrypt eller Self-Signed
```

### Scenario 5: Automatisk CI/CD-installation

```bash
./raspberry-pi-install.sh --skip-interactive

# Resultat:
# - Publicerade ARM64-binärer
# - Nginx installerat
# - Systemd-tjänst
# - Automatiska backuper
# - Ingen SSL (kräver manuell input)
# - Ingen statisk IP
# - Ingen swap-optimering
```

## Tekniska detaljer

### Nginx-konfiguration

**Location mappings:**
- `/` → `http://localhost:5274` (Blazor Web App)
- `/api/` → `http://localhost:5277/` (ASP.NET Core API)
- `/aspire/` → `http://localhost:17127/` (Aspire Dashboard)
- `/health` → Static response "healthy"

**Säkerhetsheaders:**
- X-Frame-Options: SAMEORIGIN
- X-Content-Type-Options: nosniff
- X-XSS-Protection: 1; mode=block
- Strict-Transport-Security (med SSL)

**Blazor SignalR-optimering:**
- `proxy_buffering off`
- `proxy_read_timeout 100s`
- WebSocket upgrade headers

### SSL/TLS-konfiguration

**Protokoll:**
- TLSv1.2
- TLSv1.3

**Ciphers:**
- HIGH:!aNULL:!MD5

**Let's Encrypt:**
- Automatisk förnyelse via `certbot.timer`
- Förnyas 30 dagar före utgång
- Giltighet: 90 dagar

**Self-Signed:**
- RSA 2048-bit
- Giltighet: 365 dagar
- Subject: /C=SE/ST=Sweden/L=Stockholm/O=Privatekonomi/CN=[ip-address]

### Publish-inställningar

**Kommando:**
```bash
dotnet publish \
  --runtime linux-arm64 \
  --self-contained \
  --configuration Release \
  -o ~/Privatekonomi/publish/[Project] \
  /p:PublishTrimmed=false \
  /p:PublishSingleFile=false
```

**Varför inte trimmed/single-file:**
- Aspire-kompatibilitet
- Stabilitet
- Debugging-support
- Marginell storleksvinst vs komplexitet

**Storlek:**
- AppHost: ~80-100 MB
- Web: ~80-100 MB
- API: ~80-100 MB
- Total: ~250-300 MB (inkluderar alla .NET-beroenden)

## Testning

### Validering

✅ **Bash-syntax:**
```bash
bash -n raspberry-pi-install.sh
# Output: (ingen output = OK)
```

✅ **Help-text:**
```bash
./raspberry-pi-install.sh --help
# Visar alla nya alternativ (--no-publish, --no-nginx, --no-ssl, --configure-ssl)
```

✅ **Dokumentation:**
- [x] README.md uppdaterat
- [x] RASPBERRY_PI_första_installationen.md uppdaterat
- [x] Nytt: RASPBERRY_PI_NGINX_SSL.md skapat

### Manuell testning krävs

❗ **OBS:** Följande kräver testning på faktisk Raspberry Pi-hårdvara:

- [ ] Publicering för linux-arm64 fungerar
- [ ] Publicerade binärer startar korrekt
- [ ] Nginx-installation och konfiguration
- [ ] Let's Encrypt certifikatförfrågan
- [ ] Self-signed certifikat fungerar
- [ ] Blazor SignalR via Nginx
- [ ] Systemd-tjänst med publicerade binärer
- [ ] Brandväggsregler (HTTP/HTTPS)

## Säkerhet

### Implementerade åtgärder

✅ **Nginx:**
- Säkerhetsheaders
- Rate limiting-möjlighet (dokumenterad)
- Access control för Aspire Dashboard (dokumenterad)

✅ **SSL:**
- Starka protokoll (TLSv1.2+)
- Modern cipher suite
- HSTS header (med SSL)

✅ **Firewall:**
- Automatisk konfiguration
- Endast nödvändiga portar öppna
- UFW enabled by default

### Rekommendationer

📋 **För produktion:**
1. Använd Let's Encrypt (inte self-signed)
2. Blockera /aspire/ från internet (allow endast lokalt nätverk)
3. Aktivera rate limiting i Nginx
4. Regelbunden uppdatering av Nginx och Certbot
5. Överväg Fail2Ban för brute-force-skydd

## Nästa steg

### Framtida förbättringar

1. **Docker-stöd:**
   - Containerized deployment
   - Docker Compose för alla tjänster
   - Enklare uppdateringar

2. **Monitoring:**
   - Prometheus metrics export
   - Grafana dashboard
   - Alert manager integration

3. **Backup:**
   - Automatisk off-site backup
   - Encrypted backups
   - Restoration scripts

4. **High Availability:**
   - Load balancing
   - Failover
   - Database replication

## Referenser

### Implementerad enligt guiden:
- ✅ Self-contained publish med `--runtime linux-arm64`
- ✅ Nginx reverse proxy-konfiguration
- ✅ SSL med Let's Encrypt
- ✅ SSL med self-signed certifikat (alternativ)

### Ytterligare förbättringar:
- ✅ Automatisk detektion av publicerade binärer i systemd
- ✅ Flexibla kommandoradsalternativ
- ✅ Intelligent brandväggskonfiguration
- ✅ Omfattande dokumentation

## Författare
GitHub Copilot (baserat på användarfråga och best practices)

## Licens
Följer projektets licensiering (samma som Privatekonomi)
