# EditTransactionDialog - Användarguide

## Översikt
EditTransactionDialog är en förbättrad modal dialog för redigering av transaktioner i Privatekonomi. Denna dialog ger användare möjlighet att redigera alla aspekter av en transaktion, inklusive kategoritilldelning med stöd för att dela transaktioner över flera kategorier.

## Funktioner

### 1. Grundläggande Transaktionsfält
- **Beskrivning** (obligatorisk): Beskrivande namn för transaktionen
- **Datum** (obligatoriskt): När transaktionen genomfördes
- **Belopp** (obligatoriskt): Transaktionsbelopp i SEK (minst 0.01 kr)
- **Typ**: Växla mellan Inkomst och Utgift
- **Mottagare/Betalare**: Vem som mottog eller skickade betalningen

### 2. Kategorival
Två lägen finns tillgängliga:

#### En Kategori
- Använd autocomplete-fältet för att söka bland tillgängliga kategorier
- Börja skriva för att filtrera kategorier
- Visar både kategorinamn och överordnad kategori (om sådan finns)
- Färgindikator visas för varje kategori
- Vald kategori visas som en chip som kan tas bort

#### Dela på Flera Kategorier (2-4)
- Möjlighet att dela transaktionen på 2-4 kategorier
- Två delningsmetoder:
  - **Procent**: Dela via procentandelar (måste summera till 100%)
  - **Exakta Belopp**: Dela via specifika belopp (måste summera till transaktionsbelopp)
- Lägg till/ta bort kategorirader dynamiskt
- Realtidsvalidering visar om uppdelningen är korrekt

### 3. Övriga Fält
- **Hushåll**: Koppla transaktionen till ett hushåll (valfritt)
- **Betalningsmetod**: Välj bland:
  - Swish
  - Autogiro
  - E-faktura
  - Banköverföring
  - Kort
  - Kontant
- **Valuta**: Standard SEK, kan ändras vid behov
- **Taggar**: Kommaseparerade taggar för enklare kategorisering
- **Noteringar**: Fritextfält för kommentarer

## Validering

### Realtidsvalidering
Dialogen använder MudBlazor's inbyggda validering för:
- Obligatoriska fält (Beskrivning, Datum, Belopp)
- Minimibelopp (0.01 kr)
- Korrekt datumformat

### Kategorivalidering
För split-kategorier:
- **Procentläge**: Totalen måste vara exakt 100%
- **Beloppläge**: Totalen måste matcha transaktionsbeloppet
- Minst en kategori måste väljas
- Varning visas om valideringen misslyckas

## Tillgänglighet (Accessibility)

### ARIA-Etiketter
Alla fält har beskrivande ARIA-etiketter:
- Input-fält har `aria-label` attribut
- Radio-grupper har beskrivningar
- Knappar har tydliga etiketter

### Tangentbordsnavigering
- **Tab**: Navigera framåt mellan fält
- **Shift+Tab**: Navigera bakåt mellan fält
- **Enter**: I autocomplete-fält - välj valt alternativ
- **Escape**: Stäng autocomplete-dropdown eller avbryt dialog
- **Upp/Ner-pilar**: I autocomplete - navigera bland alternativ
- **Space**: Aktivera knappar och checkboxar

### Skärmläsarstöd
- Alla fält har beskrivande etiketter
- Valideringsfel läses upp
- Hjälptexter ger ytterligare kontext

## Responsiv Design

### Desktop (≥960px)
- Tvåkolumnslayout för vissa fält (Datum/Belopp, IsIncome/Payee)
- Full bredd för kategorival och textfält

### Mobil (<960px)
- Enkolumnslayout för alla fält
- Knappar anpassas för touchinteraktion
- Optimal layout för små skärmar

## Användningsexempel

### Exempel 1: Enkel Redigering
1. Öppna dialogen från transaktionslistan
2. Ändra beskrivning till "ICA Maxi - Veckoköp"
3. Välj kategori "Mat" från autocomplete
4. Lägg till taggar: "mat, veckoköp"
5. Klicka "Spara"

### Exempel 2: Split-Transaktion
1. Öppna dialogen
2. Belopp: 1000 kr
3. Välj "Dela på flera kategorier"
4. Välj "Dela via procent"
5. Kategori 1: Mat (60%)
6. Kategori 2: Hushåll (30%)
7. Kategori 3: Nöje (10%)
8. Verifiera att totalen är 100%
9. Klicka "Spara"

## Felhantering

### Laddningsstatus
- Spara-knappen visar en spinner under sparning
- Knappen inaktiveras för att förhindra dubbelklick
- Användaren får feedback via toasts

### Felmeddelanden
- **Valideringsfel**: Visas direkt under berört fält
- **Split-valideringsfel**: Toast-meddelande visas
- **Serverfel**: Toast med felmeddelande

## Tekniska Detaljer

### Komponenter som Används
- **MudDialog**: Modal container
- **MudForm**: Formulärvalidering
- **MudTextField**: Textinmatning
- **MudNumericField**: Numerisk inmatning
- **MudDatePicker**: Datumväljare
- **MudAutocomplete**: Sökbar dropdown för kategorier
- **MudSelect**: Dropdown för hushåll och betalningsmetod
- **MudSwitch**: Toggle för inkomst/utgift
- **MudRadioGroup**: Kategorival och delningsmetod
- **MudAlert**: Valideringsinformation

### Prestanda
- Kategori-sökning körs lokalt (snabb)
- Lazy loading av kategorier vid initialisering
- Effektiv rendering med Blazor's change detection

## Tips och Tricks

1. **Snabbsökning**: I autocomplete, börja skriva direkt för att filtrera
2. **Tangentbordsnavigering**: Använd Tab för snabbare inmatning
3. **Split-beräkning**: Använd procentläge för enklare uppdelning
4. **Taggar**: Använd konsekventa taggar för bättre sökning senare
5. **Noteringar**: Lägg till kvittonummer eller andra detaljer

## Versionshistorik

### Version 2.0 (2024-10-28)
- ✅ MudAutocomplete för kategorisökning
- ✅ Förbättrad validering med MudForm
- ✅ Nya fält: IsIncome, Payee, PaymentMethod, Currency
- ✅ Omfattande ARIA-etiketter
- ✅ Laddningsstatus under sparning
- ✅ Hierarkisk visning av kategorier (med förälder)

### Version 1.0 (Initial)
- Grundläggande transaktionsredigering
- Split-funktionalitet
- Responsiv design
