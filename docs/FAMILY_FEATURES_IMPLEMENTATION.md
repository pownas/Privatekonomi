# Teknisk dokumentation: Familjesamarbetsfunktioner

Detta dokument beskriver den tekniska implementationen av familjesamarbetsfunktionerna i Privatekonomi.

## Översikt

Familjesamarbetsfunktionerna introducerar följande nya komponenter:
1. Barnkonton med veckopeng
2. Uppdrag-till-belöning system
3. Gemensamma hushållsbudgetar

## Datamodeller

### ChildAllowance

Huvudmodell för barnens allowance-konton.

```csharp
public class ChildAllowance
{
    public int ChildAllowanceId { get; set; }
    public int HouseholdMemberId { get; set; }
    public string Name { get; set; }
    public AllowanceFrequency Frequency { get; set; }
    public decimal Amount { get; set; }
    public decimal CurrentBalance { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public HouseholdMember? HouseholdMember { get; set; }
    public ICollection<AllowanceTransaction> AllowanceTransactions { get; set; }
    public ICollection<AllowanceTask> AllowanceTasks { get; set; }
}

public enum AllowanceFrequency
{
    Weekly,     // Varje vecka
    BiWeekly,   // Varannan vecka
    Monthly     // Månatligen
}
```

**Viktiga fält:**
- `CurrentBalance`: Automatiskt beräknat vid varje transaktion
- `Frequency`: Bestämmer hur ofta veckopeng ska betalas
- `IsActive`: Styr om kontot är aktivt

### AllowanceTransaction

Transaktionshistorik för barnkontona.

```csharp
public class AllowanceTransaction
{
    public int AllowanceTransactionId { get; set; }
    public int ChildAllowanceId { get; set; }
    public int? AllowanceTaskId { get; set; }
    public decimal Amount { get; set; }
    public AllowanceTransactionType Type { get; set; }
    public string Description { get; set; }
    public DateTime TransactionDate { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public ChildAllowance? ChildAllowance { get; set; }
    public AllowanceTask? AllowanceTask { get; set; }
}

public enum AllowanceTransactionType
{
    Deposit,        // Insättning (veckopeng/månadspeng)
    TaskReward,     // Belöning för uppgift
    Withdrawal,     // Uttag
    Adjustment      // Justering
}
```

**Transaktionstyper:**
- `Deposit`: Vanlig insättning eller schemalagd veckopeng
- `TaskReward`: Automatisk belöning när uppdrag godkänns
- `Withdrawal`: Uttag från kontot
- `Adjustment`: Manuell justering (kan vara positiv eller negativ)

### AllowanceTask

Uppdrag/sysslor som barn kan utföra för belöning.

```csharp
public class AllowanceTask
{
    public int AllowanceTaskId { get; set; }
    public int ChildAllowanceId { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public decimal RewardAmount { get; set; }
    public AllowanceTaskStatus Status { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public ChildAllowance? ChildAllowance { get; set; }
    public ICollection<AllowanceTransaction> AllowanceTransactions { get; set; }
}

public enum AllowanceTaskStatus
{
    Pending,        // Väntande
    InProgress,     // Pågående
    Completed,      // Klar (väntar på godkännande)
    Approved,       // Godkänd (belöning utbetald)
    Rejected        // Avvisad
}
```

**Uppdragsflöde:**
1. `Pending` → Nytt uppdrag skapas
2. `InProgress` → Barnet har påbörjat uppdraget (valfri)
3. `Completed` → Barnet markerar uppdraget som klart
4. `Approved` → Förälder godkänner, belöning betalas automatiskt
5. `Rejected` → Förälder avvisar, ingen betalning

### Budget (uppdaterad)

Befintlig Budget-modell utökad med hushållslänkning.

```csharp
public class Budget
{
    // ... existing fields
    public int? HouseholdId { get; set; }  // NY: Valfri länk till hushåll
    
    // Navigation properties
    public Household? Household { get; set; }  // NY
    public ICollection<BudgetCategory> BudgetCategories { get; set; }
}
```

## Databaskonfiguration

### Entity Framework Context

Nya DbSets tillagda i `PrivatekonomyContext`:

```csharp
public DbSet<ChildAllowance> ChildAllowances { get; set; }
public DbSet<AllowanceTransaction> AllowanceTransactions { get; set; }
public DbSet<AllowanceTask> AllowanceTasks { get; set; }
```

### Konfiguration

