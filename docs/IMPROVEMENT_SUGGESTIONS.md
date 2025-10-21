# Förbättringsförslag för Privatekonomi

**Datum:** 2025-10-20  
**Analys av:** Privatekonomi repository (pownas/Privatekonomi)  
**Version:** .NET 9, Blazor Server, MudBlazor

---

## Sammanfattning

Detta dokument innehåller en omfattande analys av Privatekonomi-projektet med konkreta förbättringsförslag. Projektet är välstrukturerat med bra dokumentation och modern teknologi (.NET 9, Aspire, Blazor), men det finns flera områden där kodkvalitet, säkerhet, testning och underhållbarhet kan förbättras.

**Projektstatistik:**
- 55 C#-filer (~5,036 rader kod)
- 15 Razor-komponenter (~4,097 rader)
- 5 huvudprojekt (AppHost, ServiceDefaults, Web, Api, Core)
- 0 enhetstester för närvarande
- 4 kompileringsvarningar

---

## Prioritetsnivåer

- 🔴 **Kritisk** - Måste åtgärdas omedelbart (säkerhet, stabilitet)
- 🟠 **Hög** - Bör åtgärdas snart (kodkvalitet, underhållbarhet)
- 🟡 **Medel** - Förbättringar som ger värde (funktionalitet, användarupplevelse)
- 🟢 **Låg** - Valfria förbättringar (optimering, nice-to-have)

---

## 1. Säkerhet & Datahantering

### 🔴 1.1 In-Memory Databas i Produktion
**Problem:** Projektet använder Entity Framework Core InMemory-databas som inte är persistent.

**Konsekvenser:**
- All data försvinner vid omstart
- Ej lämpligt för produktion
- Risk för dataförlust

**Lösning:**
```csharp
// I Program.cs (Web & Api), ersätt:
builder.Services.AddDbContext<PrivatekonomyContext>(options =>
    options.UseInMemoryDatabase("PrivatekonomyDb"));

// Med:
builder.Services.AddDbContext<PrivatekonomyContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure()
    ));
```

**Åtgärder:**
1. Lägg till `Microsoft.EntityFrameworkCore.SqlServer` NuGet-paket
2. Konfigurera connection string i `appsettings.json`
3. Skapa migrations: `dotnet ef migrations add InitialCreate`
4. Implementera databas-seeding för produktion
5. Uppdatera dokumentation med databas-setup

**Berörd fil:** `src/Privatekonomi.Web/Program.cs`, `src/Privatekonomi.Api/Program.cs`

### 🟠 1.2 Saknar Användare och Autentisering
**Problem:** Applikationen har ingen användarhantering eller autentisering.

**Konsekvenser:**
- Alla användare ser samma data
- Ingen åtkomstkontroll
- Ej lämpligt för multi-user scenarios

**Lösning:**
1. Implementera ASP.NET Core Identity
2. Lägg till användarregistrering och inloggning
3. Koppla transaktioner, budgetar etc. till användare
4. Implementera roller (Admin, User)

**Föreslagna steg:**
```bash
# Installera Identity-paket
dotnet add src/Privatekonomi.Core/Privatekonomi.Core.csproj package Microsoft.AspNetCore.Identity.EntityFrameworkCore
```

```csharp
// Uppdatera PrivatekonomyContext
public class PrivatekonomyContext : IdentityDbContext<ApplicationUser>
{
    // ... befintlig kod
}

// Lägg till UserId i modeller
public class Transaction
{
    // ... befintliga properties
    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }
}
```

**Berörd fil:** `src/Privatekonomi.Core/Data/PrivatekonomyContext.cs`, alla modeller

### 🟠 1.3 Ingen Validering av API-input
**Problem:** API-controllers saknar omfattande input-validering.

**Konsekvenser:**
- Risk för injektionsattacker
- Ogiltig data kan sparas
- Potentiella säkerhetshål

**Lösning:**
```csharp
// I controllers, lägg till Data Annotations och validering
[HttpPost]
public async Task<ActionResult<Transaction>> CreateTransaction([FromBody] TransactionDto dto)
{
    if (!ModelState.IsValid)
        return BadRequest(ModelState);
    
    // Validera affärsregler
    if (dto.Amount == 0)
        return BadRequest("Amount cannot be zero");
    
    // ...
}
```

