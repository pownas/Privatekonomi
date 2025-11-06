# Budget Alert System - Implementation Guide

## Översikt

Budgetalarmsystemet tillhandahåller real-time varningar när användare närmar sig eller överskrider sina budgetgränser. Systemet använder SignalR för real-time uppdateringar och integreras med det befintliga notifikationssystemet.

## Funktioner

### Grundfunktioner
- ✅ Automatiska varningar vid 75%, 90%, och 100% av budget
- ✅ Prognos baserad på daglig förbrukning: "Budget överskrids om X dagar"
- ✅ Real-time uppdateringar via SignalR
- ✅ Email-notifikationer
- ✅ Budget freeze - blockera utgifter temporärt
- ✅ Anpassningsbara tröskelvärden per användare

### Beräkningar

**Daglig förbrukningsrate:**
```csharp
dailyRate = totalSpent / daysElapsed
```

**Prognos:**
```csharp
daysUntilExceeded = remainingBudget / dailyRate
```

**Användningsprocent:**
```csharp
percentage = (spent / planned) * 100
```

## Arkitektur

### Datamodeller

**BudgetAlert**
- Lagrar triggade varningar
- Innehåller: belopp, procent, prognos, daglig rate
- Kan bekräftas av användare

**BudgetAlertSettings**
- Användarspecifika inställningar
- Aktivera/avaktivera tröskelvärden
- Anpassade tröskelvärden
- Budget freeze-inställningar

**BudgetFreeze**
- Spärrar budget eller specifik kategori
- Tidsstämplar för aktivering/avaktivering
- Kan vara budget-wide eller category-specific

### Services

**BudgetAlertService**
```csharp
// Kontrollera alla budgetar
await budgetAlertService.CheckAllBudgetsAsync();

// Kontrollera specifik budget
var alerts = await budgetAlertService.CheckBudgetAsync(budgetId);

// Hämta aktiva varningar
var activeAlerts = await budgetAlertService.GetActiveAlertsAsync();

// Bekräfta varning
await budgetAlertService.AcknowledgeAlertAsync(alertId);

// Freeze budget
var freeze = await budgetAlertService.FreezeBudgetAsync(budgetId, categoryId, "Budget överskriden");

// Kontrollera freeze-status
var isFrozen = await budgetAlertService.IsBudgetFrozenAsync(budgetId, categoryId);
```

**BudgetAlertBackgroundService**
- Körs var 30:e minut
- Kontrollerar alla aktiva budgetar
- Skickar varningar via SignalR
- Skapar notifikationer via NotificationService

**BudgetAlertHub (SignalR)**
```javascript
// Anslut till hub
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/budgetalert")
    .build();

// Lyssna på budget alerts
connection.on("ReceiveBudgetAlert", (data) => {
    console.log("New budget alert:", data);
});

// Hämta aktiva alerts
const result = await connection.invoke("GetActiveAlerts");

// Bekräfta alert
await connection.invoke("AcknowledgeAlert", alertId);

// Kontrollera budget
await connection.invoke("CheckBudget", budgetId);
```

## UI-Komponenter

### BudgetAlertCard
Visar detaljerad varningsinformation:
- Kategorinamn
- Förbrukning och procentandel
- Progressbar med färgkodning
- Återstående belopp och dagar
- Prognos
- Bekräfta-knapp

Användning:
```razor
<BudgetAlertCard Alert="@alert" 
               BudgetCategory="@budgetCategory"
               OnViewDetails="@HandleViewDetails"
               OnAdjustBudget="@HandleAdjustBudget"
               OnAlertAcknowledged="@LoadAlertsAsync" />
```

### BudgetAlertBadge
Visar antal aktiva varningar i en badge:
```razor
<BudgetAlertBadge />
```

### Integration i Budgets.razor
Visar upp till 3 aktiva varningar överst på sidan:
```razor
@if (_activeAlerts.Any())
{
    <MudPaper Class="pa-4 mb-4">
        @foreach (var alert in _activeAlerts.Take(3))
        {
            <BudgetAlertCard Alert="@alert" ... />
        }
    </MudPaper>
}
```

## Konfiguration

### Användarinställningar
Hämta eller skapa inställningar:
```csharp
var settings = await budgetAlertService.GetOrCreateSettingsAsync();
```

Uppdatera inställningar:
```csharp
settings.EnableAlert75 = true;
settings.EnableAlert90 = true;
settings.EnableAlert100 = true;
settings.CustomThresholds = "80,95"; // Lägg till anpassade tröskelvärden
settings.EnableBudgetFreeze = true;
settings.EnableForecastWarnings = true;
settings.ForecastWarningDays = 7;

await budgetAlertService.UpdateSettingsAsync(settings);
```

### Background Service-intervall
Standard: 30 minuter. Ändra i `BudgetAlertBackgroundService.cs`:
```csharp
private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(30);
```

