# Issue 6: Implementera Mikrointeraktioner och Animationer för Förbättrad Feedback

**Labels:** `design`, `animations`, `ux`, `medium-priority`, `fas-3`

**Prioritet:** ⭐⭐ Medel

**Estimat:** 2-3 dagar

**Fas:** Fas 3 - Polish

---

## Beskrivning

Lägg till subtila animationer och mikrointeraktioner för att ge applikationen en mer polerad och professionell känsla med bättre användarfeedback. Detta förbättrar användarupplevelsen genom att bekräfta åtgärder visuellt och göra applikationen mer engagerande.

## Bakgrund

- Begränsade animationer och övergångar idag
- Statisk känsla på många element
- Saknar tydlig feedback vid interaktion
- Användare kan vara osäkra på om deras åtgärder registrerats
- Moderna applikationer använder mikrointeraktioner för att förbättra UX

## Åtgärder

### Fas 6a: Kort och Komponenter
- [ ] Implementera smooth hover-transition för kort (lift-effekt)
- [ ] Lägg till subtil skugga vid hover (elevation-ändring)
- [ ] Implementera press-down feedback på knappar (scale 0.98)
- [ ] Lägg till ripple-effekt på klick (MudBlazor inbyggd)
- [ ] Implementera smooth transitions på alla interaktiva element

### Fas 6b: Siffror och Data
- [ ] Skapa count-up animation för belopp vid sidladdning
- [ ] Implementera fade-in-up för nya element
- [ ] Lägg till subtle pulse för viktiga värden (t.ex. lågt saldo)
- [ ] Animera progress bars (smooth fill)
- [ ] Implementera number rolling effect för stora värdeändringar

### Fas 6c: Diagram och Visualiseringar
- [ ] Implementera entrance-animationer för diagram (fade-in-up)
- [ ] Lägg till staggered animation för tabellrader
- [ ] Förbättra tooltip-animationer (smooth fade-in)
- [ ] Animera diagramlinjer (draw-in effect)
- [ ] Lägg till hover-effects på diagramsektioner

### Fas 6d: Formulär och Inputs
- [ ] Lägg till focus-ring animation på textfält
- [ ] Implementera label-float animation
- [ ] Lägg till checkmark-animation för checkboxes
- [ ] Implementera smooth switch-toggle animation
- [ ] Lägg till error shake-animation för validering

### Fas 6e: Respektera Tillgänglighet
- [ ] Implementera `prefers-reduced-motion` media query
- [ ] Säkerställ att alla animationer kan inaktiveras
- [ ] Testa med reducerade rörelser aktiverat
- [ ] Använd ARIA live regions för dynamiskt innehåll
- [ ] Säkerställ keyboard-navigation fungerar med animationer

## Teknisk Implementation

### CSS Animations (app.css)

