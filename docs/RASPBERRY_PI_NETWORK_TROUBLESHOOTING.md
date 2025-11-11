# Felsökningsguide: Nätverksåtkomst och Proxy för Raspberry Pi

Denna guide hjälper dig att lösa vanliga problem med nätverksåtkomst och proxy-konfiguration för Privatekonomi på Raspberry Pi.

## 🎯 Nyligen Åtgärdat (November 2025)

**Problem:** Applikationen lyssnade på `127.0.0.1` istället för `0.0.0.0`, vilket gjorde den onåbar från andra nätverksenheter.

**Lösning:** 
- Uppdaterade Program.cs-filer för att explicit använda `0.0.0.0` när `PRIVATEKONOMI_RASPBERRY_PI=true`
- Förbättrade systemd-tjänsten med korrekta miljövariabler
- Uppdaterade Nginx-konfiguration med upstream-block för bättre felhantering

**För att få den senaste versionen:**
```bash
cd ~/Privatekonomi
git pull origin main
dotnet build --configuration Release

# Kör om installationen för att uppdatera konfigurationsfiler
./raspberry-pi-install.sh
```

Efter uppdatering bör alla tjänster automatiskt lyssna på rätt adress (0.0.0.0) och vara tillgängliga från nätverket.

## Snabb Diagnostik

Kör diagnostikskriptet först:
```bash
cd ~/Privatekonomi
./raspberry-pi-debug.sh
```

Detta ger dig en komplett översikt över din installation och pekar ut eventuella problem.

## Problem 1: Kan inte nå applikationen från andra enheter

### Symptom
- Applikationen fungerar på Raspberry Pi (via `localhost`)
- Andra enheter på nätverket får timeout eller "Connection refused"
- `curl http://raspberry-pi-ip:5274` fungerar inte från annan enhet

### Diagnos

**Steg 1: Kontrollera att tjänsterna lyssnar på rätt adress**

```bash
ss -lntp | grep -E "17127|5274|5277"
```

Du ska se något som:
```
LISTEN 0  511  0.0.0.0:17127  0.0.0.0:*
LISTEN 0  511  0.0.0.0:5274   0.0.0.0:*
LISTEN 0  511  0.0.0.0:5277   0.0.0.0:*
```

**Problem:** Om du ser `127.0.0.1` istället för `0.0.0.0`:
```
LISTEN 0  511  127.0.0.1:5274  0.0.0.0:*  # FEL!
```

### Lösning A: Kontrollera appsettings.Production.json

**Web-konfiguration:**
```bash
cat ~/Privatekonomi/src/Privatekonomi.Web/appsettings.Production.json
```

Ska innehålla:
```json
{
  "Urls": "http://0.0.0.0:5274",
  ...
}
```

**API-konfiguration:**
```bash
cat ~/Privatekonomi/src/Privatekonomi.Api/appsettings.Production.json
```

Ska innehålla:
```json
{
  "Urls": "http://0.0.0.0:5277",
  ...
}
```

**AppHost-konfiguration:**
```bash
cat ~/Privatekonomi/src/Privatekonomi.AppHost/appsettings.Production.json
```

Ska innehålla:
```json
{
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://0.0.0.0:17127"
      }
    }
  },
  ...
}
```

Om någon av dessa filer saknas eller har fel innehåll:
```bash
# Kör om installationen för att skapa rätt konfigurationsfiler
./raspberry-pi-install.sh
```

### Lösning B: Kontrollera miljövariabler

Om du använder systemd-tjänsten:
```bash
sudo systemctl show privatekonomi --property=Environment
```

Kontrollera att `PRIVATEKONOMI_RASPBERRY_PI=true` finns:
```
Environment=PRIVATEKONOMI_RASPBERRY_PI=true
Environment=DOTNET_DASHBOARD_URLS=http://0.0.0.0:17127
```

