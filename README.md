# Privatekonomi

En privatekonomi-applikation byggd med .NET 9, Blazor Server och MudBlazor för att hjälpa användare att få koll och kontroll över sin ekonomi.

## 🎯 Funktioner

- **Användarautentisering**: Komplett användarsystem med registrering, inloggning och dataisolering per användare
- **Dashboard**: Översikt över totala inkomster, utgifter och nettoresultat
- **Transaktionshantering**: Registrera, visa och ta bort transaktioner
- **Budgethantering**: Skapa och följa upp budgetar med visualisering av planerat vs faktiskt utfall
  - **Svenska budgetmallar**: ⭐ NYT!
    - Förbyggda budgetmallar baserade på Länsförsäkringar's riktlinjer
    - Svenska Familjehushåll (15% sparkvot)
    - Svenska Singelhushåll (20% sparkvot) 
    - 50/30/20-regeln, Zero-based budgeting, Kuvertbudget
    - Stöd för årskostnader uppdelat månadsvis (t.ex. gymkort 1800 kr/år = 150 kr/månad)
    - Guidning för att behandla sparande som månadskostnad
    - Separering av mat i butik vs restaurang
    - Se [Budget guide](docs/BUDGET_GUIDE.md) och [Snabbguide för hushållsbudget](docs/HUSHALLSBUDGET_SNABBGUIDE.md)
- **Konsumentverket Jämförelse**:
  - Jämför din hushållsbudget med Konsumentverkets officiella riktlinjer 2025
  - Stöd för alla åldersgrupper och hushållsstorlekar (1-7 personer)
  - Inkluderar livsmedel, individuella och hushållsgemensamma kostnader
  - Visuell jämförelse med diagram och färgkodade differenser
  - Se [Konsumentverket Jämförelse guide](docs/KONSUMENTVERKET_JAMFORELSE.md)
- **Sparmål**: Sätt upp och följ sparmål med målbelopp, tidsgräns och prioritering
  - **Målstolpar**: Automatiska delmål (25%, 50%, 75%, 100%) för att fira framsteg
  - Notifikationer när milestones uppnås
  - Historik över uppnådda milestones
  - Se [Målstolpar dokumentation](docs/GOAL_MILESTONES.md)
- **Gemensamma Sparmål**:
  - Skapa sparmål tillsammans med andra användare
  - Inbjudningssystem med accept/reject
  - Förslagssystem med demokratiska ändringar
  - Transaktionshistorik och notifieringar
  - Rollbaserad åtkomstkontroll (Owner/Participant)
  - Se [detaljerad dokumentation](docs/README_Gemensamma_Sparmal.md)
- **Investeringshantering**: Hantera aktier, fonder, ETF, certifikat, krypto och P2P-lån med översikt över värde och avkastning
  - Stöd för ISK, KF, AF och Depå
  - Aggregering per kontotyp och investeringstyp
  - Import från Avanza Bank
  - CSV-export för analys
- **Pensionshantering**: ⭐ NYT!
  - Spåra pensioner från flera leverantörer (AMF, Alecta, SEB, etc.)
  - Tjänstepension, privat pension och allmän pension
  - Avkastningsberäkning och månatliga inbetalningar
  - Rekommendation att hämta data från minpension.se
  - Se [Investeringar & Pension guide](docs/INVESTMENT_PENSION_GUIDE.md)
- **Automatisk kursuppdatering**: Uppdatera aktiekurser via Yahoo Finance API med ett knapptryck
- **Kategorisystem**: Förkonfigurerade kategorier med färgkodning och hierarkisk struktur
  - **BAS 2025-baserad kontoplan**: Kontonummer inspirerade av svensk BAS-standard för strukturerad bokföring
  - Redigerbar kontoplan med stöd för egna konton och underkategorier
  - Se [Kontoplan BAS 2025 guide](docs/KONTOPLAN_BAS_2025.md) för detaljerad information
- **Split-kategorisering**: Möjlighet att dela upp transaktioner i flera kategorier
- **Automatisk kategorisering**: 
  - Regelbaserad kategorisering med 44+ förkonfigurerade regler
  - Stöd för olika matchningstyper (innehåller, exakt, börjar med, slutar med, regex)
  - Prioritetsbaserad regelutvärdering
  - Användarvänligt gränssnitt för att hantera kategoriseringsregler
  - Systemet föreslår också kategorier baserat på tidigare transaktioner
- **Responsiv design**: Fungerar på desktop och mobila enheter
- **Flexibel datalagring**: 
  - Stöd för InMemory (utveckling), SQLite (produktion), SQL Server (storskalig produktion) och JsonFile (backup/portabilitet)
  - Konfigurerbart via appsettings.json
  - Lämpligt för lokal användning, Raspberry Pi, NAS och molnbaserad hosting
  - Se [lagringsguide](docs/STORAGE_GUIDE.md) för mer information
