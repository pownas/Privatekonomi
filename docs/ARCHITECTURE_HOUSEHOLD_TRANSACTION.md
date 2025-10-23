# Architecture Diagram - Household Transaction Linkage

## System Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                         User Interface Layer                     │
├─────────────────────────────────────────────────────────────────┤
│                                                                   │
│  ┌──────────────────┐              ┌───────────────────────┐    │
│  │ Transactions.razor│              │EditTransactionDialog │    │
│  │                   │              │      .razor          │    │
│  │ • List view       │              │                      │    │
│  │ • Household column│              │ • Household selector │    │
│  │ • Filter dropdown │◄─────────────┤ • Clearable option   │    │
│  │ • Search          │              │ • Save to service    │    │
│  └──────────────────┘              └───────────────────────┘    │
│           │                                   │                  │
└───────────┼───────────────────────────────────┼──────────────────┘
            │                                   │
            ▼                                   ▼
┌─────────────────────────────────────────────────────────────────┐
│                      Service Layer (Blazor)                      │
├─────────────────────────────────────────────────────────────────┤
│                                                                   │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │            ITransactionService                           │   │
│  │ • GetAllTransactionsAsync()                              │   │
│  │ • GetTransactionsByHouseholdAsync(householdId)           │   │
│  │ • GetTransactionsByHouseholdAndDateRangeAsync(...)       │   │
│  │ • UpdateTransactionAsync(transaction)                    │   │
│  └──────────────────────────────────────────────────────────┘   │
│           │                                                      │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │            IHouseholdService                              │   │
│  │ • GetAllHouseholdsAsync()                                 │   │
│  └──────────────────────────────────────────────────────────┘   │
│           │                                                      │
└───────────┼──────────────────────────────────────────────────────┘
            │
            ▼
┌─────────────────────────────────────────────────────────────────┐
│                         API Layer (Optional)                     │
├─────────────────────────────────────────────────────────────────┤
│                                                                   │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │         TransactionsController                            │   │
│  │                                                           │   │
│  │ GET  /api/transactions?household_id={id}                  │   │
│  │ GET  /api/transactions/household/{id}                     │   │
│  │ GET  /api/transactions/household/{id}/date-range          │   │
│  │ POST /api/transactions                                    │   │
│  │ PUT  /api/transactions/{id}                               │   │
│  └──────────────────────────────────────────────────────────┘   │
│           │                                                      │
└───────────┼──────────────────────────────────────────────────────┘
            │
            ▼
┌─────────────────────────────────────────────────────────────────┐
│                      Business Logic Layer                        │
├─────────────────────────────────────────────────────────────────┤
│                                                                   │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │          TransactionService                               │   │
│  │ • User authentication checks                              │   │
│  │ • Query with Include(Household)                           │   │
│  │ • Household filtering logic                               │   │
│  └──────────────────────────────────────────────────────────┘   │
│           │                                                      │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │          ReportService                                    │   │
│  │ • GetCashFlowReportAsync(..., householdId?)               │   │
│  │ • GetTrendAnalysisAsync(..., householdId?)                │   │
│  │ • GetTopMerchantsAsync(..., householdId?)                 │   │
│  └──────────────────────────────────────────────────────────┘   │
│           │                                                      │
└───────────┼──────────────────────────────────────────────────────┘
            │
            ▼
┌─────────────────────────────────────────────────────────────────┐
│                       Data Access Layer                          │
├─────────────────────────────────────────────────────────────────┤
│                                                                   │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │         PrivatekonomyContext (EF Core)                    │   │
│  │                                                           │   │
│  │ • DbSet<Transaction>                                      │   │
│  │ • DbSet<Household>                                        │   │
│  │ • Relationship configuration                              │   │
│  │ • Index definitions                                       │   │
│  └──────────────────────────────────────────────────────────┘   │
│           │                                                      │
└───────────┼──────────────────────────────────────────────────────┘
            │
            ▼
