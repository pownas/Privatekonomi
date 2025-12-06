# Issue 7: Implementera Visuella Empty States med Illustrationer och Vägledning

**Labels:** `design`, `empty-states`, `ux`, `low-priority`, `fas-3`

**Prioritet:** ⭐ Låg

**Estimat:** 2-3 dagar

**Fas:** Fas 3 - Polish

---

## Beskrivning

Lägg till visuella tomma tillstånd med illustrationer och tydlig vägledning för att förbättra användarupplevelsen när data saknas. Detta hjälper särskilt nya användare att förstå vad de kan göra och hur de kommer igång med applikationen.

## Bakgrund

- Tomma tillstånd kan sakna visuell vägledning
- Saknas illustrationer för att guida användare
- Nya användare kan bli förvirrade utan tydlig nästa-steg-vägledning
- Empty states är en viktig del av onboarding-upplevelsen
- Professionella appar använder illustrationer för att göra empty states mer vänliga

## Åtgärder

### Fas 7a: Illustrationer
- [ ] Skapa/välj illustrationer för olika tomma tillstånd:
  - Inga transaktioner
  - Inga budgetar
  - Inga sparmål
  - Inga investeringar
  - Inga lån
  - Inga kategorier
  - Ingen sökresultat
  - Inget nätverksanslutning (offline)
- [ ] Säkerställ konsekvent stil mellan illustrationer
- [ ] Optimera SVG-filer för snabb laddning
- [ ] Anpassa för både light och dark mode
- [ ] Testa illustrationer på olika skärmstorlekar

### Fas 7b: Vägledande Text
- [ ] Skriv vänliga, informativa rubriker för varje empty state
- [ ] Lägg till förklarande text om vad användaren kan göra
- [ ] Håll texten kort och actionable (max 2 meningar)
- [ ] Översätt all text till svenska
- [ ] Använd konsekvent tone of voice

### Fas 7c: Call-to-Action
- [ ] Lägg till primär CTA-knapp för att påbörja
- [ ] Implementera sekundär länk för hjälp/guide när relevant
- [ ] Konsekvent knappstil över alla empty states
- [ ] Säkerställ touch-targets är tillräckligt stora (minst 44x44px)
- [ ] Testa CTA-konvertering

### Fas 7d: Återanvändbar Komponent
- [ ] Skapa `EmptyState.razor` komponent
- [ ] Parametrisera för olika scenarion
- [ ] Stöd för custom actions och länkar
- [ ] Implementera i alla relevanta vyer
- [ ] Dokumentera användning

## Teknisk Implementation

### EmptyState.razor Komponent

