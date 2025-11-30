# Kontohantering - Skärmbilder och UI-beskrivning

Denna fil beskriver de nya UI-komponenterna som har implementerats för kontohantering.

## 1. Kontoöversikt (Accounts.razor)

### Huvudsida
Sidan nås via **Inställningar → Konton** i navigationsmenyn.

**Överst på sidan:**
- Rubrik: "Mina Konton" med ikon (AccountBalance)
- Beskrivande text om funktionaliteten
- Knapp "Lägg till konto" (blå, primär färg)

**Kontolistor grupperade per typ:**

Varje kontotyp visas i en egen sektion med tabellformat:

### Lönekonton (Checking)
- Ikon: 🏦 AccountBalance
- Kolumner: Namn, Kontonummer, Institution, Valuta, Saldo, Kontoplan, Status, Åtgärder
- Exempel: "Swedbank Lönekonto | 8327-123456789 | Swedbank | SEK | 25 432 kr | 1930 | Aktiv"

### Sparkonton (Savings)
- Ikon: 💰 Savings
- Visar samma struktur som lönekonton

### Kreditkort (Credit Card)
- Ikon: 💳 CreditCard
- Visar negativa saldon (skulder) i rött

### Investeringskonton (Investment)
- Ikon: 📈 TrendingUp
- För ISK, AF, kapitalförsäkringar

### Lån (Loan)
- Ikon: 💵 CreditScore
- För bolån, billån, studielån

### Pensionskonton (Pension)
- Ikon: 👴 Elderly
- För tjänstepension, privat pensionssparande

### Kontanter (Cash)
- Ikon: 💵 AttachMoney
- För kontanter i hushållet

**Åtgärdsknappar:**
- Redigera (penna-ikon) - öppnar EditAccountDialog
- Ta bort (papperskorg-ikon) - visar bekräftelsedialog

**Informationsruta längst ner:**
- Förklaring av olika kontotyper
- Tips om kontoplan (BAS-koder)
- Färgkodad info-alert (blå)

---

## 2. Lägg till konto (AddAccountDialog.razor)

**Dialog som öppnas vid "Lägg till konto":**

### Layout
- Modal dialog med "Lägg till konto" som rubrik
- Formulär uppdelat i grid-layout (2 kolumner på större skärmar)

### Fält (från topp till botten):

**Rad 1:**
- **Kontonamn*** (textfält, obligatorisk)
  - Placeholder: "T.ex. Mitt lönekonto, Nordea sparkonto"

**Rad 2:**
- **Kontotyp*** (dropdown, obligatorisk)
  - Varje alternativ har en ikon
  - Alternativ: Lönekonto, Sparkonto, Kreditkort, Investeringskonto, Lån, Pension, Kontanter
- **Bank/Institution** (textfält)
  - Placeholder: "T.ex. Swedbank, Nordea, SEB"

**Rad 3:**
- **Clearingnummer** (textfält)
  - Placeholder: "T.ex. 8327"
  - Helper text: "För svenska bankkonton"
- **Kontonummer** (textfält)
  - Placeholder: "T.ex. 123456789"

**Rad 4:**
- **Valuta*** (textfält, obligatorisk, förifylld med "SEK")
- **Kontoplan (BAS)** (textfält)
  - Placeholder: "T.ex. 1930, 1510"
  - Helper text: "Koppla till BAS-kontoplan för redovisning"

**Rad 5:**
- **Ingående saldo** (numeriskt fält, standard 0)
  - Formaterat med kr-symbol
  - Helper text: "Saldo när kontot öppnades"
- **Öppningsdatum** (datumväljare)
  - Helper text: "När kontot öppnades (valfritt)"

**Rad 6:**
- **Färg (hex)** (textfält)
  - Placeholder: "#1976D2"
  - Helper text: "Färgkod i hex-format för att identifiera kontot i grafer"

**Knappar längst ner:**
- "Avbryt" (till vänster, neutral färg)
- "Lägg till" (till höger, blå primär färg, disabled om formuläret inte är giltigt)

---

## 3. Redigera konto (EditAccountDialog.razor)

**Liknande layout som AddAccountDialog, men med:**

### Skillnader:
- Rubrik: "Redigera konto"
- Alla fält är förifyllda med befintliga värden
- Ytterligare fält:
  - **Stängningsdatum** (datumväljare)
    - Helper text: "Lämna tomt om kontot är aktivt"
- Visar aktuellt saldo längst ner:
  - "Aktuellt saldo: 25 432 kr (beräknat från ingående saldo och transaktioner)"
  - Text i sekundär färg

