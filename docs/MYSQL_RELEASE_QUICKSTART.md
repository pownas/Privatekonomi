# Release Pipeline Snabbguide - MySQL Deployment

Snabbreferens för att deployas Privatekonomi med MySQL till webbhotell.

## 📋 Checklista före deployment

### 1. MySQL-databas setup
- [ ] MySQL/MariaDB databas skapad
- [ ] Databas-användare skapad med fullständiga rättigheter
- [ ] Connection string testad och verifierad
- [ ] Character set: `utf8mb4_unicode_ci`

### 2. GitHub Secrets konfigurerade
- [ ] `MYSQL_CONNECTION_STRING` - MySQL connection string
- [ ] `SFTP_HOST` - SFTP server hostname
- [ ] `SFTP_USERNAME` - SFTP användarnamn
- [ ] `SFTP_PASSWORD` - SFTP lösenord
- [ ] `SFTP_PORT` - SFTP port (21 eller 22)
- [ ] `SFTP_WEB_DIR` - Web deploy directory (slutar med `/`)
- [ ] `SFTP_API_DIR` - API deploy directory (slutar med `/`)
- [ ] `PRODUCTION_URL` - (valfritt) Production URL

### 3. SFTP-åtkomst verifierad
- [ ] Kan ansluta till SFTP-server
- [ ] Har skrivbehörighet i deploy-kataloger
- [ ] Kataloger för Web och API finns

## 🚀 Deployment-process

### Steg 1: Skapa version tag

```bash
# Se till att allt är commitat
git status

# Skapa version tag (semantic versioning)
git tag -a v1.0.0 -m "Initial MySQL deployment"

# Pusha tag till GitHub
git push origin v1.0.0
```

### Steg 2: Övervaka GitHub Actions

1. Gå till **Actions** tab på GitHub
2. Hitta workflow: **"Release and Deploy to Web Hosting"**
3. Följ med i stegen:

```
✓ Build (5-10 min)
  ├─ Build solution
  ├─ Run tests
  ├─ Publish Web
  └─ Publish API

✓ Deploy Web (2-5 min)
  ├─ Download artifacts
  ├─ Create appsettings.Production.json
  └─ Deploy via SFTP

✓ Deploy API (2-5 min)
  ├─ Download artifacts
  ├─ Create appsettings.Production.json
  └─ Deploy via SFTP

✓ Create Release (1-2 min)
  ├─ Create archives
  ├─ Generate release notes
  └─ Publish GitHub Release
```

**Total tid:** ~10-18 minuter

### Steg 3: Verifiera deployment

**På servern (SSH):**
```bash
# Kontrollera Web-filerna
cd /var/www/privatekonomi-web
ls -la
cat DEPLOYMENT_INFO.txt

# Kontrollera API-filerna
cd /var/www/privatekonomi-api
ls -la
cat DEPLOYMENT_INFO.txt
```

**I webbläsare:**
1. Navigera till din produktion-URL
2. Logga in
3. Skapa en test-transaktion
4. Verifiera att data sparas i MySQL

## 📦 Vad deployas?

### Web-applikation
- **Fil:** `privatekonomi-web-v1.0.0-linux-x64.tar.gz`
- **Innehåller:**
  - Privatekonomi.Web (Blazor Server)
  - appsettings.Production.json (med MySQL connection string)
  - wwwroot/ (static files)
  - All dependencies

### API-applikation
- **Fil:** `privatekonomi-api-v1.0.0-linux-x64.tar.gz`
- **Innehåller:**
  - Privatekonomi.Api (Web API)
  - appsettings.Production.json (med MySQL connection string)
  - Swagger/OpenAPI spec
  - All dependencies

## 🔒 Secrets format

### MYSQL_CONNECTION_STRING
```
Server=mysql.example.com;Port=3306;Database=privatekonomi;User=privkonomi_user;Password=YourSecurePassword123!;
```

**Tips:**
- Använd starkt lösenord (16+ tecken)
- Testa lokalt först
- Verifiera character encoding (utf8mb4)

### SFTP directories
```
SFTP_WEB_DIR=/var/www/privatekonomi-web/
SFTP_API_DIR=/var/www/privatekonomi-api/
```

**OBS:** Måste sluta med `/`

## 🎯 Semantic Versioning

Använd semantic versioning för tags:

