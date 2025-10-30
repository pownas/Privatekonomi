# SignalR Real-time Notifikationssystem - Implementation

## Översikt

Detta dokument beskriver implementeringen av ett komplett real-time notifikationssystem med SignalR för Privatekonomi-applikationen.

## Arkitektur

### Komponenter

```
┌─────────────────────────────────────────────────────────────┐
│                         Browser                              │
│  ┌────────────────────────────────────────────────────────┐ │
│  │  NotificationBell.razor (SignalR Client)               │ │
│  │  - HubConnection till /notificationHub                 │ │
│  │  - Lyssnar på ReceiveNotification                      │ │
│  │  - Lyssnar på UpdateUnreadCount                        │ │
│  │  - Visar toast notifications (MudBlazor Snackbar)      │ │
│  └────────────────────────────────────────────────────────┘ │
│                            ↕                                 │
│                      SignalR WebSocket                       │
└─────────────────────────────────────────────────────────────┘
                             ↕
┌─────────────────────────────────────────────────────────────┐
│                      ASP.NET Core Server                     │
│  ┌────────────────────────────────────────────────────────┐ │
│  │  NotificationHub.cs (SignalR Hub)                      │ │
│  │  - Hanterar anslutningar                               │ │
│  │  - MarkAsRead, MarkAllAsRead, DeleteNotification       │ │
│  │  - Autentiserad med [Authorize]                        │ │
│  └────────────────────────────────────────────────────────┘ │
│                            ↕                                 │
│  ┌────────────────────────────────────────────────────────┐ │
│  │  NotificationBroadcaster (Singleton)                   │ │
│  │  - IHubContext<NotificationHub>                        │ │
│  │  - BroadcastNotificationAsync(userId, notification)    │ │
│  │  - UpdateUnreadCountAsync(userId, count)               │ │
│  └────────────────────────────────────────────────────────┘ │
│                            ↕                                 │
│  ┌────────────────────────────────────────────────────────┐ │
│  │  RealtimeNotificationService (Scoped, Decorator)       │ │
│  │  - Wraps NotificationService                           │ │
│  │  - Broadcasts efter CRUD-operationer                   │ │
│  └────────────────────────────────────────────────────────┘ │
│                            ↕                                 │
│  ┌────────────────────────────────────────────────────────┐ │
│  │  NotificationService (Scoped)                          │ │
│  │  - Databas-operationer (Entity Framework)              │ │
│  │  - SendNotificationAsync                               │ │
│  │  - GetUserNotificationsAsync                           │ │
│  │  - MarkAsReadAsync, DeleteNotificationAsync            │ │
│  └────────────────────────────────────────────────────────┘ │
│                            ↕                                 │
│  ┌────────────────────────────────────────────────────────┐ │
│  │  PrivatekonomyContext (Database)                       │ │
│  │  - DbSet<Notification> Notifications                   │ │
│  └────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
```

## Funktioner

### 1. Real-time Notifikationer

Notifikationer skickas omedelbart till användaren via SignalR WebSocket-anslutning när:
- En ny notifikation skapas via `SendNotificationAsync`
- Användaren markerar en notifikation som läst
- Användaren tar bort en notifikation

### 2. Toast Meddelanden

När en ny notifikation tas emot visas ett toast-meddelande automatiskt med:
- Färgkodning baserat på priority (Critical=Error, High=Warning, Normal=Info)
- Automatisk stängning efter 5 sekunder
- Stängningsknapp för manuell stängning

### 3. Badge Counter

Olästa notifikationer räknas och visas i en röd badge på notifikationsikonen i top bar:
- Uppdateras automatiskt i real-time
- Döljs när inga olästa notifikationer finns

### 4. Automatisk Återanslutning

SignalR-klienten återansluter automatiskt vid:
- Nätverksavbrott
- Server-omstart
- Timeout

## Notifikationstyper

### Budgetöverdrag (BudgetExceeded)
**Prioritet:** High  
**Färg:** Röd (Error)  
**Exempel:** "Din budget för Mat & Dryck har överskridits. Budget: 5 000 kr, Spenderat: 6 500 kr"

### Budgetvarning (BudgetWarning)
**Prioritet:** Normal  
**Färg:** Orange (Warning)  
**Exempel:** "Du har använt 87% av din budget för Shopping (1 750 kr av 2 000 kr)"

### Låg balans (LowBalance)
**Prioritet:** High  
**Färg:** Röd (Error)  
**Exempel:** "Ditt konto Swedbank har låg balans: 450 kr (tröskel: 1 000 kr)"

### Sparmål uppnått (GoalAchieved)
**Prioritet:** Normal  
**Färg:** Blå (Info)  
**Ikon:** 🎉  
**Exempel:** "Du har nått ditt sparmål 'Semesterresa till Thailand' på 15 000 kr!"

### Sparmål milstolpe (GoalMilestone)
**Prioritet:** Normal  
**Färg:** Blå (Info)  
**Ikon:** 🎯  
**Exempel:** "Du har nått 50% av ditt mål 'Ny bil' (50 000 kr av 100 000 kr)"

### Banksynkronisering misslyckades (BankSyncFailed)
**Prioritet:** High  
**Färg:** Röd (Error)  
**Exempel:** "Synkronisering med Nordea misslyckades: Ogiltig token. Vänligen logga in igen."

### Banksynkronisering lyckades (BankSyncSuccess)
**Prioritet:** Normal  
**Färg:** Blå (Info)  
**Exempel:** "24 nya transaktioner från SEB har importerats"

