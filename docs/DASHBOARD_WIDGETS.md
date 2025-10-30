# Dashboard Widget-system

## Översikt

Privatekonomi har ett flexibelt widget-system som låter användare skapa personliga dashboard-layouter. Systemet stödjer drag-and-drop (framtida funktion), anpassningsbara widgets, och flera dashboard-layouter för olika användningsfall.

## Tillgängliga Widgets

### 1. Nettoförmögenhet (NetWorth)
Visar din totala nettoförmögenhet med:
- Aktuellt värde
- Procentuell förändring
- Historisk graf
- Uppdelning på tillgångar och skulder

### 2. Kassaflöde (CashFlow)
Visar kassaflöde över tid:
- Inkomster per månad
- Utgifter per månad
- Nettokassaflöde
- Linjediagram över 12 månader

### 3. Sparmål (Goals)
Översikt av dina aktiva sparmål:
- Progress bar för varje mål
- Sparad summa vs målsumma
- Måldatum
- Snabblänk för att lägga till nya mål

### 4. Budgetöversikt (BudgetOverview)
Visar aktiva budgetar:
- Planerad vs faktisk förbrukning
- Progress bars med färgkodning
- Datum för budgetperiod
- Max 3 budgetar visas

### 5. Senaste Transaktioner (RecentTransactions)
Lista över senaste transaktionerna:
- Beskrivning och datum
- Belopp med färgkodning (grönt=inkomst, rött=utgift)
- Kategorier visas som chips
- Visar max 10 transaktioner

### 6. Snabbåtgärder (QuickActions)
Snabbknappar för vanliga åtgärder:
- Lägg till transaktion
- Skapa sparmål
- Skapa budget
- Lägg till räkning
- Lägg till investering
- Lägg till lån

### 7. Periodjämförelse (PeriodComparison)
Jämför denna månad med föregående:
- Utgifter, inkomster, nettoresultat
- Procentuell förändring
- Trendpilar
- År-mot-år jämförelse

## Komma igång

### Skapa din första layout

1. Navigera till **Anpassad Dashboard** i menyn
2. Klicka på **Hantera Layouter**
3. Klicka på **Skapa ny layout**
4. Namnge din layout (t.ex. "Hem", "Investeringar", "Budget")
5. Layouten skapas med en grundkonfiguration

### Hantera layouter

#### Sätt som standard
Klicka på stjärnikonen bredvid en layout för att sätta den som standard. Standard-layouten visas automatiskt när du öppnar Anpassad Dashboard.

#### Ta bort layout
Klicka på papperskorgen för att ta bort en layout. Du kan inte ta bort din standard-layout - sätt först en annan layout som standard.

#### Välja layout
Klicka på pennaikonen för att välja och visa en layout.

## Default Layout

När en ny användare skapar sitt första konto får de automatiskt en default layout med följande widgets:

1. **Nettoförmögenhet** - Full bredd, 2 höjd
2. **Periodjämförelse** - Full bredd, 1 höjd
3. **Kassaflöde** - Halv bredd (vänster), 2 höjd
4. **Budgetöversikt** - Halv bredd (höger), 2 höjd
5. **Senaste Transaktioner** - Full bredd, 2 höjd

## Teknisk Implementation

### Datamodeller

```csharp
// Dashboard layout för en användare
public class DashboardLayout
{
    public int LayoutId { get; set; }
    public string UserId { get; set; }
    public string Name { get; set; }
    public bool IsDefault { get; set; }
    public List<WidgetConfiguration> Widgets { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

// Widget-konfiguration
public class WidgetConfiguration
{
    public int WidgetConfigId { get; set; }
    public int LayoutId { get; set; }
    public WidgetType Type { get; set; }
    public int Row { get; set; }
    public int Column { get; set; }
    public int Width { get; set; }  // 1-12 (Bootstrap grid)
    public int Height { get; set; }
    public string? Settings { get; set; }  // JSON för widget-specifika inställningar
}
```

### Tillgängliga Widget-typer