- **MAJOR** version (v2.0.0) - Brytande ändringar
- **MINOR** version (v1.1.0) - Nya features, bakåtkompatibla
- **PATCH** version (v1.0.1) - Buggfixar

Exempel:
```bash
# Initial release
git tag -a v1.0.0 -m "Initial production release"

# Bugfix
git tag -a v1.0.1 -m "Fix transaction import bug"

# New feature
git tag -a v1.1.0 -m "Add MySQL support and dual deployment"

# Breaking change
git tag -a v2.0.0 -m "Major refactoring with breaking API changes"
```

## ⚡ Snabba kommandon

### Deploy ny version
```bash
# 1. Commit alla ändringar
git add .
git commit -m "Your commit message"

# 2. Skapa och pusha tag
git tag -a v1.1.0 -m "Version 1.1.0"
git push origin v1.1.0

# 3. Vänta på GitHub Actions (~10-15 min)
# 4. Verifiera på produktion
```

### Lista alla releases
```bash
# Lokalt
git tag -l

# På GitHub
gh release list  # Kräver GitHub CLI
```

### Ta bort en tag (om fel)
```bash
# Lokalt
git tag -d v1.0.0

# Remote (använd med försiktighet!)
git push origin --delete v1.0.0
```

## 🔧 Felsökning

### Build Failed
1. Kontrollera build logs i GitHub Actions
2. Testa lokalt: `dotnet build --configuration Release`
3. Fixa kompileringsfel
4. Skapa ny tag

### Deployment Failed - SFTP Error
1. Verifiera SFTP credentials i GitHub Secrets
2. Testa SFTP-anslutning manuellt
3. Kontrollera directories och behörigheter
4. Kör deployment igen

### Database Connection Failed
1. Testa connection string lokalt
2. Verifiera MySQL-användare och lösenord
3. Kontrollera firewall-regler
4. Se till att MySQL accepterar anslutningar från servern

### Application Won't Start
1. Kontrollera .NET 9 Runtime är installerad
2. Verifiera appsettings.Production.json
3. Kontrollera loggar på servern
4. Testa med `dotnet Privatekonomi.Web.dll`

## 📚 Dokumentation

### Detaljerade guider
- [MYSQL_DEPLOYMENT_GUIDE.md](./MYSQL_DEPLOYMENT_GUIDE.md) - Komplett MySQL-setup
- [DEPLOYMENT_GUIDE.md](./DEPLOYMENT_GUIDE.md) - Allmän deployment-guide
- [STORAGE_GUIDE.md](./STORAGE_GUIDE.md) - Databas-konfiguration
- [SYSTEMD_SERVICE_GUIDE.md](./SYSTEMD_SERVICE_GUIDE.md) - Systemd-setup

### GitHub Workflow
- `.github/workflows/release-deploy.yml` - Workflow definition

## 💡 Tips & Tricks

### Snabbare deployment
- Använd caching i GitHub Actions (redan konfigurerat)
- Optimera SFTP transfer speed (kontakta hosting provider)
- Self-contained build för att undvika .NET runtime-beroende

### Säkerhet
- Rotera MySQL-lösenord regelbundet
- Använd starka SFTP-lösenord
- Backup databas före major releases
- Använd HTTPS i produktion (SSL-certifikat)

### Monitoring
- Kontrollera GitHub Actions varje release
- Sätt upp email-notifikationer för misslyckade builds
- Logga in på appen efter deployment för verifiering
- Övervaka MySQL disk space

## 🎉 Checklista efter deployment

- [ ] GitHub Actions workflow completed successfully
- [ ] Web-applikation tillgänglig på produktion-URL
- [ ] API tillgänglig och svarar (t.ex. `/swagger`)
- [ ] Kan logga in på webbapplikationen
- [ ] Transaktioner sparas korrekt i MySQL
- [ ] Inga fel i server-loggar
- [ ] GitHub Release skapad med båda archives
- [ ] Release notes är korrekta

## 📞 Support

Om problem uppstår:
1. Läs felsökningsguiden ovan
2. Kontrollera [GitHub Issues](https://github.com/pownas/Privatekonomi/issues)
3. Skapa ny issue med detaljer om problemet

---

**Snabbguide skapad:** 2025-11-09  
**Version:** 1.0.0  
**För frågor:** Se huvuddokumentation eller skapa GitHub issue
