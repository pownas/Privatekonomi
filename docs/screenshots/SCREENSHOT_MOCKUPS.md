# EditTransactionDialog - Screenshot Mockups

## Screenshot 1: Main Dialog - Single Category Mode (Desktop)

```
┌────────────────────────────────────────────────────────────────────────────┐
│ Redigera transaktion                                                   [X] │
├────────────────────────────────────────────────────────────────────────────┤
│                                                                            │
│  Beskrivning *                                                             │
│  ┌──────────────────────────────────────────────────────────────────────┐ │
│  │ ICA Maxi - Veckohandling                                             │ │
│  └──────────────────────────────────────────────────────────────────────┘ │
│  Beskrivande namn för transaktionen                                       │
│                                                                            │
│  Datum *                                 Belopp * (kr)                     │
│  ┌──────────────────────────────┐       ┌──────────────────────────────┐  │
│  │ 2024-10-28                📅│       │ 1 247,50                  kr│  │
│  └──────────────────────────────┘       └──────────────────────────────┘  │
│  När transaktionen genomfördes          Transaktionsbelopp                │
│                                                                            │
│  ╔═══════════════════════════╗          Mottagare/Betalare                │
│  ║ 🔄 OFF  │  Utgift         ║          ┌──────────────────────────────┐  │
│  ╚═══════════════════════════╝          │ ICA Supermarket              │  │
│  (Switch toggle)                        └──────────────────────────────┘  │
│                                         Vem som mottog eller skickade...  │
│                                                                            │
│  Kategorier                                                                │
│  ────────────────────────────────────────────────────────────────────────  │
│                                                                            │
│  ● En kategori       ○ Dela på flera kategorier (2-4)                     │
│                                                                            │
│  Sök och välj kategori                                                     │
│  ┌──────────────────────────────────────────────────────────────────────┐ │
│  │ Mat                                                                🔍│ │
│  └──────────────────────────────────────────────────────────────────────┘ │
│  Börja skriva för att söka bland kategorier                               │
│                                                                            │
│  ┌─────────────────┐                                                       │
│  │ 🟢 Mat       [X]│  ← Selected category chip                            │
│  └─────────────────┘                                                       │
│                                                                            │
│  Hushåll                                                                   │
│  ┌──────────────────────────────────────────────────────────────────────┐ │
│  │ Familjen                                                          ▼ │ │
│  └──────────────────────────────────────────────────────────────────────┘ │
│  Välj ett hushåll eller lämna tomt för personlig transaktion              │
│                                                                            │
│  Betalningsmetod                        Valuta                            │
│  ┌──────────────────────────────┐       ┌──────────────────────────────┐  │
│  │ Kort                      ▼ │       │ SEK                          │  │
│  └──────────────────────────────┘       └──────────────────────────────┘  │
│  Välj betalningsmetod                   Valuta (standard: SEK)            │
│                                                                            │
│  Taggar (kommaseparerade)                                                  │
│  ┌──────────────────────────────────────────────────────────────────────┐ │
│  │ mat, veckohandling, familj                                           │ │
│  └──────────────────────────────────────────────────────────────────────┘ │
│  Exempel: mat, shopping, nödvändigt                                        │
│                                                                            │
│  Noteringar                                                                │
│  ┌──────────────────────────────────────────────────────────────────────┐ │
│  │                                                                      │ │
│  │ Storhandling för veckan                                              │ │
│  │                                                                      │ │
│  └──────────────────────────────────────────────────────────────────────┘ │
│  Lägg till anteckningar eller kommentarer om transaktionen                │
│                                                                            │
│                                                                            │
│                        [Avbryt]                    [Spara]                │
└────────────────────────────────────────────────────────────────────────────┘
```

**Key Features Shown:**
- ✅ New IsIncome toggle switch (OFF = Utgift shown)
- ✅ New Payee field (Mottagare/Betalare)
- ✅ Required field indicators (*)
- ✅ MudAutocomplete for category search
- ✅ Selected category shown as colored chip
- ✅ New PaymentMethod dropdown
- ✅ New Currency field
- ✅ Responsive 2-column layout (Date/Amount, IsIncome/Payee, PaymentMethod/Currency)

---

## Screenshot 2: Category Autocomplete Dropdown (Search Active)

