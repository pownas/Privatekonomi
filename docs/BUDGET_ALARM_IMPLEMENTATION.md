# Budget Alarm och Real-time Övervakning - Implementeringssammanfattning

## Översikt

Denna implementation utökar det befintliga budgetalarmsystemet med ytterligare funktionalitet för real-time övervakning och veckovisa sammanfattningar enligt issue #3.2.

## Implementerade Funktioner

### 1. ✅ Budgetvarningar vid Tröskelvärden (75%, 90%, 100%)

**Status:** Redan implementerad i `BudgetAlertService.cs`

Systemet kontrollerar automatiskt budgetanvändning och skapar varningar när:
- 75% av budgeten är förbrukad (🟡 Gul varning)
- 90% av budgeten är förbrukad (🟠 Orange varning)  
- 100% av budgeten är överskriden (🔴 Röd varning)

```csharp
// Tröskelvärden definierade i BudgetAlertService
private static readonly decimal[] StandardThresholds = { 75m, 90m, 100m };
```

### 2. ✅ Prognoser och Daily Rate Beräkning

**Status:** Redan implementerad i `BudgetAlertService.cs`

Systemet beräknar:
- **Daglig utgiftstakt:** Totala utgifter / antal dagar sedan budgetstart
- **Dagar tills budget överskrids:** Återstående belopp / daglig takt
- **Prognos:** "Budget överskrids om X dagar i nuvarande takt"

```csharp
public async Task<int?> CalculateDaysUntilExceededAsync(int budgetId, int categoryId)
{
    var remaining = budgetCategory.PlannedAmount - spent;
    var dailyRate = await CalculateDailyRateAsync(budgetId, categoryId);
    var daysUntilExceeded = (int)Math.Ceiling(remaining / dailyRate);
    return daysUntilExceeded;
}
```

### 3. ✅ Real-time Notifieringar via SignalR

**Status:** Redan implementerad i `BudgetAlertHub.cs` och `BudgetAlertBackgroundService.cs`

- **SignalR Hub:** Möjliggör instant notifieringar till klienter
- **Background Service:** Kontrollerar budgetar var 30:e minut
- **WebSocket-anslutning:** Real-time uppdateringar utan siduppdatering

```csharp
// BudgetAlertBackgroundService kontrollerar var 30:e minut
private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(30);

// SignalR skickar varningar direkt till användare
await _hubContext.Clients.Group($"user_{budget.UserId}")
    .SendAsync("ReceiveBudgetAlert", alertData);
```

### 4. ✅ Veckovis Email-sammanfattning

**Status:** Ny implementation i `WeeklyBudgetDigestService.cs`

Skickar automatiska veckovisa sammanfattningar varje söndag kl 18:00 med:
- Sammanfattning av alla aktiva budgetar
- Visualisering med emojis (🟢🟡🟠🔴) för budgetstatus
- Prognoser för kategorier som närmar sig gränsen
- Antal kategorier över budget och som närmar sig gränsen

```csharp
// Körs varje söndag kl 18:00
private static readonly DayOfWeek DigestDay = DayOfWeek.Sunday;
private static readonly int DigestHour = 18;

// Email-innehåll exempel:
// # Veckosammanfattning - Budgetar
//
// **Sammanfattning:** 8 kategorier
// 🔴 2 över budget
// 🟡 3 närmar sig gränsen
//
// ## Budget 2025-01
// 🔴 **Mat & Dryck**: 7,650 kr / 7,500 kr (102%)
//    ⚠️ Prognos: Överskrids om 0 dagar (102 kr/dag)
// 🟡 **Transport**: 2,250 kr / 3,000 kr (75%)
//    Återstående: 750 kr
```

### 5. ✅ Budget Freeze (Budgetfrysning)

**Status:** Redan implementerad i `BudgetAlertService.cs` och `BudgetAlert.cs`

Automatisk frysning av budgetkategorier när de överskrids:
- Förhindrar nya utgifter i frysta kategorier
- Manuell avfrysning via UI
- Automatisk frysning kan aktiveras/inaktiveras i inställningar

```csharp
// Automatisk frysning vid 100% om aktiverat
if (settings.EnableBudgetFreeze && threshold >= 100m)
{
    await FreezeBudgetAsync(budgetId, budgetCategory.CategoryId, "Budget överskriden");
}
```

### 6. ✅ Budgetinställningar UI

**Status:** Ny implementation i `BudgetSettings.razor`

Komplett inställningssida på `/budgets/settings` med:
- **Varningströsklar:** Konfigurera 75%, 90%, 100% varningar
- **Anpassade trösklar:** Egna procentsatser (t.ex. 50%, 85%, 95%)
- **Prognosinställningar:** Aktivera/inaktivera prognosvarningar
- **Dagars framförhållning:** Konfigurera hur långt fram prognoser ska visas
- **Budgetfrysning:** Aktivera/inaktivera automatisk frysning
- **Aktiva frysningar:** Visa och hantera aktuella budgetfrysningar

