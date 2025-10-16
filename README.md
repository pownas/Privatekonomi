# Privatekonomi

En privatekonomi-applikation byggd med .NET 9, Blazor Server och MudBlazor för att hjälpa användare att få koll och kontroll över sin ekonomi.

## 🎯 Funktioner

- **Dashboard**: Översikt över totala inkomster, utgifter och nettoresultat
- **Transaktionshantering**: Registrera, visa och ta bort transaktioner
- **Kategorisystem**: Förkonfigurerade kategorier med färgkodning
- **Split-kategorisering**: Möjlighet att dela upp transaktioner i flera kategorier
- **Automatisk kategorisering**: Systemet föreslår kategorier baserat på tidigare transaktioner
- **Responsiv design**: Fungerar på desktop och mobila enheter
- **In-memory databas**: Använder Entity Framework Core InMemory för snabb utveckling
- **CSV-import**: Import av transaktioner från ICA-banken och Swedbank med dubbletthantering och validering

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

Se [ASPIRE_GUIDE.md](ASPIRE_GUIDE.md) för mer information.

## 🚀 Komma igång

### Förutsättningar

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (för Aspire Orchestrator)
- [Node.js](https://nodejs.org/) (för Playwright-tester)

### Installation och körning

#### Alternativ 1: Kör med .NET Aspire Orchestrator (Rekommenderat)

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

Se [ASPIRE_GUIDE.md](ASPIRE_GUIDE.md) för detaljerad information om Aspire-funktionalitet.

#### Alternativ 2: Kör tjänster individuellt

1. Klona repositoriet:
```bash
git clone https://github.com/pownas/Privatekonomi.git
cd Privatekonomi
```

2. Bygg lösningen:
```bash
dotnet build
```

3. Kör Web-applikationen:
```bash
cd src/Privatekonomi.Web
dotnet run
```

4. Öppna webbläsaren och navigera till: `http://localhost:5274`

Alternativt kan du köra API-applikationen:
```bash
cd src/Privatekonomi.Api
dotnet run
```

API Swagger-dokumentation finns på: `http://localhost:5000/swagger`

### Testdata

Applikationen seedas automatiskt med ca **50 testransaktioner** vid start för utveckling och test. Testdata inkluderar:
- Realistiska svenska transaktioner (ICA, SL-kort, Hyra, Netflix, etc.)
- Spridda över de senaste 3 månaderna
- Olika kategorier med färgkodning
- Både inkomster och utgifter
- Belopp som varierar realistiskt per kategori

För att inaktivera testdata, kommentera bort `TestDataSeeder.SeedTestData(context);` i `Program.cs`.

### CSV-Import

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

Se [CSV_IMPORT_GUIDE.md](CSV_IMPORT_GUIDE.md) för detaljerad guide och exempel.

## 📊 Skärmdumpar

### Dashboard
![Dashboard](https://github.com/user-attachments/assets/fb4eacaf-f7f8-47e5-9c08-99da4425e5ca)

### Ny Transaktion
![Ny Transaktion](https://github.com/user-attachments/assets/36a53eb7-a145-481a-805c-6a9f07663ac9)

### Transaktioner
![Transaktioner](https://github.com/user-attachments/assets/7124e7d3-5059-4bc3-8dc6-e004b1481d66)

### Kategorier
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

### Databasmigrering

För att migrera från InMemory-databasen till SQL Server:

1. Installera EF Core SQL Server-paketet:
```bash
dotnet add src/Privatekonomi.Core/Privatekonomi.Core.csproj package Microsoft.EntityFrameworkCore.SqlServer
```

2. Uppdatera `Program.cs` i både Web och Api-projekten:
```csharp
builder.Services.AddDbContext<PrivatekonomyContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
```

3. Lägg till connection string i `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=Privatekonomi;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

## 📋 Dokumentation

- **[ProgramSpecifikation.md](ProgramSpecifikation.md)**: Övergripande programspecifikation för applikationen
- **[Kravspecifikation_CSV_Import.md](Kravspecifikation_CSV_Import.md)**: Detaljerad kravspecifikation för CSV-import från ICA-banken och Swedbank
- **[CSV_IMPORT_GUIDE.md](CSV_IMPORT_GUIDE.md)**: Användarguide för CSV-import med exempel och felsökning

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

### Skärmdump av testdata

![Transaktioner med testdata](https://github.com/user-attachments/assets/93e402c7-ee15-4984-8053-9fe84512a9b5)

## 🎯 Förbättringsförslag

- [ ] Fixa formulär-bindning i NewTransaction-sidan
- [ ] Lägga till användare och autentisering
- [ ] Implementera budget-funktionalitet
- [x] Kravspecifikation för CSV-import från banker
- [x] Implementera CSV-import från ICA-banken och Swedbank
- [ ] Exportera data till Excel/CSV
- [ ] Lägg till diagram och grafer på Dashboard
- [ ] Integration med bank-API:er
- [ ] Mobilapp med samma funktionalitet
- [ ] Förbättra automatisk kategorisering med ML

## 📝 Licens

Detta projekt är skapat som ett AI-genererat exempel.

## 📚 Dokumentation

- **[ASPIRE_GUIDE.md](ASPIRE_GUIDE.md)**: Guide för .NET Aspire Orchestrator
- **[ProgramSpecifikation.md](ProgramSpecifikation.md)**: Övergripande programspecifikation för applikationen
- **[Kravspecifikation_CSV_Import.md](Kravspecifikation_CSV_Import.md)**: Detaljerad kravspecifikation för CSV-import från ICA-banken och Swedbank
- **[CSV_IMPORT_GUIDE.md](CSV_IMPORT_GUIDE.md)**: Användarguide för CSV-import med exempel och felsökning

## 🤝 Bidra

Pull requests är välkomna! För större ändringar, öppna först en issue för att diskutera vad du vill ändra.
