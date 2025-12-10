# Issue 1: Dashboard-omdesign med Trender, Hierarki och Förbättrade Färger

**Labels:** `design`, `dashboard`, `ux`, `high-priority`, `fas-1`

**Prioritet:** ⭐⭐⭐ Hög

**Estimat:** 3-4 dagar

**Fas:** Fas 1 - Snabba Vinster

---

## Beskrivning

Omdesigna Dashboard för att ge användare snabbare insikt om ekonomisk hälsa genom förbättrade trendindikatorer, tydligare visuell hierarki och bättre färgkodning.

## Bakgrund

- Dashboard visar mycket information men kan kännas överväldigande
- Viktiga KPI:er (inkomst, utgift, netto) har liknande visuell vikt
- Saknar trendindikatorer för att visa förändring över tid
- Användare kan ha svårt att snabbt förstå sin ekonomiska situation

## Åtgärder

### Fas 1a: Trendindikatorer
- [ ] Lägg till trend-indikatorer (pilar och procentuell förändring) på sammanfattningskort
- [ ] Implementera beräkning av förändring mot föregående period
- [ ] Färgkoda bakgrund baserat på positiv/negativ trend
- [ ] Visa jämförelsetext (t.ex. "vs förra månaden")

### Fas 1b: Visuell Hierarki
- [ ] Använd större typografi (Typo.h4 → Typo.h3) för huvudsiffror
- [ ] Lägg till fontvikt 600 för belopp
- [ ] Öka gap mellan kort (24px → 32px)
- [ ] Implementera section-avskiljare för tydligare gruppering

### Fas 1c: Färgschema
- [ ] Uppdatera sammanfattningskort med subtil gradient eller skuggning
- [ ] Harmonisera kategori-chips med samma mättnadsnivå
- [ ] Förbättra sidebar-kontrast (vit bakgrund → lätt grå #F8FAFC)

## Teknisk Implementation

### Föreslaget kort-design med trend

```razor
<!-- Föreslaget kort-design med trend -->
<MudPaper Class="summary-card" Elevation="2">
    <MudText Typo="Typo.caption" Class="text-muted">Inkomster</MudText>
    <MudText Typo="Typo.h3" Class="font-weight-bold">821 692 kr</MudText>
    <div class="trend-indicator">
        <MudChip Size="Size.Small" 
                 Color="Color.Success" 
                 Icon="@Icons.Material.Filled.TrendingUp">
            +12% vs förra månaden
        </MudChip>
    </div>
</MudPaper>
```

### CSS-stilar

```css
/* Förbättrad spacing och gradient */
.summary-card {
    padding: var(--spacing-xl);
    border-radius: var(--radius-lg);
    background: linear-gradient(135deg, var(--mud-palette-surface) 0%, rgba(99, 102, 241, 0.05) 100%);
}

.dashboard-grid {
    gap: 32px; /* Ökat från 24px */
}

/* Trend-indikator styling */
.trend-indicator {
    margin-top: var(--spacing-md);
}

.trend-indicator .mud-chip {
    font-weight: 500;
}
```

### Visuell Förbättring - Före och Efter

```
FÖRE:                          EFTER:
┌──────────────────────┐       ┌──────────────────────┐
│ Totala Inkomster    │       │ 💰 INKOMSTER         │
│ 821 692,22 kr       │       │ 821 692 kr    ↑ 12%  │
├──────────────────────┤       │ ────────────────────  │
│ Totala Utgifter     │       │ vs förra månaden      │
│ 515 482,98 kr       │       └──────────────────────┘
└──────────────────────┘       
```

## Berörd Kod

### Filer som ska modifieras
- `src/Privatekonomi.Web/Components/Pages/Home.razor`
  - Uppdatera sammanfattningskort med trendindikatorer
  - Öka typografi för huvudsiffror
  - Lägg till gradient-klasser
  - Öka spacing mellan grid-element

- `src/Privatekonomi.Web/wwwroot/app.css`
  - Lägg till `.summary-card` stilar med gradient
  - Uppdatera `.dashboard-grid` spacing
  - Lägg till `.trend-indicator` stilar

- `src/Privatekonomi.Core/Services/DashboardService.cs` (skapa om den inte finns)
  - Implementera metod för att beräkna trenddata
  - `Task<TrendData> GetTrendDataAsync(string userId, DateTime startDate, DateTime endDate)`
  - Beräkna procentuell förändring mot föregående period
  - Hantera edge cases (ingen tidigare data, division med noll)

### Nya filer att skapa
- `src/Privatekonomi.Core/Models/Reports/TrendData.cs`
  ```csharp
  public class TrendData
  {
      public decimal CurrentValue { get; set; }
      public decimal PreviousValue { get; set; }
      public decimal Change { get; set; }
      public decimal PercentageChange { get; set; }
      public bool IsIncrease { get; set; }
      public string FormattedChange => $"{(IsIncrease ? "+" : "")}{PercentageChange:F1}%";
  }
  ```

## Acceptanskriterier

- [ ] Sammanfattningskort visar trend-indikatorer med procentuell förändring
- [ ] Trend-indikatorer har rätt färgkodning (grön för positiv, röd för negativ)
- [ ] Huvudsiffror är mer framträdande med större typografi (Typo.h3)
- [ ] Korten har subtila gradienter och förbättrad spacing (32px gap)
- [ ] Dashboard känns mindre överväldigande med bättre visuell gruppering
- [ ] Jämförelsetext (t.ex. "vs förra månaden") visas korrekt
- [ ] Beräkningar fungerar korrekt även när tidigare data saknas
- [ ] Dark mode fungerar korrekt med nya stilar
- [ ] Responsiv design fungerar på mobil och desktop

## Referens

- **Källdokument:** `docs/DESIGN_ANALYSIS_2025.md` sektion "Förslag 1: Dashboard-omdesign"
- **Huvudissue:** `docs/issues/DESIGN_IMPLEMENTATION_SUB_ISSUES.md`
- **Relaterad dokumentation:** `docs/VISUAL_UX_IMPROVEMENTS.md`

## Estimerad Tidslinje

1. **Dag 1:** Implementera trendberäkningar i DashboardService
2. **Dag 2:** Uppdatera UI med trendindikatorer och ny typografi
3. **Dag 3:** Förbättra CSS-stilar (gradienter, spacing)
4. **Dag 4:** Testa och finslipa (dark mode, responsiv design, edge cases)

---

**Senast uppdaterad:** 2025-12-06  
**Version:** 1.0  
**Status:** Redo för implementation
