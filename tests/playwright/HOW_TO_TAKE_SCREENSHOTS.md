# Hur man tar Screenshots - Kvitto-Transaktion Koppling

Eftersom jag inte kan köra en webbläsare i denna CI-miljö har jag förberett Playwright-tester som du kan köra lokalt för att ta autentiska screenshots.

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

### Steg 3: Kör Playwright-testerna för att ta screenshots

```bash
cd /home/runner/work/Privatekonomi/Privatekonomi/tests/playwright
npx playwright test receipt-transaction-linking-screenshots.spec.ts
```

Screenshots sparas automatiskt i:
```
tests/playwright/screenshots/receipt-transaction-linking/
```

## Förberedelse av Testdata

För att få meningsfulla screenshots, skapa följande testdata:

### Transaktioner (3-4 st):
1. **ICA Maxi Storgatan** - 450 kr - 2024-11-01
2. **Circle K Bensin** - 650 kr - 2024-11-02  
3. **Elgiganten AB** - 2499 kr - 2024-10-28
4. **Hemköp** - 320 kr - 2024-10-30

### Kvitton (5-6 st):
1. **ICA Maxi** - 450 kr - 2024-11-01 - Skannat - (länka till transaktion 1)
2. **ICA Maxi** - 450 kr - 2024-11-01 - Fysiskt - (länka till transaktion 1)
3. **Circle K** - 650 kr - 2024-11-02 - E-kvitto - (lämna olänkad)
4. **Elgiganten** - 2499 kr - 2024-10-28 - Fysiskt - (länka till transaktion 3)
5. **Hemköp** - 320 kr - 2024-10-30 - E-kvitto - (lämna olänkad)

Detta ger en bra mix av:
- Transaktioner med flera kvitton (ICA Maxi = 2 kvitton)
- Transaktioner med ett kvitto (Elgiganten)
- Transaktioner utan kvitton
- Olänkade kvitton (Circle K, Hemköp)

## Screenshots som tas automatiskt

Playwright-testerna tar följande screenshots:

1. **01-transaction-list-with-receipts.png** - Transaktionslista med kvittoindikatorer
2. **02-transaction-details-receipts-section.png** - Transaktionsdetaljer med kvitton
3. **03-receipts-table-with-transaction-column.png** - Kvittotabell med transaktionskolumn
4. **04-transaction-selector-dialog.png** - Smart transaktionsväljare
5. **05-receipt-details-with-transaction-link.png** - Kvittodetaljer med transaktionslänk
6. **06-unlink-confirmation.png** - Avlänkningsbekräftelse
7. **07-receipts-page-overview.png** - Kvittosida översikt (full page)
8. **08-transactions-page-overview.png** - Transaktionssida översikt (full page)
9. **09-receipts-page-dark-mode.png** - Kvittosida i mörkt tema
10. **10-transactions-page-dark-mode.png** - Transaktionssida i mörkt tema

## Manuell Metod (om Playwright inte fungerar)

Om Playwright inte fungerar kan du ta screenshots manuellt:

### 1. Öppna Utvecklarverktyg (F12)
- Gå till Device Toolbar (Ctrl+Shift+M)
- Välj "Responsive" eller en specifik enhet
- Rekommenderad storlek: 1366x768 eller 1920x1080

### 2. Navigera och ta screenshots
För varje vy:
1. Navigera till rätt sida
2. Se till att relevant data visas
3. Ta screenshot (Ctrl+Shift+S i Chrome/Edge)
4. Spara med beskrivande namn

### 3. Viktiga vyer att fånga:

**Transaktionslista:**
- Navigera till `/transactions`
- Se till att transaktioner med kvitton visas med 🧾-ikonen

**Transaktionsdetaljer:**
- Klicka på en transaktion med kvitton
- Scrolla till "Kvitton" sektionen
- Fånga hela dialogen

**Kvittolista:**
- Navigera till `/receipts`
- Se till att både länkade och olänkade kvitton syns
- Fånga tabellen med nya "Transaktion" kolumnen

**Transaktionsväljare:**
- På Kvitton-sidan, klicka på 🔗 (länk) på ett olänkat kvitto
- Dialogenfår öppnas
- Notera föreslagna transaktioner (grön bakgrund)

**Kvittodetaljer:**
- På Kvitton-sidan, klicka på 👁 (visa)
- Se till att transaktionslänken syns (om kopplat)

## Tips för bästa resultat

### Bildkvalitet:
- PNG-format (inte JPEG)
- Fullskärm eller minst 1366px bredd
- Rensa webbläsarens cache innan
- Stäng onödiga tabs/fönster

### Innehåll:
- Använd realistisk testdata
- Svenska namn och belopp
- Varierade datum (sprida över 2-3 veckor)
- Mix av olika kvittotyper

### Konsistens:
- Ta alla screenshots i samma session
- Använd samma zoomnivå (100%)
- Samma tema (ljust ELLER mörkt)
- Helst ta både ljusa och mörka versioner

## Efter att screenshots är tagna

### 1. Kopiera till rätt plats:
```bash
mkdir -p docs/screenshots/receipt-transaction-linking
cp tests/playwright/screenshots/receipt-transaction-linking/* docs/screenshots/receipt-transaction-linking/
```

### 2. Uppdatera dokumentationen:
Lägg till bilderna i README.md eller relevanta dokument med:

```markdown
### Transaktionslista med Kvitton
![Transaktionslista](screenshots/receipt-transaction-linking/01-transaction-list-with-receipts.png)

### Transaktionsdetaljer
![Transaktionsdetaljer](screenshots/receipt-transaction-linking/02-transaction-details-receipts-section.png)
```

### 3. Commit och push:
```bash
git add docs/screenshots/receipt-transaction-linking/
git commit -m "Add screenshots for receipt-transaction linking feature"
git push
```

## Felsökning

### Playwright startar inte applikationen
- Kontrollera att port 5274 är ledig
- Starta applikationen manuellt först
- Uppdatera `playwright.config.ts` om nödvändigt

### Screenshots är tomma/vita
- Öka timeout-värden i testerna
- Kontrollera att testdata finns
- Verifiera att applikationen är fullt laddad

### Browser installation misslyckas
- Kör: `npx playwright install --force chromium`
- Eller använd manuell metod istället

## Kontakt

Om du stöter på problem, kontakta mig via PR-kommentarer.

---

**Status:** Playwright-tester förberedda och klara att köras lokalt. 
Väntar på att köras i en miljö med webbläsare och display.
