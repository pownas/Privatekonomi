# Dark Mode Testing Guide

## Översikt

Detta dokument beskriver testfall för att säkerställa att MudBlazor Dark Mode fungerar korrekt och uppfyller WCAG 2.1 nivå AA krav.

## Testmiljö

### Förutsättningar

- Webbläsare: Chrome, Firefox, Safari, Edge (senaste versioner)
- Screen readers: NVDA (Windows), JAWS (Windows), VoiceOver (macOS)
- Kontrastkontrollverktyg: 
  - [WebAIM Contrast Checker](https://webaim.org/resources/contrastchecker/)
  - Browser DevTools (Accessibility panel)
  - [axe DevTools](https://www.deque.com/axe/devtools/)

## Test Suite 1: Grundläggande Dark Mode Funktionalitet

### Test 1.1: Dark Mode Toggle

**Syfte:** Verifiera att dark mode kan aktiveras och inaktiveras

**Steg:**
1. Öppna applikationen i en webbläsare
2. Klicka på dark mode toggle-knappen (måne/sol-ikon) i header
3. Verifiera att UI:t växlar till mörkt tema
4. Klicka på toggle-knappen igen
5. Verifiera att UI:t växlar tillbaka till ljust tema

**Förväntat resultat:**
- ✅ UI växlar smidigt mellan ljust och mörkt tema
- ✅ Ikonen ändras från måne till sol och vice versa
- ✅ Alla komponenter uppdateras korrekt

### Test 1.2: Theme Persistence

**Syfte:** Verifiera att valt tema sparas mellan sessioner

**Steg:**
1. Aktivera dark mode
2. Ladda om sidan (F5)
3. Verifiera att dark mode fortfarande är aktivt
4. Inaktivera dark mode
5. Ladda om sidan
6. Verifiera att ljust tema fortfarande är aktivt

**Förväntat resultat:**
- ✅ Tema-val persisteras i localStorage
- ✅ Temat återställs korrekt vid sidladdning

### Test 1.3: System Preference Detection

**Syfte:** Verifiera att systempreferens detekteras vid första besöket

**Steg:**
1. Rensa browser localStorage
2. Sätt operativsystemet till mörkt läge
3. Öppna applikationen i en ny privat/incognito-fönster
4. Verifiera att applikationen startar i mörkt läge
5. Byt till ljust läge i operativsystemet
6. Rensa localStorage och öppna applikationen igen
7. Verifiera att applikationen startar i ljust läge

**Förväntat resultat:**
- ✅ Applikationen detekterar systempreferens
- ✅ Systempreferens sparas som användarpreferens

## Test Suite 2: WCAG 2.1 Nivå AA Compliance

### Test 2.1: Color Contrast Ratios

**Syfte:** Verifiera att alla färgkontraster uppfyller WCAG AA-krav (4.5:1 för normal text, 3:1 för stor text och UI-komponenter)

**Testfall:**

#### Dark Mode Contrasts

| Element | Förgrundsf ärg | Bakgrundsfärg | Minimikrav | Status |
|---------|---------------|---------------|------------|--------|
| Primary Text | rgba(255,255,255, 0.87) | #1a1a1f | 4.5:1 | ⬜ |
| Secondary Text | rgba(255,255,255, 0.60) | #1a1a1f | 4.5:1 | ⬜ |
| Primary Button | #776BE7 | #1a1a1f | 3:1 | ⬜ |
| Secondary Button | #FF4081 | #1a1a1f | 3:1 | ⬜ |
| Input Text | rgba(255,255,255, 0.87) | #27272f | 4.5:1 | ⬜ |
| Table Text | rgba(255,255,255, 0.87) | #27272f | 4.5:1 | ⬜ |
| Drawer Text | rgba(255,255,255, 0.87) | #27272f | 4.5:1 | ⬜ |
| AppBar Text | rgba(255,255,255, 0.87) | #27272f | 4.5:1 | ⬜ |

**Steg:**
1. Aktivera dark mode
2. Navigera till varje sida i applikationen
3. Använd contrast checker för att verifiera varje element
4. Dokumentera kontrastförhållanden

**Förväntat resultat:**
- ✅ Alla aktiva text-element har minst 4.5:1 kontrast
- ✅ Alla UI-komponenter har minst 3:1 kontrast
- ✅ Inaktiverade element kan ha lägre kontrast (men bör vara tydligt inaktiverade)

**Verktyg:**
- Chrome DevTools > Lighthouse > Accessibility
- axe DevTools extension
- Manual verification med WebAIM Contrast Checker

### Test 2.2: Focus Indicators

**Syfte:** Verifiera att fokusindikatorer är tydligt synliga (WCAG 2.4.7)

**Steg:**
1. Aktivera dark mode
2. Använd Tab för att navigera genom sidan
3. Verifiera att varje fokuserat element har en tydlig visuell indikator
4. Mät fokusindikatorns kontrast mot bakgrunden

**Element att testa:**
- ⬜ Knappar
- ⬜ Länkar
- ⬜ Formulärfält (input, select, textarea)
- ⬜ Checkboxar och radio buttons
- ⬜ Menyalternativ
- ⬜ Tabs
- ⬜ Dialoger

**Förväntat resultat:**
- ✅ Alla element visar tydlig fokusindikator vid Tab-navigation
- ✅ Fokusindikatorn har minst 3:1 kontrast mot bakgrunden
- ✅ Fokusindikatorn är minst 2px bred
- ✅ Outline-offset gör fokusindikatorn tydlig

### Test 2.3: Keyboard Navigation

**Syfte:** Verifiera att alla funktioner är tillgängliga via tangentbord (WCAG 2.1.1)

**Testfall:**

#### Navigation
- ⬜ Tab-navigering fungerar i rätt ordning
- ⬜ Shift+Tab navigerar bakåt
- ⬜ Enter aktiverar länkar och knappar
- ⬜ Space aktiverar knappar och checkboxar
- ⬜ Escape stänger dialoger och menyer

#### Formulär
- ⬜ Tab flyttar fokus mellan fält
- ⬜ Piltangenter navigerar i dropdown-listor
- ⬜ Enter submitterar formulär
- ⬜ Escape rensar/återställer fält (där relevant)

#### Tabeller
- ⬜ Tab navigerar mellan interaktiva element i tabellen
- ⬜ Piltangenter kan användas för navigation (om implementerat)

#### Dialoger
- ⬜ Fokus fångas inom dialogen
- ⬜ Tab cirkulerar genom element i dialogen
- ⬜ Escape stänger dialogen

**Förväntat resultat:**
- ✅ All funktionalitet är tillgänglig utan mus
- ✅ Tangentbordsnavigering är logisk och förutsägbar
- ✅ Fokus återställs korrekt när dialoger stängs

### Test 2.4: Use of Color

**Syfte:** Verifiera att information inte förmedlas enbart med färg (WCAG 1.4.1)

**Steg:**
1. Aktivera dark mode
2. Granska varje sida för element som använder färg för att förmedla information
3. Verifiera att samma information finns tillgänglig på annat sätt

**Element att testa:**
- ⬜ Kategorier: Har både färgkodning OCH textetikett
- ⬜ Valideringsfeedback: Använder både färg OCH text/ikoner
- ⬜ Status-indikatorer: Använder både färg OCH ikoner/text
- ⬜ Obligatoriska fält: Markerade med asterisk (*) INTE bara med färg
- ⬜ Aktiva/inaktiva tillstånd: Tydligt markerade med mer än bara färg

**Förväntat resultat:**
- ✅ Ingen information förmedlas enbart med färg
- ✅ Ikoner och text kompletterar färgkodning

### Test 2.5: Screen Reader Compatibility

**Syfte:** Verifiera att dark mode fungerar med screen readers

**Steg:**
1. Aktivera NVDA/JAWS (Windows) eller VoiceOver (macOS)
2. Aktivera dark mode
3. Navigera genom applikationen med screen reader
4. Verifiera att all information är tillgänglig

**Element att testa:**
- ⬜ Dark mode toggle-knapp har korrekt aria-label
- ⬜ Alla interaktiva element har tillgängliga namn
- ⬜ Formulär har korrekta labels
- ⬜ Felmeddelanden annonseras korrekt
- ⬜ Dynamiska uppdateringar annonseras (med aria-live där lämpligt)

**Förväntat resultat:**
- ✅ Screen reader annonserar dark mode-status
- ✅ Alla UI-element är korrekt identifierade
- ✅ Navigation är logisk och förutsägbar

### Test 2.6: Language of Page

**Syfte:** Verifiera att språket är korrekt deklarerat (WCAG 3.1.1)

**Steg:**
1. Inspektera HTML med DevTools
2. Verifiera att `<html>` elementet har `lang="sv"`

**Förväntat resultat:**
- ✅ `<html lang="sv">` är korrekt satt
- ✅ Screen readers använder korrekt uttal

## Test Suite 3: Component-Specific Tests

### Test 3.1: All Pages in Dark Mode

**Syfte:** Verifiera att alla sidor renderas korrekt i dark mode

**Sidor att testa:**
- ⬜ Dashboard
- ⬜ Transaktioner
- ⬜ Kategorier
- ⬜ Budget
- ⬜ Sparmål
- ⬜ Investeringar
- ⬜ Lån
- ⬜ Hushåll
- ⬜ Bank Connections
- ⬜ Skatteavdrag
- ⬜ Tillgångar
- ⬜ K4 Report
- ⬜ Inställningar

**För varje sida, verifiera:**
- ⬜ Texten är läsbar
- ⬜ Bakgrundsfärger är konsekventa
- ⬜ Ikoner syns tydligt
- ⬜ Tabeller är läsbara
- ⬜ Formulär är användbara
- ⬜ Knappar är tydliga

### Test 3.2: MudBlazor Components

**Syfte:** Verifiera att alla MudBlazor-komponenter fungerar korrekt i dark mode

**Komponenter att testa:**
- ⬜ MudTable
- ⬜ MudCard
- ⬜ MudTextField
- ⬜ MudSelect
- ⬜ MudButton
- ⬜ MudIconButton
- ⬜ MudDialog
- ⬜ MudDrawer
- ⬜ MudAppBar
- ⬜ MudMenu
- ⬜ MudChip
- ⬜ MudSnackbar
- ⬜ MudAlert

**Förväntat resultat:**
- ✅ Alla komponenter har korrekt dark mode-styling
- ✅ Inga ljusa "blixtrar" eller missade komponenter

### Test 3.3: Forms and Validation

**Syfte:** Verifiera att formulär och validering fungerar i dark mode

**Steg:**
1. Navigera till en sida med formulär (t.ex. "Ny Transaktion")
2. Aktivera dark mode
3. Testa formulärvalidering
4. Verifiera att felmeddelanden är tydliga

**Förväntat resultat:**
- ✅ Obligatoriska fält är tydligt markerade
- ✅ Felmeddelanden har god kontrast
- ✅ Validering-ikoner syns tydligt
- ✅ Formulärfält är tydligt avgränsade

## Test Suite 4: Cross-Browser and Device Testing

### Test 4.1: Cross-Browser Compatibility

**Webbläsare att testa:**
- ⬜ Chrome (senaste)
- ⬜ Firefox (senaste)
- ⬜ Safari (senaste)
- ⬜ Edge (senaste)

**För varje webbläsare:**
- ⬜ Dark mode aktiveras korrekt
- ⬜ Temat persisteras
- ⬜ Alla komponenter renderas korrekt
- ⬜ Färgkontraster är konsekventa

### Test 4.2: Responsive Design

**Enheter att testa:**
- ⬜ Desktop (1920x1080)
- ⬜ Laptop (1366x768)
- ⬜ Tablet (768x1024)
- ⬜ Mobile (375x667)

**För varje enhet:**
- ⬜ Dark mode fungerar
- ⬜ Toggle-knappen är tillgänglig
- ⬜ UI är användbart
- ⬜ Texten är läsbar

## Test Suite 5: Performance and User Experience

### Test 5.1: Theme Switch Performance

**Syfte:** Verifiera att tema-byten är smidiga

**Steg:**
1. Klicka på dark mode toggle
2. Observera övergången
3. Mät med DevTools Performance panel

**Förväntat resultat:**
- ✅ Tema-byte sker inom 300ms
- ✅ Ingen "blixt" av gammalt tema
- ✅ Inga layout shifts

### Test 5.2: Initial Load Performance

**Syfte:** Verifiera att dark mode inte påverkar laddningstider negativt

**Steg:**
1. Rensa cache
2. Ladda sidan med dark mode aktiverat
3. Mät laddningstid med Lighthouse

**Förväntat resultat:**
- ✅ Ingen betydande skillnad från ljust läge
- ✅ Performance score ≥ 90

## Test Results Template

### Test Execution Summary

**Test Date:** YYYY-MM-DD
**Tester:** [Namn]
**Environment:** [Browser, OS, Screen size]

| Test Suite | Test Case | Status | Notes |
|------------|-----------|--------|-------|
| Suite 1 | Test 1.1 | ✅ PASS | |
| Suite 1 | Test 1.2 | ✅ PASS | |
| Suite 1 | Test 1.3 | ⚠️ PARTIAL | System preference detection needs improvement |
| Suite 2 | Test 2.1 | ✅ PASS | All contrasts > 4.5:1 |
| ... | ... | ... | ... |

### Issues Found

| Issue ID | Description | Severity | Status |
|----------|-------------|----------|--------|
| DM-001 | Focus indicator not visible on X component | High | Open |
| DM-002 | Contrast ratio too low on Y element | Medium | Fixed |

### Recommendations

[Lista eventuella förbättringsförslag]

## Automated Testing

För automatiserad testning av dark mode, se exempel i `tests/playwright/`.

### Playwright Test Example

```typescript
test('dark mode toggle works', async ({ page }) => {
  await page.goto('/');
  
  // Click dark mode toggle
  await page.click('[aria-label*="Mörkt läge"]');
  
  // Verify dark mode is active
  await expect(page.locator('.mud-theme-dark')).toBeVisible();
  
  // Reload page
  await page.reload();
  
  // Verify dark mode persists
  await expect(page.locator('.mud-theme-dark')).toBeVisible();
});

test('color contrast in dark mode', async ({ page }) => {
  await page.goto('/');
  await page.click('[aria-label*="Mörkt läge"]');
  
  // Run accessibility scan
  const violations = await page.axe();
  expect(violations).toHaveLength(0);
});
```

## Kontakt

För frågor om testning, kontakta utvecklingsteamet eller öppna ett issue på GitHub.
