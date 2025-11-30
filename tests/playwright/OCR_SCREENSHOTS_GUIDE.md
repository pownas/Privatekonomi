# Hur man tar Screenshots - OCR Kvittoskanning

Detta dokument beskriver hur man tar screenshots av den nya OCR-kvittoskanningsfunktionen med Playwright.

## Översikt

Den nya OCR-funktionen lägger till följande komponenter som vi behöver dokumentera med screenshots:
- **OCR-knapp** på Kvitton-sidan
- **OCR-skanningsdialog** med uppladdningsområde
- **Tips och vägledning** för bästa resultat
- **Mobil- och desktopvyer**
- **Ljust och mörkt tema**

## Snabbstart - Automatiska Screenshots med Playwright

### Steg 1: Förbered miljön

```bash
cd /home/runner/work/Privatekonomi/Privatekonomi/tests/playwright
npm install
npx playwright install chromium
```

### Steg 2: Starta applikationen

I ett separat terminalfönster:

```bash
cd /home/runner/work/Privatekonomi/Privatekonomi/src/Privatekonomi.Web
dotnet run
```

Vänta tills applikationen är igång på `http://localhost:5274`

### Steg 3: Kör Playwright-testerna för OCR screenshots

```bash
cd /home/runner/work/Privatekonomi/Privatekonomi/tests/playwright
npx playwright test ocr-receipt-scanning-screenshots.spec.ts
```

Screenshots sparas automatiskt i:
```
tests/playwright/screenshots/ocr-receipt-scanning/
```

## Screenshots som tas automatiskt

Playwright-testerna tar följande screenshots:

### Desktop/Ljust Tema:
1. **01-receipts-page-with-ocr-button.png** - Kvittosida med ny "Skanna kvitto (OCR)" knapp
2. **02-ocr-dialog-initial.png** - OCR-dialog initialt tillstånd med uppladdningsområde
3. **03-ocr-dialog-upload-area.png** - OCR-dialog med fokus på uppladdningsområdet
4. **04-ocr-dialog-tips.png** - OCR-dialog med tips för bästa resultat
5. **05-receipts-page-full.png** - Fullständig kvittosida (full page)

### Mörkt Tema:
6. **06-receipts-page-dark-mode.png** - Kvittosida med OCR-knapp i mörkt tema
7. **07-ocr-dialog-dark-mode.png** - OCR-dialog i mörkt tema

### Mobil:
8. **08-receipts-page-mobile.png** - Kvittosida i mobilvy (iPhone SE)
9. **09-ocr-dialog-mobile.png** - OCR-dialog i mobilvy

### Desktop - Tabell:
10. **10-receipts-table-desktop.png** - Kvittotabell i desktop-läge

## Manuell Metod (om Playwright inte fungerar)

Om Playwright inte fungerar kan du ta screenshots manuellt:

### 1. Förbered testdata

Innan du tar screenshots, se till att ha några kvitton skapade för att visa funktionalitet.

#### Exempelkvitton:
1. **ICA Maxi** - 450 kr - 2024-11-08 - Skannat
2. **Circle K** - 650 kr - 2024-11-07 - E-kvitto
3. **Elgiganten** - 2499 kr - 2024-11-05 - Fysiskt

### 2. Öppna Utvecklarverktyg (F12)

- Gå till Device Toolbar (Ctrl+Shift+M)
- Välj "Responsive" eller en specifik enhet
- Rekommenderad storlek: 1920x1080 för desktop, 375x667 för mobil

### 3. Ta screenshots för varje vy

#### A. Kvittosida med OCR-knapp

1. Navigera till `/receipts` eller `/economy/receipts`
2. Se till att "Skanna kvitto (OCR)" knappen syns tydligt bredvid "Nytt Kvitto"
3. Ta screenshot (helst med några kvitton i listan)
4. Spara som `01-receipts-page-with-ocr-button.png`

**Fokuspunkter:**
- ✅ "Skanna kvitto (OCR)" knappen (outlined, secondary style)
- ✅ "Nytt Kvitto" knappen (filled, primary style)
- ✅ Kvittolista i bakgrunden
- ✅ Tydlig header med "Kvitton"

