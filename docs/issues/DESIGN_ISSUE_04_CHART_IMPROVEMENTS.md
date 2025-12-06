# Issue 4: Förbättra Diagram med Enhetlig Färgpalett och Interaktivitet

**Labels:** `design`, `charts`, `ux`, `medium-priority`, `fas-2`

**Prioritet:** ⭐⭐ Medel

**Estimat:** 2-3 dagar

**Fas:** Fas 2 - Visuella Förbättringar

---

## Beskrivning

Förbättra diagram med en modern, harmonisk färgpalett, bättre kortstruktur och ökad interaktivitet för en mer engagerande användarupplevelse som hjälper användare förstå sina ekonomiska data bättre.

## Bakgrund

- Diagram använder grundläggande MudBlazor-styling
- Färgpaletten kan förbättras för bättre harmoni och visuell konsistens
- Saknar interaktiva element och navigeringslänkar
- Diagramkort har minimal struktur (ingen header/footer)
- Användare kan ha svårt att tolka diagram utan kontext

## Åtgärder

### Fas 4a: Enhetlig Färgpalett
- [ ] Implementera modern färgpalett för diagram baserat på primärfärger
- [ ] Säkerställ tillräcklig kontrast mellan angränsande färger
- [ ] Skapa återanvändbara färgkonstanter i `ChartColors.cs`
- [ ] Dokumentera färgval och användningsområden
- [ ] Testa färgpalett med färgblindhetssimulator

### Fas 4b: Diagramkort
- [ ] Lägg till header med titel och filterknappar
- [ ] Implementera footer med navigeringslänk ("Se alla...")
- [ ] Förbättra padding och spacing
- [ ] Lägg till border-radius för modernare utseende
- [ ] Implementera hover-effekt på diagramkort

### Fas 4c: Interaktivitet
- [ ] Lägg till entrance-animationer för diagram
- [ ] Implementera filter-knappar (Månadsvis/Kvartalsvis/Årsvis)
- [ ] Förbättra tooltips med mer information (belopp, procent, jämförelse)
- [ ] Lägg till loading-state för diagram
- [ ] Implementera error-state med hjälptext

## Teknisk Implementation

### Färgpalett (ChartColors.cs)

```csharp
namespace Privatekonomi.Core.Constants
{
    /// <summary>
    /// Enhetlig färgpalett för diagram i Privatekonomi
    /// </summary>
    public static class ChartColors
    {
        /// <summary>
        /// Modern harmonisk färgpalett baserad på Material Design principer
        /// </summary>
        public static readonly string[] ModernPalette = new[]
        {
            "#6366F1",  // Indigo (primär) - Används för viktigaste kategorin
            "#EC4899",  // Rosa - Kontrast till indigo
            "#8B5CF6",  // Lila - Harmonisk med indigo
            "#10B981",  // Grön - Positiva värden, inkomst
            "#F59E0B",  // Orange - Uppmärksamhet, varning
            "#3B82F6",  // Blå - Lugn, stabil
            "#EF4444",  // Röd - Negativa värden, utgift
            "#06B6D4",  // Cyan - Frisk, tillgänglighet
            "#8B5A00",  // Brun - Neutral
            "#6B7280",  // Grå - Okategoriserad
        };

        /// <summary>
        /// Färgpalett för inkomst/utgift-jämförelser
        /// </summary>
        public static readonly string[] IncomeExpensePalette = new[]
        {
            "#10B981",  // Grön - Inkomst
            "#EF4444",  // Röd - Utgift
            "#6366F1",  // Indigo - Netto
        };

        /// <summary>
        /// Gradient-färger för areakurvor
        /// </summary>
        public static readonly string[] GradientColors = new[]
        {
            "#6366F1",  // Start
            "#8B5CF6",  // Mitt
            "#EC4899",  // Slut
        };
    }
}
```

### ChartCard-komponent (ChartCard.razor)

