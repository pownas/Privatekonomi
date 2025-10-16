# CSV Import Guide

This guide explains how to use the CSV import functionality to import transactions from ICA-banken and Swedbank.

## Supported Banks

### ICA-banken
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

### Swedbank
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

## How to Import

### Using the Web Interface

1. Navigate to **Importera** in the menu
2. Select your bank (ICA-banken or Swedbank)
3. Choose a CSV file to upload (max 10 MB)
4. Review the preview of transactions
5. Confirm the import

### Using the API

#### Upload and Preview
```bash
curl -X POST https://localhost:7023/api/import/upload \
  -F "file=@transactions.csv" \
  -F "bankName=ICA-banken"
```

#### Confirm Import
```bash
curl -X POST https://localhost:7023/api/import/confirm \
  -H "Content-Type: application/json" \
  -d '{"fileId":"<file-id>","bank":"ICA-banken","skipDuplicates":true}'
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

**Swedbank:**
- "K" (Kredit) → Income
- "D" (Debet) → Expense
- Currency must be SEK
- Description combines Beneficiary and Details

## Limitations

- Maximum file size: 10 MB
- Only CSV files (.csv) are supported
- Only SEK currency is supported (for Swedbank)
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
