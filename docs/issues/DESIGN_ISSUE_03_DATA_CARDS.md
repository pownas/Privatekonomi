# Issue 3: Modernisera Datakort med Gradient, Typografi och Ikoner

**Labels:** `design`, `components`, `ux`, `high-priority`, `fas-1`

**Prioritet:** ⭐⭐⭐ Hög

**Estimat:** 2-3 dagar

**Fas:** Fas 1 - Snabba Vinster

---

## Beskrivning

Omdesigna sammanfattnings- och datakort med moderna gradienter, förbättrad typografi och bakgrundsikoner för ett mer visuellt tilltalande utseende som ökar engagemanget och förmedlar information snabbare.

## Bakgrund

- Sammanfattningskort har enkel bakgrund utan visuell differentiering
- Alla kort ser likadana ut oavsett innehåll
- Saknar visuell differentiering mellan olika datatyper (inkomst, utgift, netto)
- Användare kan ha svårt att snabbt identifiera vilken typ av information varje kort innehåller

## Åtgärder

### Fas 3a: Gradient Bakgrunder
- [ ] Implementera subtila gradienter för sammanfattningskort
- [ ] Skapa olika gradient-varianter för olika korttyper:
  - Inkomst-kort: Grön gradient (#10B981 → #059669)
  - Utgifts-kort: Röd gradient (#EF4444 → #DC2626)
  - Netto-kort: Primär gradient (#6366F1 → #8B5CF6)
  - Neutral-kort: Grå gradient för övrig information
- [ ] Säkerställ WCAG-kontrast för text på gradient (minst 4.5:1)
- [ ] Implementera dark mode support för gradienter

### Fas 3b: Bakgrundsikoner
- [ ] Lägg till stora semi-transparenta bakgrundsikoner (opacity 0.15)
- [ ] Positionera ikoner i nedre högra hörnet med `position: absolute`
- [ ] Använd relevanta Material Icons för varje korttyp:
  - Inkomst: `TrendingUp` eller `AttachMoney`
  - Utgift: `TrendingDown` eller `ShoppingCart`
  - Netto: `AccountBalance` eller `Savings`
  - Budget: `PieChart`
- [ ] Säkerställ att ikoner inte stör textläsbarhet

### Fas 3c: Typografisk Förbättring
- [ ] Öka fontvikt för belopp till 600-700
- [ ] Använd Typo.h4 eller större för huvudvärden
- [ ] Lägg till trend-chips med färgkodning
- [ ] Förbättra hierarki mellan etikett och värde
- [ ] Använd `currency-amount` CSS-klass för tabular numbers

## Teknisk Implementation

### Razor-komponent (SummaryCard.razor)

```razor
@* Ny återanvändbar komponent för sammanfattningskort *@

<MudPaper Class="summary-card @GradientClass" Elevation="@Elevation">
    @if (!string.IsNullOrEmpty(IconName))
    {
        <div class="card-icon-bg">
            <MudIcon Icon="@IconName" />
        </div>
    }
    <div class="card-content">
        <MudText Typo="Typo.caption" Class="card-label">@Label</MudText>
        <MudText Typo="Typo.h4" Class="card-value">
            @Value
        </MudText>
        @if (ShowTrend && TrendPercentage.HasValue)
        {
            <MudChip Size="Size.Small" 
                     Color="@GetTrendColor()" 
                     Icon="@GetTrendIcon()"
                     Class="mt-2">
                @FormatTrend()
            </MudChip>
        }
        @if (!string.IsNullOrEmpty(SubText))
        {
            <MudText Typo="Typo.caption" Class="card-subtext">@SubText</MudText>
        }
    </div>
</MudPaper>

@code {
    [Parameter] public string Label { get; set; } = string.Empty;
    [Parameter] public string Value { get; set; } = string.Empty;
    [Parameter] public string SubText { get; set; } = string.Empty;
    [Parameter] public string IconName { get; set; } = string.Empty;
    [Parameter] public CardType Type { get; set; } = CardType.Neutral;
    [Parameter] public bool ShowTrend { get; set; } = false;
    [Parameter] public decimal? TrendPercentage { get; set; }
    [Parameter] public int Elevation { get; set; } = 0;

    private string GradientClass => Type switch
    {
        CardType.Income => "gradient-income",
        CardType.Expense => "gradient-expense",
        CardType.Net => "gradient-primary",
        CardType.Budget => "gradient-budget",
        _ => "gradient-neutral"
    };

    private Color GetTrendColor() => TrendPercentage >= 0 ? Color.Success : Color.Error;
    
    private string GetTrendIcon() => TrendPercentage >= 0 
        ? Icons.Material.Filled.ArrowUpward 
        : Icons.Material.Filled.ArrowDownward;
    
    private string FormatTrend() => $"{(TrendPercentage >= 0 ? "+" : "")}{TrendPercentage:F1}%";
}

public enum CardType
{
    Income,
    Expense,
    Net,
    Budget,
    Neutral
}
```

### CSS-stilar

```css
/* Gradient kort-stilar */
.summary-card {
    position: relative;
    overflow: hidden;
    padding: 24px;
    border-radius: 16px;
    transition: transform 0.2s ease, box-shadow 0.2s ease;
}

.summary-card:hover {
    transform: translateY(-2px);
    box-shadow: 0 8px 24px rgba(0, 0, 0, 0.12);
}

/* Gradient varianter */
.gradient-income {
    background: linear-gradient(135deg, #10B981 0%, #059669 100%);
    color: white;
}

.gradient-expense {
    background: linear-gradient(135deg, #EF4444 0%, #DC2626 100%);
    color: white;
}

.gradient-primary {
    background: linear-gradient(135deg, #6366F1 0%, #8B5CF6 100%);
    color: white;
}

.gradient-budget {
    background: linear-gradient(135deg, #3B82F6 0%, #2563EB 100%);
    color: white;
}

.gradient-neutral {
    background: linear-gradient(135deg, #F9FAFB 0%, #F3F4F6 100%);
    color: #111827;
}

/* Dark mode anpassningar */
.mud-theme-dark .gradient-neutral {
    background: linear-gradient(135deg, #1F2937 0%, #111827 100%);
    color: #F9FAFB;
}

/* Bakgrundsikon */
.card-icon-bg {
    position: absolute;
    right: -20px;
    bottom: -20px;
    opacity: 0.15;
    font-size: 100px;
    pointer-events: none;
}

.card-icon-bg .mud-icon-root {
    font-size: 120px;
}

/* Kort innehåll */
.card-content {
    position: relative;
    z-index: 1;
}

.card-label {
    opacity: 0.9;
    text-transform: uppercase;
    letter-spacing: 0.05em;
    font-weight: 500;
}

.card-value {
    font-weight: 700;
    margin: 8px 0;
    font-variant-numeric: tabular-nums;
}

.card-subtext {
    opacity: 0.8;
    margin-top: 4px;
}

/* Responsiv design */
@media (max-width: 600px) {
    .summary-card {
        padding: 16px;
    }
    
    .card-icon-bg {
        font-size: 80px;
    }
    
    .card-icon-bg .mud-icon-root {
        font-size: 100px;
    }
}
```

### Användning i Home.razor

```razor
<MudGrid Spacing="4" Class="mt-4">
    <MudItem xs="12" sm="6" md="4">
        <SummaryCard Label="Totala Inkomster"
                     Value="@incomeTotal.ToString("C0", new CultureInfo("sv-SE"))"
                     IconName="@Icons.Material.Filled.TrendingUp"
                     Type="CardType.Income"
                     ShowTrend="true"
                     TrendPercentage="12.5m"
                     SubText="vs förra månaden" />
    </MudItem>
    
    <MudItem xs="12" sm="6" md="4">
        <SummaryCard Label="Totala Utgifter"
                     Value="@expenseTotal.ToString("C0", new CultureInfo("sv-SE"))"
                     IconName="@Icons.Material.Filled.ShoppingCart"
                     Type="CardType.Expense"
                     ShowTrend="true"
                     TrendPercentage="-3.2m"
                     SubText="vs förra månaden" />
    </MudItem>
    
    <MudItem xs="12" sm="6" md="4">
        <SummaryCard Label="Nettoresultat"
                     Value="@netTotal.ToString("C0", new CultureInfo("sv-SE"))"
                     IconName="@Icons.Material.Filled.AccountBalance"
                     Type="CardType.Net"
                     ShowTrend="true"
                     TrendPercentage="8.3m"
                     SubText="vs förra månaden" />
    </MudItem>
</MudGrid>
```

## Berörd Kod

### Filer som ska modifieras
- `src/Privatekonomi.Web/Components/Pages/Home.razor`
  - Ersätt befintliga sammanfattningskort med nya `SummaryCard`-komponenter
  - Uppdatera grid spacing och layout

- `src/Privatekonomi.Web/wwwroot/app.css`
  - Lägg till alla gradient-stilar
  - Implementera bakgrundsikon-positioning
  - Lägg till responsiv design för mobil

### Nya filer att skapa
- `src/Privatekonomi.Web/Components/Shared/SummaryCard.razor`
  - Återanvändbar komponent för sammanfattningskort
  - Parametriserad för olika korttyper
  - Stöd för trend-indikatorer

## Acceptanskriterier

- [ ] Sammanfattningskort har färgade gradienter baserat på typ
- [ ] Inkomst-kort använder grön gradient
- [ ] Utgifts-kort använder röd gradient
- [ ] Netto-kort använder primär (indigo/lila) gradient
- [ ] Varje kort har en relevant bakgrundsikon
- [ ] Bakgrundsikoner är semi-transparenta (opacity 0.15) och stör inte läsbarheten
- [ ] Typografi är tydlig och läsbar mot gradientbakgrund
- [ ] Text uppfyller WCAG AA kontrast-krav (minst 4.5:1)
- [ ] Trend-chips visas korrekt med färgkodning
- [ ] Dark mode fungerar korrekt med anpassade färger
- [ ] Kort har hover-effekt (lift animation)
- [ ] Responsiv design fungerar på mobil och desktop
- [ ] `SummaryCard`-komponenten är återanvändbar i andra vyer

## Referens

- **Källdokument:** `docs/DESIGN_ANALYSIS_2025.md` sektion "Förslag 3: Moderniserade Datakort"
- **Huvudissue:** `docs/issues/DESIGN_IMPLEMENTATION_SUB_ISSUES.md`
- **Relaterad dokumentation:** `docs/VISUAL_UX_IMPROVEMENTS.md`

## Estimerad Tidslinje

1. **Dag 1:** Skapa `SummaryCard.razor` komponent och grundläggande gradient-stilar
2. **Dag 2:** Implementera bakgrundsikoner och uppdatera Home.razor
3. **Dag 3:** Testa och finslipa (dark mode, responsiv design, WCAG-kontrast)

---

**Senast uppdaterad:** 2025-12-06  
**Version:** 1.0  
**Status:** Redo för implementation
