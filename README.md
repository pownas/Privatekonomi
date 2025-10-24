# Privatekonomi

En privatekonomi-applikation byggd med .NET 9, Blazor Server och MudBlazor för att hjälpa användare att få koll och kontroll över sin ekonomi.

## 🎯 Funktioner

- **Användarautentisering**: Komplett användarsystem med registrering, inloggning och dataisolering per användare
- **Dashboard**: Översikt över totala inkomster, utgifter och nettoresultat
- **Transaktionshantering**: Registrera, visa och ta bort transaktioner
- **Budgethantering**: Skapa och följa upp budgetar med visualisering av planerat vs faktiskt utfall
- **Sparmål**: Sätt upp och följ sparmål med målbelopp, tidsgräns och prioritering
- **Gemensamma Sparmål**:
  - Skapa sparmål tillsammans med andra användare
  - Inbjudningssystem med accept/reject
  - Förslagssystem med demokratiska ändringar
  - Transaktionshistorik och notifieringar
  - Rollbaserad åtkomstkontroll (Owner/Participant)
  - Se [detaljerad dokumentation](wiki/README_Gemensamma_Sparmal.md)
- **Investeringshantering**: Hantera aktier, fonder och certifikat med översikt över värde och avkastning
- **Automatisk kursuppdatering**: Uppdatera aktiekurser via Yahoo Finance API med ett knapptryck
- **Kategorisystem**: Förkonfigurerade kategorier med färgkodning och hierarkisk struktur
- **Split-kategorisering**: Möjlighet att dela upp transaktioner i flera kategorier
- **Automatisk kategorisering**: 
  - Regelbaserad kategorisering med 44+ förkonfigurerade regler
  - Stöd för olika matchningstyper (innehåller, exakt, börjar med, slutar med, regex)
  - Prioritetsbaserad regelutvärdering
  - Användarvänligt gränssnitt för att hantera kategoriseringsregler
  - Systemet föreslår också kategorier baserat på tidigare transaktioner
- **Responsiv design**: Fungerar på desktop och mobila enheter
- **Flexibel datalagring**: 
  - Stöd för InMemory (utveckling), SQLite (produktion) och JSON-filer
  - Konfigurerbart via appsettings.json
  - Lämpligt för lokal användning, Raspberry Pi och NAS
  - Se [lagringsguide](docs/STORAGE_GUIDE.md) för mer information
- **CSV-import**: 
  - Import av transaktioner från ICA-banken och Swedbank
  - Import av investeringar från Avanza Bank med dubbletthantering
- **Automatisk bankimport via PSD2-API**:
  - Stöd för Swedbank, Avanza Bank och ICA Banken
  - OAuth2-baserad autentisering med BankID
  - Automatisk synkronisering av transaktioner
  - Realtidsdata från banken
- **CSV-export**: Exportera investeringar för analys och rapportering
- **Förbättrad datamodell**: Utökade modeller med audit trail, valutastöd och mer
- **Familjesamarbete**:
  - Hushållshantering med flera medlemmar
  - Delade utgifter med flexibel andelsfördelning
  - Barnkonton med veckopeng och sparande
  - Uppdrag-till-belöning system för sysslor
  - Gemensamma budgetar för hela familjen
- **Dark Mode & Tillgänglighet**:
  - MudBlazor Dark Mode med systempreferens-detektering
  - WCAG 2.1 Nivå AA compliance
  - Tangentbordsnavigation och fokusindikatorer
  - Optimerade färgkontraster för ljust och mörkt läge
- **Löneutveckling**:
  - Spåra och följa din lön över tid (hela karriären, 50+ år)
  - Visualisera löneutveckling med interaktiv graf
  - Beräkna genomsnittslön och lönetillväxt
  - Dokumentera jobbbyten och löneförhöjningar
  - Hantera befattning, arbetsgivare och anställningstyp

## 🏗️ Arkitektur

Projektet består av fem huvudkomponenter:

- **Privatekonomi.AppHost**: .NET Aspire orchestrator för att hantera och övervaka alla tjänster
- **Privatekonomi.ServiceDefaults**: Gemensamt bibliotek för Aspire service defaults (telemetri, health checks, resilience)
- **Privatekonomi.Web**: Blazor Server-applikation med MudBlazor UI
- **Privatekonomi.Api**: ASP.NET Core Web API med REST endpoints
- **Privatekonomi.Core**: Gemensamt klassbibliotek med modeller, services och dataåtkomst

