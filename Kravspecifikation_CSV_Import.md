# Kravspecifikation: CSV-import av transaktioner från ICA-banken och Swedbank

**Version:** 1.0  
**Datum:** 2025-10-16  
**Status:** Utkast

---

## 1. Sammanfattning

Denna kravspecifikation definierar funktionella och tekniska krav för att möjliggöra import av transaktionshistorik via CSV-filer från ICA-banken och Swedbank till Privatekonomi-applikationen. Specifikationen omfattar dataformat, valideringsregler, fältmappningar, felhantering och användargränssnittskrav.

---

## 2. Bakgrund och syfte

### 2.1 Bakgrund
Privatekonomi-applikationen använder för närvarande manuell inmatning av transaktioner. Många användare har omfattande transaktionshistorik i sina banker och önskar kunna importera denna data automatiskt för att:
- Spara tid vid initial setup av applikationen
- Undvika manuella inmatningsfel
- Få historisk data för bättre analys och rapportering
- Regelbundet uppdatera transaktioner från bankexport

### 2.2 Syfte
Denna specifikation syftar till att:
- Definiera tekniska krav för CSV-import från ICA-banken och Swedbank
- Dokumentera skillnader mellan bankernas CSV-format
- Specificera valideringsregler och felhantering
- Skapa underlag för implementering av importfunktionalitet

### 2.3 Omfattning
Första versionen omfattar:
- Import av transaktioner från ICA-banken (CSV-format)
- Import av transaktioner från Swedbank (CSV-format)
- Grundläggande validering och felhantering
- Hantering av dubbletter

Framtida versioner kan inkludera:
- Fler banker (SEB, Nordea, Handelsbanken, etc.)
- Automatisk bankkännedom baserat på CSV-struktur
- Schemalagd import via bank-API:er
- Import av kontobalanser och saldon

---

## 3. CSV-format per bank

### 3.1 ICA-banken

#### 3.1.1 Filformat
- **Filnamn:** Typiskt `transaktioner_YYYYMMDD.csv` eller liknande
- **Teckenkodning:** UTF-8 med BOM eller ISO-8859-1
- **Separator:** Semikolon (`;`) eller komma (`,`)
- **Decimalavgränsare:** Komma (`,`)
- **Datumformat:** YYYY-MM-DD eller DD.MM.YYYY
- **Radhuvud:** Ja (första raden innehåller kolumnnamn)

#### 3.1.2 Kolumnstruktur (förväntad)
Baserat på standardformat för svenska banker:

| Kolumnnamn | Beskrivning | Datatyp | Obligatorisk |
|------------|-------------|---------|--------------|
| Datum | Transaktionsdatum | Date | Ja |
| Belopp | Transaktionsbelopp (negativt för utgifter, positivt för inkomster) | Decimal | Ja |
| Beskrivning / Text | Transaktionsbeskrivning | String | Ja |
| Saldo | Kontosaldo efter transaktion | Decimal | Nej |
| Motpart | Mottagare eller avsändare | String | Nej |

#### 3.1.3 Exempeldata (ICA-banken)
```csv
Datum;Belopp;Beskrivning;Saldo
2025-01-15;-125,50;ICA Maxi Stockholm;8742,30
2025-01-16;3500,00;Lön;12242,30
2025-01-17;-45,00;Netflix AB;12197,30
```

**Observera:** Det exakta formatet från ICA-banken kan variera. Vid implementering bör ett verkligt exempel från ICA-banken inhämtas och analyseras.

### 3.2 Swedbank

#### 3.2.1 Filformat
- **Filnamn:** Typiskt `kontoutdrag_YYYYMMDD.csv` eller `account_statement.csv`
- **Teckenkodning:** UTF-8, ISO-8859-1 eller Windows-1252
- **Separator:** Semikolon (`;`)
- **Decimalavgränsare:** Komma (`,`)
- **Datumformat:** DD.MM.YYYY
- **Radhuvud:** Ja (två rader metadata innan kolumnnamn på rad 1)

#### 3.2.2 Kolumnstruktur
Baserat på dokumentation och analys av Swedbanks exportformat:

| Kolumnnamn | Beskrivning | Datatyp | Obligatorisk | Position |
|------------|-------------|---------|--------------|----------|
| Client account | Kontonummer | String | Ja | 0 |
| Row type | Radtyp (10=öppningsbalans, 20=transaktion, 82=omsättning, 86=slutbalans) | String | Ja | 1 |
| Date | Transaktionsdatum | Date | Ja | 2 |
| Beneficiary/Payer | Mottagare/betalare | String | Nej | 3 |
| Details | Transaktionsdetaljer | String | Ja | 4 |
| Amount | Belopp | Decimal | Ja | 5 |
| Currency | Valuta (EUR, SEK, etc.) | String | Ja | 6 |
| Debit/Credit | D=Debet (utgift), K=Kredit (inkomst) | String | Ja | 7 |
| Transfer reference | Överföringsreferens | String | Nej | 8 |
| Transaction type | Transaktionstyp | String | Nej | 9 |
| Reference number | Referensnummer | String | Nej | 10 |
| Document number | Dokumentnummer | String | Nej | 11 |

#### 3.2.3 Exempeldata (Swedbank)
```csv
"Client account";"Row type";"Date";"Beneficiary/Payer";"Details";"Amount";"Currency";"Debit/Credit";"Transfer reference";"Transaction type";"Reference number";"Document number";
"SE0000000000000000000000";"10";"01.01.2025";"";"Opening balance";"10000,00";"SEK";"K";"";"AS";"";"";
"SE0000000000000000000000";"20";"08.01.2025";"ICA MAXI STOCKHOLM";"Kortköp 123456789";"125,50";"SEK";"D";"";"MK";"";"5";
"SE0000000000000000000000";"20";"15.01.2025";"ARBETSGIVARE AB";"Lön januari 2025";"35000,00";"SEK";"K";"";"LB";"";"12";
"SE0000000000000000000000";"20";"16.01.2025";"NETFLIX AB";"Månadsprenumeration";"149,00";"SEK";"D";"";"AG";"";"";
```

#### 3.2.4 Radtyper att hantera
- **10:** Öppningsbalans - Ska **inte** importeras som transaktion
- **20:** Standardtransaktion - Ska importeras
- **82:** Omsättning/Turnover - Ska **inte** importeras (metainformation)
- **86:** Slutbalans - Ska **inte** importeras som transaktion

---

## 4. Mappning till applikationens datamodell

### 4.1 Befintlig datamodell
Privatekonomi-applikationen använder följande datamodell för transaktioner:

```csharp
public class Transaction
{
    public int TransactionId { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; }
    public DateTime Date { get; set; }
    public bool IsIncome { get; set; }
    public ICollection<TransactionCategory> TransactionCategories { get; set; }
}
```

### 4.2 Fältmappning ICA-banken

| CSV-fält | Applikationsfält | Transformering |
|----------|------------------|----------------|
| Datum | Date | Parsa datum enligt format YYYY-MM-DD eller DD.MM.YYYY |
| Belopp | Amount | Konvertera till decimal, ersätt komma med punkt. Använd absolutvärde. |
| Belopp (tecken) | IsIncome | `true` om belopp > 0, annars `false` |
| Beskrivning | Description | Trimma whitespace, max 500 tecken |
| Motpart | Description | Om beskrivning saknas, använd motpart |

### 4.3 Fältmappning Swedbank

| CSV-fält | Applikationsfält | Transformering |
|----------|------------------|----------------|
| Date | Date | Parsa datum enligt format DD.MM.YYYY |
| Amount | Amount | Konvertera till decimal, ersätt komma med punkt |
| Debit/Credit | IsIncome | `true` om "K" (kredit), `false` om "D" (debet) |
| Details | Description | Primär beskrivning |
| Beneficiary/Payer | Description | Om Details är tom eller generisk, använd Beneficiary/Payer som del av beskrivning |
| Currency | - | Validera att valutan är SEK (eller konvertera vid behov) |
| Row type | - | Filtrera bort rader med typ 10, 82, 86 |

### 4.4 Beskrivningsformatering
För Swedbank, kombinera fält för bättre beskrivning:
- Om `Beneficiary/Payer` finns OCH inte är tom: `"{Beneficiary/Payer} - {Details}"`
- Annars: Använd endast `Details`
- Trimma till max 500 tecken

---

## 5. Valideringsregler

### 5.1 Filvalidering