**Åtgärder:**
1. Lägg till FluentValidation eller Data Annotations på alla DTOs
2. Implementera custom validators för affärsregler
3. Lägg till global exception handler
4. Validera alla externa inputs (CSV, API calls)

**Berörd fil:** `src/Privatekonomi.Api/Controllers/*.cs`

### 🟡 1.4 Hårdkodade Seed-data
**Problem:** Bank sources och kategorier är hårdkodade i `OnModelCreating`.

**Konsekvenser:**
- Svårt att ändra initiala värden
- Migrations kan bli problematiska
- Begränsar flexibilitet

**Lösning:**
```csharp
// Flytta seed-data till separat konfigurationsfil
public class SeedDataConfiguration
{
    public List<BankSource> BankSources { get; set; }
    public List<Category> Categories { get; set; }
}

// I Program.cs
var seedConfig = builder.Configuration.GetSection("SeedData").Get<SeedDataConfiguration>();
```

**Berörd fil:** `src/Privatekonomi.Core/Data/PrivatekonomyContext.cs`

---

## 2. Kodkvalitet & Arkitektur

### 🔴 2.1 Nullable Reference Warnings
**Problem:** 4 kompileringsvarningar för potentiella null-references i `Investments.razor`.

```
warning CS8602: Dereference of a possibly null reference.
```

**Lokationer:**
- Line 397, 417, 447, 481 i `Investments.razor`

**Lösning:**
```csharp
// Lägg till null-checks
if (!result.Canceled && result.Data is decimal newPrice && newPrice > 0)
{
    investment.CurrentPrice = newPrice;  // Kontrollera att investment inte är null först
    // ...
}

// Eller använd null-conditional operator
investment?.CurrentPrice = newPrice;
```

**Berörd fil:** `src/Privatekonomi.Web/Components/Pages/Investments.razor`

### 🟠 2.2 Repository Pattern Saknas
**Problem:** Services använder DbContext direkt, vilket bryter mot separation of concerns.

**Konsekvenser:**
- Svårare att testa
- Kod-duplicering i queries
- Tight coupling till EF Core

**Lösning:**
```csharp
// Skapa repository interface
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(int id);
}

// Implementera generisk repository
public class Repository<T> : IRepository<T> where T : class
{
    private readonly PrivatekonomyContext _context;
    private readonly DbSet<T> _dbSet;
    
    public Repository(PrivatekonomyContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }
    
    // Implementera metoder...
}

// Uppdatera services för att använda repository
public class TransactionService : ITransactionService
{
    private readonly IRepository<Transaction> _repository;
    
    public TransactionService(IRepository<Transaction> repository)
    {
        _repository = repository;
    }
}
```

**Ny fil:** `src/Privatekonomi.Core/Data/IRepository.cs`, `Repository.cs`

### 🟠 2.3 DTOs Saknas för API
**Problem:** API returnerar entity-modeller direkt, vilket exponerar intern struktur.

**Konsekvenser:**
- Over-fetching av data
- Svårt att versionera API
- Säkerhetsrisk (exponerar känsliga fält)

**Lösning:**
```csharp
// Skapa DTO-modeller
public class TransactionDto
{
    public int TransactionId { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public bool IsIncome { get; set; }
    public string? BankName { get; set; }
    public List<string> Categories { get; set; } = new();
}

// Använd AutoMapper eller manuell mapping
[HttpGet]
public async Task<ActionResult<IEnumerable<TransactionDto>>> GetTransactions()
{
    var transactions = await _transactionService.GetAllTransactionsAsync();
    var dtos = transactions.Select(t => new TransactionDto
    {
        TransactionId = t.TransactionId,
        Amount = t.Amount,
        // ... map andra fält
    });
    return Ok(dtos);
}
```

**Ny fil:** `src/Privatekonomi.Core/DTOs/`

### 🟡 2.4 Magic Numbers och Strings
**Problem:** Hårdkodade värden i koden (t.ex. kategorifärger, valuta-koder).

**Exempel:**
```csharp
entity.Property(e => e.Currency).IsRequired().HasMaxLength(3);
var description.ToLower().Substring(0, Math.Min(3, description.Length))
```

