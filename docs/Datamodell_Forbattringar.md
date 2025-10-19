# Datamodellförbättringar - Implementationsrapport

## Översikt
Denna rapport beskriver de förbättringar som har implementerats i systemets datamodeller baserat på förslaget i issue #XX.

## Implementerade förbättringar

### 1. Ny modell: Goal (Sparmål)
En helt ny modell för att hantera sparmål har skapats med följande egenskaper:
- `GoalId`: Primärnyckel
- `Name`: Målets namn (t.ex. "Semesterresa till Japan")
- `Description`: Beskrivning av målet
- `TargetAmount`: Målbelopp
- `CurrentAmount`: Nuvarande sparat belopp
- `TargetDate`: Målsättningsdatum
- `Priority`: Prioritet (1-5, där 1 är högst)
- `FundedFromBankSourceId`: Vilket konto som används för sparandet
- `CreatedAt`: Tidsstämpel när målet skapades
- `UpdatedAt`: Tidsstämpel när målet senast uppdaterades

**Service**: `GoalService` och `IGoalService` har skapats med metoder för CRUD-operationer samt extra funktionalitet för att uppdatera framsteg och filtrera efter prioritet.

**Testdata**: 5 exempel-sparmål har lagts till i TestDataSeeder.

### 2. Förbättrad Transaction-modell
Följande fält har lagts till:
- `Currency`: Valuta (default "SEK")
- `Payee`: Mottagare/avsändare av betalning
- `Tags`: Kommaseparerade taggar för flexibel kategorisering
- `RecurringId`: Länk till återkommande transaktion
- `Imported`: Indikerar om transaktionen importerades från CSV/API
- `ImportSource`: Källa för import (t.ex. "ICA-banken CSV")
- `Cleared`: Om transaktionen har kvitterats/avstämts
- `CreatedAt`: Tidsstämpel när transaktionen skapades
- `UpdatedAt`: Tidsstämpel när transaktionen senast uppdaterades

**Index**: Tillagt index på `Date`, `BankSourceId + Date` (composite), och `Payee` för bättre prestanda.

### 3. Förbättrad Category-modell
Följande fält har lagts till:
- `ParentId`: För hierarkiska kategorier (t.ex. "Transport" -> "Kollektivtrafik")
- `DefaultBudgetMonthly`: Standardbudget per månad för kategorin
- `TaxRelated`: Om kategorin är skatterelaterad
- `CreatedAt`: Tidsstämpel när kategorin skapades
- `UpdatedAt`: Tidsstämpel när kategorin senast uppdaterades

**Relation**: Self-referencing relation för att stödja kategori-hierarkier med `Parent` och `SubCategories` navigation properties.

### 4. Förbättrad BankSource-modell (Account)
BankSource har uppgraderats till att fungera mer som ett fullständigt konto:
- `AccountType`: Kontotyp (checking, savings, credit_card, investment, cash)
- `Currency`: Kontots valuta
- `Institution`: Bankens institutionsnamn
- `InitialBalance`: Startbalans när kontot öppnades
- `OpenedDate`: Datum när kontot öppnades
- `ClosedDate`: Datum när kontot stängdes (null om aktivt)
- `CreatedAt`: Tidsstämpel när kontot skapades
- `UpdatedAt`: Tidsstämpel när kontot senast uppdaterades
- `CurrentBalance`: Beräknad property för nuvarande saldo (inte lagrad i DB)

### 5. Audit-fält tillagda i alla modeller
Alla huvudmodeller har nu fått audit-fält:
- `Budget`: CreatedAt, UpdatedAt
- `Investment`: CreatedAt, UpdatedAt
- `Loan`: CreatedAt, UpdatedAt (plus Currency, StartDate, MaturityDate)
- `Household`: UpdatedAt