**Problem:** Om `ASPNETCORE_URLS` finns i miljövariablerna kan det orsaka problem:
```bash
# Ta bort felfull systemd-tjänst och skapa ny
sudo systemctl stop privatekonomi
sudo systemctl disable privatekonomi
sudo rm /etc/systemd/system/privatekonomi.service
./raspberry-pi-install.sh  # Välj att skapa ny systemd-tjänst
```

### Lösning C: Starta om tjänsterna

```bash
# Med systemd
sudo systemctl restart privatekonomi

# Eller manuellt
cd ~/Privatekonomi
./raspberry-pi-start.sh
```

Vänta 30 sekunder och kontrollera portarna igen:
```bash
ss -lntp | grep -E "17127|5274|5277"
```

## Problem 2: Brandväggen blockerar anslutningar

### Symptom
- Portar lyssnar på `0.0.0.0` (korrekt)
- `curl http://localhost:5274` fungerar på Raspberry Pi
- `curl http://raspberry-pi-ip:5274` fungerar inte från annan enhet

### Diagnos

```bash
sudo ufw status
```

Om UFW är aktiverad:
```
Status: active
```

### Lösning: Öppna portar i brandväggen

```bash
# Öppna Privatekonomi-portar
sudo ufw allow 17127/tcp comment "Privatekonomi Aspire Dashboard"
sudo ufw allow 5274/tcp comment "Privatekonomi Web"
sudo ufw allow 5277/tcp comment "Privatekonomi API"

# Om du använder Nginx
sudo ufw allow 80/tcp comment "HTTP"
sudo ufw allow 443/tcp comment "HTTPS"

# Ladda om brandväggen
sudo ufw reload

# Kontrollera status
sudo ufw status numbered
```

**Förväntat resultat:**
```
Status: active

     To                         Action      From
     --                         ------      ----
[ 1] 22/tcp                     ALLOW IN    Anywhere
[ 2] 17127/tcp                  ALLOW IN    Anywhere        # Privatekonomi Aspire Dashboard
[ 3] 5274/tcp                   ALLOW IN    Anywhere        # Privatekonomi Web
[ 4] 5277/tcp                   ALLOW IN    Anywhere        # Privatekonomi API
```

## Problem 3: Nginx reverse proxy fungerar inte

### Symptom
- Direktåtkomst via portar fungerar (`:5274`, `:5277`)
- `http://raspberry-pi-ip` (port 80) fungerar inte
- Nginx är installerat men svarar inte

### Diagnos

**Steg 1: Kontrollera att Nginx körs**
```bash
systemctl status nginx
```

Ska visa:
```
● nginx.service - A high performance web server
   Active: active (running)
```

**Steg 2: Kontrollera Nginx-konfiguration**
```bash
sudo nginx -t
```

Ska visa:
```
nginx: the configuration file /etc/nginx/nginx.conf syntax is ok
nginx: configuration file /etc/nginx/nginx.conf test is successful
```

**Steg 3: Kontrollera att Privatekonomi-sajten är aktiverad**
```bash
ls -la /etc/nginx/sites-enabled/
```

Ska innehålla symbolisk länk till `privatekonomi`:
```
lrwxrwxrwx 1 root root 41 Nov  7 10:00 privatekonomi -> /etc/nginx/sites-available/privatekonomi
```

### Lösning A: Starta Nginx

```bash
sudo systemctl start nginx
sudo systemctl enable nginx
```

### Lösning B: Aktivera Privatekonomi-sajten

```bash
sudo ln -s /etc/nginx/sites-available/privatekonomi /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl reload nginx
```

### Lösning C: Skapa/återskapa Nginx-konfiguration

```bash
# Om konfigurationsfilen saknas eller är felaktig
./raspberry-pi-install.sh

# Under installationen, välj att konfigurera Nginx
```

### Lösning D: Kontrollera Nginx-loggar

```bash
# Felloggar
sudo tail -f /var/log/nginx/error.log

# Åtkomstloggar
sudo tail -f /var/log/nginx/access.log
```