**Lösning:**
```csharp
// Skapa constants-klass
public static class Constants
{
    public const string DefaultCurrency = "SEK";
    public const int CurrencyCodeLength = 3;
    public const int MinSearchLength = 3;
    public const int MaxDescriptionLength = 500;
    
    public static class Colors
    {
        public const string Income = "#4CAF50";
        public const string Expense = "#FF6B6B";
        // ...
    }
}

// Använd i kod
entity.Property(e => e.Currency)
    .IsRequired()
    .HasMaxLength(Constants.CurrencyCodeLength);
```

**Ny fil:** `src/Privatekonomi.Core/Constants.cs`

### 🟡 2.5 Async/Await Patterns
**Problem:** Vissa metoder returnerar `Task` direkt istället för att använda `await`.

**Lösning:**
```csharp
// Dåligt
public async Task<Transaction> GetTransactionAsync(int id)
{
    return await _context.Transactions.FirstOrDefaultAsync(t => t.Id == id);
}

// Bättre - returnera Task direkt (prestanda)
public Task<Transaction?> GetTransactionAsync(int id)
{
    return _context.Transactions.FirstOrDefaultAsync(t => t.Id == id);
}

// Eller behåll async om du behöver göra mer
public async Task<Transaction?> GetTransactionAsync(int id)
{
    var transaction = await _context.Transactions.FirstOrDefaultAsync(t => t.Id == id);
    // Logga, validera, etc.
    return transaction;
}
```

### 🟢 2.6 Dependency Injection Scope
**Problem:** Alla services registreras som `Scoped`, även där `Singleton` eller `Transient` kan vara lämpligare.

**Granska:**
```csharp
// ThemeService behöver kanske vara Singleton
builder.Services.AddSingleton<ThemeService>();

// StockPriceService med HttpClient är korrekt Scoped
builder.Services.AddHttpClient<IStockPriceService, YahooFinanceStockPriceService>();
```

---

## 3. Testning

### 🔴 3.1 Inga Enhetstester
**Problem:** Projektet har inga enhetstester för närvarande.

**Konsekvenser:**
- Hög risk för regressioner
- Svårt att refaktorera tryggt
- Ingen kodtäckning

**Lösning:**
```bash
# Skapa test-projekt
dotnet new xunit -n Privatekonomi.Tests -o tests/Privatekonomi.Tests
dotnet sln add tests/Privatekonomi.Tests/Privatekonomi.Tests.csproj

# Lägg till referenser
cd tests/Privatekonomi.Tests
dotnet add reference ../../src/Privatekonomi.Core/Privatekonomi.Core.csproj
dotnet add package Moq
dotnet add package FluentAssertions
```

**Exempel test:**
```csharp
public class TransactionServiceTests
{
    [Fact]
    public async Task CreateTransaction_ShouldSetCreatedAt()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<PrivatekonomyContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        using var context = new PrivatekonomyContext(options);
        var service = new TransactionService(context);
        var transaction = new Transaction
        {
            Amount = 100,
            Description = "Test",
            Date = DateTime.Now
        };
        
        // Act
        var result = await service.CreateTransactionAsync(transaction);
        
        // Assert
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }
}
```

**Prioriterade testområden:**
1. TransactionService - CRUD operationer
2. CsvImportService - parsing och validering
3. CategoryService - auto-kategorisering
4. BudgetService - beräkningar

**Ny katalog:** `tests/Privatekonomi.Tests/`

### 🟠 3.2 Integration Tests Saknas
**Problem:** Ingen testning av API endpoints eller databas-interaktioner.

**Lösning:**
```bash
dotnet new xunit -n Privatekonomi.IntegrationTests -o tests/Privatekonomi.IntegrationTests
dotnet add package Microsoft.AspNetCore.Mvc.Testing
```

**Exempel:**
```csharp
public class TransactionsControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    
    public TransactionsControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }
    
    [Fact]
    public async Task GetTransactions_ReturnsSuccessStatusCode()
    {
        // Arrange
        var client = _factory.CreateClient();
        
        // Act
        var response = await client.GetAsync("/api/transactions");
        
        // Assert
        response.EnsureSuccessStatusCode();
    }
}
```

### 🟡 3.3 E2E Test Coverage
**Problem:** Endast grundläggande Playwright-tester finns.

**Förbättringar för Playwright-tester:**
1. Lägg till test för alla huvudfunktioner:
   - Budget creation and tracking
   - Investment management
   - CSV import flow
   - Category management
