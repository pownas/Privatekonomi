# Kvitton och Transaktioner - Visuell Guide

Denna guide visar hur kvitton kopplas till transaktioner i systemet med beskrivningar av de visuella elementen.

## 1. Transaktionslista med Kvittoindikatorer

**Vy: Transaktioner (Transactions.razor)**

I transaktionslistan visas nu en kvittoindikator för transaktioner som har kopplade kvitton:

```
┌─────────────────────────────────────────────────────────────────────────┐
│ Transaktioner                                                           │
├─────────┬──────────────────────────────────────┬──────────┬────────────┤
│ Datum   │ Beskrivning                          │ Kategori │ Belopp     │
├─────────┼──────────────────────────────────────┼──────────┼────────────┤
│ 2024-11 │ ICA Maxi Storgatan 🧾 2              │ Mat      │ -450.00 kr │
│ 2024-11 │ Circle K bensin                      │ Transport│ -650.00 kr │
│ 2024-10 │ Elgiganten AB 🧾 1                   │ Shopping │ -2499.00kr │
└─────────┴──────────────────────────────────────┴──────────┴────────────┘
```

**Nytt element:**
- Grön chip med kvittoikon (🧾) och antal kvitton visas bredvid beskrivningen
- Gör det enkelt att se vilka transaktioner som har dokumentation

**Screenshot skulle visa:**
- Transaktionstabell med flera rader
- Några transaktioner med grön kvittochip som visar "🧾 1" eller "🧾 2"
- Andra transaktioner utan indikator

---

## 2. Transaktionsdetaljer med Kvitton

**Vy: TransactionDetailsDialog**

När man klickar på en transaktion för att se detaljer, visas alla kopplade kvitton i ett eget kort:

```
┌─────────────────────────────────────────────────────────────────────────┐
│ Transaktionsdetaljer                                                    │
├─────────────────────────────────────────────────────────────────────────┤
│ Beskrivning: ICA Maxi Storgatan                                         │
│ Datum: 2024-11-01                                                       │
│ Belopp: -450.00 kr                                                      │
│ Bank: [ICA-banken]                                                      │
├─────────────────────────────────────────────────────────────────────────┤
│ 🧾 Kvitton (2 st)                                                       │
├─────────────────────────────────────────────────────────────────────────┤
│ ┌──────────────┐  ┌──────────────┐                                     │
│ │ ICA Maxi     │  │ ICA Maxi     │                                     │
│ │ 2024-11-01   │  │ 2024-11-01   │                                     │
│ │ [Bild]       │  │ [Fysiskt]    │                                     │
│ │              │  │ Klicka för   │                                     │
│ │ 450.00 kr    │  │ att visa     │                                     │
│ │ 5 radposter  │  │              │                                     │
│ └──────────────┘  └──────────────┘                                     │
└─────────────────────────────────────────────────────────────────────────┘
```

**Nytt element:**
- Nytt "Kvitton" kort med rubrik och antal
- Varje kvitto visas som ett klickbart kort med:
  - Butiksnamn
  - Datum
  - Miniatyrbild (om tillgänglig)
  - Totalt belopp
  - Antal radposter
  - Kvittotyp (Fysiskt, E-kvitto, Skannat)

**Screenshot skulle visa:**
- Transaktionsdetaljer-dialog öppen
- Kvitton-sektionen synlig med 1-2 kvittokort
- Tydlig visuell separation från annan information

---

## 3. Kvittolista med Transaktionskopplingar

**Vy: Receipts.razor**

Kvittotabellen har nu en ny kolumn som visar vilken transaktion kvittot är kopplat till:

```
┌──────────────────────────────────────────────────────────────────────────────┐
│ Kvitton                                                 [+ Nytt Kvitto]      │
├────────┬──────────┬─────────┬──────────┬──────────────────────┬─────────────┤
│ Datum  │ Butik    │ Belopp  │ Typ      │ Transaktion          │ Åtgärder    │
├────────┼──────────┼─────────┼──────────┼──────────────────────┼─────────────┤
│ 2024-11│ ICA Maxi │ 450 kr  │ Skannat  │ [ICA Maxi...] 🔗     │ 👁 🔗❌ ✏️ 🗑 │
│ 2024-11│ Circle K │ 650 kr  │ E-kvitto │ Ej kopplad           │ 👁 🔗 ✏️ 🗑  │
│ 2024-10│ Elgiganten│2499 kr │ Fysiskt  │ [Elgiganten AB] 🔗   │ 👁 🔗❌ ✏️ 🗑 │
└────────┴──────────┴─────────┴──────────┴──────────────────────┴─────────────┘
```

