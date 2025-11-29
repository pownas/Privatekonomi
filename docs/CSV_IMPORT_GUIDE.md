# Transaction Import Guide (CSV/OFX)

This guide explains how to use the import functionality to import transactions from Swedish banks using CSV or OFX files.

![Importera Transaktioner](https://github.com/user-attachments/assets/e352caaf-230e-4032-baf0-b850667760f0)

## Supported Formats

### CSV Format
CSV (Comma/Semicolon Separated Values) files are supported from the following banks:
- ICA-banken
- Swedbank (both CSN and old formats)

### OFX/QFX Format
OFX (Open Financial Exchange) is a standardized format supported by most banks worldwide. QFX is Quicken's variant of OFX.

**Features:**
- Universal format supported by most banks
- Contains complete transaction data including dates, amounts, and descriptions
- Can include transaction types (debit/credit)

## Supported Banks

### ICA-banken (CSV)
**Format:** Semicolon-separated (`;`) CSV file
**Required columns:**
- Datum (Date)
- Belopp (Amount) 
- Beskrivning (Description)

**Example format:**
```csv
Datum;Belopp;Beskrivning;Saldo
2025-01-15;-125,50;ICA Maxi Stockholm;8742,30
2025-01-16;3500,00;Lön;12242,30
```

### Swedbank (CSV)

Swedbank supports two export formats:

#### Format 1: CSN Format (Tab-separated with Swedish column names)
**Format:** Tab-separated values
**Required columns:**
- Radnummer (Row number)
- Bokföringsdag (Booking date)
- Belopp (Amount)
- Beskrivning (Description)

**Example format:**
```csv
Radnummer	Clearingnummer	Kontonummer	Produkt	Valuta	Bokföringsdag	Transaktionsdag	Valutadag	Referens	Beskrivning	Belopp	Bokfört saldo
1	84525	222222222	Privatkonto	SEK	2025-09-30	2025-09-30	2025-09-30	PRIS NYCKELKUND	PRIS NYCKELKUND	-39.00	2554.18
2	84525	222222222	Privatkonto	SEK	2025-09-29	2025-09-28	2025-09-28	Swishkonto	Swishkonto	1300.00	2593.18
```

**Note:** 
- Positive amounts are treated as income
- Negative amounts are treated as expenses
- Only SEK currency is supported

#### Format 2: Old Format (Semicolon-separated with English column names)
**Format:** Semicolon-separated (`;`) CSV file with quoted fields
**Required columns:**
- Row type
- Date
- Amount
- Debit/Credit
- Details

**Example format:**
```csv
"Client account";"Row type";"Date";"Beneficiary/Payer";"Details";"Amount";"Currency";"Debit/Credit";"Transfer reference";"Transaction type";"Reference number";"Document number";
"SE0000000000000000000000";"20";"08.01.2025";"ICA MAXI STOCKHOLM";"Kortköp 123456789";"125,50";"SEK";"D";"";"MK";"";"5";
```

**Note:** Only row type 20 (standard transactions) are imported. Opening balance (10), turnover (82), and closing balance (86) are skipped.

### OFX/QFX (Universal Format)
**Format:** OFX (Open Financial Exchange) or QFX (Quicken Financial Exchange)
**Supported versions:** OFX 1.x (SGML) and OFX 2.x (XML)

OFX is a standardized format that most banks support for exporting account statements.

**Key elements parsed:**
- `<DTPOSTED>` - Transaction date
- `<TRNAMT>` - Transaction amount
- `<NAME>` - Transaction name/payee
- `<MEMO>` - Additional memo/description
- `<TRNTYPE>` - Transaction type (DEBIT, CREDIT, etc.)

**Example OFX format:**
```xml
<STMTTRN>
<TRNTYPE>DEBIT</TRNTYPE>
<DTPOSTED>20250110</DTPOSTED>
<TRNAMT>-125.50</TRNAMT>
<FITID>20250110001</FITID>
<NAME>ICA MAXI STOCKHOLM</NAME>
<MEMO>Kortköp</MEMO>
</STMTTRN>
```

**How to export OFX from your bank:**
- Most Swedish banks provide OFX export in their internet banking
- Look for "Exportera kontoutdrag" or "Ladda ner transaktioner"
- Select OFX or QFX format

## How to Import

### Using the Web Interface

1. Navigate to **Importera** in the menu
2. Select **Transaktioner**
3. Choose your format:
   - ICA-banken (CSV)
   - Swedbank (CSV)
   - OFX/QFX (Allmänt bankformat)
4. Upload your file (max 10 MB)
5. Review the preview of transactions
6. Confirm the import

### Using the API

#### Upload and Preview (CSV)
```bash
curl -X POST https://localhost:7023/api/import/upload \
  -F "file=@transactions.csv" \
  -F "bankName=ICA-banken"
```

#### Upload and Preview (OFX)
```bash
curl -X POST https://localhost:7023/api/import/upload \
  -F "file=@transactions.ofx" \
  -F "bankName=OFX (Allmän)"
```

#### Confirm Import
```bash
curl -X POST https://localhost:7023/api/import/confirm \
  -H "Content-Type: application/json" \
  -d '{"fileId":"<file-id>","bank":"ICA-banken","skipDuplicates":true}'
```

#### Get Import Job Status
```bash
curl -X GET https://localhost:7023/api/import/{jobId}/status
```

#### List Supported Banks
```bash
curl -X GET https://localhost:7023/api/import/banks
```

## Features

### Duplicate Detection
The system automatically detects duplicate transactions based on:
- Date (same day)
- Amount (exact match)
- Description (case-insensitive)
- Transaction type (income/expense)

Duplicates are skipped by default but you can see how many were found.

### Validation
Each transaction is validated for:
- Valid date format
- Valid amount (up to 10 million SEK)
- Non-empty description
- Date not in the future (max 7 days ahead)

### Data Mapping

**ICA-banken:**
- Positive amounts → Income
- Negative amounts → Expense
- Amount is stored as absolute value

**Swedbank (CSN Format):**
- Positive amounts → Income
- Negative amounts → Expense
- Amount is stored as absolute value
- Currency must be SEK
- Description from "Beskrivning" or "Referens" column

**Swedbank (Old Format):**
- "K" (Kredit) → Income
- "D" (Debet) → Expense
- Currency must be SEK
- Description combines Beneficiary and Details

## Limitations

- Maximum file size: 10 MB
- Supported file formats: CSV (.csv, .txt) and OFX (.ofx, .qfx)
- Only SEK currency is fully supported (transactions in other currencies are skipped for Swedbank)
- Dates cannot be older than 10 years (warning only)
- Dates cannot be more than 7 days in the future

## Testing

Sample CSV files are available in `/tmp/csv-test-data/`:
- `ica_sample.csv` - 10 transactions from ICA-banken
- `swedbank_sample.csv` - 8 transactions from Swedbank

## Troubleshooting

**"Filen är för stor"**
- File exceeds 10 MB limit
- Try splitting into smaller files

**"Kunde inte hitta nödvändiga kolumner"**
- CSV format doesn't match expected bank format
- Verify you selected the correct bank

**"Beskrivning saknas"**
- Transaction has empty description
- Row will be skipped

**"Transaktionen finns redan"**
- Duplicate transaction detected
- This is normal if you import the same file twice
