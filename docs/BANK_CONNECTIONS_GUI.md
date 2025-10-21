# Bank Connections GUI Implementation

## Översikt

Detta dokument beskriver implementationen av det grafiska gränssnittet för hantering av bankkopplingar i Privatekonomi.

## Implementerade funktioner

### 1. Användargränssnitt (Blazor Components)

#### BankConnections.razor
- Huvudsida för hantering av bankkopplingar
- Visar lista över alla aktiva bankkopplingar med:
  - Banknamn och färgkodning
  - Konto-ID
  - Status (Aktiv, Utgången, Fel)
  - Senaste synkroniseringstid
  - Auto-sync aktivering
- Åtgärdsknappar för varje koppling:
  - Redigera koppling
  - Synkronisera transaktioner
  - Ta bort koppling

#### AddBankConnectionDialog.razor
- Dialog för att lägga till nya bankkopplingar
- Funktioner:
  - Välj bank från dropdown
  - Valfritt konto-ID
  - Aktivera/inaktivera auto-sync
  - Initierar OAuth-flöde för bank-auktorisering
  - Kontrollerar API-support för vald bank

#### EditBankConnectionDialog.razor
- Dialog för att redigera befintliga bankkopplingar
- Möjlighet att ändra:
  - Auto-sync inställning
  - Status (Aktiv, Utgången, Fel)

### 2. API-implementering

#### BankConnectionsController (Uppdaterat)
- **Ny endpoint**: `PUT /api/bankconnections/{id}` - Uppdatera befintlig bankkoppling
- Befintliga endpoints:
  - `GET /api/bankconnections` - Hämta alla kopplingar
  - `GET /api/bankconnections/{id}` - Hämta specifik koppling
  - `POST /api/bankconnections/authorize` - Initiera OAuth-flöde
  - `GET /api/bankconnections/callback` - OAuth callback
  - `POST /api/bankconnections/connect` - Slutför anslutning
  - `GET /api/bankconnections/{id}/accounts` - Hämta konton
  - `POST /api/bankconnections/{id}/import` - Importera transaktioner
  - `DELETE /api/bankconnections/{id}` - Ta bort koppling

### 3. Säkerhet och revision

#### AuditLog Model
- Loggning av säkerhetskritiska händelser
- Spårar:
  - Action (t.ex. "BankConnectionCreated", "BankConnectionUpdated", "BankConnectionDeleted")
  - EntityType och EntityId
  - UserId och IP-adress
  - Tidsstämpel

#### AuditLogService
- Tjänst för att logga alla ändringar i bankkopplingar
- Integrerad med BankConnectionService
- Loggar automatiskt:
  - Skapande av nya kopplingar
  - Uppdateringar av kopplingar
  - Borttagning av kopplingar

### 4. Databasschema

#### BankConnection Model
```csharp
- BankConnectionId (int, PK)
- BankSourceId (int, FK)
- ApiType (string) - typ av API (PSD2, Proprietary, etc.)
- ExternalAccountId (string) - konto-ID från bankens API
- AccessToken (string) - OAuth2 access token (krypterad)
- RefreshToken (string) - OAuth2 refresh token (krypterad)
- TokenExpiresAt (DateTime?)
- LastSyncedAt (DateTime?)
- AutoSyncEnabled (bool)
- Status (string) - Active, Expired, Error
- CreatedAt, UpdatedAt (DateTime)
```

#### AuditLog Model
```csharp
- AuditLogId (int, PK)
- Action (string)
- EntityType (string)
- EntityId (int?)
- UserId (string?)
- Details (string?)
- IpAddress (string?)
- CreatedAt (DateTime)
```

### 5. Konfiguration

#### Dependency Injection
Registrering i både `Privatekonomi.Api/Program.cs` och `Privatekonomi.Web/Program.cs`:
```csharp
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<IBankConnectionService, BankConnectionService>();
```

#### HttpClient konfiguration (Web)
```csharp
builder.Services.AddHttpClient("api", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["services:api:http:0"] ?? "http://localhost:5001");
});
```

## Säkerhetsaspekter