#### B. OCR-dialog - Initialt tillstånd

1. På kvittosidan, klicka på "Skanna kvitto (OCR)"
2. Dialogen öppnas med uppladdningsområdet
3. Ta screenshot av hela dialogen
4. Spara som `02-ocr-dialog-initial.png`

**Fokuspunkter:**
- ✅ Dialog header: "Skanna kvitto med OCR"
- ✅ Beskrivningstext om funktionen
- ✅ Stort uppladdningsområde med:
  - ☁️ Cloud upload ikon
  - "Ladda upp kvittobild"
  - "eller dra och släpp här"
  - Filformat-chip (JPEG, PNG, GIF, WebP)
- ✅ Tips-sektion längst ner
- ✅ "Avbryt" knapp

#### C. OCR-dialog - Upload area (hover state)

1. Med OCR-dialogen öppen
2. Hovra över uppladdningsområdet (visar interaktivitet)
3. Ta screenshot
4. Spara som `03-ocr-dialog-upload-area.png`

**Fokuspunkter:**
- ✅ Uppladdningsområdet med border highlight
- ✅ Cursor som visar att området är klickbart
- ✅ Tydlig visuell feedback

#### D. OCR-dialog - Tips section

1. Med OCR-dialogen öppen
2. Scrolla om nödvändigt för att visa tips-sektionen tydligt
3. Ta screenshot som inkluderar tipsen
4. Spara som `04-ocr-dialog-tips.png`

**Fokuspunkter:**
- ✅ Info-ikon och "Tips för bästa resultat:"
- ✅ Bullet-lista med tips:
  - Använd god belysning
  - Fotografera rakt uppifrån
  - Se till att hela kvittot syns
  - Undvik skuggor och reflektioner

#### E. Full page - Kvittosida

1. Stäng eventuell öppen dialog
2. På kvittosidan, se till att allt innehåll syns
3. Ta full page screenshot (hela sidan från topp till botten)
4. Spara som `05-receipts-page-full.png`

**Fokuspunkter:**
- ✅ Header med navigation
- ✅ Kvittoöversikt med statistik (antal, total summa, denna månad)
- ✅ Kvittotabell med alla kvitton
- ✅ Footer om synlig

### 4. Ta Dark Mode Screenshots

#### F. Kvittosida - Mörkt tema

1. Byt till mörkt tema (theme toggle knapp)
2. Navigera till kvittosidan om nödvändigt
3. Ta screenshot
4. Spara som `06-receipts-page-dark-mode.png`

**Fokuspunkter:**
- ✅ Mörk bakgrund
- ✅ OCR-knapp med mörkt tema-färger
- ✅ God kontrast och läsbarhet

#### G. OCR-dialog - Mörkt tema

1. Med mörkt tema aktivt
2. Öppna OCR-dialogen
3. Ta screenshot
4. Spara som `07-ocr-dialog-dark-mode.png`

**Fokuspunkter:**
- ✅ Dialog med mörk bakgrund
- ✅ Uppladdningsområde anpassat för mörkt tema
- ✅ Text med god kontrast

### 5. Ta Mobile Screenshots

#### H. Kvittosida - Mobilvy

1. Byt till mobil viewport (375x667 - iPhone SE)
2. Navigera till kvittosidan
3. Ta screenshot
4. Spara som `08-receipts-page-mobile.png`

**Fokuspunkter:**
- ✅ Responsiv layout
- ✅ Knappar stackade eller anpassade för mobil
- ✅ Touch-vänligt gränssnitt

#### I. OCR-dialog - Mobilvy

1. Med mobil viewport
2. Öppna OCR-dialogen
3. Ta screenshot
4. Spara som `09-ocr-dialog-mobile.png`

**Fokuspunkter:**
- ✅ Full width dialog på mobil
- ✅ Uppladdningsområde anpassat för mindre skärm
- ✅ Läsbar text och tydliga knappar

### 6. Desktop - Kvittotabell

#### J. Kvittotabell - Desktop

