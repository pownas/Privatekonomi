# Kravspecifikation: Löneutveckling över tid

## Översikt
Denna funktion möjliggör för användare att spåra och analysera sin löneutveckling över tid i Privatekonomi-appen. Funktionen är designad för att stödja hela arbetsliv (ca 50 år) av lönehistorik.

## Funktionella krav

### 1. Registrera löneinformation
- [x] Användare ska kunna registrera sin månadslön
- [x] Användare ska kunna ange period (månad/år) för lönen
- [x] Användare ska kunna ange befattning/jobbtitel
- [x] Användare ska kunna ange arbetsgivare
- [x] Användare ska kunna välja anställningstyp (Heltid, Deltid, Timavlönad, Konsult, Praktik)
- [x] Användare ska kunna ange tjänstegrad (arbetsgrad i procent)
- [x] Användare ska kunna lägga till anteckningar om löneändring
- [x] Användare ska kunna markera en lönepost som "nuvarande lön"
- [x] System ska automatiskt avmarkera andra poster när en markeras som nuvarande

### 2. Lagra lönehistorik
- [x] System ska lagra alla löneposter per användare
- [x] System ska isolera data per användare (dataskydd)
- [x] System ska spara tidsstämplar för när poster skapas/uppdateras
- [x] System ska stödja valuta (standard SEK)
- [x] System ska hantera långa tidsperioder (50+ år)

### 3. Visa lönehistorik
- [x] Användare ska kunna se en tabell över all lönehistorik
- [x] Tabellen ska visa: Period, Månadslön, Befattning, Arbetsgivare, Anställningstyp, Tjänstegrad, Nuvarande-status
- [x] Användare ska kunna söka i lönehistoriken
- [x] Användare ska kunna sortera löneposter
- [x] System ska visa tom status när ingen historik finns

### 4. Visualisera löneutveckling
- [x] System ska visa graf över löneutveckling över tid
- [x] Grafen ska använda linjediagram med mjuka kurvor
- [x] X-axeln ska visa perioder (månad/år)
- [x] Y-axeln ska visa lönebelopp i kronor

### 5. Beräkna statistik
- [x] System ska visa nuvarande lön
- [x] System ska beräkna genomsnittslön för de senaste 12 månaderna
- [x] System ska beräkna lönetillväxt i procent för senaste 12 månaderna
- [x] System ska visa totalt antal löneposter
- [x] Statistiken ska uppdateras automatiskt när data ändras

### 6. Redigera och ta bort löneposter
- [x] Användare ska kunna redigera befintliga löneposter
- [x] Användare ska kunna ta bort löneposter
- [x] System ska bekräfta borttagning med dialog
- [x] System ska visa success/error-meddelanden

### 7. Navigation
- [x] Funktionen ska vara tillgänglig från huvudmenyn
- [x] Menyalternativ ska heta "Löneutveckling"
- [x] Ikonen ska vara TrendingUp

## Icke-funktionella krav

### 1. Säkerhet och integritet
- [x] Löneuppgifter ska isoleras per användare
- [x] Ingen användare ska kunna se andras löneuppgifter
- [x] API-endpoints ska kräva autentisering
- [x] Användar-ID ska hanteras automatiskt av systemet

### 2. Skalbarhet
- [x] System ska stödja 50+ år av lönehistorik (600+ poster)
- [x] Databas ska vara indexerad för snabba sökningar
- [x] Paginering ska användas för stora datamängder

### 3. Användarvänlighet
- [x] Gränssnittet ska vara intuitivt och lättnavigerat
- [x] Formulär ska ha tydliga labels och hjälptexter
- [x] Felmeddelanden ska vara tydliga och hjälpsamma
- [x] Success-meddelanden ska bekräfta framgångsrika operationer

### 4. Responsiv design
- [x] Funktionen ska fungera på desktop
- [x] Funktionen ska fungera på mobila enheter
- [x] Tabeller ska vara scrollbara på små skärmar
- [x] Formulär ska anpassa sig till skärmstorlek

