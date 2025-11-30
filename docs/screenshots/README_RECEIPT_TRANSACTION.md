# Screenshots och Visuell Dokumentation - Kvitto-Transaktion Koppling

Denna mapp innehåller visuell dokumentation för den nya funktionen som kopplar kvitton till transaktioner.

## Snabbguide - Vad som ändrats

### 1. Transaktionslista - Kvittoindikator
Transaktioner med kopplade kvitton visar nu en **grön chip** med kvittoikon och antal.

**Före:** Ingen visuell indikation
```
ICA Maxi Storgatan        Mat & Dryck    -450.00 kr
```

**Efter:** Grön chip visar antal kvitton
```
ICA Maxi Storgatan 🧾 2   Mat & Dryck    -450.00 kr
```

---

### 2. Transaktionsdetaljer - Kvitton-sektion
Klicka på en transaktion för att se alla kopplade kvitton med miniatyrbilder.

**Nytt kort:**
```
┌─────────────────────────────────┐
│ 🧾 Kvitton (2 st)               │
├─────────────────────────────────┤
│  [Kvittobild 1]  [Kvittobild 2] │
│  ICA Maxi        ICA Maxi        │
│  450.00 kr       450.00 kr       │
└─────────────────────────────────┘
```

---

### 3. Kvittolista - Transaktionskolumn och Åtgärder
Ny kolumn visar vilken transaktion kvittot är kopplat till + nya knappar.

**Före:**
```
Datum  | Butik     | Belopp  | Åtgärder
-------|-----------|---------|----------
Nov 1  | ICA Maxi  | 450 kr  | 👁 ✏️ 🗑
```

**Efter:**
```
Datum  | Butik     | Belopp  | Transaktion      | Åtgärder
-------|-----------|---------|------------------|----------------
Nov 1  | ICA Maxi  | 450 kr  | [ICA Maxi...] 🔗 | 👁 🔗❌ ✏️ 🗑
Nov 2  | Circle K  | 650 kr  | Ej kopplad       | 👁 🔗 ✏️ 🗑
```

**Nya knappar:**
- 🔗 **Länka** - Länka till transaktion (visas när ej kopplad)
- 🔗❌ **Avlänka** - Ta bort länk (visas när kopplad)

---

### 4. Smart Transaktionsväljare (NY DIALOG)
När du klickar på 🔗 öppnas en dialog med **smart matching**.

**Funktioner:**
- 🔍 Sökfält för att hitta transaktioner
- ✨ Föreslår transaktioner baserat på:
  - Datum (inom ±7 dagar)
  - Belopp (exakt eller nära matchning)
- Föreslagna transaktioner visas med **grön bakgrund**

**Exempel:**
```
┌──────────────────────────────────────┐
│ Välj transaktion att länka till     │
├──────────────────────────────────────┤
│ [🔍 Sök...]                          │
│                                       │
│ ╔═══════════════════════════════════╗│ ← Grön bakgrund
│ ║ ICA Maxi    [Föreslagen] 450 kr  ║│
│ ╚═══════════════════════════════════╝│
│ ┌─────────────────────────────────┐  │
│ │ Circle K              650 kr    │  │
│ └─────────────────────────────────┘  │
└──────────────────────────────────────┘
```

---

### 5. Kvittodetaljer - Transaktionslänk
När du visar ett kopplat kvitto visas transaktionsinformation.

**Nytt element:**
```
Butik: ICA Maxi Storgatan
Datum: 2024-11-01
Belopp: 450.00 kr
────────────────────────
Kopplad till transaktion:
[🔗 ICA Maxi Storgatan - 450.00 kr]  ← Blå chip med länk
```

---

## Användarflöden

### Flöde 1: Länka kvitto till transaktion
1. Gå till **Kvitton** sidan
2. Hitta kvittot du vill länka (visar "Ej kopplad")
3. Klicka på **🔗 Länka**-knappen
4. Dialog öppnas med transaktionslista
5. Föreslagna transaktioner visas överst (grön bakgrund)
6. Klicka på önskad transaktion
7. ✅ Kvittot är nu länkat - tabellen uppdateras

### Flöde 2: Se kvitton på transaktion
1. Gå till **Transaktioner** sidan
2. Hitta transaktion med **🧾 X** indikator
3. Klicka på transaktionen
4. Scrolla ner till **Kvitton** sektionen
5. Se alla kopplade kvitton med miniatyrbilder
6. Klicka på ett kvitto för att se fullständiga detaljer

### Flöde 3: Avlänka kvitto
1. Gå till **Kvitton** sidan
2. Hitta kvittot (visar transaktionsnamn)
3. Klicka på **🔗❌ Avlänka**-knappen
4. Bekräfta i dialog
5. ✅ Kvittot är nu avlänkat - visar "Ej kopplad"