```razor
@* Återanvändbar komponent för tomma tillstånd *@

<MudPaper Class="empty-state @Class" Elevation="0">
    @if (!string.IsNullOrEmpty(ImagePath))
    {
        <img src="@ImagePath" 
             alt="@Title" 
             class="empty-state-image" />
    }
    else if (!string.IsNullOrEmpty(IconName))
    {
        <div class="empty-state-icon">
            <MudIcon Icon="@IconName" Color="@IconColor" Style="font-size: 80px;" />
        </div>
    }
    
    <MudText Typo="Typo.h6" Class="empty-state-title">
        @Title
    </MudText>
    
    @if (!string.IsNullOrEmpty(Description))
    {
        <MudText Typo="Typo.body2" Class="empty-state-description">
            @Description
        </MudText>
    }
    
    @if (!string.IsNullOrEmpty(ActionText))
    {
        <div class="empty-state-actions">
            @if (OnActionClick.HasDelegate)
            {
                <MudButton Variant="Variant.Filled" 
                           Color="Color.Primary" 
                           StartIcon="@ActionIcon"
                           OnClick="OnActionClick"
                           Size="Size.Large">
                    @ActionText
                </MudButton>
            }
            else if (!string.IsNullOrEmpty(ActionHref))
            {
                <MudButton Variant="Variant.Filled" 
                           Color="Color.Primary" 
                           StartIcon="@ActionIcon"
                           Href="@ActionHref"
                           Size="Size.Large">
                    @ActionText
                </MudButton>
            }
            
            @if (!string.IsNullOrEmpty(SecondaryText) && !string.IsNullOrEmpty(SecondaryHref))
            {
                <MudButton Variant="Variant.Text" 
                           Color="Color.Primary"
                           Href="@SecondaryHref"
                           Class="mt-2">
                    @SecondaryText
                </MudButton>
            }
        </div>
    }
    
    @if (!string.IsNullOrEmpty(HelpText))
    {
        <MudText Typo="Typo.caption" Class="empty-state-help">
            @if (!string.IsNullOrEmpty(HelpHref))
            {
                <MudLink Href="@HelpHref">@HelpText</MudLink>
            }
            else
            {
                @HelpText
            }
        </MudText>
    }
</MudPaper>

@code {
    /// <summary>
    /// Path till illustration SVG (om ImagePath används, visas inte IconName)
    /// </summary>
    [Parameter] public string ImagePath { get; set; } = string.Empty;
    
    /// <summary>
    /// Material Icon namn (används om ImagePath är tom)
    /// </summary>
    [Parameter] public string IconName { get; set; } = string.Empty;
    
    /// <summary>
    /// Färg för ikon (om IconName används)
    /// </summary>
    [Parameter] public Color IconColor { get; set; } = Color.Default;
    
    /// <summary>
    /// Huvudrubrik för empty state
    /// </summary>
    [Parameter] public string Title { get; set; } = "Ingen data";
    
    /// <summary>
    /// Beskrivande text (max 2 meningar)
    /// </summary>
    [Parameter] public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Text för primär action-knapp
    /// </summary>
    [Parameter] public string ActionText { get; set; } = string.Empty;
    
    /// <summary>
    /// Ikon för action-knapp
    /// </summary>
    [Parameter] public string ActionIcon { get; set; } = Icons.Material.Filled.Add;
    
    /// <summary>
    /// Href för action-knapp (alternativ till OnActionClick)
    /// </summary>
    [Parameter] public string ActionHref { get; set; } = string.Empty;
    
    /// <summary>
    /// Callback för action-knapp
    /// </summary>
    [Parameter] public EventCallback OnActionClick { get; set; }
    
    /// <summary>
    /// Text för sekundär knapp/länk
    /// </summary>
    [Parameter] public string SecondaryText { get; set; } = string.Empty;
    
    /// <summary>
    /// Href för sekundär länk
    /// </summary>
    [Parameter] public string SecondaryHref { get; set; } = string.Empty;
    
    /// <summary>
    /// Hjälptext som visas längst ner
    /// </summary>
    [Parameter] public string HelpText { get; set; } = string.Empty;
    
    /// <summary>
    /// Href för hjälplänk
    /// </summary>
    [Parameter] public string HelpHref { get; set; } = string.Empty;
    
    /// <summary>
    /// Extra CSS-klass
    /// </summary>
    [Parameter] public string Class { get; set; } = string.Empty;
}
```

### CSS-stilar

```css
/* Empty state container */
.empty-state {
    text-align: center;
    padding: var(--spacing-2xl) var(--spacing-xl);
    background: var(--mud-palette-background-gray);
    border-radius: var(--radius-lg);
    max-width: 500px;
    margin: 0 auto;
}

/* Empty state image */
.empty-state-image {
    max-width: 280px;
    width: 100%;
    height: auto;
    margin-bottom: var(--spacing-lg);
    opacity: 0.85;
    filter: grayscale(0%);
}

/* Empty state icon (fallback om ingen bild) */
.empty-state-icon {
    margin-bottom: var(--spacing-lg);
    opacity: 0.6;
}

/* Title */
.empty-state-title {
    margin-bottom: var(--spacing-sm);
    font-weight: 600;
    color: var(--mud-palette-text-primary);
}

/* Description */
.empty-state-description {
    color: var(--mud-palette-text-secondary);
    margin-bottom: var(--spacing-lg);
    line-height: 1.6;
    max-width: 400px;
    margin-left: auto;
    margin-right: auto;
}

/* Actions */
.empty-state-actions {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: var(--spacing-sm);
    margin-bottom: var(--spacing-md);
}

/* Help text */
.empty-state-help {
    color: var(--mud-palette-text-secondary);
    margin-top: var(--spacing-md);
}

/* Dark mode adjustments */
.mud-theme-dark .empty-state {
    background: rgba(255, 255, 255, 0.02);
}

.mud-theme-dark .empty-state-image {
    opacity: 0.75;
    filter: brightness(0.9);
}

.mud-theme-dark .empty-state-icon {
    opacity: 0.5;
}

/* Animation */
@keyframes emptyStateFadeIn {
    from {
        opacity: 0;
        transform: translateY(10px);
    }
    to {
        opacity: 1;
        transform: translateY(0);
    }
}

.empty-state {
    animation: emptyStateFadeIn 0.5s ease-out;
}

/* Responsiv design */
@media (max-width: 600px) {
    .empty-state {
        padding: var(--spacing-xl) var(--spacing-md);
    }
    
    .empty-state-image {
        max-width: 200px;
    }
}

/* Respektera prefers-reduced-motion */
@media (prefers-reduced-motion: reduce) {
    .empty-state {
        animation: none;
    }
}
```