```csharp
public enum WidgetType
{
    NetWorth,              // Nettoförmögenhet
    CashFlow,              // Kassaflöde
    Goals,                 // Sparmål
    Loans,                 // Lån (ej implementerad ännu)
    Investments,           // Investeringar (ej implementerad ännu)
    BudgetOverview,        // Budgetöversikt
    UpcomingBills,         // Kommande räkningar (ej implementerad ännu)
    MonthlyExpenses,       // Månadens utgifter (ej implementerad ännu)
    CategoryPieChart,      // Kategoridiagram (ej implementerad ännu)
    TrendChart,            // Trenddiagram (ej implementerad ännu)
    QuickActions,          // Snabbåtgärder
    MonthSummary,          // Månadssammanfattning (ej implementerad ännu)
    PeriodComparison,      // Periodjämförelse
    RecentTransactions,    // Senaste transaktioner
    UnmappedTransactions   // Okategoriserade transaktioner (ej implementerad ännu)
}
```

### Services

```csharp
public interface IDashboardLayoutService
{
    Task<IEnumerable<DashboardLayout>> GetUserLayoutsAsync(string userId);
    Task<DashboardLayout?> GetDefaultLayoutAsync(string userId);
    Task<DashboardLayout?> GetLayoutByIdAsync(int layoutId);
    Task<DashboardLayout> CreateLayoutAsync(DashboardLayout layout);
    Task<DashboardLayout> UpdateLayoutAsync(DashboardLayout layout);
    Task DeleteLayoutAsync(int layoutId);
    Task SetDefaultLayoutAsync(int layoutId, string userId);
    Task<DashboardLayout> CreateDefaultLayoutForUserAsync(string userId);
}
```

### Komponenter

**Widget-komponenter:**
- `WidgetBase.razor` - Baskomponent för alla widgets
- `NetWorthWidget.razor` - Nettoförmögenhet
- `CashFlowWidget.razor` - Kassaflöde
- `GoalsWidget.razor` - Sparmål
- `BudgetOverviewWidget.razor` - Budgetöversikt
- `RecentTransactionsWidget.razor` - Senaste transaktioner
- `QuickActionsWidget.razor` - Snabbåtgärder

**Layout-komponenter:**
- `CustomDashboard.razor` - Huvudsida för anpassad dashboard
- `DashboardLayoutDialog.razor` - Dialog för att hantera layouter
- `WidgetRenderer.razor` - Dynamisk rendering av widgets

## Framtida funktioner

### Fas 1 (Planerat)
- ✅ Grundläggande widget-system
- ✅ Layout-hantering
- ✅ 6 widgets implementerade

### Fas 2 (Kommande)
- [ ] Drag-and-drop för att ordna widgets
- [ ] Widget-storlek anpassning
- [ ] Fler widget-typer (15+ totalt)
- [ ] Widget-specifika inställningar
- [ ] Dela layouter mellan familjemedlemmar

### Fas 3 (Framtida)
- [ ] Export/import av layouter
- [ ] Mallar för layouter
- [ ] Widget-galeri
- [ ] Community-delning av layouter

## Exempel på användningsfall

### "Hem" Layout
- Nettoförmögenhet
- Periodjämförelse
- Snabbåtgärder
- Senaste transaktioner

### "Investeringar" Layout
- Nettoförmögenhet
- Investeringar widget (ej implementerad ännu)
- Kassaflöde
- Dividender (ej implementerad ännu)

### "Budget" Layout
- Budgetöversikt
- Månadens utgifter (ej implementerad ännu)
- Kategorifördelning (ej implementerad ännu)
- Okategoriserade transaktioner (ej implementerad ännu)

## Felsökning

### Widget visar "Inga data"
- Kontrollera att du har transaktioner/data för widgeten
- Vissa widgets kräver minst 4 månaders data för att visa diagram

### Layout sparas inte
- Kontrollera att du är inloggad
- Kontrollera din internetanslutning
- Kontrollera browser-konsolen för felmeddelanden

### Widget renderas inte
- Kontrollera att widget-typen är implementerad
- Se browser-konsolen för fel
- Kontakta support om problemet kvarstår

## Support

För frågor eller problem med widget-systemet, vänligen:
1. Kontrollera denna dokumentation
2. Sök i GitHub Issues
3. Skapa en ny issue med taggen `dashboard` och `widgets`
