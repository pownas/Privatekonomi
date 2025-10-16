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

## 🏗️ Arkitektur

Projektet består av tre huvudkomponenter:

- **Privatekonomi.Web**: Blazor Server-applikation med MudBlazor UI
- **Privatekonomi.Api**: ASP.NET Core Web API med REST endpoints
- **Privatekonomi.Core**: Gemensamt klassbibliotek med modeller, services och dataåtkomst

## 🚀 Komma igång

### Förutsättningar

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)

### Installation och körning

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
│   ├── Privatekonomi.Web/          # Blazor Server applikation
│   │   ├── Components/
│   │   │   ├── Layout/             # Layout-komponenter
│   │   │   └── Pages/              # Sidor (Dashboard, Transactions, etc.)
│   │   └── Program.cs
│   ├── Privatekonomi.Api/          # Web API
│   │   ├── Controllers/            # API controllers
│   │   └── Program.cs
│   └── Privatekonomi.Core/         # Gemensamt bibliotek
│       ├── Data/                   # DbContext och dataåtkomst
│       ├── Models/                 # Datamodeller
│       └── Services/               # Business logic
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

## 🎯 Förbättringsförslag

- [ ] Fixa formulär-bindning i NewTransaction-sidan
- [ ] Lägga till användare och autentisering
- [ ] Implementera budget-funktionalitet
- [ ] Exportera data till Excel/CSV
- [ ] Lägg till diagram och grafer på Dashboard
- [ ] Integration med bank-API:er
- [ ] Mobilapp med samma funktionalitet
- [ ] Förbättra automatisk kategorisering med ML

## 📝 Licens

Detta projekt är skapat som ett AI-genererat exempel.

## 🤝 Bidra

Pull requests är välkomna! För större ändringar, öppna först en issue för att diskutera vad du vill ändra.
