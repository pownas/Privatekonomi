# ProblemDetails-mönster Implementation

## Sammanfattning
Backend-API:et har implementerats med stöd för ProblemDetails-mönstret enligt RFC 7807 och Microsofts rekommendationer för felhantering i ASP.NET Core.

## Implementering

### Backend (Privatekonomi.Api)

#### 1. Konfiguration i Program.cs
- `AddProblemDetails()` - Aktiverar ProblemDetails-stöd
- `AddExceptionHandler<GlobalExceptionHandler>()` - Registrerar global exception handler
- `UseExceptionHandler()` - Aktiverar exception handling middleware
- `UseStatusCodePages()` - Aktiverar status code pages för att hantera HTTP-statuskoder

#### 2. Custom Exception Types
Skapade tre specialanpassade exception-typer för strukturerad felhantering:

**NotFoundException** (`/Exceptions/NotFoundException.cs`)
- Används när en resurs inte hittas
- Returnerar HTTP 404
- Exempel: `throw new NotFoundException("Transaction", id);`

**BadRequestException** (`/Exceptions/BadRequestException.cs`)
- Används för ogiltiga förfrågningar
- Returnerar HTTP 400
- Exempel: `throw new BadRequestException("ID mismatch");`

**ValidationException** (`/Exceptions/ValidationException.cs`)
- Används för valideringsfel
- Returnerar HTTP 400
- Stödjer multipla valideringsfel med fält-mappning
- Exempel: 
```csharp
var errors = new Dictionary<string, string[]>
{
    { "Email", new[] { "Email is required", "Email format is invalid" } },
    { "Password", new[] { "Password must be at least 8 characters" } }
};
throw new ValidationException(errors);
```

#### 3. Global Exception Handler
**GlobalExceptionHandler** (`/Middleware/GlobalExceptionHandler.cs`)

Hanterar alla undantag och konverterar dem till ProblemDetails-svar enligt RFC 7807:

**Funktioner:**
- Mappar exception-typer till lämpliga HTTP-statuskoder
- Skapar strukturerade ProblemDetails-svar
- Lägger till `traceId` för korrelation och felsökning
- Inkluderar valideringsfel i `errors`-extension
- Loggar undantag baserat på allvarlighetsgrad (Error för 5xx, Warning för 404, Info för andra 4xx)

**ProblemDetails-struktur:**
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.5",
  "title": "Resource Not Found",
  "status": 404,
  "detail": "Transaction with key '123' was not found.",
  "instance": "/api/transactions/123",
  "traceId": "00-abc123-def456-00"
}
```

**Med valideringsfel:**
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Validation Error",
  "status": 400,
  "detail": "One or more validation errors occurred.",
  "instance": "/api/categories",
  "traceId": "00-abc123-def456-00",
  "errors": {
    "Name": ["Name is required", "Name must be unique"],
    "Type": ["Invalid category type"]
  }
}
```

#### 4. Uppdaterade Controllers
Uppdaterade controllers för att använda exceptions istället för att returnera StatusCode:

**Före:**
```csharp
[HttpGet("{id}")]
public async Task<ActionResult<Category>> GetCategory(int id)
{
    try
    {
        var category = await _categoryService.GetCategoryByIdAsync(id);
        if (category == null)
        {
            return NotFound();
        }
        return Ok(category);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error retrieving category {CategoryId}", id);
        return StatusCode(500, "Internal server error");
    }
}
```

**Efter:**
```csharp
[HttpGet("{id}")]
public async Task<ActionResult<Category>> GetCategory(int id)
{
    var category = await _categoryService.GetCategoryByIdAsync(id);
    if (category == null)
    {
        throw new NotFoundException("Category", id);
    }
    return Ok(category);
}
```

**Fördelar:**
- Enklare kod
- Konsekvent felhantering
- Automatisk loggning
- Strukturerade felmeddelanden
- Bättre separation of concerns

Uppdaterade controllers:
- `CategoriesController` - Alla endpoints
- `TransactionsController` - Alla endpoints

### Frontend (Privatekonomi.Web)

#### 1. ProblemDetails Model
**ProblemDetailsResponse** (`/Models/ProblemDetailsResponse.cs`)

C#-modell för att deserialisera ProblemDetails-svar från API:et:
```csharp
public class ProblemDetailsResponse
{
    public string? Type { get; set; }
    public string? Title { get; set; }
    public int Status { get; set; }
    public string? Detail { get; set; }
    public string? Instance { get; set; }
    public string? TraceId { get; set; }
    public Dictionary<string, string[]>? Errors { get; set; }
}
```

#### 2. API Error Handler
**ApiErrorHandler** (`/Services/ApiErrorHandler.cs`)

