# Issue 2: Förbättra Sidnavigation med Tydligare Aktiv Markering och Gruppering

**Labels:** `design`, `navigation`, `ux`, `high-priority`, `fas-1`

**Prioritet:** ⭐⭐⭐ Hög

**Estimat:** 1-2 dagar

**Fas:** Fas 1 - Snabba Vinster

---

## Beskrivning

Förbättra sidnavigeringen med tydligare visuell markering av aktiv sida, bättre gruppering av menypunkter och förbättrad typografisk hierarki för enklare navigering.

## Bakgrund

- Sidmenyn har många element som kan vara svåra att överblicka
- Aktiv sida markeras subtilt och kan vara svår att identifiera
- Alla menyobjekt har samma visuella vikt
- Saknar tydlig gruppering av relaterade funktioner
- Användare kan ha svårt att hitta specifika funktioner snabbt

## Åtgärder

### Fas 2a: Aktiv Markering
- [ ] Implementera tydligare färgad highlight med vänsterkant (3px solid #6366F1)
- [ ] Lägg till gradient-bakgrund för aktiv länk
- [ ] Öka fontvikt till 600 för aktiv länk
- [ ] Säkerställ tydlig kontrast i både light och dark mode

### Fas 2b: Gruppering och Hierarki
- [ ] Lägg till subtila section-headers för grupperade funktioner
- [ ] Implementera uppercase text med letter-spacing för sektionsrubriker
- [ ] Öka spacing mellan ikoner (24px → 32px)
- [ ] Gruppera relaterade menypunkter logiskt

### Fas 2c: Visuell Feedback
- [ ] Förbättra hover-effekter på menylänkar
- [ ] Lägg till slide-right animation vid hover
- [ ] Konsekvent 32px spacing för lättare klickning/tapping
- [ ] Implementera smooth transitions

## Teknisk Implementation

### CSS-stilar

```css
/* Ny aktiv-markering */
.nav-item-active {
    background: linear-gradient(90deg, 
        rgba(99, 102, 241, 0.15) 0%, 
        transparent 100%);
    border-left: 3px solid #6366F1;
    font-weight: 600;
    padding-left: 13px; /* Kompensera för border */
}

/* Dark mode support */
.mud-theme-dark .nav-item-active {
    background: linear-gradient(90deg, 
        rgba(129, 140, 248, 0.2) 0%, 
        transparent 100%);
    border-left: 3px solid #818CF8;
}

/* Grupperade sektioner med subtila headers */
.nav-section-header {
    font-size: 0.75rem;
    text-transform: uppercase;
    letter-spacing: 0.1em;
    color: #9CA3AF;
    padding: 16px 16px 8px;
    margin-top: 16px;
    font-weight: 600;
}

.mud-theme-dark .nav-section-header {
    color: #6B7280;
}

/* Förbättrad hover */
.mud-nav-link {
    transition: transform var(--transition-fast), background-color var(--transition-fast);
    padding: 12px 16px;
}

.mud-nav-link:hover {
    transform: translateX(4px);
    background-color: rgba(99, 102, 241, 0.08);
}

.mud-theme-dark .mud-nav-link:hover {
    background-color: rgba(129, 140, 248, 0.12);
}

/* Ikoner med bättre spacing */
.mud-nav-link .mud-icon-root {
    margin-right: 16px;
    font-size: 24px;
}
```

### Razor-komponent

```razor
<!-- Exempel på grupperad navigation i NavMenu.razor -->
<MudNavMenu>
    <div class="nav-section-header">ÖVERSIKT</div>
    <MudNavLink Href="/" 
                Icon="@Icons.Material.Filled.Dashboard"
                Match="NavLinkMatch.All">
        Dashboard
    </MudNavLink>
    
    <div class="nav-section-header">EKONOMI</div>
    <MudNavLink Href="/transaktioner" 
                Icon="@Icons.Material.Filled.Receipt">
        Transaktioner
    </MudNavLink>
    <MudNavLink Href="/budgetar" 
                Icon="@Icons.Material.Filled.AccountBalance">
        Budgetar
    </MudNavLink>
    <MudNavLink Href="/kategorier" 
                Icon="@Icons.Material.Filled.Label">
        Kategorier
    </MudNavLink>
    
    <div class="nav-section-header">PLANERING</div>
    <MudNavLink Href="/sparmal" 
                Icon="@Icons.Material.Filled.Savings">
        Sparmål
    </MudNavLink>
    <MudNavLink Href="/investeringar" 
                Icon="@Icons.Material.Filled.TrendingUp">
        Investeringar
    </MudNavLink>
    <MudNavLink Href="/lan" 
                Icon="@Icons.Material.Filled.AccountBalanceWallet">
        Lån
    </MudNavLink>
    
    <div class="nav-section-header">INSTÄLLNINGAR</div>
    <MudNavLink Href="/konto" 
                Icon="@Icons.Material.Filled.Person">
        Mitt Konto
    </MudNavLink>
    <MudNavLink Href="/installningar" 
                Icon="@Icons.Material.Filled.Settings">
        Inställningar
    </MudNavLink>
</MudNavMenu>
```

## Berörd Kod

### Filer som ska modifieras

- `src/Privatekonomi.Web/Components/Layout/NavMenu.razor`
  - Lägg till section-headers för gruppering
  - Uppdatera NavLink-struktur
  - Applicera nya CSS-klasser

- `src/Privatekonomi.Web/Components/Layout/MainLayout.razor`
  - Säkerställ korrekt drawer-konfiguration
  - Verifiera att aktiv markering fungerar

- `src/Privatekonomi.Web/wwwroot/app.css`
  - Lägg till `.nav-item-active` stilar
  - Implementera `.nav-section-header` stilar
  - Förbättra `.mud-nav-link` hover-effekter
  - Lägg till dark mode support

## Acceptanskriterier

- [ ] Aktiv sida har tydlig visuell markering med färgad vänsterkant (3px)
- [ ] Gradient-bakgrund visas för aktiv länk
- [ ] Aktiv länk har fontvikt 600 (semibold)
- [ ] Menyn har logiska grupperingar med tydliga sektionsrubriker
- [ ] Sektionsrubriker är uppercase med letter-spacing
- [ ] Huvudkategorier har större/tydligare text än undermenyer
- [ ] Hover-effekter ger tydlig visuell feedback (slide-right animation)
- [ ] Spacing är konsekvent (32px mellan ikoner)
- [ ] Dark mode fungerar korrekt med anpassade färger
- [ ] Touch-targets är tillräckligt stora för mobil användning (minst 44x44px)
- [ ] Transitions är smidiga och respekterar `prefers-reduced-motion`

## Exempel på Gruppering

### Före (utan gruppering)
```
☰ Dashboard
☰ Transaktioner
☰ Budgetar
☰ Kategorier
☰ Sparmål
☰ Investeringar
☰ Lån
☰ Mitt Konto
☰ Inställningar
```

### Efter (med gruppering)
```
ÖVERSIKT
☰ Dashboard

EKONOMI
☰ Transaktioner
☰ Budgetar
☰ Kategorier

PLANERING
☰ Sparmål
☰ Investeringar
☰ Lån

INSTÄLLNINGAR
☰ Mitt Konto
☰ Inställningar
```

## Referens

- **Källdokument:** `docs/DESIGN_ANALYSIS_2025.md` sektion "Förslag 2: Förbättrad Sidnavigation"
- **Huvudissue:** `docs/issues/DESIGN_IMPLEMENTATION_SUB_ISSUES.md`
- **Relaterad dokumentation:** `docs/VISUAL_UX_IMPROVEMENTS.md`

## Estimerad Tidslinje

1. **Dag 1:** Implementera CSS-stilar och aktiv markering, lägg till section-headers
2. **Dag 2:** Testa och finslipa (dark mode, hover-effekter, responsiv design)

---

**Senast uppdaterad:** 2025-12-06  
**Version:** 1.0  
**Status:** Redo för implementation