```csharp
// Child Allowance configuration
modelBuilder.Entity<ChildAllowance>(entity =>
{
    entity.HasKey(e => e.ChildAllowanceId);
    entity.Property(e => e.Amount).HasPrecision(18, 2);
    entity.Property(e => e.CurrentBalance).HasPrecision(18, 2);
    
    entity.HasOne(e => e.HouseholdMember)
        .WithMany()
        .HasForeignKey(e => e.HouseholdMemberId)
        .OnDelete(DeleteBehavior.Cascade);
});

modelBuilder.Entity<AllowanceTransaction>(entity =>
{
    entity.HasKey(e => e.AllowanceTransactionId);
    entity.Property(e => e.Amount).HasPrecision(18, 2);
    
    entity.HasOne(e => e.ChildAllowance)
        .WithMany(ca => ca.AllowanceTransactions)
        .HasForeignKey(e => e.ChildAllowanceId)
        .OnDelete(DeleteBehavior.Cascade);
    
    entity.HasOne(e => e.AllowanceTask)
        .WithMany(at => at.AllowanceTransactions)
        .HasForeignKey(e => e.AllowanceTaskId)
        .OnDelete(DeleteBehavior.SetNull);
});

modelBuilder.Entity<AllowanceTask>(entity =>
{
    entity.HasKey(e => e.AllowanceTaskId);
    entity.Property(e => e.RewardAmount).HasPrecision(18, 2);
    
    entity.HasOne(e => e.ChildAllowance)
        .WithMany(ca => ca.AllowanceTasks)
        .HasForeignKey(e => e.ChildAllowanceId)
        .OnDelete(DeleteBehavior.Cascade);
});

// Budget household linking
modelBuilder.Entity<Budget>(entity =>
{
    // ... existing configuration
    
    entity.HasOne(e => e.Household)
        .WithMany()
        .HasForeignKey(e => e.HouseholdId)
        .OnDelete(DeleteBehavior.SetNull);
});
```

**Viktiga konfigurationer:**
- `Precision(18, 2)`: För alla monetära värden
- `Cascade Delete`: När allowance raderas, raderas alla transaktioner och uppdrag
- `SetNull`: När uppdrag raderas, behålls transaktionen men länken sätts till null

## Service Layer

### IChildAllowanceService

Interface med 16 metoder för fullständig hantering av barnkonton.

```csharp
public interface IChildAllowanceService
{
    // Child Allowance operations
    Task<IEnumerable<ChildAllowance>> GetAllAllowancesAsync(int householdId);
    Task<ChildAllowance?> GetAllowanceByIdAsync(int allowanceId);
    Task<ChildAllowance> CreateAllowanceAsync(ChildAllowance allowance);
    Task<ChildAllowance> UpdateAllowanceAsync(ChildAllowance allowance);
    Task<bool> DeleteAllowanceAsync(int allowanceId);
    
    // Transaction operations
    Task<AllowanceTransaction> AddTransactionAsync(AllowanceTransaction transaction);
    Task<IEnumerable<AllowanceTransaction>> GetAllowanceTransactionsAsync(int allowanceId);
    Task<AllowanceTransaction> ProcessScheduledAllowanceAsync(int allowanceId);
    
    // Task operations
    Task<AllowanceTask> CreateTaskAsync(AllowanceTask task);
    Task<AllowanceTask> UpdateTaskAsync(AllowanceTask task);
    Task<bool> DeleteTaskAsync(int taskId);
    Task<AllowanceTask?> GetTaskByIdAsync(int taskId);
    Task<IEnumerable<AllowanceTask>> GetAllowanceTasksAsync(int allowanceId, AllowanceTaskStatus? status = null);
    Task<AllowanceTask> CompleteTaskAsync(int taskId);
    Task<AllowanceTask> ApproveTaskAsync(int taskId, string approvedBy);
    Task<AllowanceTask> RejectTaskAsync(int taskId);
}
```

### ChildAllowanceService

Implementering med automatisk balansberäkning och uppdragshantering.

**Viktiga metoder:**

#### AddTransactionAsync
```csharp
public async Task<AllowanceTransaction> AddTransactionAsync(AllowanceTransaction transaction)
{
    transaction.CreatedAt = DateTime.Now;
    _context.AllowanceTransactions.Add(transaction);
    
    // Automatisk balansuppdatering
    var allowance = await _context.ChildAllowances.FindAsync(transaction.ChildAllowanceId);
    if (allowance != null)
    {
        if (transaction.Type == AllowanceTransactionType.Deposit || 
            transaction.Type == AllowanceTransactionType.TaskReward)
        {
            allowance.CurrentBalance += transaction.Amount;
        }
        else if (transaction.Type == AllowanceTransactionType.Withdrawal)
        {
            allowance.CurrentBalance -= transaction.Amount;
        }
        else if (transaction.Type == AllowanceTransactionType.Adjustment)
        {
            allowance.CurrentBalance += transaction.Amount; // Kan vara negativ
        }
        
        allowance.UpdatedAt = DateTime.Now;
    }
    
    await _context.SaveChangesAsync();
    return transaction;
}
```

