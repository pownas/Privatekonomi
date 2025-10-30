# Kravspecifikation: Avanza Bank Integration och Förbättrad Investeringsöversikt

**Version:** 1.0  
**Datum:** 2025-10-17  
**Status:** Under utveckling

## 1. Bakgrund och Syfte

### 1.1 Bakgrund
Applikationen Privatekonomi har redan stöd för att hantera investeringar (aktier och fonder) samt import av transaktioner från ICA-banken och Swedbank. Användare vill nu kunna importera investeringsdata från Avanza Bank, som är en populär svensk nätmäklare, för att få en komplett översikt över sina investeringar.

### 1.2 Syfte
Syftet är att förbättra översikten över aktier och fonder genom att:
- Visa från vilken bank eller vilket konto innehaven kommer
- Möjliggöra import av investeringsdata från Avanza Bank
- Tillhandahålla exportfunktioner för analys och rapportering

## 2. Funktionella Krav

### 2.1 Import från Avanza Bank

#### 2.1.1 Filformat
Avanza tillhandahåller två olika CSV-exportformat:

**Format 1: Mitt innehav fördelat per konto**
- Visar innehav per specifikt konto
- Innehåller kontonummer som identifikator
- Kolumner:
  - Kontonummer
  - Namn
  - Kortnamn
  - Volym
  - Marknadsvärde
  - GAV (SEK) - Genomsnittligt Anskaffningsvärde
  - GAV
  - Valuta
  - Land
  - ISIN
  - Marknad
  - Typ (STOCK, FUND, EXCHANGE_TRADED_FUND, CERTIFICATE)

**Format 2: Mitt sammanställda innehav**
- Visar sammanställt innehav över alla konton
- Saknar kontonummer
- Kolumner: (samma som Format 1 minus Kontonummer)

#### 2.1.2 Mappning till datamodell
Import från Avanza ska skapa/uppdatera Investment-objekt med följande mappning:

| Avanza-fält | Investment-fält | Kommentar |
|-------------|-----------------|-----------|
| Namn | Name | Investeringens namn |
| Typ | Type | Översätts: STOCK→Aktie, FUND/EXCHANGE_TRADED_FUND→Fond, CERTIFICATE→Certifikat |
| Volym | Quantity | Antal innehav |
| GAV (SEK) | PurchasePrice | Genomsnittligt anskaffningsvärde |
| Marknadsvärde/Volym | CurrentPrice | Beräknas från marknadsvärde/volym |
| Kontonummer | AccountNumber | (Ny egenskap) |
| ISIN | ISIN | (Ny egenskap) |
| Land | Country | (Ny egenskap) |
| Valuta | Currency | (Ny egenskap) |
| Marknad | Market | (Ny egenskap) |
| Kortnamn | ShortName | (Ny egenskap) |
| - | BankSourceId | Avanza Bank (från BankSource-tabellen) |

### 2.2 Datamodell - Utökningar

#### 2.2.1 Investment-modellen
Följande nya egenskaper behöver läggas till:

```csharp
public class Investment
{
    // Befintliga egenskaper...
    public int InvestmentId { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
    public decimal Quantity { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal CurrentPrice { get; set; }
    public DateTime PurchaseDate { get; set; }
    public DateTime LastUpdated { get; set; }
    
    // Nya egenskaper för Avanza-integration
    public int? BankSourceId { get; set; }
    public BankSource? BankSource { get; set; }
    public string? AccountNumber { get; set; }
    public string? ShortName { get; set; }
    public string? ISIN { get; set; }
    public string? Currency { get; set; }
    public string? Country { get; set; }
    public string? Market { get; set; }
}
```

### 2.3 Användarupplevelse

#### 2.3.1 Import-process
1. Användaren navigerar till en ny "Importera Investeringar"-sida eller -dialog
2. Väljer bank (Avanza)
3. Väljer filformat (Per konto / Sammanställt)
4. Laddar upp CSV-fil
5. Systemet visar förhandsvisning
6. Användaren bekräftar import
7. Systemet visar sammanfattning med:
   - Antal nya investeringar
   - Antal uppdaterade investeringar
   - Eventuella fel eller varningar

