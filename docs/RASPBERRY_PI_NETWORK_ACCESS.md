# Nätverkskonfiguration för Lokal Åtkomst

Denna guide beskriver hur du konfigurerar din Raspberry Pi så att Privatekonomi är tillgängligt från alla enheter i ditt lokala nätverk (mobiler, surfplattor, datorer).

## 📱 Översikt - Åtkomst från alla enheter

Privatekonomi installeras med stöd för åtkomst från:
- ✅ Mobiler (iOS, Android)
- ✅ Surfplattor (iPad, Android-plattor)
- ✅ Datorer (Windows, Mac, Linux)
- ✅ Alla enheter på samma lokala nätverk som Raspberry Pi

## 🔧 Automatisk konfiguration

Installationsskriptet konfigurerar automatiskt Privatekonomi att lyssna på alla nätverksinterface:

```bash
./raspberry-pi-install.sh
```

Detta sätter automatiskt:
- **Aspire Dashboard**: `http://0.0.0.0:17127`
- **Web App**: `http://0.0.0.0:5274`
- **API**: `http://0.0.0.0:5277`

`0.0.0.0` betyder att tjänsterna lyssnar på alla nätverksinterface och är tillgängliga från andra enheter.

## 🌐 Hitta din Raspberry Pi IP-adress

### På Raspberry Pi

```bash
hostname -I
# Exempel: 192.168.1.100
```

### Från annan enhet

**Windows:**
```cmd
ping raspberrypi.local
```

**Mac/Linux:**
```bash
ping raspberrypi.local
# Eller sök efter alla enheter:
arp -a | grep raspberry
```

**Från router:**
- Logga in på din routers admin-panel (vanligtvis `192.168.1.1` eller `192.168.0.1`)
- Hitta listan över anslutna enheter
- Sök efter "raspberrypi" eller "Raspberry Pi"

## 📲 Åtkomst från olika enheter

### Smartphone (iOS/Android)

