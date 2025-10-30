# API-uppdatering av Aktiekurser

## Översikt

Detta dokument beskriver funktionaliteten för att uppdatera aktiekurser i portföljen med hjälp av Yahoo Finance API.

## Funktioner

### 1. Uppdatera alla kurser

- **Placering**: Knapp "Uppdatera alla kurser" i verktygsfältet på Investments-sidan
- **Funktionalitet**: Uppdaterar aktuell kurs för alla investeringar i portföljen
- **UI**: Visar en progress-dialog med aktuell status och resultat

### 2. Uppdatera enskild kurs

- **Placering**: Refresh-knapp (grön) i åtgärdskolumnen för varje investering
- **Funktionalitet**: Uppdaterar aktuell kurs för en specifik investering
- **UI**: Visar laddningsindikator på knappen under uppdatering

### 3. Manuell kursuppdatering

- **Placering**: Edit-knapp i åtgärdskolumnen för varje investering  
- **Funktionalitet**: Möjliggör manuell inmatning av ny kurs
- **UI**: Öppnar en dialog för att ange ny kurs manuellt

## Implementation

### Yahoo Finance Integration

API:et använder Yahoo Finance Query API (v8) för att hämta realtidskurser:

**Endpoint**: `https://query1.finance.yahoo.com/v8/finance/chart/{symbol}`

**Stödda marknader**:
- Stockholm Stock Exchange (.ST)
- Helsinki Stock Exchange (.HE)  
- Copenhagen Stock Exchange (.CO)
- Oslo Stock Exchange (.OL)
- US och andra marknader (utan suffix)

### Ticker Symbol Hantering

Systemet identifierar ticker-symboler på följande sätt:

1. **Prioritet 1**: Använder `ShortName` om det ser ut som en ticker-symbol (2-10 tecken)
2. **Prioritet 2**: Extraherar från `Name` om det finns parenteser, t.ex. "Microsoft (MSFT)"
3. **Marknadssuffix**: Lägger automatiskt till korrekt suffix baserat på `Market`-fältet
   - Stockholm → .ST
   - Helsinki → .HE
   - Copenhagen → .CO
   - Oslo → .OL

### Rate Limiting

För att undvika API rate-limiting:
- 500ms fördröjning mellan varje förfrågan vid batch-uppdatering
- Endast en uppdatering per investering åt gången

### Felhantering

- Loggar alla API-fel med Logger
- Visar användarvänliga felmeddelanden via Snackbar
- Rapporterar misslyckade uppdateringar i batch-resultatdialogen

## Kodstruktur

### Core Services

**`IStockPriceService`** - Interface för aktiekursuppdatering
- `UpdatePriceAsync(Investment)` - Uppdatera en investering
- `UpdatePricesAsync(IEnumerable<Investment>)` - Uppdatera flera investeringar

**`YahooFinanceStockPriceService`** - Implementation som använder Yahoo Finance API
- Hämtar realtidskurser
- Formaterar ticker-symboler för olika marknader
- Hanterar fel och timeout

### Web Components

**`UpdateAllPricesDialog.razor`** - Dialog för batch-uppdatering
- Visar progress under uppdatering
- Visar sammanfattning av resultat
- Listar fel som uppstått

**`Investments.razor`** - Investmentssidan med uppdateringsfunktionalitet
- Button "Uppdatera alla kurser"
- Refresh-knappar per investering
- Loading states

## Användning

### För utvecklare

1. **Registrera service i Program.cs**:
```csharp
builder.Services.AddHttpClient<IStockPriceService, YahooFinanceStockPriceService>();
```

2. **Injicera i komponenter**:
```csharp
@inject IStockPriceService StockPriceService
```

3. **Uppdatera en investering**:
```csharp
var success = await StockPriceService.UpdatePriceAsync(investment);
```

4. **Uppdatera flera investeringar**:
```csharp
var result = await StockPriceService.UpdatePricesAsync(investments);
// result innehåller SuccessCount, FailedCount och Errors
```

### För användare

#### Uppdatera alla kurser:
1. Navigera till "Aktier & Fonder"
2. Klicka på "Uppdatera alla kurser"
3. Bekräfta i dialogen
4. Vänta tills uppdateringen är klar
5. Granska resultatet

#### Uppdatera en enskild kurs:
1. Navigera till "Aktier & Fonder"
2. Klicka på den gröna refresh-knappen för önskad investering
3. Kursen uppdateras automatiskt

#### Uppdatera kurs manuellt:
1. Navigera till "Aktier & Fonder"
2. Klicka på edit-knappen (penna) för önskad investering
3. Ange ny kurs i dialogen
4. Klicka "Uppdatera"

## Konfiguration

Ingen särskild konfiguration krävs. Yahoo Finance API är fritt att använda utan API-nyckel.

## Begränsningar

- **API-tillgänglighet**: Yahoo Finance API är inte officiellt och kan ändras utan förvarning
- **Rate limiting**: Okänd, men 500ms delay mellan förfrågningar rekommenderas
- **Marknader**: Primärt fokus på nordiska marknader och USA
- **Fonder**: Kan ha begränsad support då de inte alltid har realtidskurser
- **Ticker-symboler**: Måste vara korrekta för att uppdatering ska fungera

## Framtida förbättringar

- [ ] Caching av kurser för att minska API-anrop
- [ ] Stöd för fler API-leverantörer (Alpha Vantage, IEX Cloud)
- [ ] Schemalägg automatisk uppdatering
- [ ] Historisk kursdata och diagram
- [ ] Valutakonvertering för utländska aktier
- [ ] Bättre hantering av fonder utan daglig handel

## Felsökning

### Kursen uppdateras inte

**Möjliga orsaker**:
1. Felaktig ticker-symbol - Kontrollera att `ShortName` eller `Name` innehåller korrekt symbol
2. Fel marknadssuffix - Kontrollera att `Market`-fältet är korrekt satt
3. API-problem - Kontrollera loggar för detaljer
4. Aktie handlas ej - Vissa aktier handlas inte dagligen

**Lösning**:
- Kontrollera att ticker-symbolen är korrekt enligt Yahoo Finance
- Uppdatera `Market`-fältet för att få rätt suffix
- Använd manuell kursuppdatering som fallback

### Timeout eller långsam uppdatering

**Möjliga orsaker**:
1. Många investeringar - Varje investering tar minst 500ms att uppdatera
2. Långsamt nätverk
3. Yahoo Finance API är överbelastat

**Lösning**:
- Uppdatera färre investeringar åt gången
- Använd enskild uppdatering för brådskande kurser
- Vänta och försök igen senare

## Se även

- [Yahoo Finance API Documentation](https://www.yahoofinanceapi.com/)
- [AVANZA_IMPORT_GUIDE.md](AVANZA_IMPORT_GUIDE.md) - Import av investeringar
- [ProgramSpecifikation.md](ProgramSpecifikation.md) - Övergripande specifikation
