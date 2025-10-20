# PSD2 API Guide - Automatisk bankimport

Denna guide beskriver hur du använder PSD2-API-stödet för att automatiskt importera transaktioner från Swedbank, Avanza Bank och ICA Banken.

## Översikt

Applikationen stöder nu automatisk bankimport via bank-API:er istället för manuell CSV-import. Detta ger följande fördelar:

- **Automatisk synkronisering**: Transaktioner hämtas automatiskt från banken
- **Realtidsdata**: Alltid aktuella transaktioner
- **Ingen manuell filhantering**: Slipper ladda ner och importera CSV-filer
- **Säker autentisering**: OAuth2-baserad autentisering med banken

## Banker som stöds

### 1. Swedbank (PSD2)
- **API-typ**: PSD2-kompatibel
- **Autentisering**: OAuth2 med BankID
- **Funktioner**: Kontolistning, transaktionshistorik
- **Status**: Implementerad (kräver produktionsnycklar)

### 2. Avanza Bank
- **API-typ**: Proprietary API
- **Autentisering**: Användarnamn/lösenord + TOTP (2FA)
- **Funktioner**: Investeringskonton, transaktioner, innehav
- **Status**: Implementerad (kräver användarautentisering)

### 3. ICA Banken (PSD2)
- **API-typ**: PSD2-kompatibel via Nordic API Gateway
- **Autentisering**: OAuth2 med BankID
- **Funktioner**: Kontolistning, transaktionshistorik
- **Status**: Implementerad (kräver produktionsnycklar)

## Hur man konfigurerar

### Förutsättningar

För att använda bank-API:erna behöver du:

1. **För Swedbank och ICA Banken**:
   - Registrera din applikation hos banken
   - Erhåll Client ID och Client Secret
   - Konfigurera redirect URI
   - Eventuellt: eIDAS-certifikat (för produktion)

2. **För Avanza**:
   - Ett aktivt Avanza-konto
   - Användarnamn och lösenord
   - TOTP-app om du har 2FA aktiverat

### Konfiguration i appsettings.json

Lägg till följande i `appsettings.json`:

```json
{
  "Swedbank": {
    "ClientId": "ditt-client-id",
    "ClientSecret": "ditt-client-secret",
    "Environment": "sandbox" // eller "production"
  },
  "IcaBanken": {
    "ClientId": "ditt-client-id",
    "ClientSecret": "ditt-client-secret",
    "Environment": "sandbox"
  }
}
```

**Viktigt**: I produktion, lagra aldrig nycklar i appsettings.json. Använd:
- Azure Key Vault
- Environment Variables
- User Secrets (för development)

### Säkerhet och kryptering

För produktion bör du:

1. **Kryptera tokens i databasen**:
   - Implementera kryptering för AccessToken och RefreshToken
   - Använd Data Protection API eller liknande

2. **Validera OAuth state**:
   - Implementera CSRF-skydd i OAuth-flödet
   - Lagra state i session och validera vid callback

3. **Säker lagring av credentials**:
   - Använd Azure Key Vault eller motsvarande
   - Aldrig hardcoda credentials i kod

## Användning via API

### 1. Lista tillgängliga banker

```bash
GET /api/bankconnections/available-banks
```

Response:
```json
["Swedbank", "Avanza", "ICA-banken"]
```

### 2. Initiera OAuth-autentisering

```bash
POST /api/bankconnections/authorize
Content-Type: application/json

{
  "bankName": "Swedbank"
}
```

Response:
```json
{
  "authorizationUrl": "https://psd2.auth.swedbank.com/oauth2/authorize?...",
  "state": "abc123...",
  "redirectUri": "https://localhost:7023/api/bankconnections/callback"
}
```

Användaren ska öppna `authorizationUrl` och genomföra BankID-autentisering.

### 3. Slutför anslutning

Efter att användaren autentiserat sig och blivit redirectad till callback-URL med en `code`:

```bash
POST /api/bankconnections/connect
Content-Type: application/json

{
  "bankName": "Swedbank",
  "code": "auth-code-från-callback",
  "redirectUri": "https://localhost:7023/api/bankconnections/callback",
  "bankSourceId": 2
}
```

Response:
```json
{
  "bankConnectionId": 1,
  "bankSourceId": 2,
  "apiType": "PSD2",
  "status": "Active",
  "lastSyncedAt": null,
  "autoSyncEnabled": false
}
```

### 4. Hämta konton

```bash
GET /api/bankconnections/1/accounts
```

Response:
```json
[
  {
    "accountId": "SE123456789...",
    "accountName": "Privatkonto",
    "iban": "SE1234567890123456789012",
    "currency": "SEK",
    "balance": 25430.50,
    "accountType": "checking"
  }
]
```

### 5. Importera transaktioner

```bash
POST /api/bankconnections/1/import
Content-Type: application/json

{
  "accountId": "SE123456789...",
  "fromDate": "2025-01-01",
  "toDate": "2025-01-20",
  "skipDuplicates": true
}
```

