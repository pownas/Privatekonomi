# Programspecifikation för Privatekonomi-applikation

## 1. Sammanfattning
En privatekonomi-applikation byggd med .NET 9, Blazor Server och MudBlazor med syfte att hjälpa användare att få koll och kontroll över sin ekonomi. Applikationen gör det möjligt att registrera inkomster och utgifter, kategorisera transaktioner, samt automatiskt kategorisera nya transaktioner baserat på tidigare inmatningar. Den stödjer även budgethantering, investeringshantering, och CSV-import från svenska banker som ICA-banken, Swedbank och Avanza.

---

## 2. Funktionalitet

### 2.1. Implementerade funktioner

#### Transaktionshantering
1. **Registrera transaktioner**:
   - Användaren kan lägga till inkomster och utgifter
   - Varje transaktion innehåller:
     - Datum
     - Belopp (positivt för inkomster, negativt för utgifter)
     - Beskrivning
     - Bank
     - Kategori
   - Stöd för split-kategorisering (dela upp transaktion i flera kategorier)

2. **Automatisk kategorisering**:
   - Nya transaktioner kategoriseras automatiskt baserat på tidigare transaktioner med samma eller liknande beskrivning

3. **CSV-Import**:
   - Import av transaktioner från ICA-banken och Swedbank
   - Automatisk dubblettdetektion
   - Validering av datum, belopp och beskrivning
   - Förhandsvisning innan import

#### Budgethantering
1. **Skapa budgetar**:
   - Månadsbudgetar och årsbudgetar
   - Ange planerade belopp per kategori
   - Automatisk beräkning av faktiskt utfall baserat på transaktioner

2. **Budgetuppföljning**:
   - Visuell presentation med progress bars
   - Jämförelse mellan planerat och faktiskt
   - Status (Aktiv, Kommande, Avslutad)
   - Detaljerad kategorivis uppdelning

#### Investeringshantering
1. **Hantera aktier, fonder och certifikat**:
   - Namn, ISIN, antal, inköpspris, nuvarande värde
   - Bank och kontonummer
   - Automatisk beräkning av avkastning

2. **Avanza-integration**:
   - Import från Avanza CSV-export
   - Stöd för båda formaten: "Mitt innehav fördelat per konto" och "Mitt sammanställda innehav"
   - Dubbletthantering baserat på ISIN och kontonummer
   - Filtrering per bank och konto
   - Export till CSV för analys

#### Dashboard och rapporter
1. **Översiktsdashboard**:
   - Totala inkomster, utgifter och nettoresultat
   - Antal transaktioner
   - Cirkeldiagram för utgifts- och inkomstfördelning per kategori
   - Stapeldiagram för utgifter per kategori och månad
   - Aktiva budgetar med progress
   - Transaktioner utan kategori
   - Senaste transaktioner

2. **Kategorisystem**:
   - Förkonfigurerade kategorier med färgkodning:
     - Mat & Dryck
     - Transport
     - Boende
     - Nöje
     - Shopping
     - Hälsa
     - Lön
     - Sparande
     - Övrigt

### 2.2. Teknisk arkitektur

Applikationen består av följande komponenter:

1. **Privatekonomi.AppHost**: .NET Aspire orchestrator för att hantera och övervaka alla tjänster
2. **Privatekonomi.ServiceDefaults**: Gemensamt bibliotek för Aspire service defaults (telemetri, health checks, resilience)
3. **Privatekonomi.Web**: Blazor Server-applikation med MudBlazor UI
4. **Privatekonomi.Api**: ASP.NET Core Web API med REST endpoints
5. **Privatekonomi.Core**: Gemensamt klassbibliotek med modeller, services och dataåtkomst

---

## 3. Användargränssnitt

### 3.1. Frontend
Frontend är byggd med **Blazor Server** och använder **MudBlazor** som UI-komponentbibliotek. Gränssnittet är responsivt och fungerar på både desktop och mobila enheter.

#### Implementerade vyer:
1. **Dashboard**:
   - Översikt över totala inkomster, utgifter och nettoresultat
   - Visualisering med cirkel- och stapeldiagram
   - Aktiva budgetar med progress
   - Transaktioner utan kategori
   - Senaste transaktioner