### 5. Prestanda
- [x] Laddningstid ska vara < 2 sekunder för normal datamängd
- [x] Grafritning ska vara optimerad
- [x] Databasquery ska använda index

## Datamodell

### SalaryHistory
| Fält | Typ | Beskrivning | Obligatorisk |
|------|-----|-------------|--------------|
| SalaryHistoryId | int | Primärnyckel | Ja |
| MonthlySalary | decimal(18,2) | Månadslön före skatt | Ja |
| Period | DateTime | Period (månad/år) | Ja |
| JobTitle | string(200) | Befattning | Nej |
| Employer | string(200) | Arbetsgivare | Nej |
| EmploymentType | string(50) | Anställningstyp | Nej |
| WorkPercentage | decimal(5,2) | Tjänstegrad i procent | Nej |
| Notes | string(1000) | Anteckningar | Nej |
| Currency | string(3) | Valuta (standard SEK) | Ja |
| IsCurrent | bool | Om detta är nuvarande lön | Ja |
| CreatedAt | DateTime | Skapad tidsstämpel | Ja |
| UpdatedAt | DateTime | Uppdaterad tidsstämpel | Nej |
| UserId | string | Användarkoppling | Ja |

### Index
- UserId (för snabb filtrering per användare)
- Period (för snabb sortering och tidsfiltrering)
- (UserId, Period) (för kombinerade queries)
- IsCurrent (för att snabbt hitta nuvarande lön)

## API-endpoints

### GET /api/salaryhistory
Hämta alla löneposter för inloggad användare.

**Response:**
```json
[
  {
    "salaryHistoryId": 1,
    "monthlySalary": 35000,
    "period": "2024-10-01T00:00:00",
    "jobTitle": "Utvecklare",
    "employer": "Tech AB",
    "employmentType": "Heltid",
    "workPercentage": 100,
    "notes": "Lönerevision",
    "currency": "SEK",
    "isCurrent": true,
    "createdAt": "2024-10-21T10:00:00",
    "updatedAt": null,
    "userId": "user123"
  }
]
```

### GET /api/salaryhistory/{id}
Hämta en specifik lönepost.

### GET /api/salaryhistory/current
Hämta nuvarande lön för inloggad användare.

### GET /api/salaryhistory/period?startPeriod={date}&endPeriod={date}
Hämta löneposter för specifik tidsperiod.

### GET /api/salaryhistory/average?months={number}
Beräkna genomsnittslön för antal månader bakåt.

**Response:**
```json
{
  "averageSalary": 34500.50,
  "months": 12
}
```

### GET /api/salaryhistory/growth?months={number}
Beräkna lönetillväxt i procent för antal månader bakåt.

**Response:**
```json
{
  "growthPercentage": 8.5,
  "months": 12
}
```

### POST /api/salaryhistory
Skapa ny lönepost.

**Request:**
```json
{
  "monthlySalary": 35000,
  "period": "2024-10-01T00:00:00",
  "jobTitle": "Utvecklare",
  "employer": "Tech AB",
  "employmentType": "Heltid",
  "workPercentage": 100,
  "notes": "Nytt jobb",
  "isCurrent": true
}
```

### PUT /api/salaryhistory/{id}
Uppdatera befintlig lönepost.

### DELETE /api/salaryhistory/{id}
Ta bort lönepost.

## Användargränssnitt

### Huvudvy
1. **Header**
   - Titel: "Löneutveckling"
   - Knapp: "Ny Lönepost"

2. **Statistikkort** (4 kort i rad)
   - Nuvarande Lön (primärfärg)
   - Genomsnitt 12 mån (info-färg)
   - Tillväxt 12 mån (success/error-färg)
   - Antal Poster (sekundärfärg)

3. **Graf**
   - Linjediagram med mjuka kurvor
   - X-axel: Period (ÅÅÅÅ-MM)
   - Y-axel: Lönebelopp (kr)
   - Döljs om ingen data finns

