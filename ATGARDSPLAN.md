# Åtgärdsplan - Implementering av Saknade Funktioner

**Datum:** 2025-10-21  
**Status:** Klar för implementation  
**Fullständig analys:** Se [FUNKTIONSANALYS.md](FUNKTIONSANALYS.md)

---

## Snabb Sammanfattning

Privatekonomi har **70% av önskade funktioner implementerade**. Applikationen är välutvecklad med solid teknisk grund, men behöver förbättringar inom:

### ✅ Implementerat (70%)
- Transaktionshantering med automatisk kategorisering
- Budgetering (flera metoder: 50/30/20, zero-based, envelope)
- Lånhantering med amorteringsplanering
- Investeringar med automatisk kursuppdatering
- Familjesamarbete (hushåll, barnkonton, veckopeng)
- Bankintegration (PSD2, CSV-import)
- Sverige-specifika funktioner (ROT/RUT, K4, ISK/KF, SIE)
- Användarautentisering och audit trail

### ⚠️ Delvis Implementerat / Saknas (30%)
- Persistent databas (använder in-memory)
- Notifikationssystem
- Prognosverktyg
- Återkommande transaktioner
- Nettoförmögenhet-översikt
- Kvittohantering
- Mobiloptimering

---

## Prioriterad Roadmap

### 🔴 Fas 1: Kritiska Förbättringar (1-2 veckor)
**Mål:** Gör applikationen produktionsklar