2. **Transaktioner**:
   - Lista över alla transaktioner i tabellformat
   - Sökfunktionalitet
   - Kolumner för datum, beskrivning, bank, kategori och belopp
   - Möjlighet att ta bort transaktioner

3. **Ny Transaktion**:
   - Formulär för att registrera nya transaktioner
   - Fält för datum, belopp, beskrivning, bank och kategori
   - Stöd för split-kategorisering

4. **Budget**:
   - Lista över alla budgetar (aktiva, kommande och avslutade)
   - Skapa ny budget med budgetposter per kategori
   - Detaljerad budgetuppföljning med jämförelse planerat vs faktiskt
   - Progress bars för varje kategori

5. **Aktier & Fonder**:
   - Lista över alla investeringar
   - Import från Avanza
   - Filtrering per bank och konto
   - Export till CSV
   - Statistik över totalt värde och avkastning

6. **Importera**:
   - Val av bank (ICA-banken, Swedbank, Avanza)
   - Uppladdning av CSV-fil
   - Förhandsvisning av data
   - Import med dubbletthantering

7. **Kategorier**:
   - Översikt över alla kategorier
   - Färgkodade chips för enkel identifiering
   - Möjlighet att skapa nya kategorier

8. **Hushåll**:
   - Hantering av hushåll (för framtida multi-user support)

9. **Lån & Krediter**:
   - Hantering av lån och krediter (planerad funktionalitet)

---

## 4. Backend

### 4.1. Backend-struktur
Backend är byggd med **.NET 9** och **ASP.NET Core** och erbjuder REST API endpoints för frontend-kommunikation.

#### Ansvarsområden:
1. **Databaslagring**:
   - CRUD-operationer för transaktioner, budgetar, investeringar och kategorier
   - Entity Framework Core med InMemory-databas (kan migreras till SQL Server)

2. **Affärslogik**:
   - Automatisk kategorisering av transaktioner baserat på tidigare transaktioner
   - Beräkning av budgetutfall baserat på transaktioner
   - Hantering av split-kategorisering
   - CSV-parsning och import från olika banker
   - Dubblettdetektion vid import

3. **Services**:
   - `TransactionService`: Hantering av transaktioner och kategorisering
   - `BudgetService`: Budgetberäkningar och uppföljning
   - `InvestmentService`: Hantering av investeringar
   - `CategoryService`: Kategorihantering
   - `CsvImportService`: Import av CSV-filer från banker

### 4.2. API Endpoints

#### Transaktioner
- `GET /api/transactions` - Hämta alla transaktioner
- `GET /api/transactions/{id}` - Hämta specifik transaktion
- `POST /api/transactions` - Skapa ny transaktion
- `PUT /api/transactions/{id}` - Uppdatera transaktion
- `DELETE /api/transactions/{id}` - Ta bort transaktion

#### Budgetar
- `GET /api/budgets` - Hämta alla budgetar
- `GET /api/budgets/{id}` - Hämta specifik budget
- `GET /api/budgets/active` - Hämta aktiva budgetar
- `GET /api/budgets/{id}/actual-amounts` - Hämta faktiska belopp
- `POST /api/budgets` - Skapa ny budget
- `PUT /api/budgets/{id}` - Uppdatera budget
- `DELETE /api/budgets/{id}` - Ta bort budget

#### Investeringar
- `GET /api/investments` - Hämta alla investeringar
- `POST /api/investments` - Skapa ny investering
- `POST /api/investments/import` - Importera från CSV
- `GET /api/investments/export` - Exportera till CSV

#### Kategorier
- `GET /api/categories` - Hämta alla kategorier
- `POST /api/categories` - Skapa ny kategori

---

## 5. Databasdesign

### 5.1. Implementerade tabeller/modeller

#### Transaction (Transaktioner)
- `TransactionId` (PK) - Unik identifierare
- `HouseholdId` (FK) - Koppling till hushåll
- `Date` - Datum för transaktion
- `Amount` - Belopp (negativt för utgifter, positivt för inkomster)
- `Description` - Beskrivning av transaktion
- `Bank` - Bank där transaktionen genomfördes
- `CategoryId` (FK, nullable) - Primär kategori
- Relationer: `TransactionSplits` för split-kategorisering