**Nya element:**
- "Transaktion" kolumn visar:
  - Blå chip med transaktionsbeskrivning om kopplad
  - Grå "Ej kopplad" om inte kopplad
- Åtgärder:
  - 🔗 (Länk) - Länka kvitto till transaktion (visas om ej kopplad)
  - 🔗❌ (Avlänka) - Avlänka från transaktion (visas om kopplad)
  - 👁 (Visa) - Visa kvittodetaljer
  - ✏️ (Redigera) - Redigera kvitto
  - 🗑 (Ta bort) - Ta bort kvitto

**Screenshot skulle visa:**
- Kvittotabell med nya kolumnen synlig
- Mix av kopplade och okopplade kvitton
- Olika åtgärdsknappar beroende på status

---

## 4. Smart Transaktionsväljare

**Vy: TransactionSelectorDialog**

När man klickar på länk-knappen (🔗) öppnas en dialog för att välja transaktion:

```
┌─────────────────────────────────────────────────────────────────────────┐
│ Välj transaktion att länka till                                    [✕]  │
├─────────────────────────────────────────────────────────────────────────┤
│ Välj en transaktion att länka kvittot till:                            │
│                                                                         │
│ [🔍 Sök transaktion...]                                                │
│                                                                         │
│ ╔═══════════════════════════════════════════════════════════════════╗ │
│ ║ ICA Maxi Storgatan                           [Föreslagen]         ║ │
│ ║ 2024-11-01                                   [ICA-banken]         ║ │
│ ║                                              -450.00 kr           ║ │
│ ╚═══════════════════════════════════════════════════════════════════╝ │
│ ┌───────────────────────────────────────────────────────────────────┐ │
│ │ Circle K Bensin                              [Swedbank]           │ │
│ │ 2024-10-28                                   -650.00 kr           │ │
│ └───────────────────────────────────────────────────────────────────┘ │
│ ┌───────────────────────────────────────────────────────────────────┐ │
│ │ Hemköp                                       [ICA-banken]         │ │
│ │ 2024-10-25                                   -320.50 kr           │ │
│ └───────────────────────────────────────────────────────────────────┘ │
│                                                                         │
├─────────────────────────────────────────────────────────────────────────┤
│                                                    [Avbryt]             │
└─────────────────────────────────────────────────────────────────────────┘
```

**Nya element:**
- Sökfält för att filtrera transaktioner
- Lista med transaktioner sorterad med föreslagna först
- Föreslagna transaktioner (matching datum ±7 dagar och belopp):
  - Grön bakgrundsfärg
  - "Föreslagen" chip
  - Visas överst i listan
- Varje transaktion visar:
  - Beskrivning
  - Datum
  - Bank (färgkodad chip)
  - Belopp (rött för utgifter, grönt för inkomster)
- Klicka på en transaktion för att länka

**Screenshot skulle visa:**
- Dialog öppen med transaktionslista
- Minst en transaktion med grön bakgrund (föreslagen)
- Sökfältet synligt
- Olika transaktioner med olika banker

---

## 5. Kvittodetaljer med Transaktionslänk

**Vy: ReceiptViewDialog**

När man visar ett kvitto som är kopplat till en transaktion:

```
┌─────────────────────────────────────────────────────────────────────────┐
│ Kvittodetaljer                                                      [✕] │
├─────────────────────────────────────────────────────────────────────────┤
│ ┌────────────┐  Butik: ICA Maxi Storgatan                              │
│ │            │  Datum: 2024-11-01                                       │
│ │  [Bild av  │  Belopp: 450.00 kr                                       │
│ │   kvitto]  │  Typ: Skannat                                            │
│ │            │                                                           │
│ │            │  ─────────────────────────────────────                   │
│ └────────────┘  Kopplad till transaktion:                               │
│                 [🔗 ICA Maxi Storgatan - 450.00 kr]                     │
│                                                                          │
│                 Radposter (5 st)                                         │
│                 ┌──────────────────────────────────────────────────┐    │
│                 │ Beskrivning    Antal   A-pris    Totalt          │    │
│                 │ Mjölk          2       15.00 kr  30.00 kr        │    │
│                 │ Bröd           1       25.00 kr  25.00 kr        │    │
│                 │ ...                                               │    │
│                 └──────────────────────────────────────────────────┘    │
├─────────────────────────────────────────────────────────────────────────┤
│                              [Redigera] [Ta bort] [Stäng]               │
└─────────────────────────────────────────────────────────────────────────┘
```

**Nytt element:**
- Blå chip med länkikon (🔗) visar kopplad transaktion
- Visar transaktionsbeskrivning och belopp
- Separator-linje för visuell tydlighet

**Screenshot skulle visa:**
- Kvittodetaljer-dialog öppen
- Kvittobild synlig (om tillgänglig)
- Transaktionslänk synlig under kvittoinformation
- Radposter om de finns

---

## 6. Avlänkningsbekräftelse

**Vy: Confirmation Dialog**

När man klickar på avlänka-knappen (🔗❌):

```
┌─────────────────────────────────────────────────────────────────────────┐
│ Bekräfta avlänkning                                                     │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│ Vill du avlänka kvittot från transaktionen                             │
│ 'ICA Maxi Storgatan'?                                                   │
│                                                                         │
├─────────────────────────────────────────────────────────────────────────┤
│                                              [Avbryt]  [Avlänka]        │
└─────────────────────────────────────────────────────────────────────────┘
```

**Screenshot skulle visa:**
- Bekräftelsedialog med tydligt meddelande
- Två knappar: Avbryt och Avlänka

---

## Sammanfattning av Visuella Förändringar

### Ikoner som används:
- 🧾 Kvitto (receipt icon i MudBlazor)
- 🔗 Länk (link icon)
- 🔗❌ Avlänka (link off icon)
- 👁 Visa (visibility icon)
- ✏️ Redigera (edit icon)
- 🗑 Ta bort (delete icon)

### Färger:
- **Grön**: Kvittoindikator på transaktioner, föreslagna transaktioner
- **Blå**: Kopplad transaktion på kvitto
- **Grå**: Ej kopplad status
- **Bankfärger**: Bevarade från bankens definierade färg

### Användarflöden:

1. **Länka kvitto till transaktion:**
   - Gå till Kvitton → Klicka på 🔗 → Välj transaktion → Klar

2. **Se kvitton på transaktion:**
   - Gå till Transaktioner → Klicka på transaktion med 🧾 → Se kvitton-sektion

3. **Avlänka kvitto:**
   - Gå till Kvitton → Klicka på 🔗❌ → Bekräfta → Klar

---

## Tekniska Detaljer för Screenshots

För att ta screenshots bör följande vyer fångas:

1. **Transaktionslista** (Transactions.razor)
   - Visa minst 3 transaktioner
   - Minst 2 med kvittoindikatorer (olika antal)
   - En utan indikator

2. **Transaktionsdetaljer** (TransactionDetailsDialog)
   - Transaktion med 2 kopplade kvitton
   - Kvittokort med och utan bild

3. **Kvittolista** (Receipts.razor)
   - Visa 4-5 kvitton
   - Mix av kopplade och okopplade
   - Visa olika åtgärdsknappar

4. **Transaktionsväljare** (TransactionSelectorDialog)
   - Minst 3 transaktioner
   - En markerad som "Föreslagen"
   - Sökfält synligt

5. **Kvittodetaljer** (ReceiptViewDialog)
   - Kvitto med bild
   - Transaktionslänk synlig
   - Radposter synliga

6. **Avlänkningsbekräftelse**
   - Bekräftelsedialog öppen

Alla screenshots bör tas med både ljust och mörkt tema om möjligt.
