# GitHub Issue Templates

Dessa templates kan kopieras direkt för att skapa nya GitHub issues.

---

## Issue #1: Migrera från InMemory-databas till SQL Server

**Labels:** `database`, `critical`, `infrastructure`  
**Assignees:** (lägg till)  
**Projects:** (lägg till)  
**Milestone:** Fas 1 - Kritiska Förbättringar

### Beskrivning
Migrera från Entity Framework Core InMemory-databas till SQL Server för persistent datalagring i produktion.

### Problem
- ❌ All data försvinner vid omstart
- ❌ Inte lämpligt för produktion
- ❌ Risk för dataförlust
- ❌ Ingen kryptering i vila

### Åtgärder
- [ ] Installera `Microsoft.EntityFrameworkCore.SqlServer` NuGet-paket
- [ ] Installera `Microsoft.EntityFrameworkCore.Tools` för migrations
- [ ] Konfigurera connection string i `appsettings.json` och `appsettings.Development.json`
- [ ] Uppdatera `Program.cs` i både Web och Api projekten
- [ ] Skapa initial migration: `dotnet ef migrations add InitialCreate`
- [ ] Applicera migration: `dotnet ef database update`
- [ ] Implementera databas-seeding för produktion
- [ ] Lägg till retry-logik: `EnableRetryOnFailure()`
- [ ] Testa migrations och rollback
- [ ] Uppdatera README.md med databas-setup instruktioner
- [ ] Skapa Docker Compose-fil för SQL Server i utveckling
- [ ] Dokumentera backup-strategi

### Teknisk Implementation

```csharp
// Program.cs (Web och Api)
// Ersätt:
builder.Services.AddDbContext<PrivatekonomyContext>(options =>
    options.UseInMemoryDatabase("PrivatekonomyDb"));

// Med:
builder.Services.AddDbContext<PrivatekonomyContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => 
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        }
    ));
```

```json
// appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=Privatekonomi;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```

```yaml
# docker-compose.yml (för utveckling)
version: '3.8'
services:
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      SA_PASSWORD: "YourStrong@Passw0rd"
      ACCEPT_EULA: "Y"
    ports:
      - "1433:1433"
    volumes:
      - sqldata:/var/opt/mssql
volumes:
  sqldata:
```

### Berörd Kod
- `src/Privatekonomi.Web/Program.cs`
- `src/Privatekonomi.Api/Program.cs`
- `src/Privatekonomi.Core/Privatekonomi.Core.csproj`
- `appsettings.json` och `appsettings.Development.json`

### Estimat
3-5 dagar

### Relaterade Issues
- Förutsättning för produktionsdrift
- Relaterad till säkerhetsförbättringar

---

## Issue #2: Implementera Tvåfaktorsautentisering (2FA)

**Labels:** `security`, `authentication`, `enhancement`  
**Assignees:** (lägg till)  
**Projects:** (lägg till)  
**Milestone:** Fas 1 - Kritiska Förbättringar

### Beskrivning
Implementera tvåfaktorsautentisering (2FA) för att förbättra säkerheten för användarkonton.

### Bakgrund
- ✅ ASP.NET Core Identity är redan implementerad
- ⚠️ Endast lösenordsbaserad autentisering finns idag
- 🎯 2FA förbättrar säkerheten avsevärt

### Åtgärder
- [ ] Aktivera 2FA i ASP.NET Core Identity
- [ ] Installera NuGet-paket för QR-kod generation: `QRCoder`
- [ ] Implementera TOTP (Time-based One-Time Password)
- [ ] Skapa Razor-sida för att aktivera/inaktivera 2FA
- [ ] QR-kod generation för authenticator-appar (Google Authenticator, Microsoft Authenticator)
- [ ] Backup-koder för återställning (10 koder)
- [ ] Visa backup-koder vid aktivering
- [ ] Möjlighet att regenerera backup-koder
- [ ] UI för att verifiera 2FA-kod vid inloggning
- [ ] "Remember this device" funktionalitet
- [ ] Testa med Google Authenticator och Microsoft Authenticator
- [ ] Dokumentera 2FA-setup för användare i README
- [ ] Lägg till varning om att aktivera 2FA efter registrering

### Teknisk Implementation