```
┌────────────────────────────────────────────────────────────────────────────┐
│  Sök och välj kategori                                                     │
│  ┌──────────────────────────────────────────────────────────────────────┐ │
│  │ mat_                                                              🔍│ │ ← User typing
│  └──────────────────────────────────────────────────────────────────────┘ │
│                                                                            │
│  ┌──────────────────────────────────────────────────────────────────────┐ │
│  │ ╔══════════════════════════════════════════════════════════════════╗│ │
│  │ ║ 🟢 Mat                                                          ║│ │ ← Highlighted
│  │ ║    (Utgifter)                                 ← Parent category ║│ │
│  │ ╚══════════════════════════════════════════════════════════════════╝│ │
│  │ ┌────────────────────────────────────────────────────────────────┐ │ │
│  │ │ 🟡 Matlagning                                                  │ │ │
│  │ │    (Mat)                                                       │ │ │
│  │ └────────────────────────────────────────────────────────────────┘ │ │
│  │ ┌────────────────────────────────────────────────────────────────┐ │ │
│  │ │ 🔵 Restaurang                                                  │ │ │
│  │ │    (Mat)                                                       │ │ │
│  │ └────────────────────────────────────────────────────────────────┘ │ │
│  │ ┌────────────────────────────────────────────────────────────────┐ │ │
│  │ │ 🟠 Husdjursmat                                                 │ │ │
│  │ │    (Djur)                                                      │ │ │
│  │ └────────────────────────────────────────────────────────────────┘ │ │
│  └──────────────────────────────────────────────────────────────────────┘ │
└────────────────────────────────────────────────────────────────────────────┘
```

**Key Features Shown:**
- ✅ Real-time search filtering (searching for "mat")
- ✅ Color indicators (colored circles) for each category
- ✅ Parent category hierarchy shown in gray text
- ✅ Highlighted/selected item (first result)
- ✅ Multiple matching categories displayed

---

## Screenshot 3: Split Transaction Mode - Percentage

```
┌────────────────────────────────────────────────────────────────────────────┐
│ Redigera transaktion                                                   [X] │
├────────────────────────────────────────────────────────────────────────────┤
│                                                                            │
│  [... Basic fields shown above ...]                                        │
│                                                                            │
│  Kategorier                                                                │
│  ────────────────────────────────────────────────────────────────────────  │
│                                                                            │
│  ○ En kategori       ● Dela på flera kategorier (2-4)                     │
│                                                                            │
│  ● Dela via procent       ○ Dela via exakta belopp                         │
│                                                                            │
│  ┌──────────────────────────────────────────────────────────────────────┐ │
│  │  Kategori 1                                    Procent              │ │
│  │  ┌────────────────────────────────────┐       ┌──────────────────┐ │ │
│  │  │ 🟢 Mat                          🔍│       │ 60,0          % │ │ │
│  │  └────────────────────────────────────┘       └──────────────────┘ │ │
│  └──────────────────────────────────────────────────────────────────────┘ │
│                                                                            │
│  ┌──────────────────────────────────────────────────────────────────────┐ │
│  │  Kategori 2                                    Procent          [🗑️]│ │
│  │  ┌────────────────────────────────────┐       ┌──────────────────┐ │ │
│  │  │ 🔵 Hushåll                      🔍│       │ 30,0          % │ │ │
│  │  └────────────────────────────────────┘       └──────────────────┘ │ │
│  └──────────────────────────────────────────────────────────────────────┘ │
│                                                                            │
│  ┌──────────────────────────────────────────────────────────────────────┐ │
│  │  Kategori 3                                    Procent          [🗑️]│ │
│  │  ┌────────────────────────────────────┐       ┌──────────────────┐ │ │
│  │  │ 🟡 Nöje                         🔍│       │ 10,0          % │ │ │
│  │  └────────────────────────────────────┘       └──────────────────┘ │ │
│  └──────────────────────────────────────────────────────────────────────┘ │
│                                                                            │
│  [+ Lägg till kategori]  ← Can add up to 4 total                          │
│                                                                            │
│  ┌──────────────────────────────────────────────────────────────────────┐ │
│  │ ℹ️  Total: 100,0% av 1 247,50 kr                                     │ │
│  │                                                                      │ │
│  │ ✅ Validation passed - ready to save                                 │ │
│  └──────────────────────────────────────────────────────────────────────┘ │
│                                                                            │
│  [... Household, Payment Method, Tags, Notes fields ...]                  │
│                                                                            │
│                        [Avbryt]                    [Spara]                │
└────────────────────────────────────────────────────────────────────────────┘
```

**Key Features Shown:**
- ✅ Split mode radio selection
- ✅ Percentage vs Amount radio selection
- ✅ MudAutocomplete in each split row (with search icon)
- ✅ Percentage input fields with % symbol
- ✅ Delete button [🗑️] for each row (except first)
- ✅ "Add category" button
- ✅ Real-time validation alert (100% = success)
- ✅ Each category shows color indicator

---

## Screenshot 4: Split Transaction - Validation Warning