```razor
@* Återanvändbar komponent för diagramkort *@

<MudPaper Class="chart-container chart-enter" Elevation="2">
    <div class="chart-header">
        <div>
            <MudText Typo="Typo.h6">@Title</MudText>
            @if (!string.IsNullOrEmpty(Subtitle))
            {
                <MudText Typo="Typo.caption" Class="text-muted">@Subtitle</MudText>
            }
        </div>
        @if (ShowPeriodFilter)
        {
            <MudButtonGroup Size="Size.Small" Variant="Variant.Outlined" Color="Color.Primary">
                <MudButton OnClick="@(() => OnPeriodChanged("month"))"
                           Variant="@(SelectedPeriod == "month" ? Variant.Filled : Variant.Outlined)">
                    Månadsvis
                </MudButton>
                <MudButton OnClick="@(() => OnPeriodChanged("quarter"))"
                           Variant="@(SelectedPeriod == "quarter" ? Variant.Filled : Variant.Outlined)">
                    Kvartalsvis
                </MudButton>
                <MudButton OnClick="@(() => OnPeriodChanged("year"))"
                           Variant="@(SelectedPeriod == "year" ? Variant.Filled : Variant.Outlined)">
                    Årsvis
                </MudButton>
            </MudButtonGroup>
        }
    </div>
    
    <div class="chart-content">
        @if (IsLoading)
        {
            <div class="chart-loading">
                <MudProgressCircular Color="Color.Primary" Indeterminate="true" />
                <MudText Typo="Typo.body2" Class="mt-2">Laddar diagram...</MudText>
            </div>
        }
        else if (HasError)
        {
            <div class="chart-error">
                <MudIcon Icon="@Icons.Material.Filled.Error" Color="Color.Error" Size="Size.Large" />
                <MudText Typo="Typo.body2" Class="mt-2">@ErrorMessage</MudText>
            </div>
        }
        else
        {
            @ChildContent
        }
    </div>
    
    @if (!string.IsNullOrEmpty(FooterLinkText) && !string.IsNullOrEmpty(FooterLinkHref))
    {
        <div class="chart-footer">
            <MudLink Href="@FooterLinkHref" Typo="Typo.body2">
                @FooterLinkText →
            </MudLink>
        </div>
    }
</MudPaper>

@code {
    [Parameter] public string Title { get; set; } = string.Empty;
    [Parameter] public string Subtitle { get; set; } = string.Empty;
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public bool ShowPeriodFilter { get; set; } = false;
    [Parameter] public string SelectedPeriod { get; set; } = "month";
    [Parameter] public EventCallback<string> PeriodChanged { get; set; }
    [Parameter] public bool IsLoading { get; set; } = false;
    [Parameter] public bool HasError { get; set; } = false;
    [Parameter] public string ErrorMessage { get; set; } = "Ett fel uppstod vid laddning av diagram";
    [Parameter] public string FooterLinkText { get; set; } = string.Empty;
    [Parameter] public string FooterLinkHref { get; set; } = string.Empty;

    private async Task OnPeriodChanged(string period)
    {
        SelectedPeriod = period;
        await PeriodChanged.InvokeAsync(period);
    }
}
```

### CSS-stilar

```css
/* Chart container */
.chart-container {
    padding: var(--spacing-lg);
    border-radius: var(--radius-lg);
    transition: transform 0.2s ease, box-shadow 0.2s ease;
}

.chart-container:hover {
    transform: translateY(-2px);
    box-shadow: 0 8px 24px rgba(0, 0, 0, 0.12);
}

/* Chart header */
.chart-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: var(--spacing-md);
    flex-wrap: wrap;
    gap: var(--spacing-sm);
}

.chart-header h6 {
    margin: 0;
}

/* Chart content */
.chart-content {
    min-height: 300px;
    display: flex;
    align-items: center;
    justify-content: center;
}

.chart-loading,
.chart-error {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    min-height: 300px;
}

/* Chart footer */
.chart-footer {
    margin-top: var(--spacing-md);
    text-align: right;
    padding-top: var(--spacing-sm);
    border-top: 1px solid rgba(0, 0, 0, 0.08);
}

.mud-theme-dark .chart-footer {
    border-top-color: rgba(255, 255, 255, 0.08);
}

/* Entrance animation */
@keyframes fadeInUp {
    from { 
        opacity: 0; 
        transform: translateY(20px); 
    }
    to { 
        opacity: 1; 
        transform: translateY(0); 
    }
}

.chart-enter {
    animation: fadeInUp 0.5s ease-out;
}

/* Respektera prefers-reduced-motion */
@media (prefers-reduced-motion: reduce) {
    .chart-enter {
        animation: none;
    }
}

/* Responsiv design */
@media (max-width: 600px) {
    .chart-header {
        flex-direction: column;
        align-items: flex-start;
    }
    
    .chart-container {
        padding: var(--spacing-md);
    }
}
```

