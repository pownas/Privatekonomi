# Kravspecifikation: Transaktionsredigering med Kategorival och Multi-kategori Uppdelning

**Projekt:** Privatekonomi  
**Version:** 1.0  
**Datum:** 2025-10-24  
**Författare:** Systemanalytiker  

## Innehållsförteckning
1. [Syfte och Mål](#syfte-och-mål)
2. [Nuvarande Läge](#nuvarande-läge)
3. [Funktionella Krav](#funktionella-krav)
4. [Kategoribyte - Specifika Krav](#kategoribyte---specifika-krav)
5. [Export-funktioner](#export-funktioner)
6. [Säkerhet och Begränsningar](#säkerhet-och-begränsningar)
7. [UX/Design Krav](#uxdesign-krav)
8. [API-specifikation](#api-specifikation)
9. [Testfall och Acceptanskriterier](#testfall-och-acceptanskriterier)
10. [Implementationsplan](#implementationsplan)

---

## Syfte och Mål

### Syfte
Möjliggöra för användare att redigera enskilda transaktioner i systemet, inklusive att kunna byta den kategori som transaktionen är kopplad till, samt förbättra transaktionsvyer och exportfunktioner.

### Huvudmål
- **Redigering**: Användare kan ändra alla redigerbara fält på en transaktion
- **Kategoribyte**: Enkelt byte mellan kategorier med validering
- **Multi-kategori**: Möjlighet att dela en transaktion över flera kategorier (split)
- **Export**: Fullständig export av transaktioner till CSV och JSON
- **Säkerhet**: Alla ändringar loggas och valideras
- **UX**: Intuitivt gränssnitt med snabb åtkomst från transaktionslistan

---

## Nuvarande Läge

### Befintlig Datamodell
Systemet har redan en robust datamodell med:

**Transaction-modell** (i `src/Privatekonomi.Core/Models/Transaction.cs`):
```csharp
public class Transaction
{
    public int TransactionId { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; }
    public DateTime Date { get; set; }
    public bool IsIncome { get; set; }
    public string Currency { get; set; } = "SEK";
    public string? Payee { get; set; }
    public string? Tags { get; set; }
    public string? Notes { get; set; }
    public bool Imported { get; set; }
    public bool Cleared { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    // ... relation properties
}
```

**TransactionCategory-modell** (many-to-many relation):
```csharp
public class TransactionCategory
{
    public int TransactionCategoryId { get; set; }
    public int TransactionId { get; set; }
    public int CategoryId { get; set; }
    public decimal Amount { get; set; }
    public decimal Percentage { get; set; } = 100; // Redan implementerat för split!
}
```

### Befintliga API:er
- `PUT /api/transactions/{id}` - finns redan i `TransactionsController`
- `PUT /api/transactions/{id}/categories` - finns för att uppdatera kategorier
- `GET /api/export/transactions/csv` och `/json` - export finns redan

### Befintlig UI
- `EditTransactionDialog.razor` - enkel redigeringsmodal finns
- Transaktionslista med åtgärdskolumn

### Audit och Säkerhet
- `AuditLog`-modell finns implementerad
- `AuditLogService` för spårning av ändringar
- Användarautentisering via `ICurrentUserService`

---

## Funktionella Krav

### FR1: Grundläggande Fältredigering
**Beskrivning**: Användare ska kunna redigera alla tillåtna fält på en transaktion.

**Redigerbara fält:**
- `Amount` (belopp) - med valideringskrav
- `Date` (datum) - med datumvalidering
- `Description` (beskrivning) - fritext, max 500 tecken
- `Payee` (motpart) - fritext, max 200 tecken
- `Notes` (noteringar) - fritext, valfritt
- `Tags` (taggar) - kommaseparerad sträng
- `IsIncome` (inkomst/utgift flagga)
- Kategorier (se [FR3](#fr3-kategoribyte))

**Icke-redigerbara fält:**
- `TransactionId`, `CreatedAt`, `UserId`
- `Imported`, `ImportSource` (låsta efter import)
- `BankSourceId` (endast admin kan ändra)

**Valideringsregler:**
- Belopp: Numeriskt, max 2 decimaler, > 0
- Datum: Mellan 1900-01-01 och dagens datum
- Beskrivning: Obligatorisk, 1-500 tecken
- Payee: Valfri, max 200 tecken

### FR2: Optimistic Locking
**Beskrivning**: Förhindra samtidiga uppdateringar som orsakar dataintegritetsproblem.

**Implementation:**
- Använd `UpdatedAt` som rowversion/ETag
- Vid PUT-request, kontrollera att `UpdatedAt` matchar DB
- Returnera 409 Conflict vid mismatch med aktuell data

### FR3: Kategoribyte
**Beskrivning**: Användare ska kunna byta kategori på en transaktion enkelt och säkert.

**Scenarios:**
1. **Enkel byte**: Ersätt befintlig kategori med ny kategori
2. **Lägg till kategori**: Lägg till ytterligare kategori (multi-kategori)
3. **Ta bort kategori**: Ta bort en kategori från multi-kategori transaktion
4. **Split transaktion**: Dela en transaktion över flera kategorier med specifika belopp/procent

**Valideringsregler:**
- Kategori måste existera och vara aktiv
- Vid split: summa av delbelopp = originaltransaktion
- Vid procentuell split: summa av procent = 100%
- Minst en kategori måste finnas kvar

---

## Kategoribyte - Specifika Krav

### KB1: Kategoriväljare
**Implementation:**
- Sökbar dropdown med hierarkisk visning
- Visa parent/child-relation med indentation
- Autocomplete baserat på tidigare val för användaren
- Färgkodning av kategorier
- Möjlighet att skapa ny kategori inline (om behörighet finns)

### KB2: Multi-kategori Split
**Scenarios:**
1. **Beloppsbaserad split**:
   - Ange specifikt belopp per kategori
   - Automatisk beräkning av resterande belopp
   - Visuell indikator för över/undersummering

2. **Procentbaserad split**:
   - Ange procent per kategori
   - Automatisk beräkning till belopp
   - Hantering av avrundning (sista kategorin får resten)

**UI-komponenter:**
- Split-panel med rader för varje kategori-andel
- "Lägg till kategori"-knapp
- Realtidsvalidering av summor
- Bekräftelsedialog innan sparande

### KB3: Konsekvenser för Rapporter
**Påverkan:**
- Budget-uppdateringar: Flytta belopp mellan kategoribuckets
- Rapporter: Uppdatera aggregat i real-tid eller asynkront
- Automatiska regler: Respektera manuella ändringar (flagga som `ManualCategorization`)

**Implementation:**
- Domain Event: `TransactionCategoryChanged`
- Background service uppdaterar aggregat
- Cache invalidation för rapporter

---

## Export-funktioner

### EX1: CSV Export
**Omfattning**: Förbättra befintlig CSV-export med:

**Fält i CSV:**
```
id,date,amount,currency,description,payee,category_id,category_name,category_color,split_info,tags,notes,is_income,imported,created_at,updated_at,user_id
```

**Specialhantering:**
- Split-transaktioner: En rad per kategori ELLER JSON i `split_info`
- UTF-8 encoding med BOM
- Komma som separator, quote på fält med komma/newline
- Svensk datumformat: YYYY-MM-DD

### EX2: JSON Export
**Format**: Array av transaktionsobjekt med fullständig data.

**Struktur:**
```json
[
  {
    "transactionId": 123,
    "amount": 450.00,
    "currency": "SEK",
    "description": "ICA Maxi Stormarknad",
    "payee": "ICA Maxi",
    "date": "2024-01-15",
    "isIncome": false,
    "categories": [
      {
        "categoryId": 1,
        "categoryName": "Mat & Dryck",
        "amount": 450.00,
        "percentage": 100
      }
    ],
    "tags": "dagligvaror,mat",
    "notes": "Veckohandling",
    "auditTrail": [
      {
        "action": "Created",
        "timestamp": "2024-01-15T10:30:00Z",
        "userId": "user123"
      }
    ]
  }
]
```

### EX3: Filtrering och Storlek
**Parametrar:**
- Datumintervall (from/to)
- Kategorier (lista av ID:n)
- Belopp (min/max)
- Inkludera audit-historik (bool)

**Skalbarhet:**
- Streaming för stora dataset (>10k poster)
- Paginering i API
- Zip-komprimering för stora filer
- Async job för mycket stora exports (>100k poster)

---

## Säkerhet och Begränsningar

### S1: Behörighetsmodell
**Grundläggande redigering:**
- Användare kan endast redigera sina egna transaktioner
- Household-medlemmar kan redigera gemensamma transaktioner (om `HouseholdId` är satt)

**Låsta transaktioner:**
- Importerade transaktioner: Vissa fält låsta (t.ex. `ImportSource`)
- Bokslutslåsta: Transaktioner äldre än X månader (konfigurerbart)
- Admin-lås: Specifika transaktioner kan låsas av admin

**Roller och rättigheter:**
```
Transaction.Edit (Basic) - Kan redigera egna transaktioner
Transaction.EditAll - Kan redigera alla transaktioner
Transaction.EditLocked - Kan redigera låsta transaktioner
Transaction.Split - Kan använda multi-kategori split
Category.Create - Kan skapa nya kategorier inline
```

### S2: Audit Trail
**Loggning vid ändringar:**
```json
{
  "action": "TransactionUpdated",
  "entityType": "Transaction",
  "entityId": 123,
  "userId": "user456",
  "details": {
    "changedFields": {
      "amount": {"from": 100.00, "to": 120.00},
      "categories": {
        "from": [{"categoryId": 1, "amount": 100.00}],
        "to": [{"categoryId": 2, "amount": 120.00}]
      }
    }
  },
  "timestamp": "2024-01-15T14:30:00Z",
  "ipAddress": "192.168.1.100"
}
```

### S3: Datavalidering
**Backend-validering:**
- Alla API-calls valideras oavsett frontend
- Sanitisering av input (XSS-skydd)
- Rätt datatyper och ranges
- Business rule validation

**Race condition-hantering:**
- Optimistic locking med `UpdatedAt`
- Retry-logik för concurrent updates
- UI-feedback vid konflikt

---

## UX/Design Krav

### UI1: Transaktionslista - Förbättringar
**Befintlig lista förbättras med:**
- Kategori-pill som är klickbar för snabbfiltrering
- Edit-ikon i åtgärdskolumn (bredvid befintliga knappar)
- Inline-redigering för belopp och kategori (advanced feature)
- Indikator för manuellt redigerade transaktioner
- Split-indikator för transaktioner med flera kategorier

### UI2: Redigeringsmodal
**Layout och komponenter:**
```
┌─────────────────────────────────────────────────────────────┐
│ ✏️  Redigera Transaktion                                   │
├─────────────────────────────────────────────────────────────┤
│ Beskrivning: [ICA Maxi Stormarknad........................] │
│ Datum: [2024-01-15 📅]    Belopp: [450,00 SEK]           │
│ Motpart: [ICA Maxi..............................]           │
│                                                             │
│ 🏷️ Kategori:                                               │
│ [Mat & Dryck ▼] [🔄 Split]                                │
│                                                             │
│ 📝 Noteringar:                                             │
│ [Veckohandling....................................]        │
│                                                             │
│ 🏷️ Taggar: [dagligvaror, mat]                             │
│                                                             │
│ ─────────────────────────────────────────────────────────── │
│                                    [Avbryt] [💾 Spara]      │
└─────────────────────────────────────────────────────────────┘
```

### UI3: Split-funktionalitet
**Split-panel (expanderas när "Split" klickas):**
```
┌─────────────────────────────────────────────────────────────┐
│ ✂️ Dela transaktion (450,00 SEK)                           │
├─────────────────────────────────────────────────────────────┤
│ 1. [Mat & Dryck    ▼] [300,00] SEK  (66.7%) [❌]         │
│ 2. [Hushållsvaror  ▼] [150,00] SEK  (33.3%) [❌]         │
│                                                             │
│ ➕ Lägg till kategori                                       │
│                                                             │
│ Totalt: 450,00 SEK ✅                                      │
│ ─────────────────────────────────────────────────────────── │
│ ⚠️  Bekräfta split-ändring:                                │
│ Transaktionen kommer att delas över 2 kategorier.          │
│                                    [Avbryt] [✂️ Dela]      │
└─────────────────────────────────────────────────────────────┘
```

### UI4: Responsivitet och Tillgänglighet
- **Mobil**: Single-column layout, större touch-targets
- **Tangentbord**: Tab-navigation, Enter för spara, Escape för avbryt
- **Screen readers**: Aria-labels, fokushantering
- **Färgblindhet**: Använd ikoner tillsammans med färger

---

## API-specifikation

### API1: Hämta Transaktion för Redigering
```http
GET /api/transactions/{id}
Authorization: Bearer {token}

Response 200:
{
  "transactionId": 123,
  "amount": 450.00,
  "description": "ICA Maxi",
  "date": "2024-01-15",
  "payee": "ICA Maxi Stormarknad",
  "categories": [...],
  "updatedAt": "2024-01-15T10:30:00Z",
  "canEdit": true,
  "lockReason": null
}
```

### API2: Uppdatera Transaktion
```http
PUT /api/transactions/{id}
Authorization: Bearer {token}
If-Match: "2024-01-15T10:30:00Z"

Request Body:
{
  "amount": 420.00,
  "description": "ICA Maxi Stormarknad",
  "date": "2024-01-15",
  "payee": "ICA Maxi",
  "notes": "Korrigerat belopp",
  "categories": [
    {
      "categoryId": 1,
      "amount": 300.00,
      "percentage": 71.43
    },
    {
      "categoryId": 15,
      "amount": 120.00,
      "percentage": 28.57
    }
  ]
}

Response 200: {updated transaction}
Response 409: {"error": "Conflict", "currentVersion": "..."}
Response 403: {"error": "Transaction is locked", "reason": "..."}
```

### API3: Export med Förbättringar
```http
POST /api/export/transactions
Authorization: Bearer {token}

Request Body:
{
  "format": "csv|json",
  "filters": {
    "fromDate": "2024-01-01",
    "toDate": "2024-12-31",
    "categoryIds": [1, 2, 15],
    "minAmount": 0,
    "maxAmount": 10000,
    "includeAuditTrail": false
  },
  "columns": ["id", "date", "amount", "description", "categories"]
}

Response 200: File download
Response 202: {"jobId": "abc123", "status": "processing"} (för stora exports)
```

---

## Testfall och Acceptanskriterier

### AC1: Grundläggande Redigering
**Given**: En användare öppnar en transaktion för redigering  
**When**: Användaren ändrar beskrivning och belopp och sparar  
**Then**: 
- Transaktionen uppdateras i databasen
- `UpdatedAt` timestamp uppdateras
- Audit-logg skapas med `TransactionUpdated`
- Användaren ser bekräftelsemeddelande
- Listan uppdateras med nya värden

### AC2: Kategoribyte
**Given**: En transaktion med kategori "Transport"  
**When**: Användaren byter till "Mat & Dryck" och sparar  
**Then**: 
- `TransactionCategory` uppdateras
- Budget för båda kategorier påverkas
- Rapporter uppdateras (asynkront OK)
- Audit-logg visar kategoriändring

### AC3: Multi-kategori Split
**Given**: En transaktion på 1000 SEK med kategori "Blandat"  
**When**: Användaren delar i "Mat" (600 SEK) och "Transport" (400 SEK)  
**Then**: 
- Två `TransactionCategory`-poster skapas
- Original kategori tas bort
- Summan valideras = 1000 SEK
- Procentsatser beräknas automatiskt

### AC4: Optimistic Locking
**Given**: Två användare öppnar samma transaktion samtidigt  
**When**: Första användaren sparar, sedan den andra  
**Then**: 
- Första sparningen lyckas
- Andra får 409 Conflict med aktuell data
- Användare 2 kan välja att uppdatera och försöka igen

### AC5: Export med Kategorier
**Given**: Ett set transaktioner med olika kategorier och splits  
**When**: Användaren exporterar till CSV  
**Then**: 
- CSV innehåller alla relevanta fält
- Split-transaktioner hanteras korrekt
- UTF-8 encoding fungerar för svenska tecken
- Fil heter `transaktioner_YYYYMMDD_HHMMSS.csv`

### Testfall - Implementation

#### Unit Tests
```csharp
[Test]
public async Task UpdateTransaction_ValidData_UpdatesSuccessfully()
[Test]
public async Task UpdateTransaction_StaleVersion_Returns409()
[Test]
public async Task SplitTransaction_ValidAmounts_CreatesSplitCategories()
[Test]
public async Task SplitTransaction_InvalidSum_ThrowsValidationException()
```

#### Integration Tests
```csharp
[Test]
public async Task PUT_Transactions_UpdatesAndCreatesAuditLog()
[Test]
public async Task PUT_Transactions_UpdatesBudgetAggregates()
```

#### E2E Tests (Playwright)
```typescript
test('User can edit transaction description and amount', async ({ page }) => {
  // Navigate to transactions, click edit, change values, save
});

test('User can split transaction into multiple categories', async ({ page }) => {
  // Open split panel, add categories, verify sums, save
});

test('Export includes split transactions correctly', async ({ page }) => {
  // Create split transaction, export CSV, verify content
});
```

---

## Implementationsplan

### Fas 1: Backend API och Datamodell (1-2 veckor)
1. **Audit-förbättringar**: Utöka `AuditLogService` för transaktionsändringar
2. **API-validering**: Förbättra validering i `TransactionsController`
3. **Optimistic locking**: Implementera `UpdatedAt`-kontroll
4. **Export-API**: Förbättra befintlig export med filter och kolumnval

### Fas 2: Frontend Komponenter (2-3 veckor)
1. **Förbättrad EditTransactionDialog**: Lägg till alla fält och validering
2. **Kategoriväljare**: Sökbar dropdown med hierarki
3. **Split-funktionalitet**: Panel för multi-kategori uppdelning
4. **Lista-förbättringar**: Edit-ikoner, split-indikatorer

### Fas 3: UX-förbättringar och Export (1 vecka)
1. **Inline editing**: Snabbredigering av belopp/kategori i listan
2. **Export-UI**: Dialog för att välja format och filter
3. **Responsivitet**: Mobil-anpassning av komponenter

### Fas 4: Testning och Finputsning (1 vecka)
1. **Enhetstester**: Täckning av ny funktionalitet
2. **E2E-tester**: Användingsfall och regressionstest
3. **Performance**: Optimering av större exports
4. **Dokumentation**: Uppdatera användarguider

### Beroenden och Risker
**Beroenden:**
- Befintlig audit-infrastruktur (✅ finns)
- Kategori-hierarki (✅ implementerad)
- Export-endpoints (✅ grundläggande finns)

**Risker:**
- Prestanda vid stora dataset (mitigeras med paginering/streaming)
- Komplex split-logik (mitigeras med grundlig testning)
- Concurrent modifications (mitigeras med optimistic locking)

### Definition of Done
- [ ] Alla AC:er är testade och godkända
- [ ] Unit test coverage > 80% för ny funktionalitet  
- [ ] E2E-tester passerar i CI/CD
- [ ] API-dokumentation uppdaterad
- [ ] Användarguide uppdaterad
- [ ] Performance-test för export av 10k+ transaktioner
- [ ] Audit-loggning verifierad för alla ändringsscenarier
- [ ] Code review och säkerhetsgranskning klar

---

## Bilagor

### A. Befintliga Modeller (Referens)
Se nuvarande implementation i:
- `src/Privatekonomi.Core/Models/Transaction.cs`
- `src/Privatekonomi.Core/Models/TransactionCategory.cs`
- `src/Privatekonomi.Core/Models/Category.cs`
- `src/Privatekonomi.Core/Services/TransactionService.cs`

### B. Wireframes
[Separata wireframe-filer skapas]

### C. API-kontrakt
[Detaljerade OpenAPI-specifikationer kan utökas]

---

**Dokumentversion:** 1.0  
**Senast uppdaterad:** 2025-10-24  
**Nästa review:** Efter fas 1 completion  
**Ansvarig:** Utvecklingsteam + PO