### Användningsexempel

#### Transactions.razor
```razor
@if (!transactions.Any())
{
    <EmptyState 
        ImagePath="/images/empty-states/no-transactions.svg"
        Title="Inga transaktioner än"
        Description="Börja med att importera transaktioner från din bank, eller lägg till din första transaktion manuellt."
        ActionText="Lägg till transaktion"
        ActionIcon="@Icons.Material.Filled.Add"
        OnActionClick="OpenAddTransactionDialog"
        SecondaryText="Importera från CSV"
        SecondaryHref="/import"
        HelpText="Lär dig mer om att importera transaktioner"
        HelpHref="/docs/csv-import" />
}
```

#### Budgets.razor
```razor
@if (!budgets.Any())
{
    <EmptyState 
        ImagePath="/images/empty-states/no-budgets.svg"
        Title="Inga budgetar skapade"
        Description="Skapa din första budget för att börja planera din ekonomi och få kontroll över dina utgifter."
        ActionText="Skapa budget"
        ActionIcon="@Icons.Material.Filled.AccountBalance"
        ActionHref="/budgetar/ny"
        HelpText="Vad är en budget?"
        HelpHref="/docs/budget-guide" />
}
```

#### Goals.razor
```razor
@if (!goals.Any())
{
    <EmptyState 
        ImagePath="/images/empty-states/no-goals.svg"
        Title="Inga sparmål än"
        Description="Sätt upp ditt första sparmål och följ din progress mot drömmen, oavsett om det är en resa, ny bil eller buffert."
        ActionText="Skapa sparmål"
        ActionIcon="@Icons.Material.Filled.Savings"
        OnActionClick="CreateGoal"
        SecondaryText="Se exempel på sparmål"
        SecondaryHref="/docs/goals-examples" />
}
```

#### SearchResults.razor (ingen söktreffsresultat)
```razor
@if (searchResults.Count == 0 && !string.IsNullOrEmpty(searchQuery))
{
    <EmptyState 
        IconName="@Icons.Material.Filled.SearchOff"
        IconColor="Color.Default"
        Title="Inga resultat"
        Description="@($"Din sökning efter '{searchQuery}' gav inga träffar. Försök med andra sökord.")"
        ActionText="Rensa sökning"
        OnActionClick="ClearSearch" />
}
```

#### Offline.razor (ingen internetanslutning)
```razor
@if (!isOnline)
{
    <EmptyState 
        IconName="@Icons.Material.Filled.CloudOff"
        IconColor="Color.Warning"
        Title="Ingen internetanslutning"
        Description="Kontrollera din internetanslutning och försök igen. Vissa funktioner kanske inte är tillgängliga offline."
        ActionText="Försök igen"
        OnActionClick="RetryConnection"
        HelpText="Vissa data kan vara tillgängliga offline" />
}
```

## Illustration Specifikationer