2. Lägg till visuella regressionstester
3. Implementera test för error handling
4. Lägg till performance-tester

**Berörd katalog:** `tests/playwright/tests/`

---

## 4. Performance & Skalbarhet

### 🟠 4.1 N+1 Query Problem
**Problem:** Vissa queries kan orsaka N+1 problem om inte inkludering hanteras rätt.

**Exempel i TransactionService:**
```csharp
// Kan orsaka N+1 om många transaktioner
var transactions = await _context.Transactions.ToListAsync();
foreach (var t in transactions)
{
    var categories = t.TransactionCategories; // Separat query per transaktion!
}
```

**Lösning:**
```csharp
// Använd Include/ThenInclude konsekvent
var transactions = await _context.Transactions
    .Include(t => t.BankSource)
    .Include(t => t.TransactionCategories)
        .ThenInclude(tc => tc.Category)
    .ToListAsync();
```

**Åtgärd:** Granska alla services och lägg till `.AsSplitQuery()` för komplexa inkluderingar.

### 🟡 4.2 Caching Saknas
**Problem:** Ingen caching av frekvent åtkomliga data (kategorier, bank sources).

**Lösning:**
```csharp
// Lägg till Memory Cache
builder.Services.AddMemoryCache();

// I CategoryService
private readonly IMemoryCache _cache;

public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
{
    return await _cache.GetOrCreateAsync("all_categories", async entry =>
    {
        entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
        return await _context.Categories.ToListAsync();
    });
}
```

### 🟡 4.3 Paginering Saknas i API
**Problem:** API returnerar alla resultat, vilket inte skalar.

**Lösning:**
```csharp
public class PaginatedResult<T>
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public IEnumerable<T> Items { get; set; }
}

[HttpGet]
public async Task<ActionResult<PaginatedResult<TransactionDto>>> GetTransactions(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 50)
{
    var query = _context.Transactions.AsQueryable();
    var total = await query.CountAsync();
    
    var items = await query
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();
    
    return Ok(new PaginatedResult<TransactionDto>
    {
        Page = page,
        PageSize = pageSize,
        TotalCount = total,
        TotalPages = (int)Math.Ceiling(total / (double)pageSize),
        Items = items.Select(t => MapToDto(t))
    });
}
```

### 🟢 4.4 Index Optimization
**Problem:** Indexering kan förbättras för vanliga queries.

**Lösning:**
```csharp
// I PrivatekonomyContext.OnModelCreating
modelBuilder.Entity<Transaction>(entity =>
{
    // Befintliga index är bra, men lägg till:
    entity.HasIndex(e => new { e.Date, e.IsIncome });
    entity.HasIndex(e => new { e.BankSourceId, e.Cleared });
    entity.HasIndex(e => e.CreatedAt);
});
```

---

## 5. Felhantering & Logging

### 🟠 5.1 Ingen Strukturerad Logging
**Problem:** Minimal logging i applikationen.

**Lösning:**
```csharp
// I services, lägg till ILogger
public class TransactionService : ITransactionService
{
    private readonly PrivatekonomyContext _context;
    private readonly ILogger<TransactionService> _logger;
    
    public TransactionService(
        PrivatekonomyContext context,
        ILogger<TransactionService> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    public async Task<Transaction> CreateTransactionAsync(Transaction transaction)
    {
        _logger.LogInformation(
            "Creating transaction: {Description}, Amount: {Amount}",
            transaction.Description,
            transaction.Amount);
        
        try
        {
            // ... kod
            _logger.LogInformation(
                "Transaction created successfully with ID: {TransactionId}",
                transaction.TransactionId);
            return transaction;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to create transaction: {Description}",
                transaction.Description);
            throw;
        }
    }
}
```

### 🟠 5.2 Global Exception Handler Saknas
**Problem:** Exceptions hanteras inte konsekvent i API.

**Lösning:**
```csharp
// Skapa middleware
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
    
    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }
    
    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = exception switch
        {
            ArgumentException => StatusCodes.Status400BadRequest,
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            _ => StatusCodes.Status500InternalServerError
        };
        
        await context.Response.WriteAsJsonAsync(new
        {
            error = exception.Message,
            statusCode = context.Response.StatusCode
        });
    }
}

// I Program.cs
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
```

### 🟡 5.3 Retry Logic för Externa API:er
**Problem:** Yahoo Finance API-anrop har ingen retry-logik.