### Stor transaktion (LargeTransaction)
**Prioritet:** High  
**Färg:** Orange (Warning)  
**Exempel:** "En ovanligt stor transaktion på 12 500 kr registrerades: IKEA Stockholm"

### Kommande räkning (UpcomingBill)
**Prioritet:** Normal  
**Färg:** Blå (Info)  
**Exempel:** "Hyra förfaller om 5 dagar (2025-11-04): 8 500 kr"

### Prenumerationspris ökat (SubscriptionPriceIncrease)
**Prioritet:** Normal  
**Färg:** Orange (Warning)  
**Exempel:** "Netflix Premium ökar från 169 kr till 189 kr (+11,8%) fr.o.m. 2025-11-29"

## Användning

### Extension Methods

```csharp
// Budgetöverdrag
await NotificationService.NotifyBudgetExceededAsync(
    userId, 
    "Mat & Dryck", 
    budgetAmount: 5000, 
    actualAmount: 6500);

// Sparmål uppnått
await NotificationService.NotifyGoalAchievedAsync(
    userId, 
    "Semesterresa till Thailand", 
    goalAmount: 15000);

// Låg balans
await NotificationService.NotifyLowBalanceAsync(
    userId, 
    "Swedbank", 
    balance: 450, 
    threshold: 1000);
```

### Test-sida

Navigera till `/notification-test` för att testa alla notifikationstyper interaktivt.

## Konfiguration

### Program.cs

```csharp
// Lägg till SignalR
builder.Services.AddSignalR();

// Registrera NotificationBroadcaster
builder.Services.AddSingleton<INotificationBroadcaster, NotificationBroadcaster>();

// Registrera RealtimeNotificationService som wrapper
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<INotificationService>(sp =>
{
    var innerService = sp.GetRequiredService<NotificationService>();
    var broadcaster = sp.GetRequiredService<INotificationBroadcaster>();
    var logger = sp.GetRequiredService<ILogger<RealtimeNotificationService>>();
    return new RealtimeNotificationService(innerService, broadcaster, logger);
});

// Mappa hub endpoint
app.MapHub<NotificationHub>("/notificationHub");
```

### NotificationBell.razor

```csharp
// Initiera SignalR-anslutning
_hubConnection = new HubConnectionBuilder()
    .WithUrl(NavigationManager.ToAbsoluteUri("/notificationHub"))
    .WithAutomaticReconnect()
    .Build();

// Lyssna på nya notifikationer
_hubConnection.On<Notification>("ReceiveNotification", async (notification) =>
{
    _unreadCount++;
    Snackbar.Add(notification.Message, GetSeverity(notification.Priority));
    StateHasChanged();
});

// Lyssna på uppdateringar av olästa
_hubConnection.On<int>("UpdateUnreadCount", async (count) =>
{
    _unreadCount = count;
    StateHasChanged();
});
```

## Säkerhet

- Hub kräver autentisering via `[Authorize]` attribute
- Användare kan bara se och hantera sina egna notifikationer
- SignalR använder användarens Identity för att skicka till rätt klient
- HTTPS krävs i produktion

## Prestandaoptimering

- NotificationBroadcaster är Singleton för minimal overhead
- SignalR använder WebSocket för låg latens
- Automatisk återanslutning vid nätverksproblem
- Endast in-app notifikationer skickas real-time (email/SMS skickas separat)

## Framtida förbättringar

1. **Notifikationsinställningar**
   - Användare kan aktivera/inaktivera specifika notifikationstyper
   - DND (Do Not Disturb) schema
   - Digest-läge för grupperade notifikationer

2. **E-post notifikationer**
   - Kritiska notifikationer skickas även via email
   - Konfigurerbar tröskel

3. **Push-notifikationer**
   - Progressive Web App (PWA) notifikationer
   - Fungerar även när webbläsaren är stängd

4. **Notifikationshistorik**
   - Visa alla notifikationer från senaste 30 dagarna
   - Sökfunktion
   - Filtrera per typ

5. **Scheduled notifikationer**
   - Påminnelser för kommande räkningar
   - Månatliga sammanfattningar
   - Sparmålsuppdateringar

## Teknisk Stack

- **SignalR:** ASP.NET Core SignalR för real-time kommunikation
- **MudBlazor:** Snackbar för toast-notifikationer
- **Entity Framework Core:** Databas-lagring
- **Blazor Server:** Client-side UI
- **.NET 9:** Runtime

## Testning

Kör applikationen och navigera till:
1. `/notification-test` - Testa alla notifikationstyper
2. `/notifications` - Visa notifikationshistorik
3. Öppna flera flikar/webbläsare för att se real-time synkronisering

## Screenshots

### 1. NotificationBell i Top Bar
Visar badge med antal olästa notifikationer

### 2. Toast Notification
Real-time toast-meddelande när ny notifikation tas emot

### 3. Notification Test Page
Interaktiv sida för att testa alla notifikationstyper

### 4. Notifications Page
Lista över alla notifikationer med markera som läst/ta bort funktionalitet

## Sammanfattning

Implementeringen ger en komplett real-time notifikationslösning som:
- ✅ Använder SignalR för WebSocket-baserad kommunikation
- ✅ Visar toast-meddelanden automatiskt
- ✅ Uppdaterar badge counter i real-time
- ✅ Hanterar återanslutning automatiskt
- ✅ Stödjer alla definierade notifikationstyper
- ✅ Är säker och användarspecifik
- ✅ Har minimal påverkan på prestanda
- ✅ Är lättutbyggbar för framtida förbättringar
