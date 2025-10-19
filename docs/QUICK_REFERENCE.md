# Quick Reference - New API Endpoints

## Nya Endpoints

### Accounts (Konton)
```bash
# Lista alla konton
GET /api/accounts

# Hämta specifikt konto
GET /api/accounts/{id}

# Skapa nytt konto
POST /api/accounts
Content-Type: application/json
{
  "name": "Sparkonto",
  "color": "#4CAF50"
}

# Uppdatera konto
PUT /api/accounts/{id}

# Ta bort konto
DELETE /api/accounts/{id}
```

### Reports (Rapporter)
```bash
# Nettoförmögenhet över tid
GET /api/reports/networth?start_date=2024-01-01&end_date=2024-12-31

# Månatlig sammanfattning
GET /api/reports/summary?year=2024&month=10
```

### Goals (Sparmål)
```bash
# Lista alla sparmål
GET /api/goals

# Hämta specifikt sparmål
GET /api/goals/{id}

# Hämta endast aktiva sparmål
GET /api/goals/active

# Hämta total framsteg (i procent)
GET /api/goals/progress

# Skapa nytt sparmål
POST /api/goals
Content-Type: application/json
{
  "name": "Semesterresa",
  "description": "Resa till Italien",
  "targetAmount": 50000,
  "currentAmount": 0,
  "targetDate": "2025-06-01",
  "color": "#2196F3"
}

# Uppdatera sparmål
PUT /api/goals/{id}

# Ta bort sparmål
DELETE /api/goals/{id}
```

### Transactions (Förbättrad)
```bash
# Med filtrering och paginering
GET /api/transactions?account_id=1&category_id=2&start_date=2024-01-01&end_date=2024-12-31&page=1&per_page=50
```

### Budgets (Förbättrad)
```bash
# Med periodfiltrering
GET /api/budgets?period_start=2024-01-01&period_end=2024-12-31
```

## Testa API:et

### Starta API
```bash
cd src/Privatekonomi.Api
dotnet run
```

### Öppna Swagger UI
```
http://localhost:5277/swagger
```

### Kör test script
```bash
./tests/api-tests.sh
```

## Dokumentation

- **[API_IMPROVEMENTS.md](API_IMPROVEMENTS.md)** - Fullständig API-dokumentation
- **[IMPLEMENTATION_SUMMARY.md](IMPLEMENTATION_SUMMARY.md)** - Implementation overview

## Nya Modell-fält

### Transaction
- `payee` - Betalningsmottagare
- `currency` - Valuta (default: "SEK")
- `tags` - Lista med taggar
- `imported` - Om transaktionen är importerad

### Category  
- `parentId` - Parent-kategori för hierarki
- `defaultBudgetMonthly` - Standardbudget per månad

### TransactionCategory
- `percentage` - Procentuell fördelning (default: 100)

### Goal
- `name` - Namn på sparmålet
- `description` - Beskrivning (optional)
- `targetAmount` - Målbelopp
- `currentAmount` - Nuvarande belopp
- `targetDate` - Måldatum
- `status` - Status (Active, Completed, Cancelled)
- `color` - Färg för visualisering