**Lösning:**
```csharp
// Använd Polly för resilience
dotnet add package Polly.Extensions.Http

// I Program.cs
builder.Services.AddHttpClient<IStockPriceService, YahooFinanceStockPriceService>()
    .AddTransientHttpErrorPolicy(policy =>
        policy.WaitAndRetryAsync(3, retryAttempt =>
            TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))))
    .AddTransientHttpErrorPolicy(policy =>
        policy.CircuitBreakerAsync(5, TimeSpan.FromSeconds(30)));
```

---

## 6. Dokumentation & Kod-kommentarer

### 🟡 6.1 XML-dokumentation Saknas
**Problem:** Publika metoder saknar XML-dokumentation.

**Lösning:**
```csharp
/// <summary>
/// Hämtar alla transaktioner sorterade på datum i fallande ordning.
/// </summary>
/// <returns>En lista av transaktioner med kategorier och bankkälla.</returns>
/// <exception cref="DbUpdateException">Om databasåtkomst misslyckas.</exception>
public async Task<IEnumerable<Transaction>> GetAllTransactionsAsync()
{
    // ...
}
```

**Åtgärd:** Aktivera XML-dokumentation i csproj:
```xml
<PropertyGroup>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <NoWarn>$(NoWarn);1591</NoWarn>
</PropertyGroup>
```

### 🟡 6.2 Swagger-dokumentation
**Problem:** API-dokumentation i Swagger kan förbättras.

**Lösning:**
```csharp
// I Program.cs
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Privatekonomi API",
        Description = "En ASP.NET Core Web API för hantering av privatekonomi",
        Contact = new OpenApiContact
        {
            Name = "Support",
            Email = "support@example.com"
        },
        License = new OpenApiLicense
        {
            Name = "MIT License"
        }
    });
    
    // Inkludera XML-kommentarer
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});
```

### 🟢 6.3 ADR (Architecture Decision Records)
**Förslag:** Dokumentera viktiga arkitekturbeslut.

**Exempel - `docs/adr/0001-use-in-memory-database.md`:**
```markdown
# 1. Använd In-Memory Database för Development

Datum: 2024-XX-XX

## Status
Accepted

## Context
Vi behöver en snabb utvecklingsmiljö utan krav på databas-setup.

## Decision
Använd EF Core InMemory-provider för development och testing.

## Consequences
Positiva:
- Snabb setup
- Inga externa dependencies
- Enkelt att återställa data

Negativa:
- Data försvinner vid restart
- Kan inte användas i produktion
- Begränsad funktionalitet vs. riktiga databaser
```

---

## 7. DevOps & CI/CD

### 🟠 7.1 CI/CD Pipeline Saknas
**Problem:** Ingen GitHub Actions workflow för automatisk byggning och testning.

**Lösning - `.github/workflows/dotnet.yml`:**
```yaml
name: .NET Build and Test

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main, develop ]

jobs:
  build:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: 9.0.x
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Build
      run: dotnet build --no-restore --configuration Release
    
    - name: Test
      run: dotnet test --no-build --configuration Release --verbosity normal
    
    - name: Publish
      run: dotnet publish src/Privatekonomi.Web/Privatekonomi.Web.csproj -c Release -o ./publish
    
    - name: Upload artifact
      uses: actions/upload-artifact@v4
      with:
        name: privatekonomi-app
        path: ./publish
```

### 🟡 7.2 Code Quality Checks
**Förslag:** Lägg till kod-kvalitetsverktyg i CI.

**Lösning - `.github/workflows/code-quality.yml`:**
```yaml
name: Code Quality

on: [push, pull_request]

jobs:
  analyze:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Run CodeQL Analysis
      uses: github/codeql-action/init@v2
      with:
        languages: csharp
    
    - name: Build
      run: dotnet build
    
    - name: Run CodeQL
      uses: github/codeql-action/analyze@v2
```

### 🟢 7.3 Dependabot Configuration
**Förslag:** Aktivera automatiska dependency updates.

**Lösning - `.github/dependabot.yml`:**
```yaml
version: 2
updates:
  - package-ecosystem: "nuget"
    directory: "/"
    schedule:
      interval: "weekly"
    open-pull-requests-limit: 10
  
  - package-ecosystem: "npm"
    directory: "/tests/playwright"
    schedule:
      interval: "weekly"
```