### .NET Aspire Integration

Projektet använder .NET Aspire för förbättrad utvecklarupplevelse:
- **Centraliserad orkestration** av alla tjänster
- **Inbyggd observerbarhet** med OpenTelemetry (logs, traces, metrics)
- **Service discovery** för enkel tjänst-till-tjänst kommunikation
- **Health checks** för övervaking av tjänsters hälsa
- **Resilience patterns** (retry, circuit breaker, timeout)

Se [ASPIRE_GUIDE.md](wiki/ASPIRE_GUIDE.md) för mer information.

## 🚀 Komma igång

### Förutsättningar

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (för Aspire Orchestrator)
- [Node.js](https://nodejs.org/) (för Playwright-tester)

### Installation och körning

#### Snabbstart med startskript (Enklast för Codespaces)

1. Klona repositoriet:
```bash
git clone https://github.com/pownas/Privatekonomi.git
cd Privatekonomi
```

2. Kör startskriptet:

**Linux/macOS/Codespaces:**
```bash
./app-start.sh
```

**Windows (PowerShell):**
```powershell
.\app-start.ps1
```

Skriptet installerar automatiskt Aspire workload om det behövs och startar Aspire Dashboard som visar alla tjänster, logs, traces och metrics.

#### Alternativ 1: Kör med .NET Aspire Orchestrator (Manuellt)

.NET Aspire förenklar hanteringen av alla tjänster och ger inbyggd observerbarhet.

1. Installera Aspire workload:
```bash
dotnet workload install aspire
```

2. Klona repositoriet:
```bash
git clone https://github.com/pownas/Privatekonomi.git
cd Privatekonomi
```

3. Kör applikationen med Aspire:
```bash
cd src/Privatekonomi.AppHost
dotnet run
```

4. Aspire Dashboard öppnas automatiskt och visar alla tjänster, logs, traces och metrics.

Se [ASPIRE_GUIDE.md](wiki/ASPIRE_GUIDE.md) för detaljerad information om Aspire-funktionalitet.

#### Alternativ 2: Kör tjänster individuellt

1. Klona repositoriet:
```bash
git clone https://github.com/pownas/Privatekonomi.git
cd Privatekonomi
```

3. Installera .Net SDK
```bash
# install .Net
bash <(curl -sSL https://dot.net/v1/dotnet-install.sh) --channel 9.0 --install-dir "$HOME/.dotnet"

# add to current session (and add these lines to ~/.bashrc or ~/.profile to persist)
export DOTNET_ROOT="$HOME/.dotnet"
export PATH="$HOME/.dotnet:$PATH"
```

3. Bygg lösningen:
```bash
dotnet build
```

4. Kör Web-applikationen:
```bash
cd src/Privatekonomi.Web
dotnet run
```

5. Öppna webbläsaren och navigera till: `http://localhost:5274`

Alternativt kan du köra API-applikationen:
```bash
cd src/Privatekonomi.Api
dotnet run
```

API Swagger-dokumentation finns på: `http://localhost:5000/swagger`

### Testdata och Testanvändare

Applikationen seedas automatiskt med en testanvändare och ca **50 testransaktioner** vid start för utveckling och test.

**Testanvändare:**
- E-post: test@example.com
- Lösenord: Test123!

Testdata inkluderar:
- Realistiska svenska transaktioner (ICA, SL-kort, Hyra, Netflix, etc.)
- Spridda över de senaste 3 månaderna
- Olika kategorier med färgkodning
- Både inkomster och utgifter
- Belopp som varierar realistiskt per kategori

För att inaktivera testdata, kommentera bort seeding-koden i `Program.cs`.

Se Dashboard-skärmdumpen ovan för exempel på hur testdata presenteras i applikationen.

### CSV-Import

#### Import av transaktioner

Applikationen stöder import av transaktioner från CSV-filer från ICA-banken och Swedbank:

1. Navigera till **Importera** i menyn
2. Välj bank (ICA-banken eller Swedbank)
3. Ladda upp CSV-fil (max 10 MB)
4. Granska förhandsvisningen
5. Bekräfta importen

**Funktioner:**
- Automatisk dubblettdetektion
- Validering av datum, belopp och beskrivning
- Stöd för olika CSV-format per bank
- Förhandsvisning innan import
- Detaljerad sammanfattning efter import

Se [CSV_IMPORT_GUIDE.md](wiki/CSV_IMPORT_GUIDE.md) för detaljerad guide och exempel.

#### Import av investeringar från Avanza

Applikationen stöder import av investeringar från Avanza Bank:

1. Exportera dina innehav från Avanza (två format stöds):
   - **Mitt innehav fördelat per konto** - med kontonummer
   - **Mitt sammanställda innehav** - utan kontonummer
2. Navigera till **Aktier & Fonder** i menyn
3. Klicka på **Importera**
4. Välj **Avanza** som bank
5. Ladda upp CSV-fil (max 10 MB)
6. Bekräfta importen

**Funktioner:**
- Automatisk dubblettdetektion baserat på ISIN och kontonummer
- Stöd för båda Avanza CSV-format
- Uppdatering av befintliga investeringar
- Detaljerad sammanfattning efter import
- Visning av bank och konto i investeringslistan
- Filtrering per bank och konto
- Export till CSV för analys

Se [AVANZA_IMPORT_GUIDE.md](wiki/AVANZA_IMPORT_GUIDE.md) för detaljerad guide med skärmdumpar och felsökning.

## 📊 Skärmdumpar

### Dashboard
Översikt över totala inkomster, utgifter, nettoresultat och antal transaktioner. Visar även visualisering av kategorier med cirkeldiagram och stapeldiagram för utgifter per kategori och månad.

![Dashboard](https://github.com/user-attachments/assets/43a0efb5-c9bd-4a14-be1d-3f8fc0f6bc16)

### Transaktioner
Lista över alla transaktioner med datum, beskrivning, bank, kategori och belopp. Inkluderar sökfunktion och möjlighet att ta bort transaktioner.

![Transaktioner](https://github.com/user-attachments/assets/8af8bab6-5b9a-4daf-8dec-8ce18c480621)

### Budgethantering
Översikt över aktiva och avslutade budgetar med möjlighet att skapa nya budgetar och följa upp faktiskt utfall mot planerat.

![Budget](https://github.com/user-attachments/assets/dbd0d556-e37a-43df-99fb-a99f09ffdd40)

### Importera Transaktioner
Import av transaktioner från CSV-filer från ICA-banken och Swedbank med dubbletthantering och förhandsvisning.

![Importera](https://github.com/user-attachments/assets/e352caaf-230e-4032-baf0-b850667760f0)

### Kategorier
Hantering av utgifts- och inkomstkategorier med färgkodning för enkel överblick.

![Kategorier](https://github.com/user-attachments/assets/fde2ebab-21a6-4a16-8145-08b585abdcc1)

## 🎨 Teknisk stack

- **Frontend**: Blazor Server med MudBlazor
- **Backend**: ASP.NET Core Web API
- **Databas**: Entity Framework Core InMemory (kan migreras till SQL Server)
- **UI-komponenter**: MudBlazor
- **Språk**: C# (.NET 9)

## 📁 Projektstruktur

```
Privatekonomi/
├── src/
│   ├── Privatekonomi.AppHost/        # Aspire orchestrator
│   ├── Privatekonomi.ServiceDefaults/ # Aspire service defaults
│   ├── Privatekonomi.Web/            # Blazor Server applikation
│   │   ├── Components/
│   │   │   ├── Layout/               # Layout-komponenter
│   │   │   └── Pages/                # Sidor (Dashboard, Transactions, etc.)
│   │   └── Program.cs
│   ├── Privatekonomi.Api/            # Web API
│   │   ├── Controllers/              # API controllers
│   │   └── Program.cs
│   └── Privatekonomi.Core/           # Gemensamt bibliotek
│       ├── Data/                     # DbContext och dataåtkomst
│       ├── Models/                   # Datamodeller
│       └── Services/                 # Business logic
└── Privatekonomi.sln
```

## 🔧 Konfiguration

### Lagringsmetoder

Applikationen stödjer flera lagringsmetoder som enkelt kan konfigureras via `appsettings.json`:

#### Utveckling (InMemory med testdata)
```json
{
  "Storage": {
    "Provider": "InMemory",
    "ConnectionString": "",
    "SeedTestData": true
  }
}
```

#### Produktion (SQLite)
```json
{
  "Storage": {
    "Provider": "Sqlite",
    "ConnectionString": "Data Source=privatekonomi.db",
    "SeedTestData": false
  }
}
```

#### Raspberry Pi / NAS (SQLite på delad lagring)
```json
{
  "Storage": {
    "Provider": "Sqlite",
    "ConnectionString": "Data Source=/mnt/nas/privatekonomi.db",
    "SeedTestData": false
  }
}
```

Se [STORAGE_GUIDE.md](docs/STORAGE_GUIDE.md) för detaljerad information om:
- Olika lagringsalternativ
- Nätverksåtkomst och delad lagring
- Backup och återställning
- Migration mellan lagringsmetoder
- Felsökning och prestanda

### Legacy: Databasmigrering till SQL Server

Om du vill använda SQL Server istället för SQLite (för storskalig användning):

1. Installera EF Core SQL Server-paketet:
```bash
dotnet add src/Privatekonomi.Core/Privatekonomi.Core.csproj package Microsoft.EntityFrameworkCore.SqlServer
```

2. Uppdatera `StorageExtensions.cs` för att lägga till SQL Server-stöd

3. Lägg till connection string i `appsettings.json`:
```json
{
  "Storage": {
    "Provider": "SqlServer",
    "ConnectionString": "Server=localhost;Database=Privatekonomi;Trusted_Connection=True;MultipleActiveResultSets=true",
    "SeedTestData": false
  }
}
```

## 📋 Dokumentation

### Användarguider

- **[USER_AUTHENTICATION.md](docs/USER_AUTHENTICATION.md)**: Guide för användarregistrering, inloggning och datahantering
- **[CSV_IMPORT_GUIDE.md](wiki/CSV_IMPORT_GUIDE.md)**: Guide för import av transaktioner från ICA-banken och Swedbank
- **[PSD2_API_GUIDE.md](wiki/PSD2_API_GUIDE.md)**: Guide för automatisk bankimport via PSD2-API (Swedbank, Avanza, ICA Banken)
- **[AVANZA_IMPORT_GUIDE.md](wiki/AVANZA_IMPORT_GUIDE.md)**: Guide för import av investeringar från Avanza Bank
- **[STOCK_PRICE_API_GUIDE.md](wiki/STOCK_PRICE_API_GUIDE.md)**: Guide för automatisk uppdatering av aktiekurser via API
- **[BUDGET_GUIDE.md](wiki/BUDGET_GUIDE.md)**: Guide för budgethantering
- **[AUTOMATIC_CATEGORIZATION.md](docs/AUTOMATIC_CATEGORIZATION.md)**: Guide för automatisk kategorisering av transaktioner
- **[FAMILY_COLLABORATION_GUIDE.md](wiki/FAMILY_COLLABORATION_GUIDE.md)**: Guide för familjesamarbete, barnkonton och veckopeng
- **[DARK_MODE_IMPLEMENTATION.md](docs/DARK_MODE_IMPLEMENTATION.md)**: Guide för MudBlazor Dark Mode och WCAG-compliance
- **[DARK_MODE_TESTING.md](docs/DARK_MODE_TESTING.md)**: Testguide för dark mode och tillgänglighet
- **[SALARY_HISTORY_GUIDE.md](wiki/SALARY_HISTORY_GUIDE.md)**: Guide för löneutveckling och lönehistorik

### Sverige-specifika integrationer (NYT!)

- **[SWEDISH_INTEGRATIONS_SUMMARY.md](wiki/SWEDISH_INTEGRATIONS_SUMMARY.md)**: Sammanfattning av Sverige-specifika funktioner
- **[SWEDISH_INTEGRATIONS_EVALUATION.md](wiki/SWEDISH_INTEGRATIONS_EVALUATION.md)**: Utvärdering av genomförbarhet
- **[SWEDISH_INTEGRATIONS_IMPLEMENTATION.md](wiki/SWEDISH_INTEGRATIONS_IMPLEMENTATION.md)**: Implementationsguide

**Funktioner:**
- ✅ ROT/RUT-avdrag
- ✅ K4 kapitalvinstrapport
- ✅ ISK/KF schablonbeskattning
- ✅ SIE-export för bokföring
- ✅ Bolån med bindningstid
- ✅ CSN-lån
- ✅ Reseavdrag
- ⚠️ BankID (planerad)
- ⚠️ Fortnox/Visma integration (planerad)

### Teknisk dokumentation

- **[ProgramSpecifikation.md](wiki/ProgramSpecifikation.md)**: Övergripande programspecifikation
- **[ASPIRE_GUIDE.md](wiki/ASPIRE_GUIDE.md)**: Guide för .NET Aspire Orchestrator
- **[Kravspecifikation_Loneutveckling.md](wiki/Kravspecifikation_Loneutveckling.md)**: Kravspecifikation för löneutvecklingsfunktionen
- **[Kravspecifikation_CSV_Import.md](wiki/Kravspecifikation_CSV_Import.md)**: Kravspecifikation för CSV-import av transaktioner
- **[Kravspecifikation_Avanza_Integration.md](wiki/Kravspecifikation_Avanza_Integration.md)**: Kravspecifikation för Avanza-integration
- **[Implementationsguide_Avanza.md](wiki/Implementationsguide_Avanza.md)**: Implementationsguide för Avanza-funktionalitet
- **[Datamodell_Forbattringar.md](docs/Datamodell_Forbattringar.md)**: Dokumentation av datamodellförbättringar och nya funktioner

## 🧪 Testning

### End-to-End tester med Playwright

Projektet inkluderar Playwright-tester för att verifiera användargränssnittet:

```bash
cd tests/playwright
npm install
npx playwright install chromium
npm test
```

Testerna verifierar:
- ✅ Att alla 50 testransaktioner visas korrekt
- ✅ Korrekt formatering av datum, belopp och kategorier
- ✅ Sökfunktionalitet fungerar
- ✅ Kategorier visas med färgkodade chips
- ✅ Både inkomster och utgifter presenteras

Se [tests/playwright/README.md](tests/playwright/README.md) för detaljerad dokumentation.

## 🎯 Förbättringsförslag

För en omfattande analys av förbättringsmöjligheter, se:
- **[Fullständiga Förbättringsförslag](docs/IMPROVEMENT_SUGGESTIONS.md)** - Detaljerad analys med 45+ förbättringsförslag
- **[Sammanfattning](docs/IMPROVEMENT_SUMMARY.md)** - Snabböversikt och prioriterad plan

### Högt Prioriterade
- [ ] Byt från InMemory till persistent databas (SQL Server)
- [ ] Fixa nullable reference warnings (4 st)
- [ ] Implementera enhetstester (0% täckning för närvarande)
- [x] Lägg till användarautentisering med ASP.NET Core Identity
- [ ] Skapa CI/CD pipeline med GitHub Actions
- [ ] Implementera global exception handler
- [ ] Lägg till strukturerad logging

### Funktionsönskemål
- [ ] Fixa formulär-bindning i NewTransaction-sidan
- [x] Implementera budget-funktionalitet
- [x] Kravspecifikation för CSV-import från banker
- [x] Implementera CSV-import från ICA-banken och Swedbank
- [x] Implementera CSV-import från Avanza för investeringar
- [x] Exportera investeringar till CSV
- [x] Automatisk uppdatering av aktiekurser via API
- [x] Automatisk bankimport / PSD2-API-stöd (Swedbank, Avanza, ICA Banken)
- [x] Familjesamarbete: Hushållshantering med delade utgifter
- [x] Barnkonton med veckopeng och uppdrag-till-belöning
- [x] Gemensamma familjebudgetar
- [ ] Grafiskt gränssnitt för hantering av bankkopplingar
- [ ] Exportera transaktioner och budget till Excel/CSV
- [ ] Lägg till diagram och grafer på Dashboard
- [ ] Mobilapp med samma funktionalitet
- [ ] Förbättra automatisk kategorisering med ML
- [ ] Automatisk schemaläggning av återkommande uppdrag för barn

## 📝 Licens

Detta projekt är skapat som ett AI-genererat exempel.

## 🤝 Bidra

Pull requests är välkomna! För större ändringar, öppna först en issue för att diskutera vad du vill ändra.