```css
/* ===== KORT OCH KOMPONENTER ===== */

/* Kort hover-effekt med lift */
.mud-card,
.mud-paper {
    transition: transform 0.2s ease, box-shadow 0.2s ease;
}

.mud-card:hover,
.mud-paper:hover {
    transform: translateY(-4px);
    box-shadow: 0 12px 40px rgba(0, 0, 0, 0.12);
}

.mud-theme-dark .mud-card:hover,
.mud-theme-dark .mud-paper:hover {
    box-shadow: 0 12px 40px rgba(0, 0, 0, 0.3);
}

/* Knapp-tryck feedback */
.mud-button {
    transition: transform 0.1s ease;
}

.mud-button:active {
    transform: scale(0.98);
}

.mud-button:not(:disabled):hover {
    transform: translateY(-1px);
}

/* Icon button scale */
.mud-icon-button {
    transition: transform 0.2s ease, background-color 0.2s ease;
}

.mud-icon-button:hover {
    transform: scale(1.1);
}

.mud-icon-button:active {
    transform: scale(0.95);
}

/* ===== SIFFROR OCH DATA ===== */

/* Count-up animation för belopp */
@keyframes countUp {
    from { 
        opacity: 0; 
        transform: translateY(10px); 
    }
    to { 
        opacity: 1; 
        transform: translateY(0); 
    }
}

.amount-animated {
    animation: countUp 0.4s ease-out;
}

/* Fade in up för nya element */
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

.fade-in-up {
    animation: fadeInUp 0.5s ease-out;
}

/* Pulse för viktiga värden */
@keyframes pulse {
    0%, 100% { 
        opacity: 1; 
        transform: scale(1); 
    }
    50% { 
        opacity: 0.8; 
        transform: scale(1.05); 
    }
}

.pulse-warning {
    animation: pulse 2s ease-in-out infinite;
}

/* Progress bar smooth fill */
.mud-progress-linear .mud-progress-linear-bar {
    transition: width 0.8s cubic-bezier(0.4, 0, 0.2, 1);
}

/* ===== DIAGRAM OCH VISUALISERINGAR ===== */

/* Diagram entrance animation */
.chart-enter {
    animation: fadeInUp 0.5s ease-out;
}

/* Staggered animation för tabellrader */
.table-row-enter {
    animation: fadeIn 0.3s ease-out;
    animation-fill-mode: both;
}

@keyframes fadeIn {
    from { opacity: 0; }
    to { opacity: 1; }
}

/* Stagger delay för olika rader */
.table-row-enter:nth-child(1) { animation-delay: 0.05s; }
.table-row-enter:nth-child(2) { animation-delay: 0.1s; }
.table-row-enter:nth-child(3) { animation-delay: 0.15s; }
.table-row-enter:nth-child(4) { animation-delay: 0.2s; }
.table-row-enter:nth-child(5) { animation-delay: 0.25s; }
.table-row-enter:nth-child(6) { animation-delay: 0.3s; }
.table-row-enter:nth-child(7) { animation-delay: 0.35s; }
.table-row-enter:nth-child(8) { animation-delay: 0.4s; }
.table-row-enter:nth-child(9) { animation-delay: 0.45s; }
.table-row-enter:nth-child(10) { animation-delay: 0.5s; }

/* Tooltip smooth fade */
.mud-tooltip {
    animation: tooltipFadeIn 0.2s ease-out;
}

@keyframes tooltipFadeIn {
    from { 
        opacity: 0; 
        transform: scale(0.95); 
    }
    to { 
        opacity: 1; 
        transform: scale(1); 
    }
}

/* ===== FORMULÄR OCH INPUTS ===== */

/* Focus ring animation */
.mud-input-outlined,
.mud-input-filled {
    transition: box-shadow 0.2s ease, border-color 0.2s ease;
}

.mud-input-outlined:focus-within,
.mud-input-filled:focus-within {
    box-shadow: 0 0 0 3px rgba(99, 102, 241, 0.15);
}

/* Checkmark animation */
@keyframes checkmark {
    0% { 
        transform: scale(0) rotate(-45deg); 
    }
    50% { 
        transform: scale(1.2) rotate(-45deg); 
    }
    100% { 
        transform: scale(1) rotate(-45deg); 
    }
}

.mud-checkbox-checked .mud-icon-root {
    animation: checkmark 0.3s ease-out;
}

/* Error shake animation */
@keyframes shake {
    0%, 100% { transform: translateX(0); }
    10%, 30%, 50%, 70%, 90% { transform: translateX(-5px); }
    20%, 40%, 60%, 80% { transform: translateX(5px); }
}

.error-shake {
    animation: shake 0.5s ease-in-out;
}

/* Switch toggle smooth animation */
.mud-switch {
    transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
}

/* ===== NAVIGATION ===== */

/* Nav link slide animation */
.mud-nav-link {
    transition: transform 0.2s ease, background-color 0.2s ease;
}

.mud-nav-link:hover {
    transform: translateX(4px);
}

/* Drawer slide animation */
.mud-drawer {
    transition: transform 0.3s cubic-bezier(0.4, 0, 0.2, 1);
}

/* ===== LOADING OCH SKELETON ===== */

/* Skeleton loading animation */
@keyframes skeleton-loading {
    0% { background-position: 200% 0; }
    100% { background-position: -200% 0; }
}

.skeleton-loading {
    background: linear-gradient(
        90deg, 
        rgba(0, 0, 0, 0.05) 25%, 
        rgba(0, 0, 0, 0.1) 50%, 
        rgba(0, 0, 0, 0.05) 75%
    );
    background-size: 200% 100%;
    animation: skeleton-loading 1.5s ease-in-out infinite;
}

.mud-theme-dark .skeleton-loading {
    background: linear-gradient(
        90deg, 
        rgba(255, 255, 255, 0.05) 25%, 
        rgba(255, 255, 255, 0.1) 50%, 
        rgba(255, 255, 255, 0.05) 75%
    );
}

/* Spinner smooth rotation */
.mud-progress-circular {
    animation: rotate 1.4s linear infinite;
}

@keyframes rotate {
    0% { transform: rotate(0deg); }
    100% { transform: rotate(360deg); }
}

/* ===== TILLGÄNGLIGHET ===== */

/* Respektera prefers-reduced-motion */
@media (prefers-reduced-motion: reduce) {
    *,
    *::before,
    *::after {
        animation-duration: 0.01ms !important;
        animation-iteration-count: 1 !important;
        transition-duration: 0.01ms !important;
        scroll-behavior: auto !important;
    }
}

/* ===== SNACKBAR/TOAST ===== */

/* Snackbar slide-in animation */
@keyframes snackbarSlideIn {
    from {
        transform: translateY(100%);
        opacity: 0;
    }
    to {
        transform: translateY(0);
        opacity: 1;
    }
}

.mud-snackbar {
    animation: snackbarSlideIn 0.3s ease-out;
}

/* ===== DIALOG ===== */

/* Dialog fade and scale */
@keyframes dialogEnter {
    from {
        opacity: 0;
        transform: scale(0.95) translateY(-20px);
    }
    to {
        opacity: 1;
        transform: scale(1) translateY(0);
    }
}

.mud-dialog {
    animation: dialogEnter 0.3s ease-out;
}

/* ===== UTILITY CLASSES ===== */

.transition-fast {
    transition: all 0.15s cubic-bezier(0.4, 0, 0.2, 1);
}

.transition-base {
    transition: all 0.2s cubic-bezier(0.4, 0, 0.2, 1);
}

.transition-slow {
    transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
}
```