#### 5.1.1 Innan parsning
- **Filstorlek:** Max 10 MB per fil
- **Filformat:** Måste vara `.csv` eller `.txt`
- **Teckenkodning:** Måste kunna läsas som UTF-8, ISO-8859-1 eller Windows-1252
- **Separatorer:** Automatisk detektion av `;` eller `,`

#### 5.1.2 Strukturvalidering
- **Radhuvud:** Måste finnas och innehålla förväntade kolumnnamn
- **Kolumnantal:** Minst 3 kolumner krävs (datum, belopp, beskrivning)
- **Radantal:** Minst 1 datrad (utöver radhuvud)

### 5.2 Datavalidering per rad

#### 5.2.1 Obligatoriska fält
- **Datum:** Måste finnas och kunna parsas
- **Belopp:** Måste finnas och kunna konverteras till decimal
- **Beskrivning:** Måste finnas (får inte vara tom efter trimning)

#### 5.2.2 Datumvalidering
- Måste vara ett giltigt datum
- Får inte vara senare än dagens datum + 7 dagar
- Får inte vara äldre än 10 år (varning, ej blocking)
- Format: YYYY-MM-DD, DD.MM.YYYY, DD-MM-YYYY, YYYY/MM/DD

#### 5.2.3 Beloppsvalidering
- Måste vara ett giltigt decimaltal
- Får inte vara 0 (varning, ej blocking)
- Absolut värde max 10 000 000 (tio miljoner)
- Max 2 decimaler

#### 5.2.4 Beskrivningsvalidering
- Max längd: 500 tecken
- Minsta längd: 1 tecken (efter trimning)
- Tillåtna tecken: alla Unicode-tecken

### 5.3 Dubbletthantering

En transaktion anses vara en dubblett om **alla** följande matchar en befintlig transaktion:
- **Datum** (samma dag)
- **Belopp** (exakt samma belopp)
- **Beskrivning** (case-insensitive jämförelse)
- **IsIncome** (samma typ)

#### 5.3.1 Hantering av dubbletter
- Användaren ska informeras om antal dubbletter
- Dubbletter ska **inte** importeras automatiskt
- Användaren ska kunna välja att:
  - Hoppa över alla dubbletter (standard)
  - Importera dubbletter ändå (med bekräftelse)
  - Granska dubbletter individuellt (framtida funktion)

---

## 6. Felhantering och användarfeedback

### 6.1 Filnivå-fel

| Feltyp | Felmeddelande | Åtgärd |
|--------|---------------|--------|
| Ogiltig filtyp | "Filtypen stöds inte. Endast .csv-filer accepteras." | Avbryt import |
| Fil för stor | "Filen är för stor. Max storlek är 10 MB." | Avbryt import |
| Korrupt fil | "Filen kunde inte läsas. Kontrollera att filen är en giltig CSV-fil." | Avbryt import |
| Tomt radhuvud | "CSV-filen saknar kolumnnamn. Kontrollera filformatet." | Avbryt import |
| Inga transaktioner | "Inga giltiga transaktioner hittades i filen." | Avbryt import |

### 6.2 Radnivå-fel

Fel på radnivå ska loggas men **inte** avbryta hela importen. Istället ska:
- Felaktiga rader hoppas över
- Antal överhoppade rader rapporteras
- Detaljerad logg över fel göras tillgänglig för användaren

| Feltyp | Felmeddelande | Åtgärd |
|--------|---------------|--------|
| Ogiltigt datum | "Rad X: Datumet kunde inte parsas" | Hoppa över rad |
| Ogiltigt belopp | "Rad X: Beloppet kunde inte parsas" | Hoppa över rad |
| Saknad beskrivning | "Rad X: Beskrivning saknas" | Hoppa över rad |
| Dubblett | "Rad X: Transaktionen finns redan" | Hoppa över rad (loggad som dubblett) |

### 6.3 Framgångsrik import - feedback

Efter lyckad import ska användaren få en sammanfattning:
```
Import slutförd!

✓ 45 transaktioner importerade
⚠ 3 dubbletter överhoppades
✗ 2 rader kunde inte importeras (se logg för detaljer)

Totalt belopp importerat: 12 450,00 kr
Inkomster: 8 500,00 kr (5 transaktioner)
Utgifter: -3 950,00 kr (40 transaktioner)
```

### 6.4 Loggning