### Användning i Home.razor

```razor
<ChartCard Title="Utgiftsfördelning per Kategori"
           Subtitle="Senaste 30 dagarna"
           ShowPeriodFilter="true"
           SelectedPeriod="@selectedPeriod"
           PeriodChanged="OnPeriodChanged"
           IsLoading="@isLoadingChart"
           FooterLinkText="Se alla utgifter"
           FooterLinkHref="/utgifter">
    <MudChart ChartType="ChartType.Pie" 
              InputData="@expenseData" 
              InputLabels="@expenseLabels"
              Width="100%" 
              Height="300px"
              ChartOptions="@chartOptions" />
</ChartCard>

@code {
    private string selectedPeriod = "month";
    private bool isLoadingChart = false;
    
    private ChartOptions chartOptions = new()
    {
        ChartPalette = ChartColors.ModernPalette,
        InterpolationOption = InterpolationOption.NaturalSpline,
        YAxisTicks = 5,
        MaxNumYAxisTicks = 10
    };
    
    private async Task OnPeriodChanged(string period)
    {
        isLoadingChart = true;
        StateHasChanged();
        
        // Ladda om data baserat på period
        await LoadChartData(period);
        
        isLoadingChart = false;
        StateHasChanged();
    }
}
```

## Berörd Kod

### Filer som ska modifieras
- `src/Privatekonomi.Web/Components/Pages/Home.razor`
  - Ersätt befintliga diagram med `ChartCard`-komponenter
  - Implementera period-filter funktionalitet
  - Använd nya färgpaletten

### Nya filer att skapa
- `src/Privatekonomi.Core/Constants/ChartColors.cs`
  - Enhetlig färgpalett för alla diagram
  - Dokumenterade färgval

- `src/Privatekonomi.Web/Components/Shared/ChartCard.razor`
  - Återanvändbar komponent för diagramkort
  - Header, footer, loading och error-states

- `src/Privatekonomi.Web/wwwroot/app.css`
  - Lägg till chart-container stilar
  - Entrance-animationer
  - Responsiv design

## Acceptanskriterier

- [ ] Alla diagram använder den nya harmoniska färgpaletten (`ChartColors.ModernPalette`)
- [ ] Färgpaletten har testats med färgblindhetssimulator
- [ ] Diagramkort har tydlig header med titel och subtitle
- [ ] Period-filter knappar fungerar och uppdaterar diagram
- [ ] Loading-state visas under datahämtning
- [ ] Error-state visas med hjälptext vid fel
- [ ] Navigeringslänkar i footer leder till detaljerade vyer
- [ ] Entrance-animationer fungerar smidigt
- [ ] Animationer respekterar `prefers-reduced-motion`
- [ ] Hover-effekt på diagramkort fungerar
- [ ] Tooltips visar detaljerad information (belopp, procent)
- [ ] Dark mode fungerar korrekt
- [ ] Responsiv design fungerar på mobil och desktop
- [ ] `ChartCard`-komponenten är återanvändbar i alla vyer med diagram

## Referens

- **Källdokument:** `docs/DESIGN_ANALYSIS_2025.md` sektion "Förslag 4: Diagramförbättringar"
- **Huvudissue:** `docs/issues/DESIGN_IMPLEMENTATION_SUB_ISSUES.md`
- **Relaterad dokumentation:** 
  - `docs/VISUAL_UX_IMPROVEMENTS.md`
  - `docs/CHART_DESIGN_GUIDELINES.md`

## Estimerad Tidslinje

1. **Dag 1:** Skapa `ChartColors.cs` och `ChartCard.razor` komponent
2. **Dag 2:** Implementera period-filter och uppdatera Home.razor
3. **Dag 3:** Testa och finslipa (animationer, loading/error states, responsiv design)

---

**Senast uppdaterad:** 2025-12-06  
**Version:** 1.0  
**Status:** Redo för implementation