### Razor Helper för Number Animation

```csharp
// Components/Shared/AnimatedNumber.razor

<span class="amount-animated" @key="currentValue">
    @FormattedValue
</span>

@code {
    [Parameter] public decimal Value { get; set; }
    [Parameter] public string Format { get; set; } = "C0";
    [Parameter] public CultureInfo? Culture { get; set; }
    
    private decimal currentValue;
    private string FormattedValue => Value.ToString(Format, Culture ?? new CultureInfo("sv-SE"));
    
    protected override void OnParametersSet()
    {
        if (currentValue != Value)
        {
            currentValue = Value;
            StateHasChanged();
        }
    }
}
```

## Berörd Kod

### Filer som ska modifieras
- `src/Privatekonomi.Web/wwwroot/app.css`
  - Lägg till alla animations och transitions
  - Implementera utility-klasser
  - Lägg till prefers-reduced-motion support

- `src/Privatekonomi.Web/Components/Pages/Home.razor`
  - Lägg till `amount-animated` klass på belopp
  - Implementera `fade-in-up` för kort
  - Använd `chart-enter` för diagram

- `src/Privatekonomi.Web/Components/Shared/`
  - Skapa `AnimatedNumber.razor` komponent
  - Uppdatera befintliga komponenter med animations-klasser

### Nya filer att skapa
- `src/Privatekonomi.Web/Components/Shared/AnimatedNumber.razor`
  - Komponent för animerade siffror
  - Count-up effekt vid värdeändring

## Acceptanskriterier

- [ ] Kort lyfts subtilt vid hover (translateY -4px)
- [ ] Kort har ökad box-shadow vid hover
- [ ] Knappar har tydlig feedback vid klick (scale 0.98)
- [ ] Icon buttons skalar upp vid hover (scale 1.1)
- [ ] Belopp animeras in vid sidladdning (count-up animation)
- [ ] Nya element har fade-in-up animation
- [ ] Viktiga värden med varning har pulse-animation
- [ ] Progress bars fylls smidigt (smooth transition)
- [ ] Diagram har smooth entrance-animation (fadeInUp)
- [ ] Tabellrader har staggered fade-in (varje rad med delay)
- [ ] Tooltips har smooth fade-in animation
- [ ] Textfält har focus-ring animation
- [ ] Checkboxes har checkmark-animation
- [ ] Validerings-fel har shake-animation
- [ ] Switch-toggles har smooth animation
- [ ] Nav-länkar har slide-right på hover
- [ ] Snackbars har slide-in animation
- [ ] Dialogs har fade och scale entrance
- [ ] Animationer respekterar `prefers-reduced-motion`
- [ ] Inga animationer orsakar layout shift
- [ ] Animationer fungerar i både light och dark mode
- [ ] Performance är god (60fps på de flesta enheter)

## Testing Checklist

### Manual Testing
- [ ] Testa hover på alla interaktiva element
- [ ] Testa klick-feedback på knappar
- [ ] Verifiera entrance-animationer på olika sidor
- [ ] Kontrollera staggered animations i tabeller
- [ ] Testa focus-animationer på formulär
- [ ] Verifiera error-shake på validering
- [ ] Testa i både light och dark mode

### Accessibility Testing
- [ ] Aktivera "Reduce motion" i OS
- [ ] Verifiera att animationer är minimala eller avstängda
- [ ] Testa keyboard-navigation med animationer
- [ ] Använd screen reader för att verifiera ARIA live regions
- [ ] Kontrollera att inga animationer hindrar interaktion

### Performance Testing
- [ ] Mät FPS under animationer (måste vara ≥ 60fps)
- [ ] Kontrollera CPU-användning
- [ ] Testa på mobila enheter
- [ ] Verifiera att inga memory leaks finns

## Referens

- **Källdokument:** `docs/DESIGN_ANALYSIS_2025.md` sektion "Förslag 6: Mikrointeraktioner"
- **Huvudissue:** `docs/issues/DESIGN_IMPLEMENTATION_SUB_ISSUES.md`
- **Relaterad dokumentation:** `docs/VISUAL_UX_IMPROVEMENTS.md`
- **Material Design Motion:** https://m2.material.io/design/motion/

## Estimerad Tidslinje

1. **Dag 1:** Implementera kort, knapp och komponent-animationer
2. **Dag 2:** Lägg till diagram, tabell och formulär-animationer
3. **Dag 3:** Testa tillgänglighet, performance och finslipa

---

**Senast uppdaterad:** 2025-12-06  
**Version:** 1.0  
**Status:** Redo för implementation