All import ska loggas med följande information:
- Tidpunkt för import
- Filnamn och storlek
- Antal rader totalt
- Antal importerade transaktioner
- Antal dubbletter
- Antal fel
- Detaljerad fellista (radnummer, feltyp, felmeddelande)

---

## 7. Användargränssnitt

### 7.1 Import-sida

#### 7.1.1 Plats i applikationen
- Huvudmeny: Lägg till menyalternativ "Importera transaktioner"
- URL: `/import`
- Ikon: Uppladdningsikon eller dokumentikon

#### 7.1.2 Layout och komponenter

**Steg 1: Bankval**
```
┌─────────────────────────────────────────┐
│  Välj bank                              │
│                                         │
│  ○ ICA-banken                          │
│  ○ Swedbank                            │
│  ○ Annan (detektera automatiskt)      │
│                                         │
│  [Nästa]                               │
└─────────────────────────────────────────┘
```

**Steg 2: Filuppladdning**
```
┌─────────────────────────────────────────┐
│  Välj fil att importera                │
│                                         │
│  ┌───────────────────────────────────┐ │
│  │                                   │ │
│  │     Dra och släpp CSV-fil här    │ │
│  │            eller                  │ │
│  │      [Välj fil från dator]       │ │
│  │                                   │ │
│  └───────────────────────────────────┘ │
│                                         │
│  Tillåtna format: .csv                 │
│  Max storlek: 10 MB                    │
│                                         │
│  [Tillbaka] [Importera]                │
└─────────────────────────────────────────┘
```

**Steg 3: Förhandsvisning och bekräftelse**
```
┌─────────────────────────────────────────┐
│  Förhandsvisning (första 5)            │
│                                         │
│  ┌───────────────────────────────────┐ │
│  │ Datum      Belopp    Beskrivning  │ │
│  │ 2025-01-15 -125,50   ICA Maxi     │ │
│  │ 2025-01-16 3500,00   Lön          │ │
│  │ ...                               │ │
│  └───────────────────────────────────┘ │
│                                         │
│  Sammanfattning:                       │
│  • 45 transaktioner kommer importeras │
│  • 3 dubbletter hittades (hoppas över)│
│                                         │
│  [Avbryt] [Bekräfta import]            │
└─────────────────────────────────────────┘
```

**Steg 4: Import pågår**
```
┌─────────────────────────────────────────┐
│  Importerar transaktioner...           │
│                                         │
│  ████████████████░░░░░░░░ 75%         │
│                                         │
│  Bearbetar rad 34 av 45...             │
└─────────────────────────────────────────┘
```

**Steg 5: Resultat**
```
┌─────────────────────────────────────────┐
│  ✓ Import slutförd!                    │
│                                         │
│  45 transaktioner importerade          │
│  3 dubbletter överhoppades             │
│  2 rader kunde inte importeras         │
│                                         │
│  [Visa logg] [Gå till transaktioner]  │
└─────────────────────────────────────────┘
```

### 7.2 UI-komponenter (MudBlazor)

Använd befintliga MudBlazor-komponenter:
- `MudFileUpload` för filuppladdning
- `MudRadioGroup` för bankval
- `MudTable` för förhandsvisning
- `MudProgressLinear` för importstatus
- `MudAlert` för felmeddelanden
- `MudCard` för layoutstruktur
- `MudButton` för knappar

### 7.3 Responsivitet

- Desktop: Full bredd med sidopaneler
- Tablet: Anpassad layout, stack-komponenter
- Mobil: Vertikal layout, större touch-targets

---

## 8. Teknisk implementation

### 8.1 Arkitektur

#### 8.1.1 Nya komponenter

**Backend (Privatekonomi.Core):**
```
Services/
  ├── ICsvImportService.cs        (Interface)
  ├── CsvImportService.cs         (Implementation)
  ├── Parsers/
  │   ├── ICsvParser.cs           (Interface)
  │   ├── IcaBankenParser.cs      (ICA-banken specifik parser)
  │   ├── SwedbankParser.cs       (Swedbank specifik parser)
  │   └── AutoDetectParser.cs     (Autodetektering)
  └── Validators/
      ├── CsvFileValidator.cs     (Filvalidering)
      └── TransactionValidator.cs (Transaktionsvalidering)

Models/
  └── CsvImportResult.cs          (Resultat av import)
  └── CsvImportError.cs           (Felmodell)
```