```razor
<MudSwitch @bind-Value="_settings.EnableAlert75" 
           Color="Color.Info"
           Label="Varning vid 75% av budget" />

<MudSwitch @bind-Value="_settings.EnableBudgetFreeze" 
           Color="Color.Error"
           Label="Aktivera automatisk budgetfrysning" />
```

## Systemarkitektur

### Datamodeller

**BudgetAlert** (`Models/BudgetAlert.cs`):
```csharp
public class BudgetAlert
{
    public decimal ThresholdPercentage { get; set; }      // 75, 90, 100
    public decimal CurrentPercentage { get; set; }        // Aktuell användning %
    public decimal SpentAmount { get; set; }              // Använt belopp
    public decimal PlannedAmount { get; set; }            // Planerat belopp
    public int? ForecastDaysUntilExceeded { get; set; }   // Prognos
    public decimal DailyRate { get; set; }                // Daglig takt
    public bool IsActive { get; set; }                    // Aktiv/bekräftad
}
```

**BudgetAlertSettings**:
```csharp
public class BudgetAlertSettings
{
    public bool EnableAlert75 { get; set; }
    public bool EnableAlert90 { get; set; }
    public bool EnableAlert100 { get; set; }
    public string? CustomThresholds { get; set; }
    public bool EnableForecastWarnings { get; set; }
    public int ForecastWarningDays { get; set; }
    public bool EnableBudgetFreeze { get; set; }
}
```

**BudgetFreeze**:
```csharp
public class BudgetFreeze
{
    public int BudgetId { get; set; }
    public int? BudgetCategoryId { get; set; }
    public DateTime FrozenAt { get; set; }
    public DateTime? UnfrozenAt { get; set; }
    public string? Reason { get; set; }
    public bool IsActive { get; set; }
}
```

### Services

**BudgetAlertService** (`Services/BudgetAlertService.cs`):
- `CheckAllBudgetsAsync()` - Kontrollerar alla aktiva budgetar
- `CheckBudgetAsync(budgetId)` - Kontrollerar specifik budget
- `CalculateBudgetUsagePercentageAsync()` - Beräknar användning i %
- `CalculateDailyRateAsync()` - Beräknar daglig utgiftstakt
- `CalculateDaysUntilExceededAsync()` - Prognostiserar överskriden
- `FreezeBudgetAsync()` - Fryser budget/kategori
- `UnfreezeBudgetAsync()` - Avfryser budget/kategori

**WeeklyBudgetDigestService** (`Web/Services/WeeklyBudgetDigestService.cs`):
- Kör varje söndag kl 18:00
- Samlar budgetdata för alla användare
- Genererar veckosammanfattning med Markdown
- Skickar via NotificationService

**BudgetAlertBackgroundService** (`Web/Services/BudgetAlertBackgroundService.cs`):
- Kör var 30:e minut
- Kontrollerar alla aktiva budgetar
- Skapar varningar vid tröskelvärden
- Skickar real-time notifieringar via SignalR

**BudgetAlertHub** (`Web/Hubs/BudgetAlertHub.cs`):
- WebSocket-anslutning för real-time uppdateringar
- Gruppbaserad kommunikation per användare
- Metoder för att hämta alerts, bekräfta, och kontrollera frysning

### UI Komponenter

**BudgetAlertCard** (`Components/Shared/BudgetAlertCard.razor`):
- Visar budgetvarning med färgkodning
- Progress bar för visuell återkoppling
- Prognos och daglig takt
- Knappar för "Visa detaljer" och "Justera budget"

**BudgetSettings** (`Components/Pages/BudgetSettings.razor`):
- Komplett inställningssida
- Konfigurera alla varningsparametrar
- Hantera aktiva budgetfrysningar

### Notifieringssystem

**NotificationService** (`Services/NotificationService.cs`):
- Stödjer flera kanaler: InApp, Email, SMS, Push, Slack, Teams
- Prioritetsnivåer: Low, Normal, High, Critical
- Do Not Disturb (DND) schema
- Digest mode för samlade notifieringar

**SystemNotificationType** (enum):
- `BudgetWarning` - Budgetvarning (75%, 90%)
- `BudgetExceeded` - Budget överskriden (100%)
- Plus 20+ andra notifikationstyper

## Tester

**WeeklyBudgetDigestServiceTests.cs**:
- `SendUserDigest_IncludesAllBudgetCategories` - Verifierar digest-innehåll
- `CalculateDailyRate_ReturnsCorrectRate` - Testar daglig takt-beräkning
- `CalculateForecast_PredictsCorrectDaysUntilExceeded` - Testar prognos

**BudgetAlertServiceTests.cs** (befintlig):
- Test av användningsprocent-beräkning
- Test av daglig takt
- Test av forecast-beräkning
- Test av varningsgenerering

## Exempel på Användning

### Varningsflöde