### Implementerade säkerhetsfunktioner:
1. ✅ **Audit Logging** - Alla ändringar loggas
2. ✅ **Input Validation** - MudBlazor form validation
3. ✅ **Bekräftelsedialoger** - För destruktiva operationer (ta bort)
4. ✅ **Token-hantering** - Stöd för OAuth2 access/refresh tokens
5. ✅ **Separation of Concerns** - API och Web-lager separerade

### Återstående säkerhetsimplementationer:
- 🔲 **Autentisering och behörighetskontroll** - Ej implementerad (ingen Identity provider)
- 🔲 **Kryptering av tokens** - Tokens sparas okrypterade i databas (bör krypteras i produktion)
- 🔲 **CSRF/XSS-skydd** - Använd built-in ASP.NET Core antiforgery tokens
- 🔲 **Rate limiting** - Bör implementeras för API-endpoints
- 🔲 **IP-address tracking** - Audit logs stödjer det men loggar inte ännu

## Användning

### Lägga till en bankkoppling:
1. Navigera till `/bank-connections`
2. Klicka på "Lägg till bank"
3. Välj bank från dropdown
4. (Valfritt) Ange konto-ID
5. Välj om auto-sync ska aktiveras
6. Klicka "Anslut"
7. Omdirigeras till bankens OAuth-sida
8. Godkänn åtkomst
9. Omdirigeras tillbaka till applikationen

### Redigera en bankkoppling:
1. Klicka på redigera-ikonen för önskad koppling
2. Ändra auto-sync eller status
3. Klicka "Spara"

### Synkronisera transaktioner:
1. Klicka på synkronisera-ikonen för önskad koppling
2. Systemet hämtar konton och importerar transaktioner
3. Bekräftelse visas med antal nya transaktioner

### Ta bort en bankkoppling:
1. Klicka på radera-ikonen
2. Bekräfta borttagning i dialog
3. Kopplingen tas bort permanent

## Teknisk stack

- **Frontend**: Blazor Server med MudBlazor UI-komponenter
- **Backend**: ASP.NET Core Web API
- **Databas**: Entity Framework Core med In-Memory provider (för development)
- **Orkestrering**: .NET Aspire för service discovery

## Responsiv design

Implementationen använder MudBlazor som är fullt responsiv:
- Desktop: Full tabell-layout med alla kolumner
- Tablet/Mobile: Anpassad layout med viktigast information först
- Dialogs: Automatisk anpassning till skärmstorlek med `MaxWidth` och `FullWidth` parametrar

## Testing

### Manuell testning:
1. Starta applikationen: `dotnet run --project src/Privatekonomi.AppHost`
2. Navigera till `/bank-connections`
3. Testa alla CRUD-operationer
4. Verifiera audit logs i databasen

### Automatisk testning:
- Unit tests: Bör skapas för BankConnectionService och AuditLogService
- Integration tests: Bör skapas för API-endpoints
- E2E tests: Playwright tests kan utökas för bank connections flödet

## Framtida förbättringar

1. **OAuth State validation** - Implementera korrekt state-validering för CSRF-skydd
2. **Session/localStorage** - Spara connection data under OAuth-flödet
3. **Kryptering** - Kryptera access/refresh tokens i databasen
4. **Användarautentisering** - Integrera med Identity provider
5. **Webhook support** - Auto-sync via bank webhooks istället för polling
6. **Batch operations** - Möjlighet att synkronisera flera kopplingar samtidigt
7. **Export/Import** - Möjlighet att exportera/importera bankkopplingar
8. **Status monitoring** - Dashboard för att övervaka koppling-hälsa

## Kända begränsningar

1. **In-Memory Database** - Data förloras vid omstart (använd SQL Server/PostgreSQL i produktion)
2. **Ingen autentisering** - Alla kan se och ändra alla kopplingar (implementera Identity)
3. **Demo credentials** - Bank API services använder placeholder-credentials
4. **Ingen token-refresh** - OAuth refresh token logik ej implementerad
5. **Enkel error handling** - Bör förbättras med retry-logik och mer detaljerade felmeddelanden

## Bidragsgivare

Implementerat som del av issue #[nummer] - "Skapa ett grafiskt gränssnitt för hantering av bankkopplingar"
