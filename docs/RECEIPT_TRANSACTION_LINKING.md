# Kvitton och Transaktioner - Kopplad Funktionalitet

## Översikt

Denna funktion gör det möjligt att koppla kvitton till transaktioner som bilagor. Användare kan nu länka ett eller flera kvitton till varje transaktion för bättre spårbarhet och dokumentation av utgifter.

## Funktioner

### 1. Länka Kvitton till Transaktioner

Från kvittosidan kan användare:
- Se vilka kvitton som är kopplade till transaktioner (visas i tabellen)
- Länka ett kvitto till en transaktion via länkknappen (🔗)
- Avlänka ett kvitto från en transaktion via avlänkningsknappen (🔗❌)

#### Smart Transaktion Väljare

När du länkar ett kvitto till en transaktion öppnas en dialog som:
- **Föreslår transaktioner** baserat på:
  - Datum (inom 7 dagar från kvittodatumet)
  - Belopp (samma eller liknande belopp)
- Visar alla tillgängliga transaktioner
- Har sökfunktionalitet för att hitta specifika transaktioner
- Markerar föreslagna transaktioner med grön bakgrund

### 2. Visa Kvitton på Transaktioner

När du visar transaktionsdetaljer:
- Alla kopplade kvitton visas i ett eget kort
- Kvittobilder visas som miniatyrer
- Klicka på ett kvitto för att se fullständiga detaljer
- Antalet kvitton visas

### 3. Indikator i Transaktionslistor

I alla transaktionslistor:
- Transaktioner med kvitton visar en grön chip med kvittoikon (🧾) och antal kvitton
- Gör det enkelt att se vilka transaktioner som har dokumentation

### 4. Visa Kopplad Transaktion i Kvitto

När du visar ett kvitto:
- Om kvittot är kopplat till en transaktion visas en blå chip med länkikon
- Visar transaktionens beskrivning och belopp

## Teknisk Implementation

### Databasmodell

**Transaction** (En-till-Många relation)
- Kan ha flera `Receipts`
- Navigation property: `ICollection<Receipt> Receipts`

**Receipt**
- Kan kopplas till en `Transaction` (valfritt)
- Properties:
  - `TransactionId` (nullable)
  - `Transaction` (navigation property)

### Tjänster

**ReceiptService** - Nya metoder:
- `GetReceiptsByTransactionIdAsync(int transactionId, string userId)` - Hämta alla kvitton för en transaktion
- `LinkReceiptToTransactionAsync(int receiptId, int transactionId, string userId)` - Länka kvitto till transaktion
- `UnlinkReceiptFromTransactionAsync(int receiptId, string userId)` - Avlänka kvitto från transaktion

**TransactionService** - Uppdaterade metoder:
- `GetAllTransactionsAsync()` - Inkluderar nu `Receipts` i resultat
- `GetTransactionByIdAsync(int id)` - Inkluderar nu `Receipts` i resultat

### UI-komponenter

**TransactionDetailsDialog**
- Visar alla kopplade kvitton med bilder
- Klickbara kvittokort för att se detaljer

**ReceiptViewDialog**
- Visar kopplad transaktion (om någon)

**Receipts.razor**
- Ny kolumn i tabellen för att visa kopplad transaktion
- Knappar för att länka/avlänka kvitton
- Integration med TransactionSelectorDialog

**TransactionSelectorDialog** (NY)
- Dialog för att välja transaktion att länka till
- Smart förslag baserat på datum och belopp
- Sökfunktionalitet
- Visuell indikering av föreslagna transaktioner

**TransactionListComponent**
- Visar kvittoindikator (antal kvitton) på transaktioner

## Användningsfall

### Scenario 1: Länka kvitto till befintlig transaktion
1. Gå till "Kvitton" sidan
2. Hitta kvittot du vill länka
3. Klicka på länkikonen (🔗)
4. Välj transaktion från listan (föreslagna visas överst)
5. Klicka på transaktionen för att länka

### Scenario 2: Se kvitton på en transaktion
1. Gå till "Transaktioner" sidan
2. Klicka på en transaktion med kvittoindikator
3. Scrolla ner till "Kvitton" sektionen
4. Klicka på ett kvitto för att se detaljer

### Scenario 3: Avlänka kvitto från transaktion
1. Gå till "Kvitton" sidan
2. Hitta kvittot som är kopplat (visas med blå chip)
3. Klicka på avlänkningsikonen (🔗❌)
4. Bekräfta avlänkningen

## Säkerhet och Validering

- Alla operationer validerar att användaren äger både kvittot och transaktionen
- Audit logging för alla länk/avlänk operationer
- Felhantering med användarvänliga felmeddelanden

## Enhetstester

Följande tester har lagts till:
- `GetReceiptsByTransactionIdAsync_ReturnsOnlyReceiptsForTransaction`
- `LinkReceiptToTransactionAsync_LinksReceiptSuccessfully`
- `LinkReceiptToTransactionAsync_ThrowsWhenReceiptNotFound`
- `LinkReceiptToTransactionAsync_ThrowsWhenTransactionNotFound`
- `UnlinkReceiptFromTransactionAsync_UnlinksReceiptSuccessfully`
- `UnlinkReceiptFromTransactionAsync_ThrowsWhenReceiptNotFound`

Alla tester passerar (14/14).

## Framtida Förbättringar

Möjliga förbättringar inkluderar:
- Automatisk länkning av kvitton till transaktioner baserat på belopp och datum
- Möjlighet att länka kvitton direkt vid skapande av transaktion
- Bulk-länkning av flera kvitton samtidigt
- OCR-integration för att extrahera transaktionsdetaljer från kvitton