---

## 8. Användarupplevelse & UI

### 🟡 8.1 Laddningsindikatorer
**Problem:** Ingen feedback vid långsamma operationer (CSV-import, kurshämtning).

**Lösning:**
```razor
@* I Razor-komponenter *@
@if (_isLoading)
{
    <MudProgressLinear Indeterminate="true" Color="Color.Primary" />
}
else
{
    @* Visa innehåll *@
}

@code {
    private bool _isLoading = false;
    
    private async Task LoadData()
    {
        _isLoading = true;
        try
        {
            // Ladda data
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }
}
```

### 🟡 8.2 Toast-meddelanden för Fel
**Problem:** Fel visas endast i console, inte för användare.

**Lösning:**
```csharp
try
{
    await SaveTransaction();
    Snackbar.Add("Transaktion sparad", Severity.Success);
}
catch (Exception ex)
{
    _logger.LogError(ex, "Failed to save transaction");
    Snackbar.Add($"Fel vid sparande: {ex.Message}", Severity.Error);
}
```

### 🟡 8.3 Internationalisering (i18n)
**Förslag:** Förbered för flerspråkighet.

**Lösning:**
```csharp
// Lägg till lokalisering
builder.Services.AddLocalization();
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { "sv-SE", "en-US" };
    options.SetDefaultCulture("sv-SE")
        .AddSupportedCultures(supportedCultures)
        .AddSupportedUICultures(supportedCultures);
});

// I Razor
@inject IStringLocalizer<Shared> Localizer

<h1>@Localizer["Welcome"]</h1>
```

### 🟢 8.4 Dark Mode Persist
**Problem:** Dark mode-val sparas inte mellan sessioner.

**Lösning:**
```csharp
// I ThemeService
public async Task SetDarkModeAsync(bool isDark)
{
    _isDarkMode = isDark;
    await _localStorage.SetItemAsync("darkMode", isDark);
}

public async Task<bool> GetDarkModeAsync()
{
    return await _localStorage.GetItemAsync<bool>("darkMode");
}
```

---

## 9. Datamodell & Affärslogik

### 🟡 9.1 Soft Delete
**Problem:** Data raderas permanent från databasen.

**Lösning:**
```csharp
// Lägg till i alla entiteter
public bool IsDeleted { get; set; }
public DateTime? DeletedAt { get; set; }

// I PrivatekonomyContext
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Lägg till global filter
    foreach (var entityType in modelBuilder.Model.GetEntityTypes())
    {
        if (typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
        {
            modelBuilder.Entity(entityType.ClrType)
                .HasQueryFilter(e => !EF.Property<bool>(e, "IsDeleted"));
        }
    }
}

// I DeleteAsync metoder
public async Task DeleteTransactionAsync(int id)
{
    var transaction = await _context.Transactions.FindAsync(id);
    if (transaction != null)
    {
        transaction.IsDeleted = true;
        transaction.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }
}
```

### 🟡 9.2 Audit Trail
**Problem:** Ingen spårning av ändringshistorik.

**Lösning:**
```csharp
public class AuditLog
{
    public int AuditLogId { get; set; }
    public string EntityName { get; set; }
    public int EntityId { get; set; }
    public string Action { get; set; } // Created, Updated, Deleted
    public string Changes { get; set; } // JSON
    public string UserId { get; set; }
    public DateTime Timestamp { get; set; }
}

// I SaveChangesAsync override
public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
{
    var auditEntries = OnBeforeSaveChanges();
    var result = await base.SaveChangesAsync(cancellationToken);
    await OnAfterSaveChanges(auditEntries);
    return result;
}
```

### 🟢 9.3 Recurring Transactions
**Problem:** Stöd för återkommande transaktioner är påbörjat men ej implementerat.

**Förslag:**
```csharp
public class RecurringTransaction
{
    public int RecurringTransactionId { get; set; }
    public string Description { get; set; }
    public decimal Amount { get; set; }
    public int CategoryId { get; set; }
    public RecurrencePattern Pattern { get; set; } // Daily, Weekly, Monthly, Yearly
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; }
}

// Bakgrundsjobb för att skapa transaktioner
public class RecurringTransactionHostedService : IHostedService
{
    // Implementera...
}
```

---

## 10. Säkerhet & Compliance