**Knappar längst ner:**
- "Avbryt" (till vänster)
- "Spara" (till höger, blå primär färg, disabled om formuläret inte är giltigt)

---

## 4. Förbättrad Balansräkning (BalanceSheet.razor)

**Förändring i "Bankkonton"-sektionen:**

**Tidigare:**
```
Swedbank Lönekonto    25 432 kr
```

**Nu:**
```
Swedbank Lönekonto (8327-123456789)
  Swedbank
25 432 kr
```

**Detaljer:**
- Kontonamn i fetstil
- Kontonummer i parentes (grå text)
- Institution på ny rad under namnet (grå, mindre text)
- Saldo till höger

---

## 5. Navigeringsmeny (NavMenu.razor)

**Ändring:**
- Under "Inställningar"-gruppen
- Ny länk tillagd överst: "Konton" med ikon (AccountBalance)
- Placerad före "Påminnelser"

---

## Färgschema och Design

**Färger:**
- Primär färg (knappar): Blå (#1976D2)
- Success (aktivt, positivt saldo): Grön
- Error (skuld, negativt saldo): Röd
- Info (informationsrutor): Ljusblå
- Warning: Orange

**Typografi:**
- Rubriker: h4 (sidsrubrik), h6 (sektionsrubriker)
- Brödtext: body1 och body2
- Sekundär text (kontonummer, institution): body2 i grå färg

**Layout:**
- Responsiv design med MudBlazor Grid
- 1 kolumn på mobil, 2 kolumner på desktop för formulärfält
- Tabeller med hover-effekt och randig bakgrund
- Rymliga marginaler (pa-4, mb-4)

**Ikoner:**
- Material Design ikoner från MudBlazor
- Storleken "Small" för ikoner i tabeller
- Default storlek för navigering och knappar

---

## Användargränssnittsflöde

### Scenario 1: Lägga till ett nytt lönekonto
1. Användare klickar på "Inställningar" → "Konton"
2. Klickar på "Lägg till konto"
3. Fyller i:
   - Namn: "Swedbank Lönekonto"
   - Kontotyp: "Lönekonto"
   - Institution: "Swedbank"
   - Clearingnummer: "8327"
   - Kontonummer: "123456789"
   - Valuta: "SEK"
   - Kontoplan: "1930"
   - Ingående saldo: 15000
4. Klickar "Lägg till"
5. Återgår till översiktssidan där det nya kontot visas under "Lönekonton"

### Scenario 2: Redigera ett befintligt konto
1. Användare navigerar till Konton-sidan
2. Klickar på redigera-ikonen för ett konto
3. Ändrar t.ex. namnet eller lägger till kontonummer
4. Klickar "Spara"
5. Återgår till översiktssidan med uppdaterad information

### Scenario 3: Stänga ett konto
1. Användare navigerar till Konton-sidan
2. Klickar på redigera-ikonen
3. Anger ett stängningsdatum
4. Klickar "Spara"
5. Kontot visas nu med status "Stängd" (grå chip)

---

## Accessibility (Tillgänglighet)

Alla formulärfält har:
- `aria-label` attribut för skärmläsare
- Beskrivande placeholder-texter
- Helper text för att förklara fältens syfte
- Tydliga felmeddelanden för obligatoriska fält

Knappar har:
- Beskrivande text eller title-attribut
- Tillräcklig kontrast
- Fokushantering enligt WCAG 2.1

---

## Responsivitet

**Desktop (md och större):**
- Formulär i 2 kolumner
- Tabell visar alla kolumner
- Navigation sidopanel

**Tablet:**
- Formulär övergår till 1 kolumn för vissa fält
- Tabell optimerad för mindre skärmar

**Mobil (xs):**
- Alla formulärfält i 1 kolumn
- Tabell med DataLabel för mobilvänlig visning
- Navigation som hamburgermeny

---

## Teknisk implementation

**Komponenter:**
- MudPaper för kortlayout
- MudTable för tabeller
- MudDialog för modaler
- MudForm för formulärvalidering
- MudTextField, MudSelect, MudDatePicker för formulärfält
- MudButton för åtgärdsknappar
- MudChip för status och kategorier
- MudIcon för ikoner
- MudAlert för informationsrutor

**Validering:**
- Formulärvalidering med MudForm
- Required-attribut för obligatoriska fält
- Client-side validering innan API-anrop

**State management:**
- Lokala state-variabler i Razor-komponenter
- Async/await för API-anrop
- Loading states visas med MudProgressCircular