1. Anslut till samma WiFi-nätverk som Raspberry Pi
2. Öppna webbläsare (Safari, Chrome, Firefox)
3. Navigera till:
   ```
   http://192.168.1.100:5274
   ```
   *(Byt IP-adress mot din Raspberry Pi's IP)*

4. **Installera som PWA (rekommenderat):**
   - **iOS**: Tryck på "Dela" → "Lägg till på hemskärmen"
   - **Android**: Tryck på menyn → "Lägg till på startskärmen"

### Surfplatta (iPad/Android)

Samma som för smartphone ovan.

### Dator (Windows/Mac/Linux)

1. Anslut till samma nätverk som Raspberry Pi
2. Öppna webbläsare
3. Navigera till:
   ```
   http://192.168.1.100:5274
   ```

### Smart-TV eller annan enhet

Om din enhet har webbläsare och kan ansluta till ditt WiFi:
```
http://192.168.1.100:5274
```

## 🔐 HTTPS för säkrare åtkomst

För krypterad åtkomst via HTTPS, konfigurera Nginx:

```bash
./raspberry-pi-install.sh
# Välj att installera Nginx och SSL under installationen
```

Eller för befintlig installation:

```bash
./raspberry-pi-install.sh --configure-ssl
```

Efter SSL-konfiguration, använd HTTPS:
```
https://192.168.1.100/
```

Se [RASPBERRY_PI_NGINX_SSL.md](RASPBERRY_PI_NGINX_SSL.md) för detaljerad guide.

## 📛 Konfigurera DNS-namn (valfritt)

Istället för att komma ihåg IP-adress kan du konfigurera ett lokalt DNS-namn.

### Alternativ 1: mDNS (.local)

**Fungerar automatiskt på:**
- macOS
- Linux (med Avahi)
- Windows 10/11 (begränsat)

**Åtkomst:**
```
http://raspberrypi.local:5274
```

**Ändra värdnamn:**
```bash
sudo raspi-config
# Välj: System Options → Hostname
# Ange nytt namn, t.ex. "privatekonomi"
# Starta om: sudo reboot
```

**Åtkomst efter namnbyte:**
```
http://privatekonomi.local:5274
```

### Alternativ 2: Router DHCP reservation

1. Logga in på din router
2. Hitta DHCP-inställningar
3. Skapa DHCP-reservation för Raspberry Pi MAC-adress
4. Tilldela önskad IP-adress (t.ex. `192.168.1.100`)
5. (Valfritt) Konfigurera lokalt DNS-namn i router

### Alternativ 3: Statisk IP på Raspberry Pi

Installationsskriptet kan konfigurera statisk IP:

```bash
./raspberry-pi-install.sh
# Välj att konfigurera statisk IP under installationen
```

Eller manuellt:

```bash
sudo nano /etc/dhcpcd.conf
```

Lägg till i slutet:
```
interface eth0
static ip_address=192.168.1.100/24
static routers=192.168.1.1
static domain_name_servers=192.168.1.1 8.8.8.8
```

Starta om nätverket:
```bash
sudo systemctl restart dhcpcd
```

## 🔥 Brandväggskonfiguration

### På Raspberry Pi

Installationsskriptet kan konfigurera UFW:

```bash
./raspberry-pi-install.sh
# Välj att konfigurera brandvägg under installationen
```

Eller manuellt:

```bash
# Installera UFW
sudo apt install ufw

# Tillåt SSH (viktigt!)
sudo ufw allow ssh

# Tillåt Privatekonomi-portar
sudo ufw allow 5274/tcp comment "Privatekonomi Web"
sudo ufw allow 5277/tcp comment "Privatekonomi API"
sudo ufw allow 17127/tcp comment "Aspire Dashboard"

# Om du använder Nginx
sudo ufw allow 80/tcp comment "HTTP"
sudo ufw allow 443/tcp comment "HTTPS"

# Aktivera brandvägg
sudo ufw enable

# Kontrollera status
sudo ufw status
```

### På router

För åtkomst från internet (INTE rekommenderat utan säkerhetskonfiguration):

1. Öppna router admin-panel
2. Hitta "Port Forwarding" eller "Virtual Server"
3. Skapa regler:
   - **HTTP**: Extern 80 → Intern 80 (Raspberry Pi IP)
   - **HTTPS**: Extern 443 → Intern 443 (Raspberry Pi IP)

**Säkerhetsvarning:** Exponera INTE Privatekonomi direkt på internet utan:
- ✅ HTTPS med giltigt certifikat
- ✅ Stark autentisering (starka lösenord)
- ✅ Eventuellt VPN istället för direktexponering

## 🔒 Säkerhet för lokal åtkomst

### Rekommenderade åtgärder

1. **Använd HTTPS**
   ```bash
   ./raspberry-pi-install.sh --configure-ssl
   ```

2. **Starka lösenord**
   - Minst 12 tecken
   - Blanda stora/små bokstäver, siffror, symboler
   - Använd lösenordshanterare

3. **Håll systemet uppdaterat**
   ```bash
   sudo apt update && sudo apt upgrade -y
   ./raspberry-pi-update.sh
   ```

4. **Begränsa SSH-åtkomst**
   ```bash
   # Tillåt endast från lokalt nätverk
   sudo ufw allow from 192.168.1.0/24 to any port 22
   ```

5. **Aktivera automatisk backup**
   ```bash
   ./raspberry-pi-install.sh
   # Välj att konfigurera automatisk backup
   ```

## 📡 Felsökning - Kan inte nå från andra enheter

### 1. Kontrollera att Raspberry Pi är tillgänglig

**Från annan enhet:**
```bash
ping 192.168.1.100
```

Om ping inte fungerar:
- Kontrollera att båda enheterna är på samma nätverk
- Kontrollera att Raspberry Pi är på och ansluten
- Vissa nätverk blockerar ping (prova nästa steg ändå)

### 2. Kontrollera att tjänsterna lyssnar

**På Raspberry Pi:**
```bash
ss -lntp | grep '5274\|5277\|17127'
```

**Förväntat resultat:**
```
LISTEN 0  511  0.0.0.0:5274  0.0.0.0:*
LISTEN 0  511  0.0.0.0:5277  0.0.0.0:*
LISTEN 0  511  0.0.0.0:17127 0.0.0.0:*
```

Om du ser `127.0.0.1` istället för `0.0.0.0`:
- Tjänsterna lyssnar bara lokalt
- Kontrollera att `PRIVATEKONOMI_RASPBERRY_PI=true` är satt
- Starta om tjänsterna

### 3. Kontrollera brandvägg

**På Raspberry Pi:**
```bash
sudo ufw status
```

Om portarna inte är tillåtna:
```bash
sudo ufw allow 5274/tcp
sudo ufw allow 5277/tcp
sudo ufw allow 17127/tcp
sudo ufw reload
```

### 4. Kontrollera Privatekonomi körs

```bash
sudo systemctl status privatekonomi
# Eller om manuellt startad:
ps aux | grep Privatekonomi
```

Om inte körande:
```bash
sudo systemctl start privatekonomi
# Eller:
cd ~/Privatekonomi && ./raspberry-pi-start.sh
```

### 5. Testa från Raspberry Pi först

**På Raspberry Pi:**
```bash
curl http://localhost:5274/
```

Om detta fungerar men inte från andra enheter:
- Problem med nätverk/brandvägg
- Kontrollera router-inställningar

Om detta INTE fungerar:
- Problem med applikationen
- Kontrollera loggar: `journalctl -u privatekonomi -n 50`

### 6. Kontrollera WiFi-nätverkets isolering

Vissa routers har "WiFi isolation" eller "AP isolation" som förhindrar enheter att prata med varandra.

**Lösning:**
- Logga in på router
- Sök efter "AP Isolation", "Client Isolation", eller "WiFi Isolation"
- Inaktivera funktionen
- Starta om router

### 7. Testa med olika webbläsare

Om en webbläsare inte fungerar, prova:
- Chrome/Edge
- Firefox
- Safari (iOS/Mac)

### 8. Rensa webbläsarcache

Om du tidigare nådde applikationen men inte längre:
```
Ctrl+Shift+Delete (Windows/Linux)
Cmd+Shift+Delete (Mac)
```

Eller prova inkognito/privat läge.

## 🚀 Optimering för mobil åtkomst

### PWA-installation

Privatekonomi stöder Progressive Web App (PWA):

**Fördelar:**
- Fullskärmsläge utan webbläsargränssnitt
- Snabbare laddning med caching
- Fungerar offline (begränsat)
- App-ikon på hemskärmen

**Installation:**
- **iOS**: Safari → Dela → Lägg till på hemskärmen
- **Android**: Chrome → Meny → Lägg till på startskärmen
- **Desktop**: Chrome → Installera-ikon i adressfältet

### Responsiv design

Privatekonomi använder MudBlazor med responsiv design:
- Automatisk anpassning till skärmstorlek
- Touch-optimerade knappar och gester
- Mobiloptimerad navigering

### Prestanda på mobil

För bästa prestanda:
1. Använd WiFi (inte mobil data)
2. Installera som PWA
3. Håll appen uppdaterad
4. Rensa cache regelbundet

## 📊 Nätverksstatistik

Kontrollera nätverksprestanda:

**På Raspberry Pi:**
```bash
# Bandbredd
iftop

# Aktiva anslutningar
netstat -an | grep 5274

# Antal anslutningar
ss -s
```

## 🔄 Återställ nätverkskonfiguration

Om något går fel:

```bash
# Återställ till standard DHCP
sudo rm /etc/dhcpcd.conf
sudo cp /etc/dhcpcd.conf.bak /etc/dhcpcd.conf 2>/dev/null || true
sudo systemctl restart dhcpcd

# Återställ brandvägg
sudo ufw reset
sudo ufw disable

# Starta om
sudo reboot
```

## 📚 Ytterligare resurser

- **Installation**: [RASPBERRY_PI_GUIDE.md](RASPBERRY_PI_GUIDE.md)
- **Uppdatering**: [RASPBERRY_PI_UPDATE_GUIDE.md](RASPBERRY_PI_UPDATE_GUIDE.md)
- **Nginx & SSL**: [RASPBERRY_PI_NGINX_SSL.md](RASPBERRY_PI_NGINX_SSL.md)
- **PWA-guide**: [PWA_GUIDE.md](PWA_GUIDE.md)

## ✅ Checklista för lokal åtkomst

- [ ] Raspberry Pi har fast IP-adress (DHCP-reservation eller statisk)
- [ ] Tjänsterna lyssnar på 0.0.0.0 (inte bara 127.0.0.1)
- [ ] Brandvägg tillåter portar 5274, 5277, 17127 (eller 80/443 med Nginx)
- [ ] Alla enheter är på samma WiFi-nätverk
- [ ] WiFi-isolering är inaktiverat på router
- [ ] HTTPS är konfigurerat (rekommenderat)
- [ ] mDNS eller statiskt DNS-namn är konfigurerat (valfritt)
- [ ] PWA är installerad på mobila enheter (rekommenderat)
- [ ] Backup är konfigurerat och testat

## 🎉 Sammanfattning

Med korrekt nätverkskonfiguration är Privatekonomi tillgängligt från:
- ✅ Alla smartphones i hemmet
- ✅ Alla surfplattor i hemmet
- ✅ Alla datorer i hemmet
- ✅ Alla enheter på samma lokala nätverk

**Rekommenderad setup:**
```
1. Installera med automatiskt script: ./raspberry-pi-install.sh
2. Konfigurera statisk IP eller DHCP-reservation
3. Installera Nginx + SSL för HTTPS
4. Installera som PWA på mobila enheter
5. Sätt bokmärke på datorer
```

**Åtkomst:**
```
http://privatekonomi.local:5274      # Med mDNS
http://192.168.1.100:5274            # Med IP
https://privatekonomi.local/         # Med Nginx och mDNS
https://192.168.1.100/               # Med Nginx och IP
```

Lycka till med din lokala installation! 🏠💰
