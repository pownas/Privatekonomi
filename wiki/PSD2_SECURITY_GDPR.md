# PSD2 API - Säkerhet och GDPR-guide

## Översikt

Denna guide beskriver säkerhets- och integritetsåtgärder för PSD2 API-implementationen i Privatekonomi-applikationen.

## Säkerhetsfunktioner

### 1. Token-kryptering

Alla OAuth2-tokens (access tokens och refresh tokens) krypteras automatiskt innan de lagras i databasen.

**Implementation:**
- Använder ASP.NET Core Data Protection API
- Kryptering sker automatiskt vid sparande av BankConnection
- Dekryptering sker automatiskt vid hämtning

**Best practices:**
```csharp
// Tokens krypteras automatiskt i BankConnectionService
var connection = await bankConnectionService.CreateConnectionAsync(connection);
```

**Konfiguration:**
I produktion, konfigurera Data Protection med beständig lagring:
```csharp
services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(@"/var/keys/"))
    .SetApplicationName("Privatekonomi");
```

För Azure, använd Azure Key Vault:
```csharp
services.AddDataProtection()
    .PersistKeysToAzureBlobStorage(/* blob uri */)
    .ProtectKeysWithAzureKeyVault(/* key uri */, credential);
```

### 2. OAuth State-validering (CSRF-skydd)

Implementationen använder state-parameter för att förhindra CSRF-attacker i OAuth2-flödet.

**Hur det fungerar:**
1. När användaren initierar OAuth-flödet genereras en unik state-token
2. State lagras i minnet med en TTL på 15 minuter
3. Vid callback valideras state innan token-utbyte
4. State tas bort efter användning (single-use)

**Användning:**
```csharp
// Initiering (genererar och lagrar state)
var state = oauthStateService.GenerateState("Swedbank");

// Validering vid callback
if (!oauthStateService.ValidateState(receivedState, "Swedbank"))
{
    return BadRequest("Invalid state");
}

// Ta bort använd state
oauthStateService.RemoveState(receivedState);
```

### 3. TLS/SSL-kryptering

All kommunikation med bankernas API:er sker över HTTPS.

**Krav:**
- Applikationen måste köras med HTTPS i produktion
- Använd giltiga SSL-certifikat (inte self-signed)
- Konfigurera HSTS (HTTP Strict Transport Security)

**Program.cs-konfiguration:**
```csharp
app.UseHttpsRedirection();
app.UseHsts();
```

### 4. Säker hantering av credentials

**ALDRIG lagra credentials i kod eller appsettings.json i version control!**

**Rekommenderade metoder:**

#### Development: User Secrets
```bash
dotnet user-secrets set "Swedbank:ClientId" "your-client-id"
dotnet user-secrets set "Swedbank:ClientSecret" "your-client-secret"
```

#### Production: Environment Variables
```bash
export Swedbank__ClientId="your-client-id"
export Swedbank__ClientSecret="your-client-secret"
```

#### Azure: Key Vault
```csharp
builder.Configuration.AddAzureKeyVault(
    new Uri($"https://{keyVaultName}.vault.azure.net/"),
    new DefaultAzureCredential());
```

### 5. Audit Logging

All bank-relaterad aktivitet loggas för spårbarhet:

- Skapande av bankkopplingar
- Uppdatering av kopplingar
- Radering av kopplingar
- Transaktionsimporter
- Autentiseringsfel

**Loggnivåer:**
- Information: Lyckade operationer
- Warning: OAuth-fel, utgångna tokens
- Error: API-fel, kommunikationsproblem

### 6. Felhantering och robusthet

**Implementerade åtgärder:**

1. **Token Refresh:** Automatisk förnyelse av utgångna tokens
2. **Fellogning:** Detaljerad loggning av fel utan känsliga data
3. **Status tracking:** Kopplingar markeras som "Error" vid upprepade fel
4. **Graceful degradation:** Systemet fortsätter fungera om en bank är otillgänglig

**Planerat (TODO):**
- Retry logic med exponential backoff (Polly)
- Circuit breaker för bankernas API:er
- Rate limiting för att respektera API-begränsningar

## GDPR-efterlevnad

### Lagring av personuppgifter

**Data som lagras:**
- OAuth2 tokens (krypterade)
- Bankkontoinformation (IBAN, kontonummer)
- Transaktionsdata (belopp, datum, beskrivning, motpart)
- Bankkopplingsstatus och metadata

**Lagringsplatser:**
- Databas (SQLite/SQL Server)
- Loggar (utan känsliga personuppgifter)
- Cache (state-tokens, temporärt max 15 min)