| # | Funktion | Prioritet | Estimat | Issue |
|---|----------|-----------|---------|-------|
| 1 | Migrera till SQL Server | 🔴 Kritisk | 3-5 dagar | [#1](#issue-1-migrera-till-sql-server) |
| 2 | Tvåfaktorsautentisering (2FA) | 🔴 Kritisk | 2-3 dagar | [#2](#issue-2-tvåfaktorsautentisering) |
| 3 | Nettoförmögenhet-widget | 🔴 Hög | 2-3 dagar | [#3](#issue-3-nettoförmögenhet-översikt) |

**Total tid:** ~7-11 dagar (1.5-2 veckor)

---

### 🟠 Fas 2: Viktiga Funktioner (2-3 veckor)
**Mål:** Lägg till efterfrågade kärnfunktioner

| # | Funktion | Prioritet | Estimat | Issue |
|---|----------|-----------|---------|-------|
| 4 | Notifikationssystem | 🟠 Hög | 5-7 dagar | [#4](#issue-4-notifikationssystem) |
| 5 | Prognosverktyg | 🟠 Hög | 4-5 dagar | [#5](#issue-5-prognosverktyg) |
| 6 | Återkommande transaktioner | 🟠 Hög | 5-6 dagar | [#6](#issue-6-återkommande-transaktioner) |
| 7 | Kvittohantering | 🟠 Medel | 4-5 dagar | [#7](#issue-7-kvittohantering) |

**Total tid:** ~18-23 dagar (2.5-3 veckor)

---

### 🟡 Fas 3: Förbättringar och Rapporter (2-3 veckor)
**Mål:** Förbättra användarupplevelse och insikter

| # | Funktion | Prioritet | Estimat | Issue |
|---|----------|-----------|---------|-------|
| 8 | Trend- och säsongsanalys | 🟡 Medel | 3-4 dagar | [#8](#issue-8-trend--och-säsongsanalys) |
| 9 | Topp-handlare rapport | 🟡 Medel | 2-3 dagar | [#9](#issue-9-topp-handlare-rapport) |
| 10 | Målstolpar för sparmål | 🟡 Medel | 3-4 dagar | [#10](#issue-10-målstolpar-milestones) |
| 11 | Tillgångsallokering | 🟡 Medel | 2-3 dagar | [#11](#issue-11-tillgångsallokering) |
| 12 | Investeringstransaktioner | 🟡 Medel | 4-5 dagar | [#12](#issue-12-investeringstransaktioner) |

**Total tid:** ~14-19 dagar (2-3 veckor)

---

### 🟢 Fas 4: Nice-to-have (1-2 veckor)
**Mål:** Förbättra tillgänglighet och användarupplevelse

| # | Funktion | Prioritet | Estimat | Issue |
|---|----------|-----------|---------|-------|
| 13 | PWA och offline-stöd | 🟢 Låg | 3-4 dagar | [#13](#issue-13-pwa-och-offline) |
| 14 | Dividendspårning | 🟢 Låg | 2-3 dagar | [#14](#issue-14-dividendspårning) |
| 15 | Försäkringsöversikt | 🟢 Låg | 3-4 dagar | [#15](#issue-15-försäkringsöversikt) |
| 16 | Grafisk amorteringsplan | 🟢 Låg | 2 dagar | [#16](#issue-16-grafisk-amorteringsplan) |

**Total tid:** ~10-13 dagar (1.5-2 veckor)

---

## Detaljerade Issue-Förslag

### Issue #1: Migrera till SQL Server
**Prioritet:** 🔴 Kritisk | **Estimat:** 3-5 dagar | **Labels:** `database`, `critical`, `infrastructure`

**Beskrivning:**
Migrera från Entity Framework Core InMemory-databas till SQL Server för persistent datalagring i produktion.

**Nuvarande Problem:**
- All data försvinner vid omstart
- Inte lämpligt för produktion
- Risk för dataförlust

**Åtgärder:**
- [ ] Installera `Microsoft.EntityFrameworkCore.SqlServer` NuGet-paket
- [ ] Konfigurera connection string i `appsettings.json`
- [ ] Skapa initial migration: `dotnet ef migrations add InitialCreate`
- [ ] Uppdatera `Program.cs` i både Web och Api projekten
- [ ] Implementera databas-seeding för produktion
- [ ] Lägg till retry-logik: `EnableRetryOnFailure()`
- [ ] Testa migrations och rollback
- [ ] Uppdatera dokumentation med databas-setup instruktioner
- [ ] Överväg Docker-container för SQL Server i utveckling

**Tekniska Detaljer:**
```csharp
// Ersätt i Program.cs:
builder.Services.AddDbContext<PrivatekonomyContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure()
    ));

// appsettings.json:
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=Privatekonomi;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```

**Berörd kod:**
- `src/Privatekonomi.Web/Program.cs`
- `src/Privatekonomi.Api/Program.cs`
- `src/Privatekonomi.Core/Privatekonomi.Core.csproj`
- `appsettings.json` och `appsettings.Development.json`

---

### Issue #2: Tvåfaktorsautentisering
**Prioritet:** 🔴 Kritisk | **Estimat:** 2-3 dagar | **Labels:** `security`, `authentication`, `enhancement`

**Beskrivning:**
Implementera tvåfaktorsautentisering (2FA) för att förbättra säkerheten för användarkonton.

**Nuvarande Situation:**
- ASP.NET Core Identity är implementerad
- Endast lösenordsbaserad autentisering

**Åtgärder:**
- [ ] Aktivera 2FA i ASP.NET Core Identity
- [ ] Implementera TOTP (Time-based One-Time Password)
- [ ] Skapa UI för att aktivera/inaktivera 2FA
- [ ] QR-kod generation för authenticator-appar
- [ ] Backup-koder för återställning
- [ ] Testa med Google Authenticator / Microsoft Authenticator
- [ ] Dokumentera 2FA-setup för användare

**Tekniska Detaljer:**
ASP.NET Core Identity har inbyggt stöd för 2FA. Implementera:
- `UserManager.GenerateTwoFactorTokenAsync()`
- `UserManager.VerifyTwoFactorTokenAsync()`
- UI-komponenter för QR-kod och verifiering

**Berörd kod:**
- `src/Privatekonomi.Web/Components/Pages/Account/` (nya sidor)
- `src/Privatekonomi.Core/Models/ApplicationUser.cs` (eventuella tillägg)

---

### Issue #3: Nettoförmögenhet-översikt
**Prioritet:** 🔴 Hög | **Estimat:** 2-3 dagar | **Labels:** `feature`, `dashboard`, `reporting`

**Beskrivning:**
Lägg till en widget på Dashboard som visar nettoförmögenhet (tillgångar - skulder) och trend över tid.

**Nuvarande Situation:**
- Data finns i `Asset`, `Investment` och `Loan` modeller
- Ingen samlad nettoförmögenhet-vy

**Åtgärder:**
- [ ] Utöka `ReportService` med `GetNetWorthReport()` metod
- [ ] Beräkna totala tillgångar (Assets + Investments)
- [ ] Beräkna totala skulder (Loans)
- [ ] Skapa historisk data för trendgraf (månadsvis)
- [ ] Lägg till Net Worth-kort på Dashboard (`Home.razor`)
- [ ] Implementera linjediagram för trend (använd MudBlazor Chart)
- [ ] Visa procentuell förändring
- [ ] Lägg till färgkodning (grönt för ökning, rött för minskning)
- [ ] Testa med olika tillgångs/skuldnivåer

**Tekniska Detaljer:**
```csharp
public class NetWorthReport
{
    public decimal TotalAssets { get; set; }
    public decimal TotalLiabilities { get; set; }
    public decimal NetWorth { get; set; }
    public decimal PercentageChange { get; set; }
    public List<NetWorthDataPoint> History { get; set; }
}

public class NetWorthDataPoint
{
    public DateTime Date { get; set; }
    public decimal NetWorth { get; set; }
}
```

**Berörd kod:**
- `src/Privatekonomi.Core/Services/ReportService.cs`
- `src/Privatekonomi.Web/Components/Pages/Home.razor`

---

### Issue #4: Notifikationssystem
**Prioritet:** 🟠 Hög | **Estimat:** 5-7 dagar | **Labels:** `feature`, `notifications`, `signalr`

**Beskrivning:**
Implementera ett notifikationssystem för att varna användare om viktiga händelser i realtid.

**Åtgärder:**
- [ ] Skapa `Notification` datamodell
- [ ] Implementera `NotificationService` och `INotificationService`
- [ ] Integrera SignalR för real-time notifikationer
- [ ] Skapa NotificationHub för SignalR
- [ ] Implementera notifikations-center i UI (top bar)
- [ ] Badge för antal olästa notifikationer
- [ ] Implementera specifika notifikationstyper:
  - [ ] Budgetöverdrag
  - [ ] Låg balans (konfigurbar tröskel)
  - [ ] Kommande räkningar (från återkommande transaktioner)
  - [ ] Sparmål uppnått
  - [ ] Banksynkroniseringsfel
- [ ] E-post-notifikationer för kritiska händelser (valfritt)
- [ ] Användarinställningar för notifikationstyper
- [ ] Testa real-time funktionalitet

**Datamodell:**
```csharp
public class Notification
{
    public int NotificationId { get; set; }
    public string UserId { get; set; }
    public string Title { get; set; }
    public string Message { get; set; }
    public NotificationType Type { get; set; }
    public NotificationSeverity Severity { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? RelatedEntityId { get; set; }
    public string? RelatedEntityType { get; set; }
    public ApplicationUser User { get; set; }
}

public enum NotificationType
{
    BudgetExceeded,
    LowBalance,
    UpcomingBill,
    GoalAchieved,
    SyncError,
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

**Berörd kod:**
- `src/Privatekonomi.Core/Models/Notification.cs` (ny)
- `src/Privatekonomi.Core/Services/NotificationService.cs` (ny)
- `src/Privatekonomi.Web/Hubs/NotificationHub.cs` (ny)
- `src/Privatekonomi.Web/Components/Layout/MainLayout.razor` (notification center)

---

### Issue #5: Prognosverktyg
**Prioritet:** 🟠 Hög | **Estimat:** 4-5 dagar | **Labels:** `feature`, `forecasting`, `reporting`

**Beskrivning:**
Skapa ett prognosverktyg som visualiserar förväntade saldon och kassaflöde baserat på historik och återkommande transaktioner.

**Åtgärder:**
- [ ] Skapa `ForecastService` och `IForecastService`
- [ ] Implementera algoritm för prognos baserat på historik
- [ ] Använd genomsnittliga utgifter per kategori
- [ ] Inkludera återkommande transaktioner (när de är implementerade)
- [ ] "Vad händer om"-scenarios
- [ ] Skapa Forecast-widget på Dashboard
- [ ] Visualisering med linjediagram (faktiskt vs förväntat)
- [ ] Konfigurerbar tidsperiod (3, 6, 12 månader)
- [ ] Beräkna förväntad balans per månad
- [ ] Testa noggrannhet mot faktiska data

**Datamodell:**
```csharp
public class ForecastReport
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal CurrentBalance { get; set; }
    public List<ForecastDataPoint> Forecast { get; set; }
}

public class ForecastDataPoint
{
    public DateTime Date { get; set; }
    public decimal ExpectedIncome { get; set; }
    public decimal ExpectedExpenses { get; set; }
    public decimal ExpectedBalance { get; set; }
    public decimal Confidence { get; set; } // 0-1
}
```

**Berörd kod:**
- `src/Privatekonomi.Core/Services/ForecastService.cs` (ny)
- `src/Privatekonomi.Web/Components/Pages/Home.razor`

---

### Issue #6: Återkommande Transaktioner
**Prioritet:** 🟠 Hög | **Estimat:** 5-6 dagar | **Labels:** `feature`, `transactions`, `automation`

**Beskrivning:**
Lägg till stöd för återkommande transaktioner (prenumerationer, hyra, lån) med automatisk skapande och påminnelser.

**Åtgärder:**
- [ ] Skapa `RecurringTransaction` datamodell
- [ ] Implementera `RecurringTransactionService`
- [ ] Stöd för olika frekvenser:
  - [ ] Daglig
  - [ ] Veckovis (specifik dag)
  - [ ] Månadsvis (specifik dag i månaden)
  - [ ] Årlig
- [ ] Background service för att skapa transaktioner automatiskt
- [ ] Skapa Razor-sida för att hantera återkommande transaktioner
- [ ] UI för att skapa/redigera/inaktivera återkommande transaktioner
- [ ] Påminnelser inför kommande transaktioner (använd notifikationssystem)
- [ ] Kopiera från befintlig transaktion till recurring
- [ ] Visa nästa förväntade datum
- [ ] Testa med olika frekvenser och mönster

**Datamodell:**
```csharp
public class RecurringTransaction
{
    public int RecurringTransactionId { get; set; }
    public string UserId { get; set; }
    public string Description { get; set; }
    public decimal Amount { get; set; }
    public int CategoryId { get; set; }
    public int? BankSourceId { get; set; }
    public RecurrenceFrequency Frequency { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? DayOfMonth { get; set; }
    public DayOfWeek? DayOfWeek { get; set; }
    public DateTime? LastCreated { get; set; }
    public DateTime? NextDue { get; set; }
    public bool IsActive { get; set; }
    public bool NotifyBefore { get; set; }
    public int NotifyDaysBefore { get; set; }
    
    public Category Category { get; set; }
    public BankSource? BankSource { get; set; }
    public ApplicationUser User { get; set; }
}

public enum RecurrenceFrequency
{
    Daily,
    Weekly,
    BiWeekly,
    Monthly,
    Quarterly,
    SemiAnnually,
    Annually
}
```

**Background Service:**
```csharp
public class RecurringTransactionBackgroundService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // Kör varje dag kl 00:00
            await CheckAndCreateDueTransactions();
            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        }
    }
}
```

**Berörd kod:**
- `src/Privatekonomi.Core/Models/RecurringTransaction.cs` (ny)
- `src/Privatekonomi.Core/Services/RecurringTransactionService.cs` (ny)
- `src/Privatekonomi.Web/Services/RecurringTransactionBackgroundService.cs` (ny)
- `src/Privatekonomi.Web/Components/Pages/RecurringTransactions.razor` (ny)

---

### Issue #7: Kvittohantering
**Prioritet:** 🟠 Medel | **Estimat:** 4-5 dagar | **Labels:** `feature`, `transactions`, `attachments`

**Beskrivning:**
Lägg till möjlighet att fotografera, ladda upp och koppla kvitton till transaktioner.

**Åtgärder:**
- [ ] Skapa `TransactionAttachment` datamodell
- [ ] Implementera `AttachmentService` och `IAttachmentService`
- [ ] Fil-uppladdning i UI (MudFileUpload)
- [ ] Stöd för bilder (JPEG, PNG) och PDF
- [ ] Miniatyrer för bilder
- [ ] Visa bilagor i transaktionsdetaljer
- [ ] Ladda ner bilagor
- [ ] Ta bort bilagor
- [ ] Lagring lokalt i wwwroot/uploads eller Azure Blob Storage
- [ ] Begränsa filstorlek (max 5 MB per fil)
- [ ] Validering av filtyper
- [ ] Testa med olika filstorlekar och typer

**Datamodell:**
```csharp
public class TransactionAttachment
{
    public int AttachmentId { get; set; }
    public int TransactionId { get; set; }
    public string FileName { get; set; }
    public string FilePath { get; set; } // eller BlobUrl
    public string ContentType { get; set; }
    public long FileSize { get; set; }
    public DateTime UploadedAt { get; set; }
    public string? ThumbnailPath { get; set; }
    
    public Transaction Transaction { get; set; }
}
```

**Berörd kod:**
- `src/Privatekonomi.Core/Models/TransactionAttachment.cs` (ny)
- `src/Privatekonomi.Core/Services/AttachmentService.cs` (ny)
- `src/Privatekonomi.Web/Components/Pages/Transactions.razor` (uppdatera)
- `src/Privatekonomi.Web/wwwroot/uploads/` (ny mapp)

---

### Issue #8: Trend- och Säsongsanalys
**Prioritet:** 🟡 Medel | **Estimat:** 3-4 dagar | **Labels:** `feature`, `analytics`, `reporting`

**Beskrivning:**
Skapa rapporter som visar utgiftstrender och identifierar säsongsmönster.

**Åtgärder:**
- [ ] Utöka `ReportService` med trend-analys metoder
- [ ] Identifiera stigande/fallande utgifter per kategori
- [ ] Jämföra månad-mot-månad
- [ ] Jämföra år-mot-år
- [ ] Säsongsanalys (seasonality detection)
- [ ] Visualisering med trendlinjer
- [ ] Kategori-specifika trender
- [ ] Skapa Trends-sida eller lägg till på Dashboard
- [ ] Testa med minst 12 månaders data

**Berörd kod:**
- `src/Privatekonomi.Core/Services/ReportService.cs`
- `src/Privatekonomi.Web/Components/Pages/Trends.razor` (ny)

---

### Issue #9: Topp-handlare Rapport
**Prioritet:** 🟡 Medel | **Estimat:** 2-3 dagar | **Labels:** `feature`, `reporting`

**Beskrivning:**
Skapa en rapport som visar var mest pengar spenderas och vilka handlare som är vanligast.

**Åtgärder:**
- [ ] Gruppera transaktioner per beskrivning/handlare
- [ ] Beräkna totalt belopp per handlare
- [ ] Räkna antal transaktioner per handlare
- [ ] Visa topp 10/20 handlare
- [ ] Filtrera per tidsperiod
- [ ] Filtrera per kategori
- [ ] Visualisera med stapeldiagram (MudChart)
- [ ] Lägg till på Dashboard eller Reports-sida
- [ ] Exportera till CSV

**Berörd kod:**
- `src/Privatekonomi.Core/Services/ReportService.cs`
- `src/Privatekonomi.Web/Components/Pages/Home.razor`

---

### Issue #10: Målstolpar (Milestones)
**Prioritet:** 🟡 Medel | **Estimat:** 3-4 dagar | **Labels:** `feature`, `goals`, `gamification`

**Beskrivning:**
Lägg till delmål/milestones för sparmål med notifikationer vid uppnådda milestones.

**Åtgärder:**
- [ ] Skapa `GoalMilestone` datamodell
- [ ] Utöka `GoalService` med milestone-hantering
- [ ] Automatiska milestones (25%, 50%, 75%)
- [ ] Anpassade milestones med beskrivningar
- [ ] Notifikationer vid uppnådda milestones
- [ ] Visualisera milestones i progress-bar
- [ ] Historik över uppnådda milestones
- [ ] Testa med olika målbelopp

**Datamodell:**
```csharp
public class GoalMilestone
{
    public int MilestoneId { get; set; }
    public int GoalId { get; set; }
    public decimal TargetAmount { get; set; }
    public int TargetPercentage { get; set; }
    public string? Description { get; set; }
    public bool IsReached { get; set; }
    public DateTime? ReachedAt { get; set; }
    
    public Goal Goal { get; set; }
}
```

**Berörd kod:**
- `src/Privatekonomi.Core/Models/GoalMilestone.cs` (ny)
- `src/Privatekonomi.Core/Services/GoalService.cs`
- `src/Privatekonomi.Web/Components/Pages/Goals.razor`

---

### Issue #11: Tillgångsallokering
**Prioritet:** 🟡 Medel | **Estimat:** 2-3 dagar | **Labels:** `feature`, `investments`, `visualization`

**Beskrivning:**
Visualisera hur investeringsportföljen är fördelad och jämför med målsättning.

**Åtgärder:**
- [ ] Gruppera investeringar per typ (aktier, fonder, certifikat)
- [ ] Beräkna procentuell fördelning
- [ ] Skapa cirkeldiagram för allokering (MudChart)
- [ ] Möjlighet att sätta målallokering per typ
- [ ] Visa avvikelse från mål
- [ ] Lägg till på Investments-sidan
- [ ] Export till CSV

**Berörd kod:**
- `src/Privatekonomi.Core/Services/InvestmentService.cs`
- `src/Privatekonomi.Web/Components/Pages/Investments.razor`

---

### Issue #12: Investeringstransaktioner
**Prioritet:** 🟡 Medel | **Estimat:** 4-5 dagar | **Labels:** `feature`, `investments`, `transactions`

**Beskrivning:**
Lägg till köp/sälj-historik för investeringar och beräkning av realiserade vinster/förluster.

**Åtgärder:**
- [ ] Skapa `InvestmentTransaction` datamodell
- [ ] Stöd för transaktionstyper (köp, sälj, utdelning)
- [ ] Registrera köp/sälj-transaktioner
- [ ] FIFO-metod för kapitalvinst
- [ ] Beräkna realiserade vs orealiserade vinster
- [ ] Historik-vy per investering
- [ ] Integration med K4-rapport
- [ ] Testa med olika scenarion

**Datamodell:**
```csharp
public class InvestmentTransaction
{
    public int InvestmentTransactionId { get; set; }
    public int InvestmentId { get; set; }
    public InvestmentTransactionType Type { get; set; }
    public DateTime TransactionDate { get; set; }
    public decimal Quantity { get; set; }
    public decimal PricePerShare { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal Fees { get; set; }
    public string? Notes { get; set; }
    
    public Investment Investment { get; set; }
}

public enum InvestmentTransactionType
{
    Buy,
    Sell,
    Dividend,
    Split,
    Transfer
}
```

**Berörd kod:**
- `src/Privatekonomi.Core/Models/InvestmentTransaction.cs` (ny)
- `src/Privatekonomi.Core/Services/InvestmentService.cs`
- `src/Privatekonomi.Web/Components/Pages/Investments.razor`

---

### Issue #13: PWA och Offline
**Prioritet:** 🟢 Låg | **Estimat:** 3-4 dagar | **Labels:** `enhancement`, `pwa`, `offline`

**Beskrivning:**
Konvertera applikationen till Progressive Web App med offline-funktionalitet.

**Åtgärder:**
- [ ] Lägg till service worker
- [ ] Skapa manifest.json
- [ ] Offline-cache för statiska tillgångar
- [ ] IndexedDB för offline-data
- [ ] Synkronisera data vid anslutning
- [ ] Installationsbar på mobil
- [ ] Ikoner i olika storlekar
- [ ] Testa offline-funktionalitet

**Berörd kod:**
- `src/Privatekonomi.Web/wwwroot/manifest.json` (ny)
- `src/Privatekonomi.Web/wwwroot/service-worker.js` (ny)
- `src/Privatekonomi.Web/Components/App.razor`

---

### Issue #14: Dividendspårning
**Prioritet:** 🟢 Låg | **Estimat:** 2-3 dagar | **Labels:** `feature`, `investments`

**Beskrivning:**
Lägg till funktionalitet för att spåra och rapportera utdelningar från aktier och fonder.

**Åtgärder:**
- [ ] Skapa `Dividend` datamodell (eller använd InvestmentTransaction)
- [ ] Registrera utdelningar
- [ ] Koppla till investeringar
- [ ] Beräkna direktavkastning (dividend yield)
- [ ] Historik över utdelningar
- [ ] Summera total utdelning per år
- [ ] Visualisera utdelningsinkomst
- [ ] Lägg till på Investments-sidan

**Berörd kod:**
- `src/Privatekonomi.Core/Services/InvestmentService.cs`
- `src/Privatekonomi.Web/Components/Pages/Investments.razor`

---

### Issue #15: Försäkringsöversikt
**Prioritet:** 🟢 Låg | **Estimat:** 3-4 dagar | **Labels:** `feature`, `insurance`

**Beskrivning:**
Skapa en modul för att hantera och övervaka försäkringar.

**Åtgärder:**
- [ ] Skapa `Insurance` datamodell
- [ ] Registrera försäkringar (hem, bil, liv, sjuk, etc.)
- [ ] Premie och förnyelsedatum
- [ ] Påminnelser inför förnyelse (använd notifikationssystem)
- [ ] Totala försäkringskostnader per månad/år
- [ ] Lägg till i månadsbudget automatiskt
- [ ] Skapa Insurance-sida

**Datamodell:**
```csharp
public class Insurance
{
    public int InsuranceId { get; set; }
    public string UserId { get; set; }
    public string Name { get; set; }
    public InsuranceType Type { get; set; }
    public string Provider { get; set; }
    public string PolicyNumber { get; set; }
    public decimal MonthlyPremium { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime RenewalDate { get; set; }
    public bool AutoRenewal { get; set; }
    public string? Notes { get; set; }
    
    public ApplicationUser User { get; set; }
}

public enum InsuranceType
{
    Home,
    Car,
    Life,
    Health,
    Travel,
    Pet,
    Other
}
```

**Berörd kod:**
- `src/Privatekonomi.Core/Models/Insurance.cs` (ny)
- `src/Privatekonomi.Core/Services/InsuranceService.cs` (ny)
- `src/Privatekonomi.Web/Components/Pages/Insurances.razor` (ny)

---

### Issue #16: Grafisk Amorteringsplan
**Prioritet:** 🟢 Låg | **Estimat:** 2 dagar | **Labels:** `enhancement`, `loans`, `visualization`

**Beskrivning:**
Visualisera amorteringsplaner och skuldutveckling för lån.

**Åtgärder:**
- [ ] Skapa linjediagram för skuldutveckling
- [ ] Visa ränta vs amortering över tid (stacked area chart)
- [ ] Jämföra olika amorteringsstrategier visuellt
- [ ] Lägg till på Loans-sidan
- [ ] Exportera amorteringsplan till CSV

**Berörd kod:**
- `src/Privatekonomi.Core/Services/LoanService.cs`
- `src/Privatekonomi.Web/Components/Pages/Loans.razor`

---

## Övergripande Tekniska Överväganden

### Nya NuGet-paket som kan behövas:
- `Microsoft.EntityFrameworkCore.SqlServer` (Issue #1)
- `Microsoft.AspNetCore.SignalR` (Issue #4)
- `QRCoder` eller `QRCodeGenerator` (Issue #2)

### Nya Databastabeller:
- `Notifications`
- `RecurringTransactions`
- `TransactionAttachments`
- `GoalMilestones`
- `InvestmentTransactions`
- `Insurances`

### Nya Services:
- `NotificationService`
- `ForecastService`
- `RecurringTransactionService`
- `AttachmentService`
- `InsuranceService`

### Background Services:
- `RecurringTransactionBackgroundService`
- `NotificationBackgroundService`

---

## Sammanfattning och Nästa Steg

### Vad ska göras först?

**Rekommendation:** Börja med **Fas 1** för att göra applikationen produktionsklar:

1. **Migrera till SQL Server** (Issue #1) - Kritisk för databeständighet
2. **Implementera 2FA** (Issue #2) - Viktigt för säkerhet
3. **Nettoförmögenhet** (Issue #3) - Snabb win, stor impact

Efter Fas 1 kan man fortsätta med Fas 2 funktioner baserat på användarbehov.

### Hur skapar man dessa Issues?

Kopiera beskrivningarna ovan och skapa nya GitHub Issues med:
- Titel från issue-rubriken
- Beskrivning och åtgärder från detalj-sektionen
- Labels enligt förslag
- Estimat i issue-beskrivningen

### Projektets Status efter Fas 1-2:
Med implementationen av Fas 1-2 (ca 3-5 veckor) skulle applikationen ha:
- ✅ 85%+ funktionalitet implementerad
- ✅ Produktionsklar databas
- ✅ Förbättrad säkerhet
- ✅ Viktiga användarfunktioner (notifikationer, prognoser)
- ✅ Bättre användarupplevelse

---

**Slutsats:**  
Privatekonomi är redan ett välutvecklat system med 70% av önskade funktioner. Med fokuserad implementation av Fas 1-2 kan projektet nå 85%+ och vara redo för produktion.

**Nästa steg:**  
1. Granska denna åtgärdsplan
2. Skapa GitHub Issues för prioriterade funktioner
3. Börja med Fas 1 (kritiska förbättringar)

---

**Dokument uppdaterat:** 2025-10-21  
**Se även:** [FUNKTIONSANALYS.md](FUNKTIONSANALYS.md) för fullständig funktionskartläggning
