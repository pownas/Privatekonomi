# Tillfälligt Inaktivera Autentisering för Gemensamma Sparmål (Development)

## Översikt

För att snabba upp testningen under utveckling kan autentiseringen för gemensamma sparmål tillfälligt inaktiveras. Detta gör att du inte behöver logga in varje gång applikationen startas om under utvecklingsarbetet.

## ⚠️ Viktigt

**Detta är endast för utvecklingsmiljö och ska ALDRIG användas i produktion!**

Funktionen är automatiskt begränsad till:
- Endast `Development` environment
- Kräver explicit konfiguration i `appsettings.Development.json`

## Så här aktiverar du funktionen

1. Öppna filen `src/Privatekonomi.Web/appsettings.Development.json`

2. Ändra `DevDisableAuth` till `true`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "DetailedErrors": true,
  "CircuitOptions": {
    "DetailedErrors": true
  },
  "FeatureFlags": {
    "DevDisableAuth": true
  }
}
```

3. Starta om applikationen

4. Du är nu automatiskt inloggad som testanvändaren (`test@example.com`) när du besöker sidor för gemensamma sparmål

## Så här inaktiverar du funktionen

1. Öppna filen `src/Privatekonomi.Web/appsettings.Development.json`

2. Ändra `DevDisableAuth` till `false`:

```json
{
  "FeatureFlags": {
    "DevDisableAuth": false
  }
}
```

3. Starta om applikationen

4. Normal autentisering är nu aktiverad

## Hur det fungerar tekniskt

När `DevDisableAuth` är aktiverad i utvecklingsmiljö:

1. `ICurrentUserService` implementationen byts ut från `CurrentUserService` till `DevCurrentUserService`
2. `DevCurrentUserService` returnerar alltid testanvändarens ID (`test@example.com`) istället för att kontrollera HTTP-kontexten
3. Alla säkerhetskontroller i `SharedGoalService` använder denna mock-användare
4. Inga ändringar krävs i sidorna eller andra delar av applikationen

## Begränsningar

- Fungerar endast i Development environment
- Använder alltid testanvändaren från seedade testdata
- Påverkar alla sidor som använder `ICurrentUserService`, inte bara gemensamma sparmål
- Konfigurationsfilen `appsettings.Development.json` finns inte i produktion

## Felsökning

**Problem:** Jag får fortfarande "User is not authenticated" fel

**Lösning:**
- Kontrollera att `ASPNETCORE_ENVIRONMENT` är satt till `Development`
- Verifiera att `DevDisableAuth` är `true` i `appsettings.Development.json`
- Starta om applikationen efter att ha ändrat konfigurationen
- Kontrollera att testanvändaren finns i databasen (skapas automatiskt vid start)

**Problem:** Funktionen är aktiverad i produktion

**Lösning:**
- Detta är tekniskt omöjligt eftersom:
  1. Funktionen kräver `IsDevelopment()` att vara `true`
  2. `appsettings.Development.json` laddas inte i produktion
  3. Production använder `appsettings.json` eller miljövariabler