```csharp
// Nya Razor-sidor:
// - Components/Pages/Account/Manage/EnableAuthenticator.razor
// - Components/Pages/Account/Manage/TwoFactorAuthentication.razor
// - Components/Pages/Account/Manage/ResetAuthenticator.razor
// - Components/Pages/Account/Manage/GenerateRecoveryCodes.razor

// Använd ASP.NET Core Identity metoder:
// - UserManager.GenerateTwoFactorTokenAsync()
// - UserManager.VerifyTwoFactorTokenAsync()
// - UserManager.GetAuthenticatorKeyAsync()
// - UserManager.ResetAuthenticatorKeyAsync()
// - UserManager.GenerateNewTwoFactorRecoveryCodesAsync()
```

### QR-kod Generation
```csharp
public string GenerateQrCodeUri(string email, string unformattedKey)
{
    const string AuthenticatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";
    return string.Format(
        AuthenticatorUriFormat,
        UrlEncoder.Default.Encode("Privatekonomi"),
        UrlEncoder.Default.Encode(email),
        unformattedKey);
}
```

### Berörd Kod
- `src/Privatekonomi.Web/Components/Pages/Account/Manage/` (nya sidor)
- `src/Privatekonomi.Core/Models/ApplicationUser.cs` (eventuella tillägg)
- `src/Privatekonomi.Web/Components/Layout/NavMenu.razor` (länk till 2FA-inställningar)

### Estimat
2-3 dagar

### Relaterade Issues
- Höjer säkerheten innan produktionsdrift
- Kompletterar befintlig autentisering

---

## Issue #3: Implementera Nettoförmögenhet-översikt på Dashboard

**Labels:** `feature`, `dashboard`, `reporting`, `high-priority`  
**Assignees:** (lägg till)  
**Projects:** (lägg till)  
**Milestone:** Fas 1 - Kritiska Förbättringar

### Beskrivning
Lägg till en widget på Dashboard som visar nettoförmögenhet (tillgångar - skulder) och trend över tid.

### Bakgrund
- ✅ Data finns i `Asset`, `Investment` och `Loan` modeller
- ❌ Ingen samlad nettoförmögenhet-vy
- 🎯 Detta är en kärnfunktion i privatekonomisystem

### Åtgärder
- [ ] Utöka `ReportService` med `GetNetWorthReport()` metod
- [ ] Skapa `NetWorthReport` och `NetWorthDataPoint` DTOs
- [ ] Beräkna totala tillgångar (Assets + Investments)
- [ ] Beräkna totala skulder (Loans)
- [ ] Skapa historisk data för trendgraf (månadsvis, senaste 12 månaderna)
- [ ] Lägg till Net Worth-kort på Dashboard (`Home.razor`)
- [ ] Implementera linjediagram för trend (MudBlazor Chart)
- [ ] Visa procentuell förändring (månad-mot-månad)
- [ ] Lägg till färgkodning (grönt för ökning, rött för minskning)
- [ ] Lägg till tooltip med detaljer vid hover
- [ ] Visa breakdown (tillgångar vs skulder)
- [ ] Testa med olika tillgångs/skuldnivåer
- [ ] Optimera query-prestanda

### Teknisk Implementation

```csharp
// NetWorthReport.cs (ny DTO)
public class NetWorthReport
{
    public decimal TotalAssets { get; set; }
    public decimal TotalInvestments { get; set; }
    public decimal TotalLiabilities { get; set; }
    public decimal NetWorth { get; set; }
    public decimal PercentageChange { get; set; }
    public List<NetWorthDataPoint> History { get; set; } = new();
}

public class NetWorthDataPoint
{
    public DateTime Date { get; set; }
    public decimal Assets { get; set; }
    public decimal Liabilities { get; set; }
    public decimal NetWorth { get; set; }
}

// IReportService.cs (utöka interface)
public interface IReportService
{
    // ... befintliga metoder
    Task<NetWorthReport> GetNetWorthReportAsync(string userId);
}

// ReportService.cs (implementera)
public async Task<NetWorthReport> GetNetWorthReportAsync(string userId)
{
    var assets = await _context.Assets
        .Where(a => a.UserId == userId)
        .SumAsync(a => a.Value);
    
    var investments = await _context.Investments
        .Where(i => i.UserId == userId)
        .SumAsync(i => i.Quantity * i.CurrentPrice);
    
    var liabilities = await _context.Loans
        .Where(l => l.UserId == userId)
        .SumAsync(l => l.CurrentBalance);
    
    var netWorth = (assets + investments) - liabilities;
    
    // Skapa historisk data...
    var history = await CreateNetWorthHistory(userId, 12);
    
    return new NetWorthReport
    {
        TotalAssets = assets,
        TotalInvestments = investments,
        TotalLiabilities = liabilities,
        NetWorth = netWorth,
        PercentageChange = CalculatePercentageChange(history),
        History = history
    };
}
```

