# Onboarding Guide - Privatekonomi

## Översikt

Privatekonomi har en omfattande onboarding-process som guidar nya användare genom inställningen av sin ekonomiapp. Denna guide beskriver hur onboarding-flödet fungerar och hur det kan anpassas.

## Onboarding-flödet

När en ny användare registrerar sig kommer de automatiskt till onboarding-flödet som består av 6 steg:

### Steg 1: Välkommen (Welcome)
**Route:** `/onboarding` eller `/onboarding/0`

- Presenterar applikationen och dess huvudfunktioner
- Listar vad som kommer att göras under onboarding
- Information om integritet och datasäkerhet
- Uppskattad tid: ~5 minuter

**Funktioner:**
- Visar en välkomsthälsning
- Listar kommande steg i flödet
- "Kom igång"-knapp för att fortsätta

### Steg 2: Välj bank (Bank Selection)
**Route:** `/onboarding/1`

- Användaren kan välja sin bank för automatisk transaktionsimport
- Sökning och filtrering av svenska banker
- Möjlighet att hoppa över detta steg

**Funktioner:**
- Visar lista över tillgängliga banker från `BankSource`-tabellen
- Sökfunktion för att filtrera banker
- Grafiska kort för varje bank
- "Hoppa över"-knapp för att fortsätta utan bankkoppling

**Teknisk implementation:**
- Använder `IBankSourceService` för att hämta banker
- I framtiden: Integration med PSD2 OAuth-flöde för autentisering

### Steg 3: Samtycke (Consent)
**Route:** `/onboarding/2`

- Information om dataskydd och integritetspolicy
- GDPR-information
- PSD2-information för bankkoppling
- Användaren måste godkänna innan fortsättning

**Funktioner:**
- Detaljerad information om datasäkerhet
- Expanderbar sektion med fullständig integritetspolicy
- Kryssruta som måste markeras för att fortsätta
- Länk till `/privacy` för mer information

### Steg 4: Importera transaktioner (Transaction Import)
**Route:** `/onboarding/3`

- Simulerar import av transaktioner från de senaste 12-18 månaderna
- Automatisk kategorisering av transaktioner
- Visar snabb översikt av kategorier

**Funktioner:**
- Simulerar import med laddningsindikator
- Visar antal importerade transaktioner
- Listar topp 5 kategorier med antal och belopp
- Möjlighet att hoppa över

**Teknisk implementation:**
- Just nu: Simulerad data för demonstration
- I framtiden: Riktig import via PSD2-API
- Använder `ITransactionService` och `ICategoryService`

### Steg 5: Budgetförslag (Budget Proposal)
**Route:** `/onboarding/4`

- Föreslår budget baserat på 50/30/20-regeln
- Interaktiva reglage för att justera kategorier
- Aktivering av budgetvarningar

**Funktioner:**
- Beräknar månadsbudget baserat på 50/30/20-regeln:
  - 50% Behov (boende, mat, transport)
  - 30% Önskemål (nöje, shopping)
  - 20% Sparande
- Visar detaljerad fördelning per kategori
- Justerbara belopp med sliders
- Checkbox för budgetvarningar
- Möjlighet att hoppa över

**Teknisk implementation:**
- Använder `BudgetTemplateService.ApplyFiftyThirtyTwentyTemplate()`
- I framtiden: Beräknar faktisk inkomst från importerade transaktioner
- Sparar budget med `IBudgetService`

### Steg 6: Klar (Completion)
**Route:** `/onboarding/5`

- Sammanfattning av användarens ekonomi
- Förslag på nästa steg
- Slutför onboarding och går till dashboard

**Funktioner:**
- Visar översikt med inkomster, utgifter och sparande
- Listar förslag på nästa åtgärder
- "Gå till Dashboard"-knapp som slutför onboarding

## Teknisk implementation

### ApplicationUser-modellen
Två nya fält har lagts till:
```csharp
public bool OnboardingCompleted { get; set; } = false;
public DateTime? OnboardingCompletedAt { get; set; }
```

### OnboardingService
Service för att hantera onboarding-status:

```csharp
public interface IOnboardingService
{
    Task<bool> HasCompletedOnboardingAsync(string userId);
    Task CompleteOnboardingAsync(string userId);
    Task<int> GetCurrentStepAsync(string userId);
    Task SetCurrentStepAsync(string userId, int step);
}
```

