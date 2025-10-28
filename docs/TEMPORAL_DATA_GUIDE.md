# Temporal Data Feature - Historical Economy View

## Overview

The temporal data feature enables viewing economic data as it existed at any point in time. This is achieved through bi-temporal tracking where each record has:
- `ValidFrom`: When this version of the record became active
- `ValidTo`: When this version became inactive (null = current/active version)

## Key Features

### 1. Historical Queries
Query data as it existed at any specific date:

```csharp
// Get all transactions as they existed on January 1, 2023
var historicalTransactions = await _context.Transactions
    .AsOf(new DateTime(2023, 1, 1))
    .Where(t => t.UserId == userId)
    .ToListAsync();

// Get current/active transactions only
var currentTransactions = await _context.Transactions
    .CurrentOnly()
    .Where(t => t.UserId == userId)
    .ToListAsync();

// Same as AsOf(null)
var currentTransactions2 = await _context.Transactions
    .AsOf(null)
    .Where(t => t.UserId == userId)
    .ToListAsync();
```

### 2. Creating Temporal Entities
When creating new records, set ValidFrom and ValidTo:

```csharp
var transaction = new Transaction
{
    Amount = 100m,
    Description = "Purchase",
    Date = DateTime.UtcNow,
    IsIncome = false,
    Currency = "SEK",
    ValidFrom = DateTime.UtcNow,  // Required
    ValidTo = null,                // null = active version
    CreatedAt = DateTime.UtcNow,
    UserId = userId
};

// Or use the service
var service = new TemporalEntityService(_context);
var created = await service.CreateTemporalEntityAsync(transaction);
```

### 3. Updating Temporal Entities
When updating records, close the old version and create a new one:

```csharp
// Find current version
var current = await _context.Transactions
    .CurrentOnly()
    .FirstOrDefaultAsync(t => t.TransactionId == id);

if (current != null)
{
    // Create new version with updated values
    var updated = new Transaction
    {
        Amount = 150m,  // New value
        Description = current.Description,
        Date = current.Date,
        IsIncome = current.IsIncome,
        Currency = current.Currency,
        UserId = current.UserId
        // Other fields copied from current
    };

    var service = new TemporalEntityService(_context);
    await service.UpdateTemporalEntityAsync(current, updated);
}
```

### 4. Deleting Temporal Entities
Soft delete by setting ValidTo (preserves history):

```csharp
var current = await _context.Transactions
    .CurrentOnly()
    .FirstOrDefaultAsync(t => t.TransactionId == id);

if (current != null)
{
    var service = new TemporalEntityService(_context);
    await service.DeleteTemporalEntityAsync(current);
    // Record is now "closed" but still in database for historical queries
}
```

### 5. Viewing History
Get all versions of a record:

```csharp
var service = new TemporalEntityService(_context);
var allVersions = await service.GetAllVersionsAsync<Transaction>(
    t => t.TransactionId == id || t.Description == "Original Description"
);

// Display timeline
foreach (var version in allVersions)
{
    Console.WriteLine($"{version.ValidFrom:yyyy-MM-dd} - {version.ValidTo?.ToString("yyyy-MM-dd") ?? "Current"}: {version.Amount}");
}
```

## Example: Time Travel Dashboard

```csharp
public async Task<DashboardViewModel> GetDashboardAsync(string userId, DateTime? asOfDate = null)
{
    // Get data as it existed at the specified date (or current if null)
    var transactions = await _context.Transactions
        .AsOf(asOfDate)
        .Where(t => t.UserId == userId)
        .ToListAsync();

    var assets = await _context.Assets
        .AsOf(asOfDate)
        .Where(a => a.UserId == userId)
        .ToListAsync();

    var loans = await _context.Loans
        .AsOf(asOfDate)
        .Where(l => l.UserId == userId)
        .ToListAsync();

    return new DashboardViewModel
    {
        AsOfDate = asOfDate ?? DateTime.UtcNow,
        TotalIncome = transactions.Where(t => t.IsIncome).Sum(t => t.Amount),
        TotalExpenses = transactions.Where(t => !t.IsIncome).Sum(t => t.Amount),
        TotalAssets = assets.Sum(a => a.CurrentValue),
        TotalLiabilities = loans.Sum(l => l.Amount),
        // ... etc
    };
}
```