```
┌────────────────────────────────────────────────────────────────────────────┐
│  Kategorier - Split Mode                                                   │
│  ────────────────────────────────────────────────────────────────────────  │
│                                                                            │
│  ● Dela via procent       ○ Dela via exakta belopp                         │
│                                                                            │
│  ┌──────────────────────────────────────────────────────────────────────┐ │
│  │  Kategori 1: 🟢 Mat                              60,0 %              │ │
│  └──────────────────────────────────────────────────────────────────────┘ │
│                                                                            │
│  ┌──────────────────────────────────────────────────────────────────────┐ │
│  │  Kategori 2: 🔵 Hushåll                          25,0 %         [🗑️] │ │
│  └──────────────────────────────────────────────────────────────────────┘ │
│                                                                            │
│  [+ Lägg till kategori]                                                    │
│                                                                            │
│  ┌──────────────────────────────────────────────────────────────────────┐ │
│  │ ℹ️  Total: 85,0% av 1 247,50 kr                                      │ │
│  │                                                                      │ │
│  │ ⚠️  Totalen måste vara 100%                     ← Warning message   │ │
│  └──────────────────────────────────────────────────────────────────────┘ │
│                                                                            │
└────────────────────────────────────────────────────────────────────────────┘
```

**Key Features Shown:**
- ✅ Real-time validation warning (85% ≠ 100%)
- ✅ Orange/warning icon and message
- ✅ Shows current total vs. required total
- ✅ Prevents saving until validation passes

---

## Screenshot 5: Mobile View (Single Column Layout)

```
┌──────────────────────────────────┐
│ Redigera transaktion         [X] │
├──────────────────────────────────┤
│                                  │
│ Beskrivning *                    │
│ ┌──────────────────────────────┐ │
│ │ ICA Maxi - Veckohandling     │ │
│ └──────────────────────────────┘ │
│                                  │
│ Datum *                          │
│ ┌──────────────────────────────┐ │
│ │ 2024-10-28              📅  │ │
│ └──────────────────────────────┘ │
│                                  │
│ Belopp * (kr)                    │
│ ┌──────────────────────────────┐ │
│ │ 1 247,50                  kr│ │
│ └──────────────────────────────┘ │
│                                  │
│ ╔══════════════════════════════╗ │
│ ║ 🔄 OFF  │  Utgift            ║ │
│ ╚══════════════════════════════╝ │
│                                  │
│ Mottagare/Betalare               │
│ ┌──────────────────────────────┐ │
│ │ ICA Supermarket              │ │
│ └──────────────────────────────┘ │
│                                  │
│ Kategorier                       │
│ ───────────────────────────────  │
│ ● En kategori                    │
│ ○ Dela på flera kategorier       │
│                                  │
│ Sök och välj kategori            │
│ ┌──────────────────────────────┐ │
│ │ Mat                       🔍│ │
│ └──────────────────────────────┘ │
│                                  │
│ ┌────────────┐                   │
│ │ 🟢 Mat  [X]│                   │
│ └────────────┘                   │
│                                  │
│ Hushåll                          │
│ ┌──────────────────────────────┐ │
│ │ Familjen                  ▼ │ │
│ └──────────────────────────────┘ │
│                                  │
│ Betalningsmetod                  │
│ ┌──────────────────────────────┐ │
│ │ Kort                      ▼ │ │
│ └──────────────────────────────┘ │
│                                  │
│ Valuta                           │
│ ┌──────────────────────────────┐ │
│ │ SEK                          │ │
│ └──────────────────────────────┘ │
│                                  │
│ Taggar                           │
│ ┌──────────────────────────────┐ │
│ │ mat, veckohandling           │ │
│ └──────────────────────────────┘ │
│                                  │
│ Noteringar                       │
│ ┌──────────────────────────────┐ │
│ │ Storhandling för veckan      │ │
│ └──────────────────────────────┘ │
│                                  │
│ [Avbryt]          [Spara]        │
└──────────────────────────────────┘
```

**Key Features Shown:**
- ✅ Single-column responsive layout (< 960px)
- ✅ All fields stacked vertically
- ✅ Touch-friendly controls
- ✅ Same functionality as desktop
- ✅ Optimized for mobile screens

---

## Screenshot 6: Loading State (Save in Progress)

```
┌────────────────────────────────────────────────────────────────────────────┐
│ Redigera transaktion                                                   [X] │
├────────────────────────────────────────────────────────────────────────────┤
│                                                                            │
│  [... All fields shown ...]                                                │
│                                                                            │
│                                                                            │
│                  [Avbryt]              [⏳ Sparar...]                      │
│                                         ────────────                       │
│                                         (Disabled)                         │
└────────────────────────────────────────────────────────────────────────────┘
```

**Key Features Shown:**
- ✅ Save button disabled during save operation
- ✅ Spinner icon (⏳) shown
- ✅ Text changes to "Sparar..."
- ✅ Prevents double-submission

---

