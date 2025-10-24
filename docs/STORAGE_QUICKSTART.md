# Snabbguide - Lagringsmetoder

## Snabbstart

### Utveckling (testdata)
```json
{
  "Storage": {
    "Provider": "InMemory",
    "SeedTestData": true
  }
}
```

### Produktion (persistent data)
```json
{
  "Storage": {
    "Provider": "Sqlite",
    "ConnectionString": "Data Source=privatekonomi.db",
    "SeedTestData": false
  }
}
```

### Raspberry Pi / NAS
```json
{
  "Storage": {
    "Provider": "Sqlite",
    "ConnectionString": "Data Source=/mnt/nas/privatekonomi.db",
    "SeedTestData": false
  }
}
```

### Storskalig produktion (SQL Server)
```json
{
  "Storage": {
    "Provider": "SqlServer",
    "ConnectionString": "Server=localhost;Database=Privatekonomi;Trusted_Connection=True;MultipleActiveResultSets=true",
    "SeedTestData": false
  }
}
```

### Azure SQL Database
```json
{
  "Storage": {
    "Provider": "SqlServer",
    "ConnectionString": "Server=tcp:yourserver.database.windows.net,1433;Initial Catalog=Privatekonomi;User ID=yourusername;Password=yourpassword;MultipleActiveResultSets=False;Encrypt=True;",
    "SeedTestData": false
  }
}
```

## Växla lagringsmetod

1. Öppna `appsettings.json` (eller `appsettings.Development.json`)
2. Ändra `Provider` till önskad lagringsmetod
3. Ange `ConnectionString` om nödvändigt
4. Sätt `SeedTestData` till `true` eller `false`
5. Starta om applikationen

## Vanliga scenarier

### Scenario 1: Utveckling till produktion

**Före (utveckling):**
```json
{
  "Storage": {
    "Provider": "InMemory",
    "SeedTestData": true
  }
}
```

**Efter (produktion):**
```json
{
  "Storage": {
    "Provider": "Sqlite",
    "ConnectionString": "Data Source=privatekonomi.db",
    "SeedTestData": false
  }
}
```

### Scenario 2: Lokal till nätverkslagring

**Före (lokal):**
```json
{
  "Storage": {
    "Provider": "Sqlite",
    "ConnectionString": "Data Source=./data/privatekonomi.db"
  }
}
```

**Efter (nätverkslagring):**
```json
{
  "Storage": {
    "Provider": "Sqlite",
    "ConnectionString": "Data Source=/mnt/nas/privatekonomi.db"
  }
}
```

### Scenario 3: Test till produktion med bibehållen data

1. Exportera data från UI
2. Ändra konfiguration till SQLite
3. Starta om applikationen
4. Importera data via UI

## Felsökning

### Problem: Data försvinner vid omstart
**Lösning:** Byt från InMemory till Sqlite

### Problem: "Database is locked"
**Lösning:** Stäng av alla instanser av applikationen

### Problem: Kan inte hitta databas-fil
**Lösning:** Kontrollera att sökvägen i ConnectionString är korrekt

## Mer information

Se [STORAGE_GUIDE.md](STORAGE_GUIDE.md) för detaljerad dokumentation.