## Entities with Temporal Tracking

The following entities support temporal tracking:
- `Transaction`
- `Asset`
- `Loan`
- `Investment`
- `BankSource`
- `Budget`
- `Goal`
- `Pocket`

## Database Schema

Each temporal entity has the following fields added:

```sql
ValidFrom DATETIME2 NOT NULL
ValidTo DATETIME2 NULL

INDEX IX_Entity_ValidFrom (ValidFrom)
INDEX IX_Entity_ValidTo (ValidTo)
INDEX IX_Entity_ValidFrom_ValidTo (ValidFrom, ValidTo)
```

## Extension Methods

### AsOf<T>(DateTime? asOfDate)
Filters temporal entities to show only records valid at the specified date.

### CurrentOnly<T>()
Filters temporal entities to show only current/active records (ValidTo = null).

### IsActive()
Checks if an individual entity is currently active.

### IsActiveAt(DateTime asOfDate)
Checks if an individual entity was active at a specific date.

## Service Methods

### CreateTemporalEntityAsync<T>
Creates a new temporal entity with ValidFrom set to now (or specified date) and ValidTo set to null.

### UpdateTemporalEntityAsync<T>
Updates a temporal entity by closing the current version and creating a new one.

### DeleteTemporalEntityAsync<T>
Soft deletes a temporal entity by setting ValidTo to now (or specified date).

### GetCurrentVersionAsync<T>
Gets the current/active version of an entity.

### GetVersionAtDateAsync<T>
Gets the version of an entity that was valid at a specific date.

### GetAllVersionsAsync<T>
Gets all versions of an entity ordered by ValidFrom.

## UI Integration Example

```razor
@page "/dashboard"
@inject ITransactionService TransactionService

<MudContainer>
    <MudDatePicker Label="View data as of date" 
                   @bind-Date="selectedDate" 
                   Clearable="true"
                   OnChange="LoadData" />
    
    @if (selectedDate.HasValue)
    {
        <MudAlert Severity="Severity.Info">
            Showing data as it was on @selectedDate.Value.ToString("yyyy-MM-dd")
        </MudAlert>
    }
    
    <!-- Display dashboard data -->
    <MudCard>
        <MudCardContent>
            <MudText>Total Income: @dashboard.TotalIncome SEK</MudText>
            <MudText>Total Expenses: @dashboard.TotalExpenses SEK</MudText>
        </MudCardContent>
    </MudCard>
</MudContainer>

@code {
    private DateTime? selectedDate;
    private DashboardViewModel dashboard = new();

    protected override async Task OnInitializedAsync()
    {
        await LoadData();
    }

    private async Task LoadData()
    {
        dashboard = await TransactionService.GetDashboardAsync(UserId, selectedDate);
    }
}
```

## Best Practices

1. **Always use AsOf() or CurrentOnly()** when querying temporal entities to avoid confusion.
2. **Never hard delete** temporal entities - always use soft delete to preserve history.
3. **Use TemporalEntityService** for create/update/delete operations to ensure consistency.
4. **Index optimization**: The database includes indexes on (ValidFrom, ValidTo) for efficient queries.
5. **Date handling**: Always use UTC dates for temporal tracking to avoid timezone issues.
6. **Testing**: Always test historical queries with known dates to verify correctness.

## Migration Considerations

Existing data will need to be migrated to have ValidFrom and ValidTo fields:
- Set ValidFrom to CreatedAt for all existing records
- Set ValidTo to null for all existing records (they are all current/active)
- The migration will be created automatically when you run `dotnet ef migrations add AddTemporalTracking`

## Performance Notes

- Temporal queries are optimized with composite indexes on (ValidFrom, ValidTo)
- For large datasets, consider partitioning by date range
- Historical queries may be slightly slower than current-only queries due to date comparisons
- Consider adding caching for frequently accessed historical data