## Screenshot 7: Validation Errors (Required Fields)

```
┌────────────────────────────────────────────────────────────────────────────┐
│ Redigera transaktion                                                   [X] │
├────────────────────────────────────────────────────────────────────────────┤
│                                                                            │
│  Beskrivning *                                                             │
│  ┌──────────────────────────────────────────────────────────────────────┐ │
│  │                                                                      │ │
│  └──────────────────────────────────────────────────────────────────────┘ │
│  ⚠️ Beskrivning är obligatorisk        ← Error message                    │
│                                                                            │
│  Datum *                                 Belopp * (kr)                     │
│  ┌──────────────────────────────┐       ┌──────────────────────────────┐  │
│  │                          📅│       │ 0,00                      kr│  │
│  └──────────────────────────────┘       └──────────────────────────────┘  │
│  ⚠️ Datum är obligatoriskt               ⚠️ Belopp är obligatoriskt        │
│                                                                            │
└────────────────────────────────────────────────────────────────────────────┘
```

**Key Features Shown:**
- ✅ Required field validation
- ✅ Error messages shown below fields
- ✅ Warning icon (⚠️)
- ✅ Red/orange colored error text
- ✅ Prevents saving until fields are filled

---

## Screenshot 8: Payment Method Dropdown

```
┌────────────────────────────────────────────────────────────────────────────┐
│  Betalningsmetod                                                           │
│  ┌──────────────────────────────────────────────────────────────────────┐ │
│  │ Kort                                                              ▼ │ │ ← Clicked
│  └──────────────────────────────────────────────────────────────────────┘ │
│                                                                            │
│  ┌──────────────────────────────────────────────────────────────────────┐ │
│  │ ┌────────────────────────────────────────────────────────────────┐ │ │
│  │ │ Swish                                                          │ │ │
│  │ └────────────────────────────────────────────────────────────────┘ │ │
│  │ ┌────────────────────────────────────────────────────────────────┐ │ │
│  │ │ Autogiro                                                       │ │ │
│  │ └────────────────────────────────────────────────────────────────┘ │ │
│  │ ┌────────────────────────────────────────────────────────────────┐ │ │
│  │ │ E-faktura                                                      │ │ │
│  │ └────────────────────────────────────────────────────────────────┘ │ │
│  │ ┌────────────────────────────────────────────────────────────────┐ │ │
│  │ │ Banköverföring                                                 │ │ │
│  │ └────────────────────────────────────────────────────────────────┘ │ │
│  │ ╔══════════════════════════════════════════════════════════════════╗│ │
│  │ ║ Kort                                                            ║│ │ ← Selected
│  │ ╚══════════════════════════════════════════════════════════════════╝│ │
│  │ ┌────────────────────────────────────────────────────────────────┐ │ │
│  │ │ Kontant                                                        │ │ │
│  │ └────────────────────────────────────────────────────────────────┘ │ │
│  └──────────────────────────────────────────────────────────────────────┘ │
└────────────────────────────────────────────────────────────────────────────┘
```

**Key Features Shown:**
- ✅ Payment method dropdown with Swedish options
- ✅ Swish, Autogiro, E-faktura, Banköverföring, Kort, Kontant
- ✅ Selected value highlighted
- ✅ Clearable dropdown (can be empty)

---

## Summary of New Features Visible in Screenshots

### 1. New Fields
- ✅ **IsIncome Toggle**: Switch-style control for Income/Expense
- ✅ **Payee Field**: Text input for payment recipient/sender
- ✅ **Payment Method**: Dropdown with Swedish payment types
- ✅ **Currency**: Text input with SEK default

### 2. Enhanced Category Selection
- ✅ **MudAutocomplete**: Searchable dropdown with real-time filtering
- ✅ **Hierarchy Display**: Shows parent categories in gray
- ✅ **Color Indicators**: Colored circles for each category
- ✅ **Selected Chip**: Visual feedback of selected category

### 3. Form Validation
- ✅ **Required Fields**: Marked with * and validated
- ✅ **Error Messages**: Clear, descriptive error text below fields
- ✅ **Split Validation**: Real-time percentage/amount validation
- ✅ **Visual Feedback**: Warning icons and colored text

### 4. UX Improvements
- ✅ **Loading State**: Disabled button with spinner during save
- ✅ **Responsive Layout**: 2-column desktop, 1-column mobile
- ✅ **Helper Text**: Guidance below each field
- ✅ **ARIA Labels**: Accessibility support

### 5. Split Transaction Features
- ✅ **Autocomplete in Splits**: Each split row has searchable category
- ✅ **Add/Remove**: Dynamic category split rows (2-4)
- ✅ **Validation Alert**: Real-time total calculation and feedback
- ✅ **Percentage/Amount**: Two modes for splitting