### 🟠 10.1 HTTPS Enforcement
**Problem:** HTTPS-redirection endast i produktion.

**Lösning:**
```csharp
// I Program.cs - tvinga HTTPS alltid
app.UseHttpsRedirection();

// Lägg till HSTS header även i development
if (app.Environment.IsDevelopment())
{
    app.UseHsts(); // 30 days default
}
else
{
    app.UseHsts(); // Consider longer expiry in production
}
```

### 🟡 10.2 Content Security Policy
**Förslag:** Implementera CSP headers.

**Lösning:**
```csharp
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("Content-Security-Policy",
        "default-src 'self'; " +
        "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
        "style-src 'self' 'unsafe-inline'; " +
        "img-src 'self' data: https:;");
    
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    
    await next();
});
```

### 🟡 10.3 Rate Limiting
**Problem:** API har ingen rate limiting.

**Lösning:**
```csharp
// Lägg till rate limiting
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.User.Identity?.Name ?? context.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            }));
});

app.UseRateLimiter();
```

### 🟢 10.4 GDPR Compliance
**Förslag:** Implementera funktionalitet för GDPR.

**Åtgärder:**
1. Lägg till "exportera min data"-funktion
2. Implementera "radera mitt konto"-funktion
3. Lägg till privacy policy-sida
4. Implementera cookie consent
5. Dokumentera datahantering

---

## 11. Konfiguration & Environment

### 🟡 11.1 Environment-specifik Konfiguration
**Problem:** Konfiguration är mestadels hårdkodad.

**Lösning - `appsettings.Production.json`:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Database=...;TrustServerCertificate=True"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "StockPriceService": {
    "ApiEndpoint": "https://api.example.com",
    "CacheDurationMinutes": 15
  }
}
```

### 🟢 11.2 Feature Flags
**Förslag:** Implementera feature toggles.

**Lösning:**
```csharp
builder.Services.AddFeatureManagement();

// I appsettings.json
{
  "FeatureManagement": {
    "RecurringTransactions": false,
    "MultiCurrency": true,
    "AdvancedCharts": false
  }
}

// I kod
@inject IFeatureManager FeatureManager

@if (await FeatureManager.IsEnabledAsync("AdvancedCharts"))
{
    <AdvancedChartsComponent />
}
```

---

## 12. Övriga Förbättringar

### 🟢 12.1 EditorConfig
**Förslag:** Lägg till .editorconfig för konsekvent kodstil.

**Lösning - `.editorconfig`:**
```ini
root = true

[*]
charset = utf-8
insert_final_newline = true
trim_trailing_whitespace = true

[*.cs]
indent_style = space
indent_size = 4

# C# kod-stil
csharp_new_line_before_open_brace = all
csharp_new_line_before_else = true
csharp_new_line_before_catch = true
csharp_new_line_before_finally = true

# Namngivning
dotnet_naming_rule.interfaces_should_be_prefixed_with_i.severity = warning
dotnet_naming_rule.interfaces_should_be_prefixed_with_i.symbols = interface
dotnet_naming_rule.interfaces_should_be_prefixed_with_i.style = begins_with_i

dotnet_naming_symbols.interface.applicable_kinds = interface
dotnet_naming_style.begins_with_i.required_prefix = I
dotnet_naming_style.begins_with_i.capitalization = pascal_case
```

### 🟢 12.2 Docker Support
**Förslag:** Lägg till Dockerfile för containerisering.

**Lösning - `Dockerfile`:**
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/Privatekonomi.Web/Privatekonomi.Web.csproj", "Privatekonomi.Web/"]
COPY ["src/Privatekonomi.Core/Privatekonomi.Core.csproj", "Privatekonomi.Core/"]
RUN dotnet restore "Privatekonomi.Web/Privatekonomi.Web.csproj"

COPY src/ .
WORKDIR "/src/Privatekonomi.Web"
RUN dotnet build "Privatekonomi.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Privatekonomi.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Privatekonomi.Web.dll"]
```

### 🟢 12.3 CHANGELOG
**Förslag:** Dokumentera ändringar mellan versioner.