- **Data Persistens & Backup**:
  - Automatisk sparning var 5:e minut (för JsonFile provider)
  - Fullständig backup/export till JSON
  - **Export per år**: Exportera ekonomisk data uppdelat på år i JSON eller CSV format
  - Import med merge- eller ersättningsläge
  - Webbaserat gränssnitt för datahantering
  - Perfekt för Raspberry Pi-installationer - se [Raspberry Pi guide](docs/RASPBERRY_PI_GUIDE.md)
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
- **Mobil-optimerad UI med Gester**: ⭐ NYT!
  - Touch-optimerade gester för mobil användning
  - Swipe vänster/höger för ta bort/redigera transaktioner
  - Pull-to-refresh för uppdatering av data
  - Större touch targets (min 44×44px) enligt WCAG
  - Thumbzone-optimerad layout för enkel navigering
  - Bottom sheets för mobilmenyer
  - Se [Mobile Gestures Guide](docs/MOBILE_GESTURES_GUIDE.md)
- **Löneutveckling**:
  - Spåra och följa din lön över tid (hela karriären, 50+ år)
  - Visualisera löneutveckling med interaktiv graf
  - Beräkna genomsnittslön och lönetillväxt
  - Dokumentera jobbbyten och löneförhöjningar
  - Hantera befattning, arbetsgivare och anställningstyp
- **Smart Notifikationssystem**: ⭐ NYT!
  - Multi-kanal notifikationer (In-app, Email, SMS, Push, Slack, Teams)
  - Konfigurerbart per notifikationstyp
  - Do Not Disturb-scheman
  - Digest-läge för grupperade notifikationer
  - Prioritetsnivåer (Low, Normal, High, Critical)
  - 20+ notifikationstyper (budget, räkningar, sparmål, investeringar, etc.)
  - Se [Notifikationssystem guide](docs/NOTIFICATION_SYSTEM.md)

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

Se [ASPIRE_GUIDE.md](docs/ASPIRE_GUIDE.md) för mer information.

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

Skriptet säkerställer att .NET 9 finns installerat och startar Aspire Dashboard som visar alla tjänster, logs, traces och metrics.

#### Alternativ 1: Kör med .NET Aspire Orchestrator (Manuellt)

.NET Aspire förenklar hanteringen av alla tjänster och ger inbyggd observerbarhet.

1. Klona repositoriet:
```bash
git clone https://github.com/pownas/Privatekonomi.git
cd Privatekonomi
```

2. Kör applikationen med Aspire:
```bash
cd src/Privatekonomi.AppHost
dotnet run
```

3. Aspire Dashboard öppnas automatiskt och visar alla tjänster, logs, traces och metrics.

> Aspire-funktionaliteten levereras via projektets NuGet-paket – ingen separat workload-installation krävs längre.

Se [ASPIRE_GUIDE.md](docs/ASPIRE_GUIDE.md) för detaljerad information om Aspire-funktionalitet.

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

#### Lokal konfiguration (appsettings.local.json & User Secrets)

För lokal utveckling rekommenderas att du lägger dina maskinspecifika inställningar i `appsettings.local.json` och känsliga värden i **User Secrets**. Dessa filer laddas automatiskt när miljön heter `Local` (vilket `local-app-start.ps1` sätter åt dig).

1. **Skapa lokala konfigurationsfiler** (lagras utanför Git):
  - `src/Privatekonomi.Web/appsettings.local.json`
  - `src/Privatekonomi.Api/appsettings.local.json` (valfritt om du behöver andra värden i API:t)

  Exempel på innehåll utan hemligheter:
  ```json
  {
    "Storage": {
     "Provider": "Sqlite",
     "ConnectionString": "Data Source=C:/Data/privatekonomi-web.db",
     "SeedTestData": false
    }
  }
  ```

2. **Lägg till hemliga värden via User Secrets** (lagras i `%APPDATA%/Microsoft/UserSecrets/` på Windows):
  ```powershell
  # Web-projektet
  cd src/Privatekonomi.Web
  dotnet user-secrets init          # Endast första gången – redan satt i repo men skadar inte
  dotnet user-secrets set "Storage:ConnectionString" "Data Source=C:/Data/privatekonomi-web.db"

  # API-projektet
  cd ../Privatekonomi.Api
  dotnet user-secrets init
  dotnet user-secrets set "Storage:ConnectionString" "Data Source=C:/Data/privatekonomi-api.db"
  dotnet user-secrets set "Swedbank:ClientId" "din-client-id"
  dotnet user-secrets set "Swedbank:ClientSecret" "ditt-client-secret"
  ```

  Använd de nycklar som dokumenteras i respektive guide (t.ex. `docs/BANK_API_CREDENTIALS_GUIDE.md`) för andra hemligheter.

