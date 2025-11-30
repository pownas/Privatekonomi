# Deployment Guide - Privatekonomi Webbhotell

Denna guide beskriver hur du använder den automatiserade release-pipelinen för att driftsätta Privatekonomi till ett webbhotell via SFTP.

> **📚 Nytt! MySQL/MariaDB-support:** Se [MYSQL_DEPLOYMENT_GUIDE.md](./MYSQL_DEPLOYMENT_GUIDE.md) för detaljerad guide om MySQL-deployment.

## Innehållsförteckning

1. [Översikt](#översikt)
2. [Förutsättningar](#förutsättningar)
3. [Konfiguration av GitHub Secrets](#konfiguration-av-github-secrets)
4. [Att Skapa en Release](#att-skapa-en-release)
5. [Pipeline-steg](#pipeline-steg)
6. [Verifiering av Deployment](#verifiering-av-deployment)
7. [Felsökning](#felsökning)
8. [Återställning (Rollback)](#återställning-rollback)
9. [Säkerhetsrekommendationer](#säkerhetsrekommendationer)

## Översikt

Release-pipelinen automatiserar följande steg:
1. Bygger både Web och API-applikationen i Release-läge
2. Kör enhetstester för att säkerställa kvalitet
3. Publicerar optimerade versioner för Linux
4. Deployar automatiskt Web och API till webbhotell via SFTP/FTPS
5. Skapar en GitHub Release med separata installationspaket

### Teknisk Stack

- **CI/CD:** GitHub Actions
- **Build:** .NET 9 SDK
- **Deployment:** SFTP/FTPS med separata kataloger för Web och API
- **Database:** MySQL/MariaDB, SQLite, SQL Server
- **Trigger:** Git tags (v1.0.0, v2.1.0, etc.)

## Förutsättningar

### Webbhotell-krav

1. **SFTP/FTPS-åtkomst**
   - Hostname/IP-adress
   - Port (vanligtvis 21 för FTP/FTPS, 22 för SFTP)
   - Användarnamn
   - Lösenord eller SSH-nyckel
   - Två separata kataloger (en för Web, en för API)

2. **Server-miljö**
   - Linux server (rekommenderat)
   - .NET 9 Runtime installerad (eller använd self-contained deployment)
   - Skrivbehörighet i deployment-katalogerna
   - Minst 1 GB ledigt diskutrymme

3. **Databashantering**
   - **MySQL/MariaDB** - ⭐ **Rekommenderat för webbhotell**
   - SQLite - För utveckling och små installationer
   - Eller SQL Server / PostgreSQL för större installationer
   - Backup-strategi för databas

### GitHub-behörighet

- Admin-åtkomst till GitHub-repositoryt
- Behörighet att skapa GitHub Secrets
- Behörighet att skapa Git tags

## Konfiguration av GitHub Secrets

Följ dessa steg för att konfigurera dina deployment-credentials:

### Steg 1: Navigera till Repository Settings

1. Gå till ditt GitHub-repository
2. Klicka på **Settings** (Inställningar)
3. I vänstermenyn, klicka på **Secrets and variables** → **Actions**

### Steg 2: Skapa Required Secrets

Klicka på **New repository secret** för varje av följande:

#### MYSQL_CONNECTION_STRING ⭐ **Nytt**
- **Namn:** `MYSQL_CONNECTION_STRING`
- **Värde:** Din MySQL connection string
- **Exempel:** `Server=mysql.example.com;Port=3306;Database=privatekonomi;User=privkonomi_user;Password=YourSecurePassword123!;`
- **Se:** [MYSQL_DEPLOYMENT_GUIDE.md](./MYSQL_DEPLOYMENT_GUIDE.md) för detaljerad setup

#### SFTP_HOST
- **Namn:** `SFTP_HOST`
- **Värde:** Din SFTP-server hostname eller IP-adress
- **Exempel:** `ftp.example.com` eller `192.168.1.100`

#### SFTP_USERNAME
- **Namn:** `SFTP_USERNAME`
- **Värde:** Ditt SFTP-användarnamn
- **Exempel:** `privatekonomi_user`

#### SFTP_PASSWORD
- **Namn:** `SFTP_PASSWORD`
- **Värde:** Ditt SFTP-lösenord
- **Säkerhet:** Använd ett starkt, unikt lösenord

#### SFTP_PORT
- **Namn:** `SFTP_PORT`
- **Värde:** SFTP-portnummer
- **Exempel:** `21` (FTPS) eller `22` (SFTP)

#### SFTP_WEB_DIR ⭐ **Uppdaterat**
- **Namn:** `SFTP_WEB_DIR`
- **Värde:** Målkatalog på servern för webbapplikationen
- **Exempel:** `/var/www/privatekonomi-web/` eller `/home/user/public_html/web/`
- **OBS:** Måste sluta med `/`

#### SFTP_API_DIR ⭐ **Nytt**
- **Namn:** `SFTP_API_DIR`
- **Värde:** Målkatalog på servern för API
- **Exempel:** `/var/www/privatekonomi-api/` eller `/home/user/public_html/api/`
- **OBS:** Måste sluta med `/`

#### PRODUCTION_URL (Valfritt)
- **Namn:** `PRODUCTION_URL`
- **Värde:** URL till din webbapplikation
- **Exempel:** `https://privatekonomi.example.com`

### Steg 3: Verifiera Secrets

1. Kontrollera att alla secrets är skapade
2. Dubbelkolla att inga typos finns i namnen
3. Testa SFTP-anslutningen manuellt innan första deployment:

```bash
# För FTPS
lftp -u your_username ftps://your_host:21

# För SFTP
sftp -P 22 your_username@your_host
```

## Att Skapa en Release

### Metod 1: Via Git Tags (Rekommenderat)

1. **Förbered din kod:**
   ```bash
   git pull origin main
   git status  # Kontrollera att allt är commitat
   ```

2. **Skapa en version tag:**
   ```bash
   # Semantic versioning: MAJOR.MINOR.PATCH
   git tag -a v1.0.0 -m "Release 1.0.0: Initial production release"
   ```

3. **Pusha taggen till GitHub:**
   ```bash
   git push origin v1.0.0
   ```

4. **Pipelinen startar automatiskt** när taggen detekteras

### Metod 2: Via GitHub UI

1. Gå till repository på GitHub
2. Klicka på **Releases** (höger sida)
3. Klicka på **Draft a new release**
4. I "Choose a tag", skriv in ny version (t.ex. `v1.0.0`)
5. Välj **Target: main** (eller annan branch)
6. Fyll i **Release title** och **Description**
7. Klicka på **Publish release**

### Metod 3: Manuell Trigger (Test)

För att testa pipelinen utan att skapa en release:

1. Gå till **Actions** i GitHub
2. Välj workflow: **Release and Deploy to Web Hosting**
3. Klicka på **Run workflow**
4. Välj branch
5. Klicka på **Run workflow**

**OBS:** Manuella trigger kommer INTE deployas automatiskt till produktion.

## Pipeline-steg

### 1. Build Job

```
✓ Checkout code
✓ Setup .NET 9
✓ Restore dependencies
✓ Build solution (Release mode)
✓ Run tests
✓ Publish application
✓ Create deployment package info
✓ Upload build artifacts
```

**Varaktighet:** ~5-10 minuter

### 2. Deploy Job

```
✓ Download build artifacts
✓ Display file structure (för loggning)
✓ Deploy via SFTP
✓ Verify deployment
```

**Varaktighet:** ~2-5 minuter (beroende på filstorlek och anslutning)

**OBS:** Deploy-jobbet körs bara för tags, inte för manuella triggers.

### 3. Create Release Job

```
✓ Checkout code
✓ Download build artifacts
✓ Create release archive (.tar.gz)
✓ Generate release notes
✓ Create GitHub Release
```

**Varaktighet:** ~1-2 minuter

## Verifiering av Deployment

### 1. Kontrollera GitHub Actions

1. Gå till **Actions** i GitHub
2. Hitta din workflow-körning
3. Kontrollera att alla steg är gröna (✓)

### 2. Kontrollera SFTP-server

Logga in på din server och verifiera att filerna finns:

```bash
# SSH till servern
ssh your_username@your_host

# Navigera till deployment-katalogen
cd /var/www/privatekonomi/

# Lista filer
ls -la

# Kontrollera deployment info
cat DEPLOYMENT_INFO.txt
```

Förväntad filstruktur:
```
.
├── Privatekonomi.Web
├── Privatekonomi.Web.dll
├── appsettings.json
├── appsettings.Production.json
├── wwwroot/
├── DEPLOYMENT_INFO.txt
└── [övriga dll-filer och dependencies]
```

### 3. Verifiera Applikationen

1. Starta applikationen på servern:
   ```bash
   cd /var/www/privatekonomi/
   ./Privatekonomi.Web
   ```

2. Testa i webbläsare:
   - Öppna `http://your_host:5000` (eller konfigurerad port)
   - Logga in
   - Verifiera att funktionalitet fungerar

3. Kontrollera loggar:
   ```bash
   # Om du använder systemd
   journalctl -u privatekonomi -n 50 -f
   ```

### 4. Kontrollera GitHub Release

1. Gå till **Releases** på GitHub
2. Hitta din nya release (v1.0.0)
3. Verifiera att .tar.gz-filen finns
4. Läs release notes

## Felsökning

### Problem: Build Failed

**Symptom:** Build-jobbet misslyckas med kompileringsfel

**Lösning:**
1. Kontrollera build-loggarna i GitHub Actions
2. Testa lokalt:
   ```bash
   dotnet build --configuration Release
   dotnet test --configuration Release
   ```
3. Fixa eventuella kompileringsfel
4. Commit och pusha ändringar
5. Skapa ny release

### Problem: Tests Failed

**Symptom:** Test-steget misslyckas

**Lösning:**
1. Kör tester lokalt:
   ```bash
   dotnet test --configuration Release --verbosity detailed
   ```
2. Fixa misslyckade tester
3. Commit och pusha
4. Skapa ny release

### Problem: SFTP Connection Failed

**Symptom:** Deploy-jobbet misslyckas med anslutningsfel

**Möjliga orsaker och lösningar:**

1. **Felaktiga credentials:**
   - Kontrollera GitHub Secrets
   - Testa manuell SFTP-anslutning
   - Uppdatera secrets om nödvändigt

2. **Felaktig port:**
   - FTPS använder vanligtvis port 21
   - SFTP använder vanligtvis port 22
   - Kontrollera med din hostingleverantör

3. **Firewall-blockering:**
   - Kontakta din hostingleverantör
   - Be dem vitlista GitHub Actions IP-adresser
   - Överväg alternativ deployment-metod

4. **Felaktig remote directory:**
   - Kontrollera att `SFTP_REMOTE_DIR` existerar
   - Kontrollera skrivbehörigheter
   - Katalogen måste sluta med `/`

### Problem: Permission Denied

**Symptom:** "Permission denied" under deployment

**Lösning:**
1. Kontrollera filbehörigheter på servern:
   ```bash
   ls -la /var/www/
   ```
2. Sätt korrekta behörigheter:
   ```bash
   sudo chown -R your_username:your_username /var/www/privatekonomi/
   sudo chmod -R 755 /var/www/privatekonomi/
   ```

### Problem: Application Won't Start

**Symptom:** Applikationen startar inte efter deployment

**Lösning:**
1. Kontrollera att .NET 9 Runtime är installerad:
   ```bash
   dotnet --list-runtimes
   ```
2. Kontrollera att alla filer deployades korrekt
3. Kontrollera applikationsloggar
4. Verifiera `appsettings.json` konfiguration
5. Kontrollera databas-anslutning

### Problem: GitHub Actions Quota Exceeded

**Symptom:** "No space left on device" eller quota errors

**Lösning:**
1. Rensa gamla artifacts:
   - Gå till **Actions** → **Settings**
   - Justera artifact retention policy
2. Ta bort gamla workflow runs manuellt

## Återställning (Rollback)

Om något går fel med en deployment kan du återställa till en tidigare version:

### Metod 1: Ny Release från Tidigare Tag

1. Identifiera sista fungerande version (t.ex. v1.0.0)
2. Skapa ny release från den taggen:
   ```bash
   git tag -a v1.0.1 v1.0.0^{}
   git push origin v1.0.1
   ```

### Metod 2: Manuell Återställning

1. Ladda ner tidigare release från GitHub Releases
2. Extrahera och uploada manuellt via SFTP:
   ```bash
   tar -xzf privatekonomi-v1.0.0-linux-x64.tar.gz
   lftp -u username ftps://host:21
   mirror -R privatekonomi/ /var/www/privatekonomi/
   ```

### Metod 3: Database Rollback

Om databasschema ändrades, återställ databas:
```bash
# SQLite
cp backup/privatekonomi.db.backup privatekonomi.db

# SQL Server
# Använd SQL Server Management Studio för restore
```

## Säkerhetsrekommendationer

### 1. Secrets Management

- ✅ Använd aldrig lösenord direkt i kod
- ✅ Rotera SFTP-lösenord regelbundet
- ✅ Använd SSH-nycklar istället för lösenord om möjligt
- ✅ Begränsa SFTP-användare till specifik katalog (chroot)

### 2. SFTP Security

- ✅ Använd FTPS eller SFTP (aldrig vanlig FTP)
- ✅ Använd starka lösenord (min 16 tecken)
- ✅ Aktivera two-factor authentication om möjligt
- ✅ Begränsa IP-whitelist för SFTP-åtkomst

### 3. Application Security

- ✅ Sätt `ASPNETCORE_ENVIRONMENT=Production`
- ✅ Använd HTTPS med giltiga SSL-certifikat
- ✅ Säkra `appsettings.json` (rätt file permissions)
- ✅ Regelbundna backups av databas
- ✅ Håll .NET runtime uppdaterad

### 4. GitHub Repository Security

- ✅ Aktivera branch protection på `main`
- ✅ Kräv code reviews för pull requests
- ✅ Aktivera Dependabot för säkerhetsuppdateringar
- ✅ Använd signed commits (GPG)

### 5. Monitoring och Logging

- ✅ Sätt upp loggar för deployment-misslyckanden
- ✅ Övervaka serverresurser (CPU, RAM, disk)
- ✅ Implementera health checks
- ✅ Konfigurera alerts för kritiska fel

## Backup-strategi

### Automatiska Backups

Skapa ett cronjob på servern:

```bash
# Redigera crontab
crontab -e

# Lägg till daily backup kl 03:00
0 3 * * * /home/user/scripts/backup-privatekonomi.sh
```

Exempel backup-script (`backup-privatekonomi.sh`):

```bash
#!/bin/bash
BACKUP_DIR="/home/user/backups"
APP_DIR="/var/www/privatekonomi"
DATE=$(date +%Y%m%d-%H%M%S)

# Skapa backup-katalog
mkdir -p "$BACKUP_DIR"

# Backup databas (SQLite)
cp "$APP_DIR/privatekonomi.db" "$BACKUP_DIR/privatekonomi-$DATE.db"

# Backup konfiguration
cp "$APP_DIR/appsettings.json" "$BACKUP_DIR/appsettings-$DATE.json"

# Ta bort backups äldre än 30 dagar
find "$BACKUP_DIR" -name "privatekonomi-*.db" -mtime +30 -delete

echo "Backup completed: $DATE"
```

## Support och Hjälp

### Dokumentation

- [README](../README.md)
- [Raspberry Pi Guide](./RASPBERRY_PI_GUIDE.md)
- [Storage Guide](./STORAGE_GUIDE.md)
- [Developer Quickstart](./DEVELOPER_QUICKSTART.md)

### Felsökning

- [Issue Templates](./ISSUE_TEMPLATES.md)
- [Issue Examples](./ISSUE_EXAMPLES.md)

### Community

- **GitHub Issues:** [github.com/pownas/Privatekonomi/issues](https://github.com/pownas/Privatekonomi/issues)
- **Discussions:** [github.com/pownas/Privatekonomi/discussions](https://github.com/pownas/Privatekonomi/discussions)

## Versionshistorik

### v1.0.0 (Initial Release)
- ✅ Automatisk build pipeline
- ✅ SFTP/FTPS deployment
- ✅ GitHub Release creation
- ✅ Artifact management

### Planerade Förbättringar

- [ ] Docker container deployment
- [ ] Azure Web App deployment
- [ ] Health check validation post-deployment
- [ ] Automatisk databas-migration
- [ ] Blue-green deployment support
- [ ] Canary deployment support

---

**Senast uppdaterad:** 2025-11-09  
**Version:** 1.0.0  
**Dokumenterad av:** GitHub Copilot Coding Agent