Vanliga fel:
- `Connection refused to localhost:5274` - Backend-tjänsterna körs inte
- `502 Bad Gateway` - Backend-tjänsterna körs inte eller svarar inte
- `Permission denied` - SELinux eller filrättigheter blockerar

### Lösning E: Verifiera backend-tjänster

Nginx behöver att Web/API-tjänsterna körs:
```bash
# Kontrollera att tjänsterna körs
curl http://localhost:5274
curl http://localhost:5277

# Om inte, starta dem
sudo systemctl start privatekonomi
# eller
./raspberry-pi-start.sh
```

## Problem 4: HTTPS/SSL fungerar inte

### Symptom
- HTTP (`http://raspberry-pi-ip`) fungerar
- HTTPS (`https://raspberry-pi-ip`) fungerar inte eller visar certifikatfel

### Diagnos

**Steg 1: Kontrollera att port 443 lyssnar**
```bash
ss -lntp | grep :443
```

Ska visa:
```
LISTEN 0  511  *:443  *:*  users:(("nginx",pid=1234,...))
```

**Steg 2: Kontrollera SSL-certifikat**
```bash
# För Let's Encrypt
sudo ls -la /etc/letsencrypt/live/

# För self-signed
sudo ls -la /etc/ssl/privatekonomi/
```

### Lösning A: Konfigurera Let's Encrypt (för produktion)

**Förutsättningar:**
- Du har ett domännamn
- Domänen pekar på din Raspberry Pi IP-adress
- Port 80 och 443 är öppna i både brandvägg och router

```bash
# Installera certbot
sudo apt update
sudo apt install -y certbot python3-certbot-nginx

# Hämta certifikat (ersätt example.com med din domän)
sudo certbot --nginx -d privatekonomi.example.com

# Testa automatisk förnyelse
sudo certbot renew --dry-run

# Aktivera automatisk förnyelse
sudo systemctl enable certbot.timer
sudo systemctl start certbot.timer
```

### Lösning B: Skapa self-signed certifikat (för lokal användning)

```bash
# Använd installationsskriptet
./raspberry-pi-install.sh --configure-ssl

# Under installationen, välj "Self-signed certificate"
```

Eller manuellt:
```bash
# Skapa certifikatmapp
sudo mkdir -p /etc/ssl/privatekonomi

# Generera certifikat (ersätt IP-adressen)
sudo openssl req -x509 -nodes -days 365 -newkey rsa:2048 \
  -keyout /etc/ssl/privatekonomi/privatekonomi.key \
  -out /etc/ssl/privatekonomi/privatekonomi.crt \
  -subj "/C=SE/ST=Sweden/L=Stockholm/O=Privatekonomi/CN=192.168.1.100"

# Uppdatera Nginx-konfiguration
sudo nano /etc/nginx/sites-available/privatekonomi

# Lägg till SSL-konfiguration (se docs/RASPBERRY_PI_NGINX_SSL.md)

# Testa och ladda om
sudo nginx -t
sudo systemctl reload nginx
```

**Observera:** Self-signed certifikat ger säkerhetsvarningar i webbläsare. Detta är normalt och säkert för privat, lokal användning.

### Lösning C: Förnya utgånget Let's Encrypt-certifikat

```bash
# Kontrollera certifikatets giltighetstid
sudo certbot certificates

# Förnya certifikat manuellt
sudo certbot renew

# Ladda om Nginx
sudo systemctl reload nginx
```

## Problem 5: Kan inte nå från specifika enheter

### Symptom
- Vissa enheter kan nå applikationen
- Andra enheter (t.ex. smartphone) kan inte

### Diagnos och Lösningar

**A. WiFi-isolering/AP-isolering**

Vissa routers har "WiFi Isolation" eller "AP Isolation" aktiverat:
1. Logga in på din router (vanligtvis `192.168.1.1` eller `192.168.0.1`)
2. Leta efter "WiFi Isolation", "Client Isolation" eller "AP Isolation"
3. Inaktivera funktionen
4. Starta om router om nödvändigt