3. **Starta applikationen**:
  ```powershell
  cd ../../
  .\local-app-start.ps1
  ```

  Skriptet sätter `ASPNETCORE_ENVIRONMENT=Development` och `PRIVATEKONOMI_ENVIRONMENT=Local`, vilket gör att både standardinställningar och dina lokala overrides (appsettings + User Secrets) läses in samtidigt. All annan utveckling (t.ex. i GitHub Codespaces) använder fortsatt miljön `Development`.

  Som standard startar skriptet AppHost med `dotnet watch run` för hot reload. Lägg till flaggan `-NoWatch` om du vill köra utan watch-läget.

  I Aspire Dashboard syns tydligt vilken miljö (`Local/Development/Production`) och vilken lagringsprovider som används via miljövariablerna som exponeras för varje tjänst.

> `appsettings.local.json` finns i `.gitignore`, så du kan tryggt ha lokala inställningar utan risk att lägga dem i en commit.

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

Se [CSV_IMPORT_GUIDE.md](docs/CSV_IMPORT_GUIDE.md) för detaljerad guide och exempel.

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

Se [AVANZA_IMPORT_GUIDE.md](docs/AVANZA_IMPORT_GUIDE.md) för detaljerad guide med skärmdumpar och felsökning.

## 📊 Skärmdumpar

> **📸 Se [SCREENSHOTS.md](docs/SCREENSHOTS.md) för fullständig screenshot-dokumentation av alla funktioner!**

Dokumentationen innehåller screenshots av:
- Dashboard (ljust och mörkt läge)
- Transaktioner med split-kategorisering
- Budgethantering
- Lån & Krediter med amorteringsplan
- Löneutveckling över tid
- Och mycket mer...

### Dashboard
Översikt över totala inkomster, utgifter, nettoresultat och antal transaktioner. Visar även visualisering av kategorier med cirkeldiagram och stapeldiagram för utgifter per kategori och månad.