### 6. Förbättrade Services
Alla service-klasser har uppdaterats för att automatiskt sätta CreatedAt vid skapande och UpdatedAt vid uppdatering:
- `TransactionService`
- `CategoryService`
- `BudgetService`
- `InvestmentService`
- `LoanService`
- `GoalService` (ny)

`CategoryService` inkluderar nu även Parent och SubCategories i queries.

### 7. Uppdaterad DbContext
`PrivatekonomyContext` har uppdaterats med:
- Konfiguration för alla nya fält
- Index för prestandaoptimering
- Korrekt precision för decimal-fält
- Self-referencing relation för Category
- Ignored computed properties (CurrentBalance, TotalValue, etc.)
- Uppdaterad seed data med nya fält

## Backward Compatibility
Alla ändringar är backward compatible:
- Nya fält har defaultvärden eller är nullable
- Befintlig funktionalitet påverkas inte
- Seed data har uppdaterats för att inkludera nya fält

## Databasmigrering
För produktionsmiljö med persistent databas (SQL Server):
```bash
dotnet ef migrations add EnhanceDataModels --project src/Privatekonomi.Core
dotnet ef database update --project src/Privatekonomi.Core
```

## Användning av nya funktioner

### Exempel: Skapa ett sparmål
```csharp
var goal = new Goal
{
    Name = "Ny bil",
    Description = "Spara till handpenning för bil",
    TargetAmount = 100000m,
    CurrentAmount = 25000m,
    TargetDate = DateTime.Now.AddYears(2),
    Priority = 2,
    FundedFromBankSourceId = 1
};
await goalService.CreateGoalAsync(goal);
```

### Exempel: Skapa hierarkisk kategori
```csharp
var parentCategory = new Category { Name = "Transport", Color = "#4ECDC4" };
await categoryService.CreateCategoryAsync(parentCategory);

var subCategory = new Category 
{ 
    Name = "Kollektivtrafik", 
    Color = "#4ECDC4",
    ParentId = parentCategory.CategoryId 
};
await categoryService.CreateCategoryAsync(subCategory);
```

### Exempel: Transaktion med nya fält
```csharp
var transaction = new Transaction
{
    Amount = 150m,
    Description = "SL-kort månadskort",
    Date = DateTime.Now,
    IsIncome = false,
    Currency = "SEK",
    Payee = "SL",
    Tags = "transport,pendling,månadskort",
    Imported = true,
    ImportSource = "ICA-banken CSV",
    Cleared = true,
    BankSourceId = 1
};
await transactionService.CreateTransactionAsync(transaction);
```

## Testresultat
- ✅ Build lyckades utan fel
- ✅ Applikationen startar korrekt
- ✅ Alla seed data (inklusive 5 nya goals) sparas framgångsrikt
- ✅ In-memory databas fungerar med alla nya modeller

## Framtida förbättringar
Baserat på förslaget i issuen finns det ytterligare funktionalitet som kan implementeras:
1. **User-modell**: Multi-user support med autentisering
2. **Assets/Liabilities**: Mer omfattande modell för tillgångar och skulder
3. **Recurring Transactions**: Implementera återkommande transaktioner
4. **Budgetering per period**: BudgetPeriod-modell för mer flexibla budgetperioder
5. **Import history audit**: Spåra all import-historik för revision

## Sammanfattning
Implementationen har framgångsrikt förbättrat datamodellerna enligt förslaget i issuen med fokus på:
- **Sparmål**: Ny Goal-modell för att kunna följa sparframsteg
- **Utökad Transaction**: Stöd för valuta, import-spårning, och clearing-status
- **Hierarkiska kategorier**: Möjlighet att organisera kategorier i träd-struktur
- **Förbättrade konton**: BankSource uppgraderad till fullständigt konto med balans-spårning
- **Audit trail**: Alla modeller har nu CreatedAt/UpdatedAt för spårbarhet
- **Prestanda**: Index tillagda för vanliga queries

Alla ändringar är testade och backward compatible.
