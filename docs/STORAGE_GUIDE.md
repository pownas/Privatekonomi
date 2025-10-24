# Guide för lagringsmetoder - Privatekonomi

Denna guide beskriver hur du konfigurerar olika lagringsmetoder i Privatekonomi-applikationen.

## Översikt

Privatekonomi stödjer fyra olika lagringsmetoder:

1. **InMemory** - Data lagras i minnet, försvinner när applikationen stängs av (standard för utveckling)
2. **SQLite** - Data lagras i en lokal SQLite-databas (persistent, lämplig för produktion och lokal användning)
3. **SQL Server** - Data lagras i Microsoft SQL Server (persistent, lämplig för storskalig produktion)
4. **JsonFile** - Data lagras i JSON-filer (planerad, ej implementerad än)

## Konfiguration

Lagringsmetoden konfigureras via `appsettings.json` eller `appsettings.Development.json`.

### Storage-sektion

```json
{
  "Storage": {
    "Provider": "InMemory",
    "ConnectionString": "",
    "SeedTestData": true
  }
}
```

### Konfigurationsalternativ

| Parameter | Beskrivning | Möjliga värden |
|-----------|-------------|----------------|
| `Provider` | Vilken lagringsmetod som ska användas | `InMemory`, `Sqlite`, `SqlServer`, `JsonFile` |
| `ConnectionString` | Anslutningssträng eller sökväg till databas/fil | Se exempel nedan |
| `SeedTestData` | Om testdata ska laddas (endast för utveckling) | `true`, `false` |

## Exempel på konfigurationer

### 1. InMemory (Standard för utveckling)

Lämplig för: Utveckling, testning, demonstration

```json
{
  "Storage": {
    "Provider": "InMemory",
    "ConnectionString": "",
    "SeedTestData": true
  }
}
```

**Fördelar:**
- Snabb uppstart
- Inga filer skapas
- Perfekt för utveckling och testning

**Nackdelar:**
- Data försvinner när applikationen stängs av
- Begränsat av tillgängligt minne

### 2. SQLite (Rekommenderas för produktion)

Lämplig för: Produktion, lokal användning, Raspberry Pi, NAS

```json
{
  "Storage": {
    "Provider": "Sqlite",
    "ConnectionString": "Data Source=privatekonomi.db",
    "SeedTestData": false
  }
}
```

**Alternativa ConnectionStrings:**

```json
// Absolut sökväg
"ConnectionString": "Data Source=/var/data/privatekonomi.db"

// Relativ sökväg
"ConnectionString": "Data Source=./data/privatekonomi.db"

// Minnesläge med backup på disk
"ConnectionString": "Data Source=privatekonomi.db;Mode=Memory;Cache=Shared"

// Nätverkssökväg (för delad åtkomst)
"ConnectionString": "Data Source=/mnt/nas/privatekonomi.db"
```

**Fördelar:**
- Persistent lagring
- Snabb och pålitlig
- Ingen server behövs
- Lämplig för Raspberry Pi och NAS
- Kan delas över nätverk

**Nackdelar:**
- Filbaserad, kräver läs/skrivrättigheter
- Begränsad samtidig åtkomst (lämplig för familj, inte storskalig användning)

### 3. SQL Server (Rekommenderas för storskalig produktion)

Lämplig för: Storskalig produktion, företag, molnbaserad hosting

```json
{
  "Storage": {
    "Provider": "SqlServer",
    "ConnectionString": "Server=localhost;Database=Privatekonomi;Trusted_Connection=True;MultipleActiveResultSets=true",
    "SeedTestData": false
  }
}
```

**Alternativa ConnectionStrings:**

```json
// Lokal SQL Server med Windows Authentication
"ConnectionString": "Server=localhost;Database=Privatekonomi;Trusted_Connection=True;MultipleActiveResultSets=true"

// SQL Server med SQL Authentication
"ConnectionString": "Server=localhost;Database=Privatekonomi;User Id=sa;Password=YourPassword;MultipleActiveResultSets=true;TrustServerCertificate=True"

// Azure SQL Database
"ConnectionString": "Server=tcp:yourserver.database.windows.net,1433;Initial Catalog=Privatekonomi;Persist Security Info=False;User ID=yourusername;Password=yourpassword;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

// SQL Server Express LocalDB
"ConnectionString": "Server=(localdb)\\mssqllocaldb;Database=Privatekonomi;Trusted_Connection=True;MultipleActiveResultSets=true"
```

**Fördelar:**
- Mycket hög prestanda och skalbarhet
- Stöd för många samtidiga användare
- Avancerade säkerhetsfunktioner
- Backup och återställning inbyggt
- Stöd för replikering och hög tillgänglighet
- Perfekt för molnbaserad hosting (Azure SQL)

**Nackdelar:**
- Kräver SQL Server-installation (eller Azure SQL)
- Mer komplext att konfigurera
- Kostnad för SQL Server-licens (utom Express Edition)
- Överkapacitet för små installationer

