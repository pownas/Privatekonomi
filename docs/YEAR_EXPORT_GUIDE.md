# Guide: Export av ekonomisk data per år

## Översikt

Privatekonomi stöder export av ekonomisk data uppdelat per år, vilket gör det enkelt att följa och analysera din ekonomi över tid samt förenklar årsvis bokföring och jämförelse.

## Funktioner

- **Årsvis uppdelning**: Välj vilket år du vill exportera data för
- **Flera format**: Exportera i både JSON och CSV format
- **Automatisk filtrering**: Endast data för det valda året exporteras
- **Användarspecifik**: Exporterar endast din egen data om du är inloggad
- **Omfattande data**: Inkluderar transaktioner, budgetar, mål, investeringar, lån och löneutveckling

## Hur använder jag funktionen?

### 1. Navigera till Datahantering

Gå till menyn och välj **Datahantering**.

### 2. Exportera per år

I sektionen "Exportera per År":

1. **Välj år** från dropdown-menyn
   - Listan visar automatiskt alla år som har transaktionsdata
   - Senaste året är förvalt som standard

2. **Välj exportformat**:
   - **JSON**: Fullständig export med all data i strukturerat format
   - **CSV**: Transaktioner i CSV-format med summering

3. **Klicka på önskad exportknapp** (JSON eller CSV)

4. Filen laddas ner automatiskt till din enhet

## Exportformat

### JSON-export

JSON-exporten innehåller:

```json
{
  "year": 2024,
  "exportDate": "2025-01-15T10:30:00Z",
  "version": "1.0",
  "data": {
    "transactions": [...],
    "budgets": [...],
    "goals": [...],
    "investments": [...],
    "loans": [...],
    "salaryHistory": [...]
  }
}
```

**Innehåll**:
- Alla transaktioner för året
- Budgetar som påbörjats eller avslutats under året
- Sparmål skapade eller som förfaller under året
- Investeringar
- Lån
- Löneutveckling för året

**Användning**:
- Backup av årsdata
- Import till andra system
- Analys och bearbetning i andra verktyg

**Filnamn**: `privatekonomi_YYYY.json` (t.ex. `privatekonomi_2024.json`)

### CSV-export

CSV-exporten innehåller:

- Header med årsinformation och exportdatum
- Antal transaktioner
- Alla transaktioner med kolumner:
  - Datum
  - Beskrivning
  - Belopp
  - Typ (Inkomst/Utgift)
  - Bank
  - Kategorier
  - Taggar
  - Noteringar
  - Källa
  - Valuta
- Footer med summering:
  - Totala inkomster
  - Totala utgifter
  - Nettoresultat

**Användning**:
- Öppna i Excel eller LibreOffice Calc
- Bokföring och revision
- Snabb översikt av årets ekonomi

**Filnamn**: `privatekonomi_YYYY.csv` (t.ex. `privatekonomi_2024.csv`)

## Exempel på användningsfall

### Årsvis bokföring

Exportera data för varje år i CSV-format för att enkelt överlämna till din revisor eller för egen bokföring:

1. Välj år (t.ex. 2024)
2. Klicka på "CSV"
3. Öppna filen i Excel
4. Använd summeringen för att kontrollera årets resultat

### Långsiktig analys

Exportera flera år i JSON-format för att analysera trender:

1. Exportera 2022 → `privatekonomi_2022.json`
2. Exportera 2023 → `privatekonomi_2023.json`
3. Exportera 2024 → `privatekonomi_2024.json`
4. Använd ett analysverktyg för att jämföra utvecklingen

### Backup och arkivering

Skapa årliga backups av din ekonomiska data:

1. I slutet av varje år, exportera årets data i JSON-format
2. Spara filen på en säker plats (NAS, molnlagring, extern hårddisk)
3. Behåll historik för skattedeklaration och framtida referens

### Skattedeklaration

Förenkla skattedeklarationen genom att exportera föregående års data:

1. I januari, exportera föregående års data (t.ex. 2024)
2. Använd CSV-exporten för att få en översikt
3. Summorna hjälper dig att fylla i din deklaration korrekt

## Tekniska detaljer

### Datafiltrering

- Export filtrerar automatiskt på:
  - Årtal (baserat på transaktionsdatum)
  - Användar-ID (om inloggad)
  
### Budgetfiltrering för JSON

Budgetar inkluderas om:
- Budgetens startår matchar det valda året, eller
- Budgetens slutår matchar det valda året

### Löneutvecklingsfiltrering för JSON

Löneutvecklingsdata inkluderas om:
- Lönens period (år och månad) matchar det valda året

### Teckenkodning

Både JSON och CSV exporteras med:
- UTF-8 encoding med BOM (Byte Order Mark)
- Korrekt hantering av svenska tecken (å, ä, ö)
- Direkt kompatibilitet med Excel och svenska system

## Felsökning

### Inga år visas i dropdown

**Problem**: Dropdown-menyn för årval är tom.

**Lösning**: Detta beror på att inga transaktioner finns i systemet. Lägg till minst en transaktion för att kunna exportera data per år.

### Exporten är tom

**Problem**: Exporten innehåller ingen data trots att år är valt.

**Lösning**: 
- Kontrollera att du har transaktioner för det valda året
- Verifiera att du är inloggad med rätt användare
- Prova att välja ett annat år

### CSV-filen öppnas inte korrekt i Excel

**Problem**: Svenska tecken visas felaktigt i Excel.

**Lösning**: 
1. Öppna Excel först (tom arbetsbok)
2. Gå till Data → Hämta data → Från fil → Från text/CSV
3. Välj den exporterade filen
4. Excel bör automatiskt detektera UTF-8 encoding
5. Klicka på "Läs in"

### JSON-filen är för stor

**Problem**: JSON-exporten blir mycket stor för år med många transaktioner.

**Lösning**: 
- Detta är normalt för år med många transaktioner
- CSV-exporten är mer kompakt och rekommenderas för stora datamängder
- Överväg att använda extern lagring för stora JSON-filer

## Säkerhet och integritet

### Datasäkerhet

- Exporterade filer innehåller känslig finansiell information
- Lagra filer säkert och krypterat
- Ta bort temporära nedladdade filer efter användning

### Användarfiltrering

- Systemet exporterar endast data för inloggad användare
- Ingen annan användares data inkluderas i exporten
- Perfekt för delad installation med flera användare

### Backup-rekommendationer

1. Exportera regelbundet (t.ex. årligen)
2. Lagra på minst två olika platser
3. Använd krypterad lagring för känslig data
4. Testa återimport för att verifiera backup

## Relaterad funktionalitet

- **Full Backup**: Exportera all data (alla år) i JSON-format
- **Transaction Export**: Exportera transaktioner med anpassat datumintervall
- **Budget Export**: Exportera specifika budgetar

Se även:
- [Datahantering Guide](../docs/STORAGE_GUIDE.md)
- [CSV Import Guide](CSV_IMPORT_GUIDE.md)

## Support

Om du stöter på problem med årsvis export:
1. Kontrollera att du har senaste versionen av Privatekonomi
2. Verifiera att din databas innehåller data för det önskade året
3. Öppna en issue på GitHub med detaljerad beskrivning av problemet
