# Användningsexempel för Förbättrad Grafdesign

Detta dokument innehåller praktiska exempel på hur man använder den nya grafdesignen i Privatekonomi-systemet.

## Innehållsförteckning
1. [Grundläggande Implementation](#grundläggande-implementation)
2. [Responsiva Grafer](#responsiva-grafer)
3. [Tillgänglighet (WCAG)](#tillgänglighet-wcag)
4. [Tomma Tillstånd](#tomma-tillstånd)
5. [Filter och Kontroller](#filter-och-kontroller)
6. [Datatabeller](#datatabeller)
7. [Kompletta Exempel](#kompletta-exempel)

## Grundläggande Implementation

### 1. Inkludera CSS-filen
CSS-filen är redan inkluderad i `App.razor`:
```html
<link rel="stylesheet" href="@Assets["css/charts.css"]" />
```

### 2. Enkel Linjegraf med Container

```razor
<div class="chart-container">
    <MudText Typo="Typo.h4" Class="mb-4">Min Graf</MudText>
    <MudText Typo="Typo.body2" Color="Color.Secondary" Class="mb-4">
        Beskrivning av vad grafen visar
    </MudText>
</div>

<MudPaper Class="pa-4 chart-wrapper" Elevation="2">
    <div class="chart-header">
        <MudText Typo="Typo.h6" Class="chart-title">Grafens Titel</MudText>
        <MudText Typo="Typo.body2" Color="Color.Secondary" Class="chart-subtitle">
            Undertitel eller förklaring
        </MudText>
    </div>
    
    <div role="img" aria-label="Beskrivande text för skärmläsare">
        <MudChart ChartType="ChartType.Line" 
                  ChartSeries="@series" 
                  XAxisLabels="@labels"
                  Width="100%" 
                  Height="350px"
                  ChartOptions="@chartOptions"
                  Class="chart-responsive chart-height-lg" />
    </div>
    
    <!-- Skärmläsarbeskrivning -->
    <p class="chart-sr-only">
        Detaljerad beskrivning av grafen för skärmläsare.
        Inkludera nyckeltal och trender.
    </p>
</MudPaper>
```

### 3. ChartOptions Setup

```csharp
@code {
    private ChartOptions chartOptions = new ChartOptions
    {
        YAxisTicks = 10000,
        YAxisFormat = "C0", // Valuta utan decimaler
        InterpolationOption = InterpolationOption.NaturalSpline,
        LineStrokeWidth = 2,
        MaxNumYAxisTicks = 8
    };
}
```

## Responsiva Grafer

### Side-by-Side Grafer (Desktop) / Stacked (Mobil)

```razor
<MudGrid Spacing="3">
    <MudItem xs="12" md="6">
        <MudPaper Class="pa-4 chart-wrapper" Elevation="2">
            <div class="chart-header">
                <MudText Typo="Typo.h6" Class="chart-title">Graf 1</MudText>
            </div>
            <div role="img" aria-label="Beskrivning av graf 1">
                <MudChart ChartType="ChartType.Pie" 
                          InputData="@data1"
                          InputLabels="@labels1"
                          Width="100%" 
                          Height="300px"
                          Class="chart-responsive chart-height-md" />
            </div>
        </MudPaper>
    </MudItem>

    <MudItem xs="12" md="6">
        <MudPaper Class="pa-4 chart-wrapper" Elevation="2">
            <div class="chart-header">
                <MudText Typo="Typo.h6" Class="chart-title">Graf 2</MudText>
            </div>
            <div role="img" aria-label="Beskrivning av graf 2">
                <MudChart ChartType="ChartType.Pie" 
                          InputData="@data2"
                          InputLabels="@labels2"
                          Width="100%" 
                          Height="300px"
                          Class="chart-responsive chart-height-md" />
            </div>
        </MudPaper>
    </MudItem>
</MudGrid>
```

**Resultat:**
- **Mobile (xs)**: Graferna staplas vertikalt, 100% bredd vardera
- **Tablet (md+)**: Graferna visas sida vid sida, 50% bredd vardera

### Responsiva Höjder

```razor
<!-- Liten graf för sammanfattningar -->
<MudChart ... Class="chart-height-sm" />  <!-- 280px mobil, 300px desktop -->

<!-- Medium graf för sekundära visualiseringar -->
<MudChart ... Class="chart-height-md" />  <!-- 320px mobil, 400px desktop -->

<!-- Stor graf för primära visualiseringar -->
<MudChart ... Class="chart-height-lg" />  <!-- 350px mobil, 450px desktop -->
```

## Tillgänglighet (WCAG)

### ARIA-labels och Roller

```razor
<!-- Graf med ARIA-attribut -->
<div role="img" aria-label="Linjediagram som visar nettoförmögenhet från jan 2023 till dec 2024">
    <MudChart ... />
</div>

<!-- Skärmläsartext (dold visuellt, läsbar för skärmläsare) -->
<p class="chart-sr-only">
    Grafen visar en ökning av nettoförmögenhet från 500 000 kr till 750 000 kr,
    vilket motsvarar en ökning på 50% under perioden.
</p>
```

### Tangentbordsnavigering

```razor
<!-- Filter med tangentbordsstöd -->
<MudButtonGroup Color="Color.Primary" Variant="Variant.Outlined" Size="Size.Small">
    <MudButton OnClick="..." 
               Variant="@(selected ? Variant.Filled : Variant.Outlined)"
               aria-label="Visa data för senaste året"
               Class="chart-filter-button chart-touch-target">
        1 år
    </MudButton>
    <MudButton OnClick="..." 
               Variant="@(selected ? Variant.Filled : Variant.Outlined)"
               aria-label="Visa data för senaste 5 åren"
               Class="chart-filter-button chart-touch-target">
        5 år
    </MudButton>
</MudButtonGroup>
```

### Trend-indikatorer (Inte bara färg)

```razor
<div class="trend-indicator @(value >= 0 ? "positive" : "negative")">
    <MudIcon Icon="@(value >= 0 ? Icons.Material.Filled.TrendingUp : Icons.Material.Filled.TrendingDown)" 
             Size="Size.Small" 
             Class="trend-icon" />
    <MudText Color="@(value >= 0 ? Color.Success : Color.Error)">
        @(value >= 0 ? "+" : "")@value.ToString("C0", new System.Globalization.CultureInfo("sv-SE"))
    </MudText>
</div>
```

**Resultat:** Positiva värden visas med grön färg + uppåtpil, negativa med röd färg + nedåtpil. Informationen förmedlas både visuellt och via ikoner.

## Tomma Tillstånd

### Tom Graf med Informativ Meddelande

```razor
@if (data.Any())
{
    <div role="img" aria-label="Graf beskrivning">
        <MudChart ChartType="ChartType.Line" ... />
    </div>
}
else
{
    <div class="chart-empty">
        <MudIcon Icon="@Icons.Material.Filled.ShowChart" Class="chart-empty-icon" />
        <MudText Class="chart-empty-message">Ingen data att visa</MudText>
        <MudText Typo="Typo.body2" Class="chart-empty-hint">
            Data kommer att visas när du har registrerat transaktioner.
        </MudText>
    </div>
}
```

### Laddar-tillstånd

```razor
@if (loading)
{
    <div class="chart-loading">
        <MudProgressCircular Color="Color.Primary" Indeterminate="true" />
    </div>
}
else
{
    <!-- Graf här -->
}
```

## Filter och Kontroller

### Responsiv Filterlayout

```razor
<MudPaper Class="pa-4 mb-4 chart-filters" Elevation="2">
    <MudGrid Spacing="3">
        <!-- Tidsperiod -->
        <MudItem xs="12" sm="6" md="4">
            <div class="filter-group">
                <MudText Typo="Typo.subtitle1" Class="filter-label mb-2">Tidsperiod</MudText>
                <MudButtonGroup Color="Color.Primary" Variant="Variant.Outlined" Size="Size.Small">
                    <MudButton OnClick="..." 
                               Variant="@(selectedPeriod == 12 ? Variant.Filled : Variant.Outlined)"
                               aria-label="Visa senaste året"
                               Class="chart-filter-button chart-touch-target">
                        1 år
                    </MudButton>
                    <MudButton OnClick="..." 
                               Variant="@(selectedPeriod == 60 ? Variant.Filled : Variant.Outlined)"
                               aria-label="Visa senaste 5 åren"
                               Class="chart-filter-button chart-touch-target">
                        5 år
                    </MudButton>
                </MudButtonGroup>
            </div>
        </MudItem>

        <!-- Gruppering -->
        <MudItem xs="12" sm="6" md="4">
            <div class="filter-group">
                <MudText Typo="Typo.subtitle1" Class="filter-label mb-2">Gruppering</MudText>
                <MudButtonGroup Color="Color.Primary" Variant="Variant.Outlined" Size="Size.Small">
                    <MudButton OnClick="..." 
                               Variant="@(groupBy == "month" ? Variant.Filled : Variant.Outlined)"
                               aria-label="Gruppera månadsvis"
                               Class="chart-filter-button chart-touch-target">
                        Månadsvis
                    </MudButton>
                    <MudButton OnClick="..." 
                               Variant="@(groupBy == "year" ? Variant.Filled : Variant.Outlined)"
                               aria-label="Gruppera årligen"
                               Class="chart-filter-button chart-touch-target">
                        Årligen
                    </MudButton>
                </MudButtonGroup>
            </div>
        </MudItem>

        <!-- Export -->
        <MudItem xs="12" md="4" Class="d-flex align-end justify-md-end justify-start">
            <div class="chart-export-group">
                <MudButtonGroup Variant="Variant.Outlined" Size="Size.Small">
                    <MudButton StartIcon="@Icons.Material.Filled.FileDownload" 
                               OnClick="ExportCSV"
                               Color="Color.Primary"
                               aria-label="Exportera data som CSV-fil"
                               Class="chart-action-button chart-touch-target">
                        <span class="d-none d-sm-inline">Exportera </span>CSV
                    </MudButton>
                    <MudButton StartIcon="@Icons.Material.Filled.Image" 
                               OnClick="ExportPNG"
                               Color="Color.Primary"
                               aria-label="Exportera graf som bild"
                               Class="chart-action-button chart-touch-target">
                        <span class="d-none d-sm-inline">Exportera </span>PNG
                    </MudButton>
                </MudButtonGroup>
            </div>
        </MudItem>
    </MudGrid>
</MudPaper>
```

**Förklaring:**
- `xs="12"` - Full bredd på mobil
- `sm="6"` - Halv bredd på tablet
- `md="4"` - Tredjedel bredd på desktop
- `d-none d-sm-inline` - Döljer "Exportera" text på mobil, visar på tablet+
- `chart-touch-target` - Säkerställer minst 44x44px klickområde

## Datatabeller

### WCAG-kompatibel Datatabell som Alternativ

```razor
<MudPaper Class="pa-4 mt-4 chart-data-table" Elevation="2">
    <div class="chart-header">
        <MudText Typo="Typo.h6" Class="chart-title">Detaljerad Data</MudText>
        <MudText Typo="Typo.body2" Color="Color.Secondary" Class="chart-subtitle">
            Tabellvy av all data som visas i grafen
        </MudText>
    </div>
    
    <MudTable Items="@data" 
              Dense="true" 
              Hover="true" 
              Striped="true"
              aria-label="Detaljerad historik över ekonomiska data"
              Breakpoint="Breakpoint.Sm">
        <HeaderContent>
            <MudTh>Period</MudTh>
            <MudTh Style="text-align: right">Värde</MudTh>
            <MudTh Style="text-align: right">Förändring</MudTh>
        </HeaderContent>
        <RowTemplate>
            <MudTd DataLabel="Period">@context.Period</MudTd>
            <MudTd DataLabel="Värde" Style="text-align: right">
                @context.Value.ToString("C0", new System.Globalization.CultureInfo("sv-SE"))
            </MudTd>
            <MudTd DataLabel="Förändring" Style="text-align: right">
                <div class="trend-indicator @(context.Change >= 0 ? "positive" : "negative")">
                    <MudIcon Icon="@(context.Change >= 0 ? Icons.Material.Filled.TrendingUp : Icons.Material.Filled.TrendingDown)" 
                             Size="Size.Small" />
                    <MudText Color="@(context.Change >= 0 ? Color.Success : Color.Error)">
                        @(context.Change >= 0 ? "+" : "")@context.Change.ToString("C0", new System.Globalization.CultureInfo("sv-SE"))
                    </MudText>
                </div>
            </MudTd>
        </RowTemplate>
    </MudTable>
</MudPaper>
```

**WCAG-fördelar:**
- `aria-label` - Beskriver tabellens syfte
- `Breakpoint="Breakpoint.Sm"` - Responsiv tabell som anpassas för mobil
- Trend-indikatorer använder både färg och ikoner
- Tydliga DataLabel för mobil vy

## Kompletta Exempel

### Exempel 1: Dashboard med Flera Grafer

```razor
@page "/dashboard"
@rendermode InteractiveServer

<PageTitle>Dashboard - Privatekonomi</PageTitle>

<div class="chart-container">
    <MudText Typo="Typo.h4" Class="mb-4">Ekonomisk Dashboard</MudText>
    <MudText Typo="Typo.body2" Color="Color.Secondary" Class="mb-4">
        Översikt av din ekonomiska situation
    </MudText>
</div>

<!-- Sammanfattande Cards -->
<div class="chart-summary-cards">
    <div class="chart-summary-card">
        <div class="summary-card-label">Totala Tillgångar</div>
        <div class="summary-card-value">1 234 567 kr</div>
        <div class="summary-card-change trend-indicator positive">
            <MudIcon Icon="@Icons.Material.Filled.TrendingUp" Size="Size.Small" />
            +12.5%
        </div>
    </div>
    
    <div class="chart-summary-card">
        <div class="summary-card-label">Totala Skulder</div>
        <div class="summary-card-value">500 000 kr</div>
        <div class="summary-card-change trend-indicator negative">
            <MudIcon Icon="@Icons.Material.Filled.TrendingDown" Size="Size.Small" />
            -5.2%
        </div>
    </div>
</div>

<!-- Filter -->
<MudPaper Class="pa-4 mb-4 chart-filters" Elevation="2">
    <div class="filter-group">
        <MudText Typo="Typo.subtitle1" Class="filter-label mb-2">Tidsperiod</MudText>
        <MudButtonGroup Color="Color.Primary" Variant="Variant.Outlined" Size="Size.Small">
            <MudButton OnClick="@(() => ChangePeriod(12))" 
                       Variant="@(_selectedMonths == 12 ? Variant.Filled : Variant.Outlined)"
                       aria-label="Visa senaste året"
                       Class="chart-filter-button chart-touch-target">
                1 år
            </MudButton>
            <MudButton OnClick="@(() => ChangePeriod(60))" 
                       Variant="@(_selectedMonths == 60 ? Variant.Filled : Variant.Outlined)"
                       aria-label="Visa senaste 5 åren"
                       Class="chart-filter-button chart-touch-target">
                5 år
            </MudButton>
        </MudButtonGroup>
    </div>
</MudPaper>

<!-- Grafer -->
<MudGrid Spacing="3">
    <!-- Huvudgraf -->
    <MudItem xs="12">
        <MudPaper Class="pa-4 chart-wrapper" Elevation="2">
            <div class="chart-header">
                <MudText Typo="Typo.h6" Class="chart-title">Nettoförmögenhet över tid</MudText>
                <MudText Typo="Typo.body2" Color="Color.Secondary" Class="chart-subtitle">
                    Utveckling av tillgångar minus skulder
                </MudText>
            </div>
            
            @if (netWorthData.Any())
            {
                <div role="img" aria-label="Linjediagram som visar nettoförmögenhet över tid">
                    <MudChart ChartType="ChartType.Line" 
                              ChartSeries="@netWorthSeries" 
                              XAxisLabels="@xLabels"
                              Width="100%" 
                              Height="400px"
                              ChartOptions="@chartOptions"
                              Class="chart-responsive chart-height-lg" />
                </div>
                <p class="chart-sr-only">
                    Grafen visar nettoförmögenhet från @netWorthData.First().Period 
                    till @netWorthData.Last().Period med en total förändring på 
                    @((netWorthData.Last().Value - netWorthData.First().Value).ToString("C0")).
                </p>
            }
            else
            {
                <div class="chart-empty">
                    <MudIcon Icon="@Icons.Material.Filled.ShowChart" Class="chart-empty-icon" />
                    <MudText Class="chart-empty-message">Ingen data tillgänglig</MudText>
                    <MudText Typo="Typo.body2" Class="chart-empty-hint">
                        Data samlas automatiskt varje dag.
                    </MudText>
                </div>
            }
        </MudPaper>
    </MudItem>

    <!-- Side-by-side grafer -->
    <MudItem xs="12" md="6">
        <MudPaper Class="pa-4 chart-wrapper" Elevation="2">
            <div class="chart-header">
                <MudText Typo="Typo.h6" Class="chart-title">Utgiftsfördelning</MudText>
            </div>
            <div role="img" aria-label="Cirkeldiagram för utgiftsfördelning">
                <MudChart ChartType="ChartType.Pie" 
                          InputData="@expenseData"
                          InputLabels="@expenseLabels"
                          Width="100%" 
                          Height="300px"
                          LegendPosition="Position.Bottom"
                          Class="chart-responsive chart-height-md" />
            </div>
        </MudPaper>
    </MudItem>

    <MudItem xs="12" md="6">
        <MudPaper Class="pa-4 chart-wrapper" Elevation="2">
            <div class="chart-header">
                <MudText Typo="Typo.h6" Class="chart-title">Inkomstfördelning</MudText>
            </div>
            <div role="img" aria-label="Cirkeldiagram för inkomstfördelning">
                <MudChart ChartType="ChartType.Pie" 
                          InputData="@incomeData"
                          InputLabels="@incomeLabels"
                          Width="100%" 
                          Height="300px"
                          LegendPosition="Position.Bottom"
                          Class="chart-responsive chart-height-md" />
            </div>
        </MudPaper>
    </MudItem>
</MudGrid>

<!-- Datatabell -->
<MudPaper Class="pa-4 mt-4 chart-data-table" Elevation="2">
    <div class="chart-header">
        <MudText Typo="Typo.h6" Class="chart-title">Detaljerad Historik</MudText>
    </div>
    <MudTable Items="@netWorthData" 
              Dense="true" 
              Hover="true"
              aria-label="Detaljerad historik över nettoförmögenhet"
              Breakpoint="Breakpoint.Sm">
        <HeaderContent>
            <MudTh>Period</MudTh>
            <MudTh Style="text-align: right">Nettoförmögenhet</MudTh>
            <MudTh Style="text-align: right">Förändring</MudTh>
        </HeaderContent>
        <RowTemplate>
            <MudTd DataLabel="Period">@context.Period</MudTd>
            <MudTd DataLabel="Nettoförmögenhet" Style="text-align: right">
                @context.Value.ToString("C0", new System.Globalization.CultureInfo("sv-SE"))
            </MudTd>
            <MudTd DataLabel="Förändring" Style="text-align: right">
                <div class="trend-indicator @(context.Change >= 0 ? "positive" : "negative")">
                    <MudIcon Icon="@(context.Change >= 0 ? Icons.Material.Filled.TrendingUp : Icons.Material.Filled.TrendingDown)" 
                             Size="Size.Small" />
                    <MudText Color="@(context.Change >= 0 ? Color.Success : Color.Error)">
                        @(context.Change >= 0 ? "+" : "")@context.Change.ToString("C0", new System.Globalization.CultureInfo("sv-SE"))
                    </MudText>
                </div>
            </MudTd>
        </RowTemplate>
    </MudTable>
</MudPaper>

@code {
    private int _selectedMonths = 12;
    private List<ChartSeries> netWorthSeries = new();
    private string[] xLabels = Array.Empty<string>();
    private ChartOptions chartOptions = new ChartOptions
    {
        YAxisFormat = "C0",
        InterpolationOption = InterpolationOption.NaturalSpline,
        LineStrokeWidth = 2,
        MaxNumYAxisTicks = 8
    };

    private async Task ChangePeriod(int months)
    {
        _selectedMonths = months;
        await LoadData();
    }
}
```

## Best Practices Sammanfattning

### ✅ Gör Detta
1. **Använd semantiska klasser**: `chart-container`, `chart-wrapper`, `chart-header`
2. **Lägg till ARIA-labels**: `role="img"` och `aria-label` för grafer
3. **Inkludera skärmläsartext**: Använd `chart-sr-only` för detaljerade beskrivningar
4. **Använd trend-indikatorer**: Kombinera färg med ikoner
5. **Tillhandahåll alternativ**: Visa datatabeller tillsammans med grafer
6. **Testa responsivitet**: Använd MudGrid med xs/sm/md/lg breakpoints
7. **Touch-vänliga kontroller**: Använd `chart-touch-target` för minimum 44x44px
8. **Hantera tomma tillstånd**: Visa informativa meddelanden med `chart-empty`
9. **Lägg till laddningstillstånd**: Använd `chart-loading` med spinner
10. **Svenska lokalen**: `new System.Globalization.CultureInfo("sv-SE")`

### ❌ Undvik Detta
1. Använd inte enbart färg för att förmedla information
2. Hårdkoda inte höjder - använd responsiva klasser
3. Glöm inte `aria-label` på interaktiva element
4. Ignorera inte tangentbordsanvändare
5. Skapa inte för små klickområden (<44px)
6. Utelämna inte tomma tillstånd
7. Glöm inte att testa med skärmläsare
8. Använd inte för små fontstorlekar (<12px)

## Verifiering och Testning

### Accessibility Checklist
- [ ] Har alla grafer `role="img"` och `aria-label`?
- [ ] Finns skärmläsarbeskrivningar (`chart-sr-only`)?
- [ ] Använder trend-indikatorer både färg och ikoner?
- [ ] Är alla knappar keyboard-accessible?
- [ ] Har interaktiva element minst 44x44px?
- [ ] Finns alternativa datatabeller?
- [ ] Är fokusindikatorer tydliga?
- [ ] Fungerar allt med 200% zoom?

### Responsive Testing
- [ ] Testa på mobil (xs: <600px)
- [ ] Testa på tablet (sm: 600-959px)
- [ ] Testa på desktop (md: 960px+)
- [ ] Verifiera att filter är lättanvända på touch
- [ ] Kontrollera att grafer skalas korrekt

### Tools
- Chrome DevTools (Device Mode)
- Firefox Developer Tools
- Lighthouse Accessibility Audit
- axe DevTools Extension
- NVDA/VoiceOver Screen Reader

---

**Version**: 1.0  
**Datum**: 2025-10-24  
**Författare**: Privatekonomi Development Team