![Dashboard](https://github.com/user-attachments/assets/c189bdfc-981c-447e-a46d-16425d865389)

### Transaktioner
Lista över alla transaktioner med datum, beskrivning, bank, kategori och belopp. Inkluderar sökfunktion och möjlighet att ta bort transaktioner.

![Transaktioner](https://github.com/user-attachments/assets/7532e67c-73ea-4327-8798-fec454b1b22f)

### Budgethantering
Översikt över aktiva och avslutade budgetar med möjlighet att skapa nya budgetar och följa upp faktiskt utfall mot planerat.

![Budget](https://github.com/user-attachments/assets/cd89a07a-ef13-4444-8caf-168b7213eeb6)

### Importera Transaktioner
Import av transaktioner från CSV-filer från ICA-banken och Swedbank med dubbletthantering och förhandsvisning.

![Importera](https://github.com/user-attachments/assets/18c51318-823f-476e-a571-f5fc0969dade)

### Kategorier
Hantering av utgifts- och inkomstkategorier med färgkodning för enkel överblick.

![Kategorier](https://github.com/user-attachments/assets/642cd585-7954-43da-8a85-e1c4a97f19fa)

## 🎨 Teknisk stack

- **Frontend**: Blazor Server med MudBlazor
- **Backend**: ASP.NET Core Web API
- **Databas**: Entity Framework Core med flera providers (InMemory, SQLite, SQL Server, JsonFile)
- **UI-komponenter**: MudBlazor
- **Språk**: C# (.NET 9)
- **Orchestration**: .NET Aspire

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

#### Storskalig produktion (SQL Server)
```json
{
  "Storage": {
    "Provider": "SqlServer",
    "ConnectionString": "Server=localhost;Database=Privatekonomi;Trusted_Connection=True;MultipleActiveResultSets=true",
    "SeedTestData": false
  }
}
```

#### Backup/Portabilitet (JsonFile)
```json
{
  "Storage": {
    "Provider": "JsonFile",
    "ConnectionString": "./data",
    "SeedTestData": false
  }
}
```

Se [STORAGE_GUIDE.md](docs/STORAGE_GUIDE.md) för detaljerad information om:
- Olika lagringsalternativ (InMemory, SQLite, SQL Server, JsonFile)
- Nätverksåtkomst och delad lagring
- Backup och återställning
- Migration mellan lagringsmetoder
- Felsökning och prestanda

## 📋 Dokumentation

### Användarguider

- **[USER_AUTHENTICATION.md](docs/USER_AUTHENTICATION.md)**: Guide för användarregistrering, inloggning och datahantering
- **[RASPBERRY_PI_GUIDE.md](docs/RASPBERRY_PI_GUIDE.md)**: ⭐ Komplett guide för installation på Raspberry Pi med lokal datalagring
- **[STORAGE_GUIDE.md](docs/STORAGE_GUIDE.md)**: Guide för olika lagringsalternativ och konfiguration
- **[YEAR_EXPORT_GUIDE.md](docs/YEAR_EXPORT_GUIDE.md)**: Guide för export av ekonomisk data per år
- **[CSV_IMPORT_GUIDE.md](docs/CSV_IMPORT_GUIDE.md)**: Guide för import av transaktioner från ICA-banken och Swedbank
- **[PSD2_API_GUIDE.md](docs/PSD2_API_GUIDE.md)**: Guide för automatisk bankimport via PSD2-API (Swedbank, Avanza, ICA Banken)
- **[AVANZA_IMPORT_GUIDE.md](docs/AVANZA_IMPORT_GUIDE.md)**: Guide för import av investeringar från Avanza Bank
- **[STOCK_PRICE_API_GUIDE.md](docs/STOCK_PRICE_API_GUIDE.md)**: Guide för automatisk uppdatering av aktiekurser via API
- **[INVESTMENT_PENSION_GUIDE.md](docs/INVESTMENT_PENSION_GUIDE.md)**: ⭐ NYT! Guide för investeringar och pensionshantering
- **[BUDGET_GUIDE.md](docs/BUDGET_GUIDE.md)**: Guide för budgethantering
- **[MOBILE_GESTURES_GUIDE.md](docs/MOBILE_GESTURES_GUIDE.md)**: ⭐ NYT! Guide för mobil-optimerad UI med touch-gester
- **[AUTOMATIC_CATEGORIZATION.md](docs/AUTOMATIC_CATEGORIZATION.md)**: Guide för automatisk kategorisering av transaktioner
- **[FAMILY_COLLABORATION_GUIDE.md](docs/FAMILY_COLLABORATION_GUIDE.md)**: Guide för familjesamarbete, barnkonton och veckopeng
- **[DARK_MODE_IMPLEMENTATION.md](docs/DARK_MODE_IMPLEMENTATION.md)**: Guide för MudBlazor Dark Mode och WCAG-compliance
- **[DARK_MODE_TESTING.md](docs/DARK_MODE_TESTING.md)**: Testguide för dark mode och tillgänglighet
- **[SALARY_HISTORY_GUIDE.md](docs/SALARY_HISTORY_GUIDE.md)**: Guide för löneutveckling och lönehistorik

### Sverige-specifika integrationer (NYT!)

- **[SWEDISH_INTEGRATIONS_SUMMARY.md](docs/SWEDISH_INTEGRATIONS_SUMMARY.md)**: Sammanfattning av Sverige-specifika funktioner
- **[SWEDISH_INTEGRATIONS_EVALUATION.md](docs/SWEDISH_INTEGRATIONS_EVALUATION.md)**: Utvärdering av genomförbarhet
- **[SWEDISH_INTEGRATIONS_IMPLEMENTATION.md](docs/SWEDISH_INTEGRATIONS_IMPLEMENTATION.md)**: Implementationsguide

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

- **[ProgramSpecifikation.md](docs/ProgramSpecifikation.md)**: Övergripande programspecifikation
- **[ASPIRE_GUIDE.md](docs/ASPIRE_GUIDE.md)**: Guide för .NET Aspire Orchestrator
- **[Kravspecifikation_Loneutveckling.md](docs/Kravspecifikation_Loneutveckling.md)**: Kravspecifikation för löneutvecklingsfunktionen
- **[Kravspecifikation_CSV_Import.md](docs/Kravspecifikation_CSV_Import.md)**: Kravspecifikation för CSV-import av transaktioner
- **[Kravspecifikation_Avanza_Integration.md](docs/Kravspecifikation_Avanza_Integration.md)**: Kravspecifikation för Avanza-integration
- **[Implementationsguide_Avanza.md](docs/Implementationsguide_Avanza.md)**: Implementationsguide för Avanza-funktionalitet
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

### Nya Förbättringsförslag 2025 🆕
- **[Förbättringsförslag 2025](docs/FÖRBÄTTRINGSFÖRSLAG_2025.md)** - 50+ nya idéer och förslag organiserade som en förslagslåda
- **[Issue Examples](docs/ISSUE_EXAMPLES.md)** - Färdiga GitHub issue-templates att kopiera och använda

### Befintliga Analyser
- **[Fullständiga Förbättringsförslag](docs/IMPROVEMENT_SUGGESTIONS.md)** - Detaljerad analys med 45+ förbättringsförslag
- **[Sammanfattning](docs/IMPROVEMENT_SUMMARY.md)** - Snabböversikt och prioriterad plan
- **[Funktionsanalys](docs/FUNKTIONSANALYS.md)** - Omfattande funktionskartläggning
- **[Åtgärdsplan](docs/ATGARDSPLAN.md)** - Roadmap med prioriterade issues

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