## Integration med NotificationService

Budget alerts integreras automatiskt med NotificationService:

**Notifikationstyper:**
- `SystemNotificationType.BudgetWarning` - för varningar
- `SystemNotificationType.BudgetExceeded` - för överskridanden

**Prioritet:**
- `NotificationPriority.Critical` - 100% eller mer
- `NotificationPriority.High` - 90%
- `NotificationPriority.Normal` - 75%

**Kanaler:**
- In-App notifikationer (alltid)
- Email (om aktiverat i användarens NotificationPreference)
- Push (om aktiverat)

## Testing

### Unit Tests
8 unit tests validerar kärnfunktionalitet:

```csharp
// Test budgetberäkningar
CalculateBudgetUsagePercentageAsync_ReturnsCorrectPercentage()
CalculateDailyRateAsync_ReturnsCorrectRate()
CalculateDaysUntilExceededAsync_ReturnsCorrectForecast()

// Test settings
GetOrCreateSettingsAsync_CreatesDefaultSettings()

// Test freeze
FreezeBudgetAsync_CreatesFreezeSuccessfully()
IsBudgetFrozenAsync_ReturnsTrueWhenFrozen()

// Test alerts
CheckBudgetAsync_CreatesAlertWhenThresholdExceeded()
AcknowledgeAlertAsync_MarksAlertAsInactive()
```

Kör tester:
```bash
dotnet test --filter "FullyQualifiedName~BudgetAlertServiceTests"
```

### Manuell testning

1. **Skapa budget med låg gräns:**
   - Skapa budget med PlannedAmount = 1000 kr
   - Lägg till transaktioner för 750 kr
   - Verifiera varning vid 75%

2. **Testa prognos:**
   - Skapa budget med 7500 kr för en månad
   - Lägg till 6750 kr i utgifter efter 10 dagar
   - Verifiera prognos: "Budget överskrids om ~2 dagar"

3. **Testa freeze:**
   - Skapa budget med freeze aktiverat
   - Överskrid budget (100%)
   - Verifiera att kategori är frozen

4. **Testa SignalR:**
   - Öppna två webbläsarflikar
   - Skapa alert i ena fliken
   - Verifiera real-time uppdatering i andra fliken

## Säkerhet

### Authorization
- Alla alerts är användarspecifika
- Endast ägare kan bekräfta/radera sina alerts
- SignalR Hub kräver autentisering (`[Authorize]`)

### Data Protection
- UserId valideras i alla service-metoder
- Freeze-operationer kräver ägarskap
- LINQ-queries filtreras alltid på UserId

### CodeQL Scan
- 0 säkerhetsproblem hittades
- Inga SQL-injektions risker
- Inga XSS-vulnerabiliteter

## Prestandaoptimering

### Database Queries
- Index på: BudgetId, UserId, IsActive
- Eager loading med `.Include()` för related entities
- Filtrering på database-nivå

### Background Service
- Kör endast var 30:e minut
- Processar endast aktiva budgetar
- Använder scoped services (memory-effektivt)

### SignalR
- Endast uppkopplade användare får uppdateringar
- Gruppbaserad routing (`user_{userId}`)
- Minimal payload (endast nödvändig data)

## Framtida förbättringar

### Ej implementerat (kan läggas till)
- PWA Push-notifikationer (kräver service worker)
- Email-sammanfattning veckovis (finns i NotificationService.SendDigestNotificationsAsync)
- Grafisk historik över budget alerts
- Predictive analytics med ML
- Budgetalert-dashboard med statistik

### Potentiella optimeringar
- Cache för beräkningar
- Batch-processing av alerts
- WebSocket-optimering
- Database sharding för stora dataset

## Felsökning

### Alert skapas inte
1. Kontrollera att BudgetAlertBackgroundService körs
2. Verifiera att budget är aktiv (StartDate <= now <= EndDate)
3. Kontrollera att BudgetAlertSettings är korrekt konfigurerad
4. Se logs för fel i background service

### SignalR fungerar inte
1. Verifiera att SignalR är konfigurerad i Program.cs
2. Kontrollera att hub endpoint är mappat: `/hubs/budgetalert`
3. Verifiera autentisering
4. Kontrollera browser console för JavaScript-fel

### Prognos är felaktig
1. Kontrollera att transaktioner har korrekta datum
2. Verifiera daglig rate-beräkning
3. Kontrollera att budget har startat (inte framtida startdatum)

### Freeze fungerar inte
1. Verifiera att EnableBudgetFreeze är true i settings
2. Kontrollera att freeze är active
3. Verifiera freeze-logik (budget-wide vs category-specific)

## Support

För frågor eller problem:
1. Kontrollera denna dokumentation
2. Se unit tests för användningsexempel
3. Granska kod-kommentarer i services
4. Öppna GitHub issue med detaljerad beskrivning