#### ApproveTaskAsync
```csharp
public async Task<AllowanceTask> ApproveTaskAsync(int taskId, string approvedBy)
{
    var task = await _context.AllowanceTasks
        .Include(at => at.ChildAllowance)
        .FirstOrDefaultAsync(at => at.AllowanceTaskId == taskId);
        
    if (task == null)
        throw new InvalidOperationException("Task not found");

    if (task.Status != AllowanceTaskStatus.Completed)
        throw new InvalidOperationException("Task must be completed before approval");

    task.Status = AllowanceTaskStatus.Approved;
    task.ApprovedDate = DateTime.Now;
    task.ApprovedBy = approvedBy;

    // Automatisk belöning
    var transaction = new AllowanceTransaction
    {
        ChildAllowanceId = task.ChildAllowanceId,
        AllowanceTaskId = taskId,
        Amount = task.RewardAmount,
        Type = AllowanceTransactionType.TaskReward,
        Description = $"Reward for task: {task.Name}",
        TransactionDate = DateTime.Now,
        CreatedAt = DateTime.Now
    };

    await AddTransactionAsync(transaction);
    
    return task;
}
```

## UI-komponenter

### ChildAllowances.razor

Huvudkomponent för barnkonton (21+ kb).

**Struktur:**
- **3 flikar:**
  1. Konton - Översikt av alla barnkonton
  2. Uppdrag - Hantera uppdrag per barn
  3. Transaktioner - Se och skapa transaktioner

**Dialog-komponenter:**
- CreateAllowanceDialog - Skapa nytt barnkonto
- CreateTaskDialog - Lägg till uppdrag
- TransactionDialog - Insättning/uttag

**Viktiga funktioner:**
- Breadcrumb-navigering
- Card-baserad layout för konton
- Tabellvy för uppdrag och transaktioner
- Färgkodade status-chips
- Interaktiva knappar för uppdragshantering

### HouseholdDetails.razor (uppdaterad)

Lagt till knapp för navigation till barnkonton:

```razor
<MudButton Variant="Variant.Filled" 
           Color="Color.Secondary" 
           StartIcon="@Icons.Material.Filled.ChildCare" 
           OnClick="@(() => NavigationManager.NavigateTo($"/households/{HouseholdId}/allowances"))" 
           Class="mb-4">
    Barnkonton & Veckopeng
</MudButton>
```

### Budgets.razor (uppdaterad)

Lagt till household-dropdown:

```razor
<MudItem xs="12" md="6">
    <MudSelect @bind-Value="_newBudget.HouseholdId" 
              Label="Hushåll (valfritt)" 
              Variant="Variant.Outlined"
              Clearable="true"
              Class="mb-3">
        @foreach (var household in _households)
        {
            <MudSelectItem Value="@((int?)household.HouseholdId)">@household.Name</MudSelectItem>
        }
    </MudSelect>
</MudItem>
```

## Registrering av services

### Program.cs (Web)

```csharp
builder.Services.AddScoped<IChildAllowanceService, ChildAllowanceService>();
```

### Program.cs (Api)

```csharp
builder.Services.AddScoped<IHouseholdService, HouseholdService>();
builder.Services.AddScoped<IChildAllowanceService, ChildAllowanceService>();
```

## Routing

Nya routes:
- `/households/{HouseholdId:int}/allowances` - Barnkonton och veckopeng

Befintliga routes som använder funktionerna:
- `/households` - Lista hushåll
- `/households/{HouseholdId:int}` - Hushållsdetaljer
- `/budgets` - Budgethantering (nu med household-support)

## Säkerhetsaspekter

### Implementerade skydd

1. **Valideringar:**
   - Belopp måste vara större än 0
   - Namn och beskrivningar har maxlängder
   - Datum valideras

2. **Cascade deletes:**
   - När allowance raderas, raderas alla associerade transaktioner och uppdrag
   - Förhindrar orphaned records

3. **Status-validering:**
   - Uppdrag kan endast godkännas om status är "Completed"
   - Förhindrar dubbel utbetalning

### Framtida förbättringar

1. **Autentisering:**
   - Koppla användare till hushåll
   - Kontrollera behörighet för åtgärder