### UI Implementation (Home.razor)
```razor
<MudCard>
    <MudCardHeader>
        <CardHeaderContent>
            <MudText Typo="Typo.h6">Nettoförmögenhet</MudText>
        </CardHeaderContent>
    </MudCardHeader>
    <MudCardContent>
        <MudText Typo="Typo.h4" Color="@GetNetWorthColor()">
            @netWorthReport.NetWorth.ToString("C0", new CultureInfo("sv-SE"))
        </MudText>
        <MudText Typo="Typo.body2" Color="@GetChangeColor()">
            @netWorthReport.PercentageChange.ToString("+0.00%;-0.00%;0%")
            <MudIcon Icon="@GetChangeIcon()" Size="Size.Small" />
        </MudText>
        
        <MudChart ChartType="ChartType.Line" 
                  ChartSeries="@netWorthSeries" 
                  XAxisLabels="@netWorthLabels"
                  Width="100%" Height="250px" />
        
        <MudGrid Class="mt-4">
            <MudItem xs="6">
                <MudText Typo="Typo.body2" Color="Color.Success">
                    Tillgångar: @netWorthReport.TotalAssets.ToString("C0")
                </MudText>
            </MudItem>
            <MudItem xs="6">
                <MudText Typo="Typo.body2" Color="Color.Error">
                    Skulder: @netWorthReport.TotalLiabilities.ToString("C0")
                </MudText>
            </MudItem>
        </MudGrid>
    </MudCardContent>
</MudCard>
```

### Berörd Kod
- `src/Privatekonomi.Core/Services/ReportService.cs`
- `src/Privatekonomi.Core/Services/IReportService.cs`
- `src/Privatekonomi.Core/Models/Reports/NetWorthReport.cs` (ny)
- `src/Privatekonomi.Web/Components/Pages/Home.razor`

### Estimat
2-3 dagar

### Relaterade Issues
- Använder befintlig Asset, Investment och Loan data
- Kompletterar befintliga dashboard-widgets

---

## Issue #4: Implementera Notifikationssystem med SignalR

**Labels:** `feature`, `notifications`, `signalr`, `ux`, `high-priority`  
**Assignees:** (lägg till)  
**Projects:** (lägg till)  
**Milestone:** Fas 2 - Viktiga Funktioner

### Beskrivning
Implementera ett komplett notifikationssystem för att varna användare om viktiga händelser i realtid.

### Bakgrund
- ❌ Ingen notifikationsfunktionalitet finns idag
- 🎯 Notifikationer förbättrar användarupplevelsen avsevärt
- 🎯 Real-time varningar om budgetöverdrag, låga saldon etc.

### Åtgärder

#### Fas 1: Grundläggande Infrastructure (2 dagar)
- [ ] Installera SignalR NuGet-paket (redan inkluderat i ASP.NET Core)
- [ ] Skapa `Notification` datamodell
- [ ] Skapa `NotificationService` och `INotificationService`
- [ ] Lägg till `Notifications` DbSet i `PrivatekonomyContext`
- [ ] Skapa migration för Notification-tabellen
- [ ] Implementera grundläggande CRUD-operationer

#### Fas 2: SignalR Integration (1-2 dagar)
- [ ] Skapa `NotificationHub` för SignalR
- [ ] Registrera SignalR i `Program.cs`
- [ ] Konfigurera endpoints
- [ ] Implementera client-side SignalR anslutning i Blazor
- [ ] Testa real-time meddelanden

#### Fas 3: UI Implementation (1-2 dagar)
- [ ] Skapa notifikations-center i top bar (MainLayout.razor)
- [ ] Badge för antal olästa notifikationer
- [ ] Dropdown med notifikationslista
- [ ] Markera som läst funktionalitet
- [ ] Ta bort notifikation funktionalitet
- [ ] Toast-meddelanden för nya notifikationer (MudBlazor Snackbar)
- [ ] Ljud-varning för kritiska notifikationer (valfritt)