1. **Background Service** kontrollerar budget var 30:e minut
2. **Detekterar** att Mat & Dryck är på 90% (6,750 kr av 7,500 kr)
3. **Skapar** BudgetAlert med:
   - ThresholdPercentage = 90
   - CurrentPercentage = 90
   - ForecastDaysUntilExceeded = 4 dagar
   - DailyRate = 94 kr/dag
4. **Skickar** in-app notifikation via NotificationService
5. **Broadcaster** via SignalR till användarens klient
6. **Visar** popup med BudgetAlertCard:

```
🚨 Budgetvarning: Mat & Dryck

Du har använt 6,750 kr av 7,500 kr (90%)
Återstående: 750 kr för 8 dagar

Prognos: Budget överskrids om 4 dagar
i nuvarande takt (94 kr/dag)

[Visa detaljer] [Justera budget]
```

### Veckosammanfattning

Varje söndag kl 18:00:
1. **WeeklyBudgetDigestService** startar
2. **Hämtar** alla användare med aktiva budgetar
3. För varje användare:
   - Beräknar budgetanvändning per kategori
   - Identifierar kategorier över budget (🔴)
   - Identifierar kategorier som närmar sig gränsen (🟡🟠)
   - Genererar prognoser
4. **Skickar** email via NotificationService:

```markdown
# Veckosammanfattning - Budgetar

Här är din budgetöversikt för vecka 45:

**Sammanfattning:** 8 kategorier
🔴 2 över budget
🟡 3 närmar sig gränsen

## Budget 2025-01
Period: 2025-01-01 - 2025-01-31

🔴 **Mat & Dryck**: 7,650 kr / 7,500 kr (102%)
   ⚠️ Prognos: Överskrids om 0 dagar (102 kr/dag)

🟡 **Transport**: 5,625 kr / 7,500 kr (75%)
   Återstående: 1,875 kr

🟢 **Nöje**: 1,200 kr / 3,000 kr (40%)
   Återstående: 1,800 kr
```

## Konfiguration

### Inställningar i appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Privatekonomi.Web.Services.BudgetAlertBackgroundService": "Information",
      "Privatekonomi.Web.Services.WeeklyBudgetDigestService": "Information"
    }
  }
}
```

### Användarinställningar via UI

Navigera till `/budgets/settings` för att konfigurera:
- ✅/❌ Varning vid 75%
- ✅/❌ Varning vid 90%
- ✅/❌ Varning vid 100%
- Anpassade trösklar (kommaseparerade %, t.ex. "50,85,95")
- ✅/❌ Prognosvarningar
- Antal dagar framåt för prognosvarningar (1-30)
- ✅/❌ Automatisk budgetfrysning

## Tekniska Detaljer

### Background Services

Båda background services registreras i `Program.cs`:
```csharp
builder.Services.AddHostedService<BudgetAlertBackgroundService>();
builder.Services.AddHostedService<WeeklyBudgetDigestService>();
```

### SignalR Hub

Registrerad i `Program.cs`:
```csharp
builder.Services.AddSignalR();
app.MapHub<BudgetAlertHub>("/budgethub");
```

### Database Entities

Alla modeller har DbSet i `PrivatekonomyContext`:
```csharp
public DbSet<BudgetAlert> BudgetAlerts { get; set; }
public DbSet<BudgetAlertSettings> BudgetAlertSettings { get; set; }
public DbSet<BudgetFreeze> BudgetFreezes { get; set; }
```

## PWA-stöd (Pågående)

**Status:** Grundläggande struktur finns, ytterligare arbete krävs för fullt stöd

- OfflineIndicator.razor - Visar offline-status ✅
- InstallPwaPrompt.razor - Promptar installation ✅
- UpdateNotification.razor - Notifierar om uppdateringar ✅
- JavaScript timing-problem fixade (OnAfterRenderAsync) ✅

**Återstående för PWA Push:**
- Service Worker-konfiguration
- Push notification subscription
- Web Push API-integration

## Nästa Steg

1. **PWA Push Notifications:**
   - Implementera Service Worker
   - Konfigurera Web Push API
   - Testa push på mobila enheter

2. **Email-template förbättringar:**
   - HTML-formaterad email istället för Markdown
   - Inbäddade grafer och charts
   - Direktlänkar till budgetdetaljer

3. **Utökad prognostik:**
   - Maskininlärningsbaserade prognoser
   - Trendanalys över tid
   - Säsongsbaserade justeringar

4. **Mobil app:**
   - Native iOS/Android apps via .NET MAUI
   - Dedikerade push notifications
   - Offline-first arkitektur

## Referenser

- Issue #3.2: Budgetalarm och Real-time Övervakning
- BudgetAlertService.cs - Kärnlogik för alerts
- WeeklyBudgetDigestService.cs - Veckosammanfattningar
- BudgetSettings.razor - Användarinställningar UI
- BudgetAlertHub.cs - Real-time SignalR hub