### Stilguide för Illustrationer
- **Stil:** Flat design med subtila skuggor
- **Färgschema:** Primärfärg (#6366F1), sekundärfärg (#EC4899), neutral grå
- **Format:** SVG för skalbarhet
- **Storlek:** 280x210px (4:3 aspect ratio)
- **Filstorlek:** Max 50KB per illustration
- **Bakgrund:** Transparent

### Lista över Illustrationer

| Illustration | Beskrivning | Rekommenderad Källa |
|--------------|-------------|---------------------|
| no-transactions.svg | Person med tom lista/dokument | Undraw.co: "Empty" |
| no-budgets.svg | Person med kalkylator/planering | Undraw.co: "Financial data" |
| no-goals.svg | Person med sparbössa eller mål | Undraw.co: "savings" |
| no-investments.svg | Person med graf | Undraw.co: "Investment data" |
| no-loans.svg | Tom plånbok | Undraw.co: "Wallet" |
| no-categories.svg | Person med etiketter/taggar | Undraw.co: "Add notes" |
| no-search-results.svg | Person med förstoringsglas | Undraw.co: "Not found" |
| offline.svg | Person med brutit moln | Undraw.co: "Server down" |

### Illustrationskällor
1. **Undraw.co** (Gratis, anpassningsbara)
   - https://undraw.co/illustrations
   - Kan anpassa färger till #6366F1

2. **Storyset.com** (Gratis med attribution)
   - https://storyset.com/
   - Många varianter och stilar

3. **DrawKit.com** (Gratis och premium)
   - https://www.drawkit.com/
   - Högkvalitativa illustrationer

## Berörd Kod

### Nya filer att skapa
- `src/Privatekonomi.Web/Components/Shared/EmptyState.razor`
  - Återanvändbar komponent för alla empty states
  - Stöd för både illustrationer och ikoner
  - Parametriserad för olika användningsfall

- `src/Privatekonomi.Web/wwwroot/images/empty-states/`
  - Alla illustration SVG-filer
  - Optimerade för både light och dark mode

- `src/Privatekonomi.Web/wwwroot/app.css`
  - Lägg till `.empty-state` stilar
  - Responsiv design
  - Dark mode support

### Filer som ska modifieras
- `src/Privatekonomi.Web/Components/Pages/Transactions.razor`
  - Ersätt tom lista med `EmptyState`
  
- `src/Privatekonomi.Web/Components/Pages/Budgets.razor`
  - Lägg till `EmptyState` för inga budgetar

- `src/Privatekonomi.Web/Components/Pages/Goals.razor`
  - Lägg till `EmptyState` för inga sparmål

- `src/Privatekonomi.Web/Components/Pages/Investments.razor`
  - Lägg till `EmptyState` för inga investeringar

- `src/Privatekonomi.Web/Components/Pages/Loans.razor`
  - Lägg till `EmptyState` för inga lån

- `src/Privatekonomi.Web/Components/Pages/Categories.razor`
  - Lägg till `EmptyState` för inga kategorier

## Acceptanskriterier

- [ ] Alla vyer med möjliga tomma tillstånd har visuella illustrationer eller ikoner
- [ ] Vägledande text är tydlig och på svenska
- [ ] Text är kort (max 2 meningar) och actionable
- [ ] CTA-knappar leder till rätt åtgärder
- [ ] Sekundära länkar visas när relevant
- [ ] Hjälplänkar leder till dokumentation
- [ ] `EmptyState`-komponenten är återanvändbar
- [ ] Fungerar i både light och dark mode
- [ ] Illustrationer är optimerade (max 50KB per fil)
- [ ] Illustrationer har konsekvent stil
- [ ] Responsiv design fungerar på mobil och desktop
- [ ] Touch-targets är minst 44x44px
- [ ] Entrance-animation fungerar smidigt
- [ ] Animation respekterar `prefers-reduced-motion`
- [ ] Komponenten är väldokumenterad med exempel

## Testing Checklist

### Visual Testing
- [ ] Testa empty states i alla relevanta vyer
- [ ] Verifiera illustrationer i light mode
- [ ] Verifiera illustrationer i dark mode
- [ ] Kontrollera responsiv design på mobil
- [ ] Testa med olika skärmstorlekar

### Functional Testing
- [ ] Testa alla CTA-knappar fungerar
- [ ] Verifiera navigering från sekundära länkar
- [ ] Testa hjälplänkar leder till rätt sida
- [ ] Kontrollera callback-funktioner

### Accessibility Testing
- [ ] Verifiera alt-text för illustrationer
- [ ] Testa keyboard-navigation
- [ ] Kontrollera kontrast för text
- [ ] Testa med screen reader
- [ ] Verifiera touch-target storlekar

## Referens

- **Källdokument:** `docs/DESIGN_ANALYSIS_2025.md` sektion "Förslag 7: Empty States och Feedback"
- **Huvudissue:** `docs/issues/DESIGN_IMPLEMENTATION_SUB_ISSUES.md`
- **Relaterad dokumentation:** `docs/VISUAL_UX_IMPROVEMENTS.md`
- **Empty State Best Practices:** https://www.nngroup.com/articles/empty-state-interface-design/

## Estimerad Tidslinje

1. **Dag 1:** Hitta/skapa illustrationer, skapa `EmptyState.razor` komponent
2. **Dag 2:** Implementera empty states i alla relevanta vyer
3. **Dag 3:** Testa, optimera och finslipa (responsiv design, dark mode, accessibility)

---

**Senast uppdaterad:** 2025-12-06  
**Version:** 1.0  
**Status:** Redo för implementation