#### Fas 4: Notifikationstyper (2 dagar)
- [ ] **Budgetöverdrag** - Varna när kategoribudget överskrids
- [ ] **Låg balans** - Varning vid låg balans (konfigurerbar tröskel)
- [ ] **Kommande räkningar** - Påminnelser från återkommande transaktioner (kräver Issue #6)
- [ ] **Sparmål uppnått** - Gratulera vid uppnått sparmål
- [ ] **Banksynkroniseringsfel** - Varna vid fel vid PSD2-synk
- [ ] **Stor transaktion** - Notifiera vid ovanligt stor transaktion (valfritt)

#### Fas 5: Inställningar och E-post (1 dag, valfritt)
- [ ] Användarinställningar för notifikationstyper (aktivera/inaktivera)
- [ ] E-post-notifikationer för kritiska händelser
- [ ] Schemaläggning av notifikationer
- [ ] Gruppera notifikationer (digest)

### Teknisk Implementation

#### Datamodell
```csharp
// Notification.cs
public class Notification
{
    public int NotificationId { get; set; }
    public string UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public NotificationSeverity Severity { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? RelatedEntityId { get; set; }
    public string? RelatedEntityType { get; set; }
    public string? ActionUrl { get; set; }
    
    public ApplicationUser User { get; set; } = null!;
}

public enum NotificationType
{
    BudgetExceeded,
    LowBalance,
    UpcomingBill,
    GoalAchieved,
    SyncError,
    LargeTransaction,
    Info
}

public enum NotificationSeverity
{
    Info,
    Warning,
    Error,
    Success
}
```

#### Service Interface
```csharp
// INotificationService.cs
public interface INotificationService
{
    Task<IEnumerable<Notification>> GetUserNotificationsAsync(string userId, bool unreadOnly = false);
    Task<int> GetUnreadCountAsync(string userId);
    Task<Notification> CreateNotificationAsync(Notification notification);
    Task MarkAsReadAsync(int notificationId);
    Task MarkAllAsReadAsync(string userId);
    Task DeleteNotificationAsync(int notificationId);
    Task DeleteAllReadAsync(string userId);
    
    // Specifika notifikationstyper
    Task NotifyBudgetExceededAsync(string userId, int budgetId, string categoryName, decimal amount);
    Task NotifyLowBalanceAsync(string userId, decimal balance, decimal threshold);
    Task NotifyGoalAchievedAsync(string userId, int goalId, string goalName);
    Task NotifySyncErrorAsync(string userId, string bankName, string error);
}
```

#### SignalR Hub
```csharp
// NotificationHub.cs
public class NotificationHub : Hub
{
    public async Task SendNotificationToUser(string userId, Notification notification)
    {
        await Clients.User(userId).SendAsync("ReceiveNotification", notification);
    }
    
    public async Task MarkAsRead(int notificationId)
    {
        // Implementera logik
    }
}
```

#### Client-side SignalR (Blazor)
```csharp
// NotificationService.cs (client-side)
@inject NavigationManager Navigation
@inject ISnackbar Snackbar

private HubConnection? hubConnection;

protected override async Task OnInitializedAsync()
{
    hubConnection = new HubConnectionBuilder()
        .WithUrl(Navigation.ToAbsoluteUri("/notificationHub"))
        .Build();

    hubConnection.On<Notification>("ReceiveNotification", (notification) =>
    {
        // Uppdatera UI
        unreadCount++;
        notifications.Insert(0, notification);
        
        // Visa toast
        Snackbar.Add(notification.Message, GetSeverity(notification.Severity));
        
        StateHasChanged();
    });

    await hubConnection.StartAsync();
}
```

#### UI Component (MainLayout.razor)
```razor
<MudAppBar Elevation="1">
    <!-- Befintlig kod -->
    
    <MudSpacer />
    
    <!-- Notification Bell -->
    <MudMenu Icon="@Icons.Material.Filled.Notifications" 
             Color="Color.Inherit"
             AnchorOrigin="Origin.BottomRight"
             TransformOrigin="Origin.TopRight">
        <ActivatorContent>
            <MudBadge Content="@unreadCount" 
                      Color="Color.Error" 
                      Overlap="true" 
                      Visible="@(unreadCount > 0)">
                <MudIconButton Icon="@Icons.Material.Filled.Notifications" 
                               Color="Color.Inherit" />
            </MudBadge>
        </ActivatorContent>
        <ChildContent>
            <MudList Clickable="true" Dense="true">
                @if (!notifications.Any())
                {
                    <MudListItem>
                        <MudText Typo="Typo.body2" Color="Color.Default">
                            Inga notifikationer
                        </MudText>
                    </MudListItem>
                }
                else
                {
                    @foreach (var notification in notifications.Take(10))
                    {
                        <MudListItem OnClick="@(() => HandleNotificationClick(notification))">
                            <MudText Typo="Typo.body2" 
                                     Style="@(notification.IsRead ? "" : "font-weight: bold")">
                                @notification.Title
                            </MudText>
                            <MudText Typo="Typo.caption">
                                @notification.Message
                            </MudText>
                        </MudListItem>
                        <MudDivider />
                    }
                    <MudListItem OnClick="MarkAllAsRead">
                        <MudText Typo="Typo.body2" Color="Color.Primary" Align="Align.Center">
                            Markera alla som lästa
                        </MudText>
                    </MudListItem>
                }
            </MudList>
        </ChildContent>
    </MudMenu>
</MudAppBar>
```

### Berörd Kod
- `src/Privatekonomi.Core/Models/Notification.cs` (ny)
- `src/Privatekonomi.Core/Services/NotificationService.cs` (ny)
- `src/Privatekonomi.Core/Services/INotificationService.cs` (ny)
- `src/Privatekonomi.Core/Data/PrivatekonomyContext.cs` (lägg till DbSet)
- `src/Privatekonomi.Web/Hubs/NotificationHub.cs` (ny)
- `src/Privatekonomi.Web/Program.cs` (registrera SignalR)
- `src/Privatekonomi.Web/Components/Layout/MainLayout.razor`

### Estimat
5-7 dagar totalt
- Infrastructure: 2 dagar
- SignalR: 1-2 dagar
- UI: 1-2 dagar
- Notifikationstyper: 2 dagar
- Inställningar (valfritt): 1 dag

### Relaterade Issues
- Förutsätter Issue #1 (SQL Server) för persistent lagring
- Kompletterar Issue #6 (Återkommande transaktioner) för kommande räkningar
- Kompletterar budgetering och sparmål

---

## Instruktioner för att Skapa Issues

1. **Kopiera template** för den issue du vill skapa
2. Gå till GitHub repository
3. Klicka på **"Issues"** -> **"New Issue"**
4. Klistra in template-innehåll
5. Lägg till relevanta:
   - **Labels** (enligt förslag i template)
   - **Assignees** (utvecklare som ska jobba på det)
   - **Project** (om ni använder GitHub Projects)
   - **Milestone** (Fas 1, Fas 2, etc.)
6. Klicka **"Submit new issue"**

### Labels att skapa:

**Prioritet:**
- `critical` (röd)
- `high-priority` (orange)
- `medium-priority` (gul)
- `low-priority` (grön)

**Typ:**
- `feature` (ljusblå)
- `enhancement` (blå)
- `bug` (röd)
- `documentation` (grå)
- `infrastructure` (lila)

**Område:**
- `database`
- `security`
- `authentication`
- `dashboard`
- `reporting`
- `notifications`
- `signalr`
- `ux`
- `transactions`
- `investments`
- `loans`
- `goals`

**Fas:**
- `fas-1-kritisk`
- `fas-2-viktig`
- `fas-3-förbättring`
- `fas-4-nice-to-have`

---

## Relaterade Dokument

- **[FUNKTIONSANALYS.md](FUNKTIONSANALYS.md)** - Fullständig funktionskartläggning
- **[ATGARDSPLAN.md](ATGARDSPLAN.md)** - Detaljerad roadmap och alla 16 issue-beskrivningar
- **[SNABBREFERENS.md](SNABBREFERENS.md)** - Snabböversikt av status och prioriteringar

---

**Senast uppdaterad:** 2025-10-21  
**Version:** 1.0  
**Antal templates:** 4 av 16 (Issue #1-4 inkluderade här, se ATGARDSPLAN.md för resterande 12)