Response:
```json
{
  "success": true,
  "importedCount": 45,
  "duplicateCount": 3,
  "errorCount": 0,
  "errors": [],
  "lastTransactionDate": "2025-01-20T10:30:00"
}
```

## Användning via UI (kommande)

En grafisk gränssnitt kommer att läggas till i Blazor Web-applikationen för att hantera bankkopplingar:

1. Navigera till **Bankkopplingar** i menyn
2. Klicka på **Lägg till bank**
3. Välj bank från listan
4. Autentisera dig med BankID
5. Välj konton att synkronisera
6. Konfigurera automatisk synkronisering (valfritt)

## Automatisk synkronisering

För att aktivera automatisk synkronisering kan du implementera en background service som:

1. Kör periodiskt (t.ex. varje timme eller dagligen)
2. Kontrollerar alla aktiva bankkopplingar
3. Synkroniserar transaktioner för varje koppling
4. Skickar notifikationer vid fel

Exempel på implementation kommer i framtida uppdatering.

## Felsökning

### Problem: "Session expired" för Avanza

**Lösning**: Avanza-sessioner har begränsad livslängd. Du måste autentisera dig på nytt.

### Problem: "Token expired" för PSD2-banker

**Lösning**: Applikationen försöker automatiskt förnya token med refresh token. Om det misslyckas måste du autentisera på nytt.

### Problem: "Failed to obtain access token"

**Möjliga orsaker**:
- Felaktiga Client ID/Secret
- Ogiltig redirect URI
- Användaren avbröt autentiseringen
- Nätverksproblem

**Lösning**: Kontrollera konfigurationen och försök igen.

### Problem: Kan inte hitta konton

**Möjliga orsaker**:
- Kontot stöds inte (t.ex. utländska valutor)
- API-åtkomst inte beviljad för kontotyp
- Token har inte rätt scope

**Lösning**: Kontrollera att rätt scope begärdes vid autentisering.

## API-begränsningar och bästa praxis

### Rate Limiting

Bankernas API:er har begränsningar:
- Swedbank PSD2: Max 4 anrop per sekund
- ICA Banken: Max 10 anrop per minut
- Avanza: Okänd, men begränsat

**Rekommendation**: Implementera exponential backoff och respektera rate limits.

### Datalagring

- Tokens ska krypteras i databas
- Transaktionsdata kan cachas
- Respektera GDPR och dataskydd

### Token Management

- Access tokens är giltiga i 90 dagar (PSD2)
- Refresh tokens i 180 dagar (PSD2)
- Implementera automatisk förnyelse
- Notifiera användare när token snart går ut

## Säkerhetsöverväganden

### PSD2-krav

- Stark kundautentisering (SCA) krävs
- BankID används för autentisering i Sverige
- Samtycke från användare krävs för dataåtkomst
- Tidsbegränsad åtkomst (90-180 dagar)

### Rekommendationer

1. Använd HTTPS för all kommunikation
2. Kryptera känslig data i databas
3. Implementera audit logging
4. Regelbunden säkerhetsgenomgång
5. Följ OWASP security guidelines

## Utveckling och testning

### Sandbox-miljöer

Alla tre banker erbjuder sandbox-miljöer för utveckling:

- **Swedbank**: https://psd2.api.swedbank.com/sandbox
- **ICA Banken**: Kontakta ICA för sandbox-åtkomst
- **Avanza**: Använd test-konto (risk för fel med produktionsdata)

### Testdata

I sandbox kan du:
- Skapa testkonton
- Generera testtransaktioner
- Testa OAuth-flöden
- Verifiera integration

## Support och dokumentation

### Officiell dokumentation

- **Swedbank PSD2**: https://developer.swedbank.com
- **ICA Banken**: Kontakta ICA Banken direkt
- **Avanza**: Inofficiell API-dokumentation finns på GitHub

### Frågor och problem

För frågor om denna implementation:
1. Öppna ett issue på GitHub
2. Kontrollera befintliga issues först
3. Inkludera relevanta logs (utan känslig data)

## Framtida förbättringar

Planerade förbättringar inkluderar:

- [ ] Grafiskt gränssnitt för hantering av bankkopplingar
- [ ] Automatisk synkronisering med scheduler
- [ ] Notifikationer vid nya transaktioner
- [ ] Stöd för fler banker (Nordea, SEB, Handelsbanken)
- [ ] Token-kryptering i databas
- [ ] OAuth state-validering
- [ ] Exportera konfiguration för backup

## Licens och ansvar

Denna implementation är ett exempel och bör ses över av säkerhetsexperter innan produktion. Utvecklaren ansvarar inte för dataförlust eller säkerhetsbrott. Följ alltid bankernas användarvillkor och API-policy.

## Changelog

### Version 1.0 (2025-01-20)
- Initial implementation av PSD2-stöd
- Stöd för Swedbank, Avanza och ICA Banken
- Basic OAuth2-flöde
- API-endpoints för bank connection management
- Transaction import funktionalitet