### Flödeskontroll
1. **Register.razor:** Efter registrering → redirect till `/onboarding`
2. **Home.razor:** Kontrollerar `OnboardingCompleted`, redirectar till `/onboarding` om false
3. **Onboarding.razor:** Huvudkomponent som koordinerar stegen
4. När användaren klickar "Gå till Dashboard" → `OnboardingService.CompleteOnboardingAsync()` → redirect till `/`

### Komponenter
Alla onboarding-komponenter finns i:
```
/src/Privatekonomi.Web/Components/Pages/Onboarding/
├── Onboarding.razor                    # Huvudkomponent
├── OnboardingWelcome.razor             # Steg 1
├── OnboardingBankSelection.razor       # Steg 2
├── OnboardingConsent.razor             # Steg 3
├── OnboardingTransactionImport.razor   # Steg 4
├── OnboardingBudgetProposal.razor      # Steg 5
└── OnboardingCompletion.razor          # Steg 6
```

### Styling
CSS-styling finns i:
```
/src/Privatekonomi.Web/wwwroot/css/onboarding.css
```

Inkluderar:
- Gradient bakgrund
- Fade-in animationer
- Hover-effekter för bank-kort
- Responsiv design för mobil/desktop

## Anpassning

### Lägga till fler steg
1. Skapa ny Razor-komponent i `/Onboarding/` katalogen
2. Lägg till steget i `Onboarding.razor` med rätt `_currentStep`-värde
3. Uppdatera navigeringen i `NextStep()`-metoden

### Ändra stegordning
Redigera `Onboarding.razor` och justera if-villkoren för `_currentStep`.

### Anpassa 50/30/20-fördelning
Redigera `OnboardingBudgetProposal.razor` och justera procentvärdena i `OnInitializedAsync()`.

### Byta bank-API
När riktig PSD2-integration finns:
1. Uppdatera `OnboardingBankSelection.razor` → `SelectBank()`-metoden
2. Lägg till OAuth-flöde för autentisering
3. Integrera med `IBankConnectionService`

## Testning

### Manuell testning
1. Registrera ny användare på `/Account/Register`
2. Följ onboarding-flödet steg för steg
3. Verifiera att varje steg fungerar korrekt
4. Kontrollera att redirect till dashboard sker efter slutfört

### Unit tests
Kör onboarding-testerna:
```bash
dotnet test --filter "FullyQualifiedName~OnboardingServiceTests"
```

Täcker:
- `HasCompletedOnboardingAsync()` - olika scenarios
- `CompleteOnboardingAsync()` - markera som klar
- `GetCurrentStepAsync()` - hämta nuvarande steg

## Vanliga frågor

### Kan användare hoppa över onboarding?
Ja, många steg har "Hoppa över"-knappar. Hela onboarding-flödet kan tekniskt hoppas över genom att manuellt sätta `OnboardingCompleted = true` i databasen.

### Kan användare gå tillbaka till onboarding?
Just nu finns ingen funktion för detta. I framtiden kan en "Kör onboarding igen"-knapp läggas till i inställningar.

### Vad händer med befintliga användare?
De får `OnboardingCompleted = false` som standard. Vid första inloggning efter uppdatering kommer de att omdirigeras till onboarding. För att undvika detta kan en migration köras som sätter befintliga användare till `true`.

### Hur påverkar detta test-användare?
Test-användaren (`test@example.com`) som skapas vid seed behöver också uppdateras för att sätta `OnboardingCompleted = true`.

## Framtida förbättringar

1. **Steg-indikator:** Visuell progress bar som visar var användaren är i flödet
2. **Bakåt-navigation:** Möjlighet att gå tillbaka till tidigare steg
3. **Spara framsteg:** Spara vilket steg användaren är på om de avbryter
4. **A/B-testning:** Testa olika budgetregler (60/20/20, Zero-based, etc.)
5. **Personalisering:** Fråga om hushållstyp (singel, par, familj) för bättre budgetförslag
6. **Integrationsguide:** Efter onboarding, visa interaktiv guide för första transaktionen
7. **Video tutorials:** Korta videoklipp för varje huvudfunktion

## Support och bidrag

För frågor eller problem, öppna en issue på GitHub.
För bidrag, skapa en PR med beskrivning av ändringar.

## Se även

- [LANDING_PAGE_GUIDE.md](LANDING_PAGE_GUIDE.md) - Landningssida
- [BUDGET_GUIDE.md](BUDGET_GUIDE.md) - Budgethantering
- [CSV_IMPORT_GUIDE.md](CSV_IMPORT_GUIDE.md) - CSV-import
- [PSD2_API_GUIDE.md](PSD2_API_GUIDE.md) - Bankkoppling