#### 2.3.2 Visning på Investments-sidan
Investeringssidan ska uppdateras med:

**Filteringsmöjligheter:**
- Filtrera per bank (dropdown/chips)
- Filtrera per konto (dropdown/chips)
- Filtrera per typ (Aktie/Fond/Certifikat)

**Tabellkolumner (utökad):**
- Namn
- Kortnamn (om finns)
- Typ
- Bank (badge/chip med färg)
- Konto (om finns)
- Antal
- Köpkurs
- Aktuell kurs
- Värde
- Vinst/Förlust
- %
- Åtgärder

**Gruppering (valfritt):**
- Möjlighet att gruppera per bank
- Möjlighet att gruppera per konto

#### 2.3.3 Export-funktioner
Användaren ska kunna exportera investeringsdata:

**Exportformat:**
- CSV (kommaseparerad)
- Excel (.xlsx)

**Exportalternativ:**
- Exportera all investeringsdata
- Exportera filtrerad data
- Exportera markerade investeringar

**Exporterade kolumner:**
- Alla synliga kolumner i tabellen
- Inkludera beräknade värden (vinst/förlust, procent)

### 2.4 Validering och Felhantering

#### 2.4.1 Import-validering
- Kontrollera att CSV-filen har rätt format och kolumner
- Validera att numeriska värden (Volym, Marknadsvärde, GAV) kan parsas
- Validera ISIN-format (12 tecken, alphanumerisk)
- Hantera tomma eller ogiltiga rader gracefully

#### 2.4.2 Dubbletthantering
Vid import:
- Om en investering med samma ISIN och kontonummer redan finns: **Uppdatera**
- Om en investering med samma namn och typ finns (men ingen ISIN): **Uppdatera**
- Annars: **Skapa ny**

#### 2.4.3 Felmeddelanden
Tydliga felmeddelanden vid:
- Ogiltigt filformat
- Saknade obligatoriska kolumner
- Parse-fel för numeriska värden
- Importfel (tekniska problem)

## 3. Tekniska Krav

### 3.1 Nya Komponenter

#### 3.1.1 CSV Parsers
- `AvanzaHoldingsPerAccountParser.cs` - Parser för "Mitt innehav fördelat per konto"
- `AvanzaConsolidatedHoldingsParser.cs` - Parser för "Mitt sammanställda innehav"

Båda parsers ska implementera ett nytt interface:
```csharp
public interface IInvestmentCsvParser
{
    string BankName { get; }
    Task<List<Investment>> ParseAsync(Stream csvStream);
    bool CanParse(string csvContent);
}
```

#### 3.1.2 Services
Utöka `IInvestmentService` med:
```csharp
Task<CsvImportResult> ImportFromCsvAsync(Stream csvStream, string bankName, string? accountFilter = null);
Task<byte[]> ExportToCsvAsync(IEnumerable<Investment> investments);
Task<byte[]> ExportToExcelAsync(IEnumerable<Investment> investments);
Task<IEnumerable<Investment>> GetInvestmentsByBankAsync(int bankSourceId);
Task<IEnumerable<Investment>> GetInvestmentsByAccountAsync(string accountNumber);
```

#### 3.1.3 UI-komponenter
- `ImportInvestments.razor` - Sida/dialog för import av investeringar
- `InvestmentFilters.razor` - Komponent för filtrering
- `InvestmentExportDialog.razor` - Dialog för export-alternativ

### 3.2 Databasmigrering
Eftersom projektet använder InMemory-databas behöver PrivatekonomyContext uppdateras med:
- Utökad Investment entity configuration
- Relation mellan Investment och BankSource
- Index på ISIN och AccountNumber för snabbare sökning

### 3.3 Beroenden
Inga nya externa NuGet-paket behövs för grundfunktionaliteten. För Excel-export kan vi överväga:
- `ClosedXML` eller `EPPlus` för .xlsx-generering (valfritt i första versionen)