#### TransactionSplit (Split-kategorisering)
- `TransactionSplitId` (PK)
- `TransactionId` (FK)
- `CategoryId` (FK)
- `Percentage` - Procentandel av transaktionsbeloppet

#### Category (Kategorier)
- `CategoryId` (PK)
- `Name` - Kategorinamn
- `Type` - Typ (Income/Expense)
- `Color` - Färgkod för UI
- `Icon` - Ikon för UI

#### Budget (Budgetar)
- `BudgetId` (PK)
- `Name` - Budgetnamn
- `Description` - Beskrivning (optional)
- `StartDate` - Startdatum
- `EndDate` - Slutdatum
- `Period` - Period (Monthly/Yearly)
- Relationer: `BudgetCategories` för budgetposter

#### BudgetCategory (Budgetposter)
- `BudgetCategoryId` (PK)
- `BudgetId` (FK)
- `CategoryId` (FK)
- `PlannedAmount` - Planerat belopp för kategorin

#### Investment (Investeringar)
- `InvestmentId` (PK)
- `HouseholdId` (FK)
- `Name` - Namn på investering
- `ISIN` - ISIN-kod
- `Quantity` - Antal
- `PurchasePrice` - Inköpspris
- `CurrentValue` - Nuvarande värde
- `Bank` - Bank/mäklare
- `AccountNumber` - Kontonummer
- `Type` - Typ (Stock/Fund/Certificate)

#### Household (Hushåll)
- `HouseholdId` (PK)
- `Name` - Hushållsnamn
- `Description` - Beskrivning

#### Loan (Lån)
- `LoanId` (PK)
- `HouseholdId` (FK)
- `Name` - Lånenamn
- `Amount` - Lånebelopp
- `InterestRate` - Ränta
- `StartDate` - Startdatum
- `EndDate` - Slutdatum (optional)

---

## 6. Teknisk stack

### 6.1. Implementerad teknisk stack

#### Frontend
- **Blazor Server** - För dynamisk UI med C#
- **MudBlazor** - UI-komponentbibliotek för Material Design
- **ApexCharts.Blazor** - För diagram och visualisering

#### Backend
- **.NET 9** - Senaste versionen av .NET
- **ASP.NET Core** - Web API framework
- **Entity Framework Core** - ORM för databasåtkomst
- **InMemory Database** - För snabb utveckling (kan migreras till SQL Server)

#### Orkestration och Observerbarhet
- **.NET Aspire** - För orkestration, service discovery och observerbarhet
  - Aspire.Hosting - Orkestreringsplattform
  - OpenTelemetry - Telemetri och distribuerad spårning
  - Health Checks - Övervakning av tjänsternas hälsa

#### Testning
- **Playwright** - End-to-end testning av UI
- **xUnit** - Enhetstester (kan implementeras)

#### Deployment
- **Docker** - Containerisering (via .NET Aspire)
- **Azure/AWS** - Molnplattform (kan konfigureras)

---

## 7. Implementation Status

### 7.1. Implementerade funktioner ✅

#### Fas 1 – Grundläggande funktioner
- ✅ Backend API för transaktioner och kategorier
- ✅ Frontend-vyer för registrering och visning av transaktioner
- ✅ Databas med Entity Framework Core InMemory
- ✅ .NET Aspire integration för orkestration

#### Fas 2 – Automatisering och import
- ✅ Automatisk kategorisering av transaktioner
- ✅ Split-kategorisering (upp till 5 kategorier)
- ✅ CSV-import från ICA-banken och Swedbank
- ✅ CSV-import från Avanza för investeringar
- ✅ Dubbletthantering vid import

#### Fas 3 – Rapporter och visualisering
- ✅ Dashboard med översikt och diagram
- ✅ Cirkeldiagram för utgifts- och inkomstfördelning
- ✅ Stapeldiagram för utgifter per kategori och månad
- ✅ Budgethantering med visuell uppföljning
- ✅ Filtrering och sökning av transaktioner

#### Fas 4 – Investeringar och export
- ✅ Investeringshantering för aktier, fonder och certifikat
- ✅ Import från Avanza
- ✅ Export av investeringar till CSV
- ✅ Testdata för utveckling och test

### 7.2. Pågående utveckling 🚧