Service för att hantera API-fel och extrahera användarvänliga felmeddelanden:

**Metoder:**

**GetErrorMessageAsync()**
- Försöker parsa ProblemDetails från HTTP-svar
- Extraherar titel, detalj och valideringsfel
- Fallback till svenska felmeddelanden baserat på HTTP-statuskod
- Returnerar användarvänlig text

**EnsureSuccessAsync()**
- Kontrollerar om HTTP-svar är framgångsrikt
- Kastar HttpRequestException med felmeddelande om inte

**Användning i komponenter:**
```csharp
var response = await Http.PostAsync(url, content);

if (!response.IsSuccessStatusCode)
{
    var errorMessage = await ApiErrorHandler.GetErrorMessageAsync(response);
    Snackbar.Add(errorMessage, Severity.Error);
}
```

#### 3. Uppdaterade komponenter
- `Import.razor` - Använder ApiErrorHandler för att visa strukturerade felmeddelanden

## Fördelar med implementeringen

### 1. Konsistens
- Alla API-fel följer samma struktur (RFC 7807)
- Enhetlig felhantering i hela applikationen
- Förutsägbart beteende för klienter

### 2. Förbättrad utvecklarupplevelse
- Enklare controller-kod
- Automatisk loggning
- TraceId för korrelation mellan frontend och backend
- Tydliga felmeddelanden i swagger/OpenAPI

### 3. Bättre användarvänlighet
- Strukturerade och informativa felmeddelanden
- Stöd för valideringsfel med fält-mappning
- Svenska felmeddelanden i frontend

### 4. Säkerhet
- Känslig information exponeras inte i produktionsmiljö
- Kontrollerad felexponering
- Loggning av allvarliga fel

### 5. Testbarhet
- Lättare att testa felscenarier
- Konsekvent felformat underlättar automatiserad testning
- Möjlighet att mocka exceptions

## Kompatibilitet med Frontend

Frontend hanterar ProblemDetails på följande sätt:

1. **Parsing**: `ApiErrorHandler.GetErrorMessageAsync()` parsar ProblemDetails-svar
2. **Visning**: Extraherar användarvänliga meddelanden från Detail eller Title
3. **Validering**: Visar valideringsfel från `errors`-property
4. **Fallback**: Om parsing misslyckas, används svenska felmeddelanden baserat på HTTP-statuskod

## Best Practices

### Backend
1. Använd specifika exception-typer (`NotFoundException`, `BadRequestException`, `ValidationException`)
2. Inkludera tydliga felmeddelanden i exceptions
3. Låt GlobalExceptionHandler hantera alla undantag
4. Använd inte try-catch i controllers (såvida inte särskild hantering krävs)

### Frontend
1. Använd `ApiErrorHandler.GetErrorMessageAsync()` för att extrahera felmeddelanden
2. Visa tydliga felmeddelanden till användaren via Snackbar eller annan feedback-mekanism
3. Logga traceId för felsökning
4. Hantera olika HTTP-statuskoder på lämpligt sätt

## Exempel på användning

### Backend - Kasta exception
```csharp
// Not found
throw new NotFoundException("Transaction", transactionId);

// Bad request
throw new BadRequestException("Invalid date range");

// Validation
var errors = new Dictionary<string, string[]>
{
    { "Amount", new[] { "Amount must be greater than 0" } }
};
throw new ValidationException(errors);
```

### Frontend - Hantera fel
```csharp
try
{
    var response = await Http.GetAsync($"/api/transactions/{id}");
    
    if (!response.IsSuccessStatusCode)
    {
        var errorMessage = await ApiErrorHandler.GetErrorMessageAsync(response);
        Snackbar.Add(errorMessage, Severity.Error);
        return;
    }
    
    var transaction = await response.Content.ReadFromJsonAsync<Transaction>();
    // ... använd transaction
}
catch (Exception ex)
{
    Snackbar.Add($"Ett oväntat fel uppstod: {ex.Message}", Severity.Error);
}
```

## Referenser
- [RFC 7807 - Problem Details for HTTP APIs](https://datatracker.ietf.org/doc/html/rfc7807)
- [Microsoft Docs - Error handling in ASP.NET Core Web API](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/error-handling-api?view=aspnetcore-9.0&tabs=controllers#problem-details)
- [Microsoft Docs - Handle errors in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/error-handling)

## Framtida förbättringar
1. Utöka fler controllers med exception-baserad felhantering
2. Lägg till fler custom exception-typer vid behov (t.ex. `UnauthorizedException`, `ForbiddenException`)
3. Implementera globalt error boundary i Blazor för att fånga rendering-fel
4. Lägg till strukturerad loggning med Serilog eller Application Insights
5. Skapa automatiserade tester för felscenarier