## 4. Icke-funktionella Krav

### 4.1 Prestanda
- Import ska kunna hantera filer upp till 10 MB
- Import ska klara minst 1000 investeringar på < 5 sekunder
- Export ska vara responsiv för upp till 1000 investeringar

### 4.2 Användarvänlighet
- Intuitiv import-process med tydliga steg
- Förhandsvisning innan import bekräftas
- Tydliga felmeddelanden på svenska
- Responsiv design (fungerar på desktop och mobil)

### 4.3 Säkerhet
- CSV-filer valideras noggrant innan parsing
- Ingen känslig data loggas
- SQL-injection skydd (EF Core hanterar detta)

### 4.4 Underhållbarhet
- Kod ska följa befintliga mönster i projektet
- Välkommenterad kod för komplexa parsers
- Enhetstester för parsers (om testinfrastruktur finns)

## 5. Gränser och Begränsningar

### 5.1 Utanför scope
- Automatisk synkronisering med Avanza API (endast manuell CSV-import)
- Realtidsuppdatering av kurser
- Integration med andra banker än Avanza i denna iteration
- Avancerad portföljanalys eller grafer

### 5.2 Framtida förbättringar
- Automatisk schemalagd import
- Integration med bank-API:er för automatisk uppdatering
- Visualisering av portföljallokeringentering per bank/konto
- Historisk utveckling och grafer
- Valutakonvertering

## 6. Implementationsordning

### Fas 1: Datamodell och Parsers (Prioritet 1)
1. Utöka Investment-modellen med nya egenskaper
2. Uppdatera PrivatekonomyContext
3. Implementera AvanzaHoldingsPerAccountParser
4. Implementera AvanzaConsolidatedHoldingsParser
5. Utöka InvestmentService med import-funktionalitet

### Fas 2: Import UI (Prioritet 1)
1. Skapa ImportInvestments.razor
2. Integrera import-flöde
3. Implementera förhandsvisning
4. Implementera sammanfattning efter import

### Fas 3: Förbättrad Visning (Prioritet 2)
1. Uppdatera Investments.razor med nya kolumner
2. Implementera InvestmentFilters-komponent
3. Lägg till bank/konto-badges i tabellen
4. Implementera gruppering (valfritt)

### Fas 4: Export (Prioritet 2)
1. Implementera CSV-export i InvestmentService
2. Skapa InvestmentExportDialog
3. Integrera export-funktionalitet i Investments.razor
4. (Valfritt) Implementera Excel-export

## 7. Testscenarios

### 7.1 Manuella tester
1. Import av "Mitt innehav fördelat per konto" CSV
2. Import av "Mitt sammanställda innehav" CSV
3. Uppdatering av befintliga investeringar via import
4. Filtrering per bank och konto
5. Export till CSV
6. Responsivitet på mobil enhet

### 7.2 Testdata
Skapa testfiler baserat på exemplen i issue-beskrivningen.

## 8. Acceptanskriterier

Implementeringen är klar när:
- [ ] Användaren kan importera båda typerna av Avanza CSV-filer
- [ ] Investeringsdata mappas korrekt till Investment-modellen
- [ ] Bank- och kontoinformation visas i investeringstabellen
- [ ] Användaren kan filtrera investeringar per bank och konto
- [ ] Användaren kan exportera investeringar till CSV
- [ ] Import-processen har förhandsvisning och sammanfattning
- [ ] Befintliga funktioner fungerar fortfarande (ingen regression)
- [ ] Dokumentation uppdaterad med instruktioner för Avanza-import

## 9. Dokumentation

Efter implementering ska följande dokumenteras:
- README.md: Kort beskrivning av Avanza-import
- Ny wiki-sida: `AVANZA_IMPORT_GUIDE.md` med:
  - Hur man exporterar data från Avanza
  - Steg-för-steg guide för import
  - Vanliga fel och lösningar
  - Exempel på filformat