- 🚧 Formulär-bindning i NewTransaction-sidan (behöver fixas)
- 🚧 Migrering från InMemory till SQL Server databas
- 🚧 Lån & Krediter-funktionalitet (grundstruktur finns)

### 7.3. Planerade förbättringar 📋

- [ ] Användare och autentisering för multi-user support
- [ ] Export av transaktioner och budget till Excel/CSV
- [ ] Förbättrade diagram och grafer på Dashboard
- [ ] Automatisk uppdatering av aktiekurser via API
- [ ] Integration med bank-API:er för transaktioner (t.ex. Open Banking)
- [ ] Mobilapp med samma funktionalitet
- [ ] Förbättrad automatisk kategorisering med Machine Learning
- [ ] Notifieringar och påminnelser
- [ ] Budgetmallar och kopiera budget från föregående period
- [ ] Mål och sparande-funktionalitet

---

## 8. Testning

### 8.1. Implementerade tester

#### End-to-End tester med Playwright
Projektet inkluderar automatiserade UI-tester som verifierar:
- ✅ Att alla testransaktioner visas korrekt
- ✅ Korrekt formatering av datum, belopp och kategorier
- ✅ Sökfunktionalitet fungerar
- ✅ Kategorier visas med färgkodade chips
- ✅ Både inkomster och utgifter presenteras korrekt

### 8.2. Testdata

Applikationen inkluderar automatisk seeding av cirka 50 testransaktioner vid start för utveckling och testning:
- Realistiska svenska transaktioner (ICA, SL-kort, Hyra, Netflix, etc.)
- Spridda över de senaste 3 månaderna
- Olika kategorier med färgkodning
- Både inkomster och utgifter
- Belopp som varierar realistiskt per kategori

Testdata kan inaktiveras genom att kommentera bort `TestDataSeeder.SeedTestData(context);` i `Program.cs`.

## 9. Installation och körning

### 9.1. Förutsättningar
- .NET 9 SDK
- Docker Desktop (för Aspire Orchestrator)
- Node.js (för Playwright-tester)

### 9.2. Kör med .NET Aspire (Rekommenderat)
```bash
# Installera Aspire workload
dotnet workload install aspire

# Klona repository
git clone https://github.com/pownas/Privatekonomi.git
cd Privatekonomi

# Kör med Aspire
cd src/Privatekonomi.AppHost
dotnet run
```

Aspire Dashboard öppnas automatiskt och visar alla tjänster, logs, traces och metrics.

### 9.3. Kör tjänster individuellt
```bash
# Bygg lösningen
dotnet build

# Kör Web-applikationen
cd src/Privatekonomi.Web
dotnet run
# Tillgänglig på http://localhost:5274

# Eller kör API-applikationen
cd src/Privatekonomi.Api
dotnet run
# Swagger på http://localhost:5000/swagger
```

## 10. Dokumentation

All detaljerad dokumentation finns i wiki-katalogen:

### Användarguider
- **[BUDGET_GUIDE.md](BUDGET_GUIDE.md)**: Guide för budgethantering
- **[CSV_IMPORT_GUIDE.md](CSV_IMPORT_GUIDE.md)**: Guide för import av transaktioner
- **[AVANZA_IMPORT_GUIDE.md](AVANZA_IMPORT_GUIDE.md)**: Guide för import av investeringar från Avanza

### Teknisk dokumentation
- **[ASPIRE_GUIDE.md](ASPIRE_GUIDE.md)**: Guide för .NET Aspire Orchestrator
- **[Kravspecifikation_CSV_Import.md](Kravspecifikation_CSV_Import.md)**: Kravspec för CSV-import
- **[Kravspecifikation_Avanza_Integration.md](Kravspecifikation_Avanza_Integration.md)**: Kravspec för Avanza-integration
- **[Implementationsguide_Avanza.md](Implementationsguide_Avanza.md)**: Implementationsguide för Avanza
- **[IMPLEMENTATION_SUMMARY.md](IMPLEMENTATION_SUMMARY.md)**: Sammanfattning av implementationen
- **[IMPLEMENTATION_SUMMARY_AVANZA.md](IMPLEMENTATION_SUMMARY_AVANZA.md)**: Sammanfattning av Avanza-implementation