4. **Tabell**
   - Kolumner: Period, Månadslön, Befattning, Arbetsgivare, Anställningstyp, Tjänstegrad, Nuvarande, Åtgärder
   - Sökfält ovanför tabellen
   - Edit- och Delete-knappar per rad
   - Paginering

5. **Empty State** (när ingen data)
   - Ikon (TrendingUp)
   - Rubrik: "Ingen lönehistorik ännu"
   - Text: "Börja spåra din löneutveckling..."
   - Knapp: "Lägg till Lönepost"

### Formulär (Ny/Redigera Lönepost)
- Månadslön (kr) - obligatorisk, numeric
- Period (månad/år) - obligatorisk, datepicker
- Befattning - text
- Arbetsgivare - text
- Anställningstyp - dropdown (Heltid, Deltid, Timavlönad, Konsult, Praktik)
- Tjänstegrad (%) - numeric, 0-100
- Anteckningar - textarea
- "Detta är min nuvarande lön" - checkbox
- Knappar: "Lägg till"/"Uppdatera" och "Avbryt"

## Testscenarier

### 1. Lägg till första lönepost
1. Navigera till Löneutveckling
2. Klicka "Ny Lönepost"
3. Fyll i formulär med giltiga värden
4. Klicka "Lägg till"
5. Verifiera: Post visas i tabell, statistik uppdateras

### 2. Redigera lönepost
1. Klicka Edit-ikonen på en post
2. Ändra värden
3. Klicka "Uppdatera"
4. Verifiera: Ändringar sparas

### 3. Ta bort lönepost
1. Klicka Delete-ikonen
2. Bekräfta i dialog
3. Verifiera: Post tas bort, statistik uppdateras

### 4. Markera nuvarande lön
1. Skapa flera poster
2. Markera en som "nuvarande"
3. Verifiera: Endast en post markerad som nuvarande
4. Markera en annan som "nuvarande"
5. Verifiera: Första avmarkeras automatiskt

### 5. Visa löneutveckling över tid
1. Lägg till 6+ poster över olika perioder
2. Verifiera: Graf visas korrekt
3. Verifiera: Trendlinje följer värdena

### 6. Beräkna statistik
1. Lägg till flera poster
2. Verifiera: Genomsnitt beräknas korrekt
3. Verifiera: Tillväxt beräknas korrekt
4. Verifiera: Nuvarande lön visas

## Framtida förbättringar (ej implementerade)

### Exportera/importera
- [ ] Exportera lönehistorik till CSV/Excel
- [ ] Importera historisk löneinformation från fil
- [ ] Integration med lönekö-leverantörer

### Notifieringar
- [ ] Påminnelse att uppdatera lön (t.ex. vid årsskifte)
- [ ] Notifiering vid lönerevision
- [ ] Automatiska rekommendationer baserat på marknadslöner

### Jämförelser
- [ ] Jämföra med statistik (SCB genomsnittslöner)
- [ ] Branschjämförelser
- [ ] Löneutvecklingsprognos

### Avancerad analys
- [ ] Inflation-justerad löneutveckling
- [ ] Reallöneutveckling
- [ ] Exportera till skattedeklaration
- [ ] Integration med pension och försäkring

## Teknisk implementation

### Backend
- **Språk**: C# (.NET 9)
- **Framework**: ASP.NET Core
- **ORM**: Entity Framework Core
- **Databas**: InMemory (kan migreras till SQL Server)

### Frontend
- **Framework**: Blazor Server
- **UI-bibliotek**: MudBlazor
- **Rendering**: InteractiveServer
- **Språk**: Svenska

### Säkerhet
- **Autentisering**: ASP.NET Core Identity
- **Authorization**: [Authorize] attribut på API
- **Dataisolering**: ICurrentUserService

## Slutsats
Denna funktion ger användare ett kraftfullt verktyg för att spåra och analysera sin löneutveckling över tid. Implementationen följer befintliga mönster i projektet och är designad för skalbarhet och långsiktig användning.