1. Byt till desktop viewport (1920x1080)
2. På kvittosidan, fokusera på kvittotabellen
3. Ta screenshot
4. Spara som `10-receipts-table-desktop.png`

**Fokuspunkter:**
- ✅ Bred tabell med alla kolumner
- ✅ Kvittotyp chips (Fysiskt, E-kvitto, Skannat)
- ✅ Åtgärder kolumn med ikoner

## Tips för bästa resultat

### Bildkvalitet:
- ✅ PNG-format (inte JPEG)
- ✅ Fullskärm eller minst 1920px bredd för desktop
- ✅ Rensa webbläsarens cache innan
- ✅ Stäng onödiga tabs/fönster

### Innehåll:
- ✅ Använd realistisk testdata
- ✅ Svenska namn och belopp
- ✅ Variera kvittotyper (Fysiskt, E-kvitto, Skannat)
- ✅ Ha några kvitton i listan för kontext

### Konsistens:
- ✅ Ta alla screenshots i samma session
- ✅ Använd samma zoomnivå (100%)
- ✅ Ta både ljusa och mörka versioner
- ✅ Använd samma viewport-storlekar

### Fokusområden:
När du tar screenshots, se till att dessa element syns tydligt:

**Kvittosida:**
- "Skanna kvitto (OCR)" knappen (outlined button)
- "Nytt Kvitto" knappen (filled button)
- Kvittostatistik (antal, summa, denna månad)
- Kvittotabell med olika typer av kvitton

**OCR-dialog:**
- Dialog header och close-knapp
- Beskrivande text om funktionen
- Stort uppladdningsområde med drag-drop support
- Filformat-indikator (chip med tillåtna format)
- Tips-sektion med bullet points
- Avbryt-knapp

## Efter att screenshots är tagna

### 1. Skapa screenshots-katalog i docs

```bash
mkdir -p docs/screenshots/ocr-receipt-scanning
```

### 2. Kopiera screenshots från Playwright

```bash
cp tests/playwright/screenshots/ocr-receipt-scanning/* docs/screenshots/ocr-receipt-scanning/
```

### 3. Eller kopiera manuella screenshots

```bash
# Kopiera dina manuellt tagna screenshots till docs-mappen
cp /path/to/your/screenshots/* docs/screenshots/ocr-receipt-scanning/
```

### 4. Verifiera screenshots

Kontrollera att alla screenshots är:
- ✅ I PNG-format
- ✅ Med rätt filnamn
- ✅ Tillräcklig storlek/upplösning
- ✅ Visar rätt innehåll

### 5. Uppdatera dokumentationen

Om du vill lägga till screenshots i dokumentationen, använd:

```markdown
### OCR Kvittoskanning - Kvittosida

![Kvittosida med OCR-knapp](screenshots/ocr-receipt-scanning/01-receipts-page-with-ocr-button.png)

### OCR-dialog

![OCR-skanningsdialog](screenshots/ocr-receipt-scanning/02-ocr-dialog-initial.png)
```

## Felsökning

### Playwright startar inte
- Kontrollera att port 5274 är ledig
- Starta applikationen manuellt först
- Kontrollera att `localhost:5274` är tillgängligt

### Screenshots är tomma/vita
- Öka timeout-värden i testerna
- Kontrollera att applikationen är fullt laddad
- Verifiera att navigationen fungerar

### OCR-knapp syns inte
- Kontrollera att koden är korrekt byggd och körande
- Verifiera att du är på rätt sida (`/receipts`)
- Testa med `git status` att senaste ändringarna är inkluderade

### Browser installation misslyckas
- Kör: `npx playwright install --force chromium`
- Eller använd manuell metod med egen webbläsare

### Dialogen öppnas inte
- Kontrollera JavaScript-fel i konsolen
- Verifiera att MudBlazor fungerar korrekt
- Försök med manuell klickning

## Kontakt

Om du stöter på problem, kontakta via PR-kommentarer eller GitHub issues.

---

**Status:** Playwright-tester förberedda och redo att köras.
**Nästa steg:** Kör testerna lokalt och ladda upp screenshots till docs-mappen.