**Frontend (Privatekonomi.Web):**
```
Components/Pages/
  └── Import.razor                (Import-sida)
Components/Shared/
  └── ImportWizard.razor          (Wizard-komponent)
```

**API (Privatekonomi.Api):**
```
Controllers/
  └── ImportController.cs         (Import endpoints)
```

### 8.2 API Endpoints

#### POST /api/import/upload
- Laddar upp CSV-fil
- Validerar filformat
- Returnerar förhandsvisning och sammanfattning
- **Request:** multipart/form-data med fil
- **Response:** 
```json
{
  "success": true,
  "preview": [...],
  "summary": {
    "totalRows": 48,
    "validTransactions": 45,
    "duplicates": 3,
    "errors": 0
  }
}
```

#### POST /api/import/confirm
- Bekräftar och utför import
- Returnerar detaljerat resultat
- **Request:**
```json
{
  "fileId": "temp-guid",
  "bank": "Swedbank",
  "skipDuplicates": true
}
```
- **Response:**
```json
{
  "success": true,
  "imported": 45,
  "duplicates": 3,
  "errors": [],
  "log": "..."
}
```

### 8.3 Säkerhet

- **Filuppladdning:** Validera filtyp och storlek på servern
- **Temp-filer:** Lagra uppladdade filer temporärt med GUID-namn
- **Cleanup:** Ta bort temp-filer efter 1 timme eller efter lyckad import
- **Rate limiting:** Max 10 imports per användare per timme
- **Validering:** All input valideras på servern även om validering sker på klienten

---

## 9. Testning

### 9.1 Enhetstester

#### Parsers
- ✓ Parsa giltig ICA-banken CSV
- ✓ Parsa giltig Swedbank CSV
- ✓ Hantera olika datumformat
- ✓ Hantera olika decimalseparatorer
- ✓ Hantera olika teckenkodningar
- ✓ Felhantering för ogiltig data

#### Validators
- ✓ Validera fildatum
- ✓ Validera belopp
- ✓ Detektera dubbletter
- ✓ Hantera edge cases

#### Service
- ✓ Importera transaktioner
- ✓ Hoppa över dubbletter
- ✓ Loggning av fel

### 9.2 Integrationstester

- ✓ Full importflöde från uppladdning till sparad transaktion
- ✓ API endpoints returnerar korrekt data
- ✓ Databas-transaktioner hanteras korrekt

### 9.3 Testdata

Skapa testfiler för:
- **ICA-banken:** 
  - `ica_valid_50_transactions.csv` (50 giltiga transaktioner)
  - `ica_with_duplicates.csv` (innehåller dubbletter)
  - `ica_with_errors.csv` (innehåller felaktiga rader)
  
- **Swedbank:**
  - `swedbank_valid_100_transactions.csv` (100 giltiga transaktioner)
  - `swedbank_with_metadata.csv` (inkl. öppnings- och slutbalanser)
  - `swedbank_with_duplicates.csv` (innehåller dubbletter)
  - `swedbank_mixed_currency.csv` (SEK och EUR)

---

## 10. Framtida utökningar

### 10.1 Fas 2 - Fler banker
- SEB
- Nordea
- Handelsbanken
- Länsförsäkringar Bank

### 10.2 Fas 3 - Avancerade funktioner
- Automatisk kategoriförslag baserat på beskrivning
- Import av kontobalanser
- Schemalagd import
- Export av transaktioner till CSV

### 10.3 Fas 4 - Bank-API integration
- Open Banking API (PSD2)
- Automatisk synkronisering
- Realtidsuppdateringar

---

## 11. Acceptanskriterier

Implementeringen anses klar när följande kriterier är uppfyllda:

### Funktionella krav
- [ ] Användare kan importera CSV från ICA-banken
- [ ] Användare kan importera CSV från Swedbank
- [ ] Dubbletter detekteras och hanteras korrekt
- [ ] Felaktiga rader loggas och hoppas över
- [ ] Användaren får tydlig feedback om importresultat
- [ ] Importerade transaktioner visas i transaktionslistan
- [ ] UI är responsivt och fungerar på desktop och mobil