**OBS:** ConnectionString måste anges för SQL Server. Om du inte anger en ConnectionString får du ett felmeddelande.

### 4. JsonFile (Planerad)

Lämplig för: Backup, import/export, versionshantering

```json
{
  "Storage": {
    "Provider": "JsonFile",
    "ConnectionString": "./data",
    "SeedTestData": false
  }
}
```

**OBS:** Denna lagringsmetod är inte fullt implementerad ännu.

## Miljöspecifik konfiguration

Du kan ha olika konfigurationer för olika miljöer:

### appsettings.json (Bas)
```json
{
  "Storage": {
    "Provider": "Sqlite",
    "ConnectionString": "Data Source=privatekonomi.db",
    "SeedTestData": false
  }
}
```

### appsettings.Development.json (Utveckling)
```json
{
  "Storage": {
    "Provider": "InMemory",
    "ConnectionString": "",
    "SeedTestData": true
  }
}
```

### appsettings.Production.json (Produktion)
```json
{
  "Storage": {
    "Provider": "Sqlite",
    "ConnectionString": "Data Source=/var/app/data/privatekonomi.db",
    "SeedTestData": false
  }
}
```

## Nätverksåtkomst

För att göra applikationen tillgänglig från andra enheter på det lokala nätverket:

### 1. Konfigurera SQLite med delad sökväg

Placera databasen på en delad nätverkssökväg:

```json
{
  "Storage": {
    "Provider": "Sqlite",
    "ConnectionString": "Data Source=/mnt/nas/privatekonomi.db"
  }
}
```

### 2. Montera nätverksenhet

**Linux/Raspberry Pi:**
```bash
# NFS
sudo mount -t nfs nas.local:/share /mnt/nas

# CIFS/SMB
sudo mount -t cifs //nas.local/share /mnt/nas -o user=username
```

**Windows:**
```powershell
net use Z: \\nas\share /user:username
```

### 3. Konfigurera Aspire/Kestrel för nätverksåtkomst

Se `ASPIRE_GUIDE.md` för information om hur du exponerar tjänsten på det lokala nätverket.

## Migrera data mellan lagringsmetoder

### Från InMemory till SQLite

1. Exportera data (via Export-funktionen i UI)
2. Ändra konfiguration till SQLite
3. Starta om applikationen
4. Importera data (via Import-funktionen i UI)

### Från SQLite till annan SQLite-databas

Kopiera helt enkelt databas-filen:

```bash
cp privatekonomi.db /ny/sökväg/privatekonomi.db
```

Uppdatera sedan ConnectionString i konfigurationen.

## Backup och återställning

### SQLite Backup

```bash
# Enkel kopia
cp privatekonomi.db privatekonomi.backup.db

# Med SQLite-verktyg
sqlite3 privatekonomi.db ".backup privatekonomi.backup.db"

# Schemalägga backup (cron)
0 2 * * * cp /var/data/privatekonomi.db /backups/privatekonomi-$(date +\%Y\%m\%d).db
```

### Återställa backup

```bash
# Stoppa applikationen först
cp privatekonomi.backup.db privatekonomi.db
# Starta applikationen igen
```

## Felsökning

### Problem: "Database is locked"

**Lösning:** Endast en process kan skriva till SQLite-databasen åt gången. Se till att endast en instans av applikationen körs.

### Problem: "Permission denied"

**Lösning:** Kontrollera att applikationen har läs/skrivrättigheter till katalogen där databasen finns:

```bash
chmod 755 /var/data
chmod 664 /var/data/privatekonomi.db
```

### Problem: "Database file not found"

**Lösning:** Kontrollera att ConnectionString pekar på rätt sökväg. För relativa sökvägar, använd `./` prefix.

## Prestanda

### InMemory
- **Läshastighet:** Mycket snabb
- **Skrivhastighet:** Mycket snabb
- **Minneskrav:** Högt (all data i minnet)
- **Max datamängd:** Begränsad av RAM

### SQLite
- **Läshastighet:** Snabb
- **Skrivhastighet:** Snabb
- **Minneskrav:** Lågt
- **Max datamängd:** Praktiskt obegränsad (flera TB möjligt)

### Rekommendationer

- **Utveckling:** InMemory med SeedTestData=true
- **Småskalig produktion (1-10 användare):** SQLite
- **Raspberry Pi/NAS:** SQLite med regelbunden backup
- **Familjeanvändning:** SQLite på delad lagring

## Säkerhet

### SQLite
- Placera databas-filen utanför web root
- Använd filsystembehörigheter för att begränsa åtkomst
- Överväg kryptering av databas-filen (SQLCipher)
- Ta regelbundna backuper

### Nätverksåtkomst
- Använd VPN för åtkomst utanför hemmanätverket
- Konfigurera brandvägg för att begränsa åtkomst
- Använd HTTPS (se Aspire-guide)

## Support

För fler frågor, se:
- `README.md` - Allmän information
- `ASPIRE_GUIDE.md` - Information om Aspire och nätverkskonfiguration
- GitHub Issues - Rapportera problem eller föreslå förbättringar