**Lösning - `CHANGELOG.md`:**
```markdown
# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Nothing yet

### Changed
- Nothing yet

### Deprecated
- Nothing yet

### Removed
- Nothing yet

### Fixed
- Nothing yet

### Security
- Nothing yet

## [1.0.0] - 2024-XX-XX

### Added
- Initial release
- Basic transaction management
- CSV import from ICA-banken and Swedbank
- Budget tracking
- Investment portfolio
- ...
```

---

## Prioriterad Implementationsplan

### Fas 1: Kritiska Förbättringar (Vecka 1-2)
1. ✅ Fixa nullable reference warnings
2. ✅ Implementera databas-migrering från InMemory till SQL Server
3. ✅ Lägg till global exception handler
4. ✅ Skapa grundläggande enhetstester
5. ✅ Implementera strukturerad logging

### Fas 2: Kodkvalitet (Vecka 3-4)
1. ✅ Implementera Repository Pattern
2. ✅ Skapa DTOs för API
3. ✅ Lägg till input-validering
4. ✅ Implementera caching
5. ✅ Lägg till API-paginering

### Fas 3: Testning & CI/CD (Vecka 5-6)
1. ✅ Expandera enhetstester till >80% coverage
2. ✅ Implementera integration tests
3. ✅ Skapa CI/CD pipeline
4. ✅ Lägg till code quality checks
5. ✅ Implementera Playwright E2E-tester för alla features

### Fas 4: Säkerhet & Användarhantering (Vecka 7-8)
1. ✅ Implementera ASP.NET Core Identity
2. ✅ Lägg till autentisering och auktorisering
3. ✅ Implementera rate limiting
4. ✅ Lägg till säkerhetsheaders
5. ✅ GDPR-compliance features

### Fas 5: UX & Polish (Vecka 9-10)
1. ✅ Förbättra laddningsindikatorer
2. ✅ Implementera soft delete
3. ✅ Lägg till audit trail
4. ✅ Implementera feature flags
5. ✅ Förbättra felmeddelanden

---

## Mätbara Mål

Efter implementering av förbättringsförslagen ska projektet uppnå:

- ✅ **0 kompileringsvarningar**
- ✅ **>80% kod-täckning** i enhetstester
- ✅ **<200ms** svarstid för API-anrop (median)
- ✅ **A-betyg** i CodeQL security scan
- ✅ **0 kritiska** sårbarheter i dependencies
- ✅ **>90%** Lighthouse score för Web-appen
- ✅ **<5 sekunder** för fullständig Playwright-testsvit

---

## Underhållsplan

### Dagligen
- Övervaka CI/CD pipelines
- Granska nya pull requests
- Kolla Dependabot-notifieringar

### Veckovis
- Kör security scans
- Granska loggfiler för fel
- Uppdatera dependencies (om inga breaking changes)

### Månadsvis
- Granska kodkvalitetsmetrik
- Uppdatera dokumentation
- Planera nästa iteration

### Kvartalsvis
- Större refactoring om behövs
- Arkitektur-review
- Användartestning och UX-feedback

---

## Resurser & Länkar

### Dokumentation
- [ASP.NET Core Best Practices](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/best-practices)
- [EF Core Performance](https://learn.microsoft.com/en-us/ef/core/performance/)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)

### Verktyg
- [NDepend](https://www.ndepend.com/) - Kod-analys
- [SonarQube](https://www.sonarqube.org/) - Kodkvalitet
- [BenchmarkDotNet](https://benchmarkdotnet.org/) - Performance-testing

### Community
- [.NET Blog](https://devblogs.microsoft.com/dotnet/)
- [Blazor Community](https://github.com/AdrienTorris/awesome-blazor)
- [MudBlazor Docs](https://mudblazor.com/)

---

## Slutsats

Privatekonomi är ett välstrukturerat projekt med solid grund. Genom att implementera dessa förbättringsförslag kommer projektet att:

1. **Bli produktionsklart** med persistent databas och säkerhet
2. **Vara enklare att underhålla** med tester och bättre arkitektur
3. **Skala bättre** med caching och optimeringar
4. **Vara säkrare** med autentisering och säkerhetsåtgärder
5. **Ha bättre UX** med feedback och felhantering

Prioritera förbättringar baserat på projektets omedelbara behov och tillgängliga resurser. Börja med kritiska säkerhets- och stabilitetsfrågor, följt av kodkvalitet och testning.

---

**Senast uppdaterad:** 2025-10-20  
**Version:** 1.0  
**Författare:** GitHub Copilot Code Review