┌─────────────────────────────────────────────────────────────────┐
│                         Database Layer                           │
├─────────────────────────────────────────────────────────────────┤
│                                                                   │
│  ┌────────────────────────┐      ┌──────────────────────────┐   │
│  │   Transactions Table   │      │   Households Table       │   │
│  │                        │      │                          │   │
│  │ TransactionId (PK)     │      │ HouseholdId (PK)         │   │
│  │ Description            │      │ Name                     │   │
│  │ Amount                 │      │ Description              │   │
│  │ Date                   │      │ CreatedDate              │   │
│  │ HouseholdId (FK, NULL) ├─────►│ ...                      │   │
│  │ ...                    │      │                          │   │
│  │                        │      │                          │   │
│  │ Indexes:               │      └──────────────────────────┘   │
│  │ • IX_HouseholdId       │                                     │
│  │ • IX_HouseholdId_Date  │                                     │
│  └────────────────────────┘                                     │
│                                                                   │
└───────────────────────────────────────────────────────────────────┘
```

## Data Flow Diagram

### Transaction Creation/Update Flow
```
User Action
    │
    ├──► Opens Edit Dialog
    │        │
    │        ├──► Load Households (IHouseholdService)
    │        │
    │        ├──► User Selects Household (or leaves blank)
    │        │
    │        └──► Save Transaction
    │                  │
    │                  ├──► Validate Data
    │                  │
    │                  ├──► Set HouseholdId (or NULL)
    │                  │
    │                  ├──► Call TransactionService.UpdateTransactionAsync()
    │                  │
    │                  ├──► EF Core updates database
    │                  │
    │                  └──► Return success
    │
    └──► Refresh Transaction List
             │
             └──► Load with Household data included
```

### Transaction Filtering Flow
```
User Action
    │
    ├──► Select Household from Filter
    │        │
    │        └──► Client-side filtering (FilterFunc)
    │                  │
    │                  ├──► Check _selectedHouseholdId
    │                  │
    │                  ├──► Filter transactions where
    │                  │    HouseholdId == selected
    │                  │
    │                  └──► Display filtered results
    │
    └──► OR Use API Endpoint
             │
             └──► GET /api/transactions?household_id={id}
                      │
                      ├──► Server-side filtering
                      │
                      └──► Return filtered JSON
```

## Component Interaction

```
┌──────────────────────────────────────────────────────────────┐
│                    User Interface                            │
│                                                              │
│  ┌────────────────────┐       ┌───────────────────────┐    │
│  │ Transaction List   │       │ Edit Dialog           │    │
│  │                    │       │                       │    │
│  │ [Filter: Household▼]◄──────┤ [Household: Select ▼] │    │
│  │                    │       │ [Description: ____]   │    │
│  │ ┌────────────────┐ │       │ [Amount: _____]       │    │
│  │ │Date│Desc │House││       │ [Save] [Cancel]       │    │
│  │ │────┼─────┼─────││       └───────────────────────┘    │
│  │ │01..│ICA  │🏠Fam││              ▲                      │
│  │ │02..│SEB  │Pers ││              │                      │
│  │ └────────────────┘ │              │                      │
│  └────────────────────┘              │                      │
│           │                          │                      │
└───────────┼──────────────────────────┼──────────────────────┘
            │                          │
            ▼                          ▼
    [TransactionService]    [HouseholdService]
            │                          │
            └──────────┬───────────────┘
                       ▼
              [PrivatekonomyContext]
                       │
                       ▼
                  [Database]
```

## Key Design Decisions

### 1. Nullable Foreign Key
```
Transaction.HouseholdId: int?
                         ^^^^
                         Nullable to support personal transactions
```

### 2. Delete Behavior
```
OnDelete: SetNull
          ^^^^^^^^
          Preserves transaction history when household is deleted
```

### 3. Indexing Strategy
```
Indexes:
├── Single: HouseholdId (for filtering)
└── Composite: (HouseholdId, Date) (for date-range queries)
```

### 4. User Isolation
```
All queries include:
Where(t => t.UserId == _currentUserService.UserId)
```

## Feature Usage Examples

### Example 1: Personal Transaction
```
Transaction {
    Description: "Personal coffee",
    Amount: 45,
    HouseholdId: null  ← No household
}

Display: "Personlig"
```

### Example 2: Household Transaction
```
Transaction {
    Description: "Grocery shopping",
    Amount: 500,
    HouseholdId: 1  ← Linked to "Vår familj"
}

Display: "🏠 Vår familj"
```

### Example 3: Filtering
```
User selects: "Vår familj" from filter

Result: Shows only transactions where
        HouseholdId == 1

Statistics:
- Total household expenses: 12,450 kr
- Number of transactions: 34
```