---

## Ikoner och Färger

### Ikoner
- 🧾 Kvitto (MudBlazor Receipt icon)
- 🔗 Länk (Link icon)
- 🔗❌ Avlänka (LinkOff icon)
- 👁 Visa (Visibility icon)
- ✏️ Redigera (Edit icon)
- 🗑 Ta bort (Delete icon)
- 🔍 Sök (Search icon)
- ✨ Föreslagen (i dialogtext)

### Färgkodning
- **Grön** (#4CAF50): Kvittoindikator, föreslagna transaktioner
- **Blå** (#2196F3): Kopplad transaktion på kvitto
- **Grå** (#9E9E9E): "Ej kopplad" status
- **Gul/Varning** (#FF9800): Avlänka-knapp
- **Bankfärger**: Bevarade enligt bank (ICA-banken röd, etc.)

---

## Skärmbilder som behövs

För fullständig dokumentation bör följande skärmbilder tas:

### Obligatoriska screenshots:
1. ✅ **Transaktionslista med kvittoindikatorer**
   - Visa 3-4 transaktioner
   - Minst 2 med 🧾-indikatorer
   - Olika antal kvitton (1, 2, 3)

2. ✅ **Transaktionsdetaljer med kvitton**
   - Dialog öppen
   - Kvitton-sektion synlig med 2 kvittokort
   - Miniatyrbild synlig på minst ett kvitto

3. ✅ **Kvittolista med nya kolumnen**
   - Minst 4 kvitton
   - Mix av kopplade och okopplade
   - Olika åtgärdsknappar synliga

4. ✅ **Transaktionsväljare-dialog**
   - Dialog öppen med sökning
   - Minst en "Föreslagen" transaktion (grön)
   - 2-3 vanliga transaktioner

5. ✅ **Kvittodetaljer med transaktionslänk**
   - Dialog med kvitto
   - Transaktionslänk tydligt synlig
   - Kvittobild om möjligt

### Valfria screenshots:
6. 🔲 Avlänkningsbekräftelse-dialog
7. 🔲 Ljust vs Mörkt tema jämförelse
8. 🔲 Mobil responsiv vy

---

## Testinstruktioner för Screenshots

För att ta autentiska screenshots:

### Förberedelse:
1. Starta applikationen: `cd src/Privatekonomi.Web && dotnet run`
2. Logga in som testanvändare
3. Skapa några test-transaktioner och kvitton

### Data att förbereda:
- **3-4 transaktioner** från olika datum och butiker
- **5-6 kvitton** med olika:
  - Butiker (ICA, Circle K, Elgiganten, etc.)
  - Belopp (50-3000 kr)
  - Datum (sprida över 2 veckor)
  - Typ (Fysiskt, E-kvitto, Skannat)
- **Länka några kvitton** till transaktioner
- **Lämna några olänkade** för att visa kontrast

### För varje screenshot:
1. Navigera till rätt vy
2. Se till att data är synlig och realistisk
3. Ta screenshot i fullskärm eller anpassad storlek
4. Spara med beskrivande namn: `01-transaktionslista-med-kvitton.png`
5. Föredra PNG-format för klarhet

### Rekommenderad upplösning:
- Desktop: 1920x1080 eller 1366x768
- Tablet: 768x1024
- Mobil: 375x667 (iPhone SE) eller 360x640 (Android)

---

## Placering av Screenshots

Screenshots bör placeras i:
```
docs/
  screenshots/
    receipt-transaction-linking/
      01-transaction-list-with-receipts.png
      02-transaction-details-receipts-section.png
      03-receipts-table-with-transaction-column.png
      04-transaction-selector-dialog.png
      05-receipt-details-with-transaction-link.png
      06-unlink-confirmation.png (optional)
```

Och refereras i dokumentationen som:
```markdown
![Transaktionslista med kvitton](screenshots/receipt-transaction-linking/01-transaction-list-with-receipts.png)
```

---

## Nästa Steg

1. **Ta screenshots** enligt instruktionerna ovan
2. **Lägg till bilderna** i `docs/screenshots/receipt-transaction-linking/`
3. **Uppdatera README.md** i huvudmappen med referens till nya funktionen
4. **Uppdatera PR** med bilder i beskrivningen

---

För detaljerad teknisk dokumentation, se:
- [RECEIPT_TRANSACTION_LINKING.md](RECEIPT_TRANSACTION_LINKING.md) - Teknisk dokumentation
- [RECEIPT_TRANSACTION_LINKING_SCREENSHOTS.md](RECEIPT_TRANSACTION_LINKING_SCREENSHOTS.md) - ASCII mockups och detaljerad UI-beskrivning