**B. Olika nätverk/VLAN**

Kontrollera att enheten är på samma nätverk:
```bash
# På Raspberry Pi
hostname -I  # t.ex. 192.168.1.100

# På klientenheten
# iOS/Android: Inställningar -> WiFi -> Nätverksinformation
# Windows: ipconfig
# Mac/Linux: ifconfig eller ip addr

# Kontrollera att de första tre oktettarna matchar
# t.ex. båda ska vara 192.168.1.x
```

**C. Mobildata vs WiFi**

Säkerställ att mobilenheten använder WiFi (inte mobildata):
- iOS: Inställningar -> WiFi -> Kontrollera att rätt nätverk är anslutet
- Android: Inställningar -> Anslutningar -> WiFi

**D. DNS-problem**

Om du använder hostname istället för IP-adress:
```bash
# Testa med IP-adress istället
http://192.168.1.100:5274

# Om IP fungerar men hostname inte, använd mDNS
http://raspberrypi.local:5274
```

## Problem 6: Långsam prestanda från andra enheter

### Symptom
- Applikationen är tillgänglig men mycket långsam
- Sidor tar lång tid att ladda
- Anslutningen är instabil

### Lösningar

**A. Använd Ethernet istället för WiFi**
```bash
# Kontrollera nätverksinterface
ip addr show
```

Ethernet (eth0) ger bättre prestanda än WiFi (wlan0).

**B. Kontrollera nätverksbelastning**
```bash
# Installera iftop
sudo apt install iftop

# Övervaka nätverkstrafik
sudo iftop -i eth0
```

**C. Optimera Nginx-konfiguration**

Redigera `/etc/nginx/sites-available/privatekonomi`:
```nginx
# Aktivera gzip-komprimering
gzip on;
gzip_vary on;
gzip_proxied any;
gzip_comp_level 6;
gzip_types text/plain text/css text/xml text/javascript application/json application/javascript;

# Öka buffert-storlekar
proxy_buffer_size 128k;
proxy_buffers 4 256k;
proxy_busy_buffers_size 256k;
```

```bash
sudo nginx -t
sudo systemctl reload nginx
```

**D. Kontrollera Raspberry Pi-prestanda**
```bash
# CPU-användning
top

# Minne
free -h

# Disk I/O
iostat -x 2
```

**E. Uppgradera till Raspberry Pi 4 eller 5**

Om du använder Raspberry Pi 3, överväg att uppgradera:
- Raspberry Pi 4 (4GB+ RAM) - Bättre prestanda
- Raspberry Pi 5 - Ännu bättre prestanda

## Problem 7: Router port forwarding fungerar inte

### Symptom
- Lokal åtkomst fungerar
- Extern åtkomst (från internet) fungerar inte

### Säkerhetsvarning

⚠️ **VARNING:** Exponera INTE Privatekonomi direkt på internet utan ordentlig säkerhet!

Rekommenderad säkerhet:
- ✅ Använd alltid HTTPS med giltigt certifikat
- ✅ Använd starka lösenord (minst 16 tecken)
- ✅ Överväg att använda VPN istället för port forwarding
- ✅ Aktivera två-faktor-autentisering om tillgängligt
- ✅ Övervaka loggar regelbundet

### Lösning (endast för avancerade användare)

**Alternativ 1: VPN (Rekommenderat)**

Använd WireGuard eller OpenVPN för säker fjärråtkomst:
```bash
# Installera WireGuard
sudo apt install wireguard

# Konfigurera WireGuard (se separat guide)
```

**Alternativ 2: Port Forwarding**

Om du måste använda port forwarding:

1. Logga in på router
2. Hitta "Port Forwarding" eller "Virtual Server"
3. Skapa regel:
   - **Extern port:** 443 (HTTPS)
   - **Intern IP:** Raspberry Pi IP (t.ex. 192.168.1.100)
   - **Intern port:** 443
   - **Protokoll:** TCP