2. **Audit trail:**
   - Logga alla ändringar av allowances
   - Spåra vem som godkände uppdrag

3. **Limits:**
   - Maxbelopp per transaktion
   - Maxbelopp per uppdrag
   - Åldersbaserade restriktioner

## Prestandaoptimering

### Implementerade optimeringar

1. **Include-statements:**
   - Eager loading av relaterade entiteter
   - Minskar antal databasanrop

2. **Indexering:**
   - Foreign keys har automatiska index
   - Sökningar på ChildAllowanceId är snabba

3. **Minimal data fetching:**
   - Endast nödvändiga fält hämtas
   - ToList() anropas endast när data ska itereras

### Framtida optimeringar

1. **Caching:**
   - Cache household-data
   - Cache kategorilistor

2. **Paging:**
   - Implementera paginering för transaktionshistorik
   - Lazy loading av gamla uppdrag

3. **Batch operations:**
   - Bulkskapa uppdrag
   - Batch-godkännande av uppdrag

## Testning

### Manuella tester

1. **Skapa barnkonto:**
   - Verifiera att saldo initieras till 0
   - Kontrollera att veckopeng sparas korrekt

2. **Transaktioner:**
   - Insättning ökar saldo
   - Uttag minskar saldo
   - Veckopeng skapar korrekt transaktion

3. **Uppdragsflöde:**
   - Skapa → Slutför → Godkänn
   - Verifiera att belöning läggs till automatiskt
   - Kontrollera att transaktionen länkas till uppdraget

### Enhetstest (framtida)

```csharp
[Test]
public async Task ApproveTask_ShouldCreateTransaction()
{
    // Arrange
    var service = new ChildAllowanceService(context);
    var task = await service.CreateTaskAsync(new AllowanceTask { ... });
    await service.CompleteTaskAsync(task.AllowanceTaskId);
    
    // Act
    await service.ApproveTaskAsync(task.AllowanceTaskId, "Parent");
    
    // Assert
    var transactions = await service.GetAllowanceTransactionsAsync(task.ChildAllowanceId);
    Assert.That(transactions.Count(), Is.EqualTo(1));
    Assert.That(transactions.First().Type, Is.EqualTo(AllowanceTransactionType.TaskReward));
}
```

## Felsökning

### Vanliga problem

**Problem:** Saldo uppdateras inte vid transaktion
**Lösning:** Kontrollera att `AddTransactionAsync` anropas, inte direkt `_context.Add()`

**Problem:** Uppdrag kan inte godkännas
**Lösning:** Verifiera att status är "Completed" först

**Problem:** Foreign key constraint error vid radering
**Lösning:** Kontrollera cascade delete-konfiguration i OnModelCreating

### Logging

För debugging, lägg till logging i service-metoder:

```csharp
_logger.LogInformation("Creating allowance for member {MemberId}", allowance.HouseholdMemberId);
_logger.LogWarning("Attempting to approve task {TaskId} with status {Status}", taskId, task.Status);
```

## Framtida utveckling

### Planerade funktioner

1. **Återkommande uppdrag:**
   - Schemalägg uppdrag att skapas automatiskt
   - T.ex. "Diska varje dag"

2. **Notifikationer:**
   - E-post när uppdrag godkänns
   - Push-notis när veckopeng betalas

3. **Sparmål för barn:**
   - Visuella målmätare
   - Automatisk sparregel (t.ex. "Spara 20% av veckopeng")

4. **Rapporter:**
   - Månatlig sammanfattning per barn
   - Exportera till PDF
   - Graf över balans över tid

5. **Mobil-optimering:**
   - PWA för barn att se sina konton
   - Enklare gränssnitt för yngre barn

## API Endpoints (framtida)

Om API-endpoints ska exponeras:

```
GET    /api/households/{id}/allowances
POST   /api/allowances
GET    /api/allowances/{id}
PUT    /api/allowances/{id}
DELETE /api/allowances/{id}

POST   /api/allowances/{id}/transactions
GET    /api/allowances/{id}/transactions

POST   /api/allowances/{id}/tasks
GET    /api/tasks/{id}
PUT    /api/tasks/{id}
POST   /api/tasks/{id}/complete
POST   /api/tasks/{id}/approve
POST   /api/tasks/{id}/reject
```

## Bidrag

Vid bidrag till familjesamarbetsfunktionerna:
1. Följ befintlig kodstil
2. Lägg till kommentarer för komplex logik
3. Uppdatera denna dokumentation
4. Skriv enhetstest för ny funktionalitet
5. Testa manuellt alla flöden

## Licens

Samma licens som huvudprojektet.
