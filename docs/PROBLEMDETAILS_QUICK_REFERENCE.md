# ProblemDetails Quick Reference

## För Backend-utvecklare

### Använda custom exceptions i controllers

#### Not Found (404)
```csharp
var entity = await _service.GetByIdAsync(id);
if (entity == null)
{
    throw new NotFoundException("EntityName", id);
}
```

#### Bad Request (400)
```csharp
if (id != entity.Id)
{
    throw new BadRequestException("ID mismatch between URL and body");
}
```

#### Validation Error (400)
```csharp
var errors = new Dictionary<string, string[]>
{
    { "Email", new[] { "Email is required", "Email format is invalid" } },
    { "Password", new[] { "Password must be at least 8 characters" } }
};
throw new ValidationException(errors);
```

### Rensa try-catch blocks

❌ **Gör inte så här:**
```csharp
try
{
    var result = await _service.DoSomethingAsync();
    return Ok(result);
}
catch (Exception ex)
{
    _logger.LogError(ex, "Error");
    return StatusCode(500, "Internal server error");
}
```

✅ **Gör så här istället:**
```csharp
var result = await _service.DoSomethingAsync();
return Ok(result);
// GlobalExceptionHandler tar hand om exceptions automatiskt
```

## För Frontend-utvecklare

### Hantera API-fel

#### Enkel felhantering
```csharp
var response = await Http.GetAsync(url);

if (!response.IsSuccessStatusCode)
{
    var errorMessage = await ApiErrorHandler.GetErrorMessageAsync(response);
    Snackbar.Add(errorMessage, Severity.Error);
    return;
}

var data = await response.Content.ReadFromJsonAsync<MyModel>();
```

#### Med try-catch för oväntade fel
```csharp
try
{
    var response = await Http.PostAsJsonAsync(url, model);
    
    if (!response.IsSuccessStatusCode)
    {
        var errorMessage = await ApiErrorHandler.GetErrorMessageAsync(response);
        Snackbar.Add(errorMessage, Severity.Error);
        return;
    }
    
    var result = await response.Content.ReadFromJsonAsync<MyResult>();
    Snackbar.Add("Success!", Severity.Success);
}
catch (Exception ex)
{
    Snackbar.Add($"Ett oväntat fel uppstod: {ex.Message}", Severity.Error);
}
```

### Importera nödvändiga namespaces

```csharp
@using Privatekonomi.Web.Services
@inject HttpClient Http
@inject ISnackbar Snackbar
```

## ProblemDetails-struktur

### Standardfält (RFC 7807)
- `type` - URI som identifierar problemtypen
- `title` - Kort sammanfattning av problemet
- `status` - HTTP-statuskod
- `detail` - Detaljerad förklaring
- `instance` - URI till den specifika instansen av problemet

### Anpassade fält
- `traceId` - Spårnings-ID för korrelation mellan loggar
- `errors` - Dictionary med valideringsfel (endast för ValidationException)

## HTTP-statuskoder

| Kod | Exception | Användning |
|-----|-----------|------------|
| 400 | BadRequestException | Ogiltig förfrågan |
| 400 | ValidationException | Valideringsfel |
| 400 | ArgumentException | Ogiltiga argument |
| 404 | NotFoundException | Resurs hittades inte |
| 500 | (Övriga) | Serverfel |

## Loggning

### Automatisk loggning
GlobalExceptionHandler loggar automatiskt alla exceptions:

- **Error** (500-fel) - Serverfel
- **Warning** (404-fel) - Resurs hittades inte
- **Information** (Övriga 4xx) - Klientfel

### TraceId
Varje fel får ett unikt traceId som kan användas för felsökning:
- Loggas automatiskt i backend
- Inkluderas i ProblemDetails-svar
- Kan användas för att korrelera frontend- och backend-loggar

## Testa implementeringen

### Manuellt via Swagger
1. Starta API:et: `dotnet run --project src/Privatekonomi.Api`
2. Öppna Swagger: `https://localhost:7023/swagger`
3. Testa endpoints med ogiltiga värden
4. Verifiera att ProblemDetails returneras

### Automatiserat via script
```bash
./tests/problemdetails-tests.sh
```

### Förväntat svar för 404
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.5",
  "title": "Resource Not Found",
  "status": 404,
  "detail": "Category with key '99999' was not found.",
  "instance": "/api/categories/99999",
  "traceId": "00-1234567890abcdef-1234567890abcdef-00"
}
```

## Vanliga frågor

### Måste jag använda try-catch i controllers?
Nej, GlobalExceptionHandler fångar alla undantag automatiskt. Använd endast try-catch om du behöver särskild hantering för specifika exceptions.

### Hur lägger jag till anpassade exception-typer?
1. Skapa en ny klass som ärver från `Exception`
2. Lägg till den i `GlobalExceptionHandler.GetStatusCode()`
3. Lägg till den i `GlobalExceptionHandler.GetTitle()`

### Fungerar detta med ModelState-validering?
Ja, men du kan också använda ValidationException för mer kontroll över valideringsfel.

### Kan jag anpassa felmeddelanden per miljö?
Ja, GlobalExceptionHandler kan utökas för att visa olika detaljer i Development vs Production.

## Best Practices

### Backend
✅ Använd specifika exception-typer
✅ Inkludera tydliga felmeddelanden
✅ Låt GlobalExceptionHandler hantera exceptions
✅ Undvik att returnera raw error strings

### Frontend
✅ Använd ApiErrorHandler för alla API-fel
✅ Visa användarvänliga meddelanden
✅ Spara traceId för support
✅ Hantera både förväntade och oväntade fel