4. Testa med HTTPS (inte HTTP):
   ```
   https://din-publika-ip
   ```

## Testprocedur efter felsökning

### 1. Testa lokalt på Raspberry Pi
```bash
curl http://localhost:5274
curl http://localhost:5277
curl http://localhost:17127
```

### 2. Testa via nätverks-IP från Raspberry Pi
```bash
MY_IP=$(hostname -I | awk '{print $1}')
curl http://$MY_IP:5274
curl http://$MY_IP:5277
```

### 3. Testa från annan enhet på nätverket
```bash
# På Windows (PowerShell)
Invoke-WebRequest -Uri http://raspberry-pi-ip:5274

# På Mac/Linux
curl http://raspberry-pi-ip:5274

# I webbläsare
http://raspberry-pi-ip:5274
```

### 4. Testa via Nginx proxy
```bash
# HTTP
curl http://raspberry-pi-ip

# HTTPS (om konfigurerat)
curl -k https://raspberry-pi-ip
```

### 5. Testa från mobil enhet
1. Anslut till samma WiFi
2. Öppna webbläsare
3. Navigera till: `http://raspberry-pi-ip:5274` eller `http://raspberry-pi-ip`

## Komplett diagnostikkommando

Spara detta som `full-diagnostics.sh`:
```bash
#!/bin/bash

echo "=== Raspberry Pi Network Diagnostics ==="
echo ""
echo "IP Address:"
hostname -I
echo ""

echo "Listening Ports:"
ss -lntp | grep -E "17127|5274|5277|:80|:443"
echo ""

echo "Running Processes:"
ps aux | grep -E "[d]otnet|[n]ginx"
echo ""

echo "Firewall Status:"
sudo ufw status
echo ""

echo "Nginx Status:"
systemctl status nginx --no-pager | head -5
echo ""

echo "Privatekonomi Service:"
systemctl status privatekonomi --no-pager | head -5
echo ""

echo "Test Local Access:"
curl -s -o /dev/null -w "localhost:5274 -> %{http_code}\n" http://localhost:5274
curl -s -o /dev/null -w "localhost:5277 -> %{http_code}\n" http://localhost:5277
echo ""

MY_IP=$(hostname -I | awk '{print $1}')
echo "Test Network Access (from Pi):"
curl -s -o /dev/null -w "$MY_IP:5274 -> %{http_code}\n" http://$MY_IP:5274
curl -s -o /dev/null -w "$MY_IP:80 -> %{http_code}\n" http://$MY_IP
echo ""

echo "=== End Diagnostics ==="
```

```bash
chmod +x full-diagnostics.sh
./full-diagnostics.sh
```

## Ytterligare resurser

- [Raspberry Pi Guide](RASPBERRY_PI_GUIDE.md) - Komplett installationsguide
- [Network Access Guide](RASPBERRY_PI_NETWORK_ACCESS.md) - Nätverkskonfiguration
- [Nginx & SSL Guide](RASPBERRY_PI_NGINX_SSL.md) - Proxy och SSL-konfiguration
- [Felsökning Aspire](RASPBERRY_PI_FELSOKNING.md) - Aspire-specifik felsökning
- [Update Guide](RASPBERRY_PI_UPDATE_GUIDE.md) - Uppdateringsguide

## Support

Om ingen av dessa lösningar fungerar:

1. Kör fullständig diagnostik:
   ```bash
   ./raspberry-pi-debug.sh > diagnostics.txt
   ```

2. Samla loggar:
   ```bash
   journalctl -u privatekonomi -n 200 > privatekonomi.log
   sudo tail -100 /var/log/nginx/error.log > nginx-error.log
   ```

3. Skapa GitHub Issue med:
   - `diagnostics.txt`
   - `privatekonomi.log`
   - `nginx-error.log`
   - Beskrivning av problemet
   - Vad du försökt göra

GitHub Issues: https://github.com/pownas/Privatekonomi/issues