### Tekniska krav
- [ ] All kod är testad (>80% code coverage)
- [ ] API endpoints är dokumenterade (Swagger)
- [ ] Ingen säkerhetsrisk (validering, rate limiting)
- [ ] Performance: Kan importera 1000 transaktioner på <5 sekunder
- [ ] Loggning implementerad för debugging

### Dokumentation
- [ ] Användardokumentation uppdaterad
- [ ] API-dokumentation uppdaterad
- [ ] README uppdaterad med importfunktion
- [ ] Inline code comments för komplex logik

---

## 12. Projektplan och estimat

### 12.1 Utvecklingsfaser

| Fas | Beskrivning | Estimat | Beroenden |
|-----|-------------|---------|-----------|
| 1 | Design och specifikation | 1 dag | - |
| 2 | Backend: Parsers och validators | 3 dagar | Fas 1 |
| 3 | Backend: Import service | 2 dagar | Fas 2 |
| 4 | API: Import endpoints | 1 dag | Fas 3 |
| 5 | Frontend: Import-sida | 3 dagar | Fas 4 |
| 6 | Testning och bugfixar | 2 dagar | Fas 5 |
| 7 | Dokumentation | 1 dag | Fas 6 |

**Total estimerad tid:** 13 dagar

### 12.2 Prioritering

**Must-have (MVP):**
- Import från Swedbank (mest dokumenterat format)
- Grundläggande filvalidering
- Dubblettdetektion
- Enkel användarfeedback

**Should-have:**
- Import från ICA-banken
- Förhandsvisning
- Detaljerad loggning

**Nice-to-have:**
- Autodetektering av bank
- Grafisk progressbar
- Export-funktion

---

## 13. Risker och utmaningar

| Risk | Sannolikhet | Påverkan | Åtgärd |
|------|-------------|----------|--------|
| CSV-format ändras av banker | Medium | Hög | Versionshantering av parsers, flexibel parsing |
| Olika datumformat | Hög | Medium | Robust datumparser med multipla format |
| Specialtecken i beskrivningar | Hög | Låg | UTF-8 hantering, teckenvalidering |
| Stora filer (>10MB) | Låg | Medium | Streaming parsing, chunkad uppladdning |
| Användare laddar upp fel fil | Hög | Låg | Tydlig validering och felmeddelanden |

---

## 14. Referenser

### 14.1 Standarder och best practices
- CSV RFC 4180: https://tools.ietf.org/html/rfc4180
- ISO 8601 (Datumformat): https://www.iso.org/iso-8601-date-and-time-format.html

### 14.2 Externa resurser
- Swedbank API-dokumentation: https://developer.swedbank.com/
- Open Banking Standard (PSD2): https://www.openbanking.org.uk/

### 14.3 Liknande implementationer
- swed2beancount: https://github.com/Sudneo/swed2beancount
- beancountswedbank: https://pypi.org/project/beancountswedbank/

---

## Bilaga A: Exempel på CSV-filer

### A.1 ICA-banken exempel
```csv
Datum;Belopp;Beskrivning;Saldo
2025-01-15;-125,50;ICA Maxi Stockholm;8742,30
2025-01-16;3500,00;Lön;12242,30
2025-01-17;-45,00;Netflix AB;12197,30
2025-01-18;-850,00;Hyra januari;11347,30
```

### A.2 Swedbank exempel
```csv
"Client account";"Row type";"Date";"Beneficiary/Payer";"Details";"Amount";"Currency";"Debit/Credit";"Transfer reference";"Transaction type";"Reference number";"Document number";
"SE0000000000000000000000";"10";"01.01.2025";"";"Opening balance";"10000,00";"SEK";"K";"";"AS";"";"";
"SE0000000000000000000000";"20";"08.01.2025";"ICA MAXI STOCKHOLM";"Kortköp 123456789";"125,50";"SEK";"D";"";"MK";"";"5";
```

---

## Ändringshistorik

| Version | Datum | Författare | Ändring |
|---------|-------|-----------|---------|
| 1.0 | 2025-10-16 | Copilot AI | Första versionen av kravspecifikation |

---

**Godkännande:**
- [ ] Produktägare
- [ ] Teknisk lead
- [ ] UX-designer

**Nästa steg:**
1. Granska och godkänn specifikationen
2. Inhämta verkliga CSV-exempel från ICA-banken
3. Påbörja implementation enligt projektplan