### Rättslig grund

- **Användarens samtycke:** Användaren ger explicit samtycke vid anslutning av bank
- **Avtalsfullgörande:** Nödvändigt för att tillhandahålla tjänsten
- **PSD2-regler:** Följer EU:s PSD2-direktiv för betaltjänster

### Registrerades rättigheter

#### 1. Rätt till radering
Användare kan när som helst ta bort sina bankkopplingar:

```http
DELETE /api/bankconnections/{id}
```

Detta raderar:
- Bankkopplingen och tokens
- Metadata om kopplingar
- Audit logs behålls för lagliga krav (men anonymiseras)

#### 2. Rätt till dataportabilitet
Användare kan exportera sina data via befintliga export-funktioner.

#### 3. Rätt till information
Denna dokumentation och PSD2_API_GUIDE.md informerar om datahantering.

#### 4. Rätt till rättelse
Användare kan uppdatera sina bankkopplingar när som helst.

### Dataminimering

**Principer som följs:**
- Endast nödvändiga data lagras
- Transaktioner hämtas endast för valt datumintervall
- Tokens har begränsad livslängd (90-180 dagar enligt PSD2)
- State-tokens är temporära (15 minuter)

### Säkerhet vid behandling

**Implementerade åtgärder:**
1. Kryptering av tokens vid lagring
2. HTTPS för all kommunikation
3. Ingen loggning av tokens eller lösenord
4. Access control via användarsystem
5. Audit logging av alla ändringar

### Dataöverföring

**Till tredje part:**
- Data delas ENDAST med bankernas officiella API:er
- Användarens samtycke krävs för varje bank
- Ingen data delas med andra tredje parter

**Från banker:**
- Data hämtas direkt från bankens API
- Inga mellanhänder eller aggregatorer
- Krypterad överföring (TLS)

### Lagringstid

- **Bankkopplingar:** Så länge användaren vill, eller tills token går ut
- **Transaktioner:** Permanent (eller enligt användarens önskemål)
- **Audit logs:** 12 månader (lagkrav)
- **State-tokens:** 15 minuter (automatisk rensning)

### Automatiserad behandling

**Automatisk synkronisering:**
- Kan aktiveras av användaren per bankkoppling
- Sker endast för aktiva kopplingar
- Användaren kan när som helst inaktivera
- Konfigureras i appsettings.json:

```json
{
  "BankSync": {
    "Enabled": true,
    "IntervalMinutes": 60
  }
}
```

## Säkerhets-checklista för produktion

- [ ] **Använd HTTPS** för all kommunikation
- [ ] **Kryptera tokens** (redan implementerat)
- [ ] **Säkra credentials** med Key Vault eller miljövariabler
- [ ] **Konfigurera Data Protection** med beständig lagring
- [ ] **Aktivera HSTS** och säkra HTTP-headers
- [ ] **Implementera rate limiting** för API:er
- [ ] **Övervaka och logga** säkerhetshändelser
- [ ] **Regelbundna säkerhetsgranskningar**
- [ ] **Håll dependencies uppdaterade** (NuGet-paket)
- [ ] **Implementera retry logic** med Polly
- [ ] **Dokumentera incidenthantering**
- [ ] **Utbilda användare** om säkerhet

## Incidenthantering

Vid säkerhetsincident:

1. **Identifiera:** Upptäck och bekräfta incidenten
2. **Innehåll:** Stoppa pågående angrepp eller läckage
3. **Utreda:** Analysera omfattning och påverkan
4. **Åtgärda:** Fixa sårbarheter
5. **Meddela:** Informera berörda användare (inom 72h enligt GDPR)
6. **Dokumentera:** Logga incident för framtida referens

## Kontakt och rapportering

För säkerhets- eller integritetsfrågor:
- Öppna ett issue på GitHub (för generella frågor)
- Kontakta utvecklare direkt för säkerhetsincidenter
- Se CONTRIBUTING.md för mer information

## Uppdateringar

Denna dokumentation uppdateras regelbundet. Senaste version: 2025-01-20

## Referenser

- [PSD2 Directive (EU 2015/2366)](https://eur-lex.europa.eu/legal-content/EN/TXT/?uri=CELEX:32015L2366)
- [GDPR (EU 2016/679)](https://gdpr.eu/)
- [ASP.NET Core Data Protection](https://docs.microsoft.com/en-us/aspnet/core/security/data-protection/)
- [OAuth 2.0 Security Best Practices](https://tools.ietf.org/html/draft-ietf-oauth-security-topics)
- [OWASP Top 10](https://owasp.org/www-project-top-ten/)
