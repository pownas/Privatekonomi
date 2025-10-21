# Implementationsguide: Sverige-specifika integrationer

Detta dokument beskriver hur de Sverige-specifika funktionerna har lagts till i systemet och hur de kan användas.

## Översikt

Följande funktioner har lagts till eller utökats:

1. ✅ **Utökade datamodeller** för svenska behov
2. ✅ **SIE-export** för bokföringssystem
3. ✅ **K4-generator** för kapitalvinster
4. ✅ **ROT/RUT-avdrag** hantering
5. ✅ **ISK/KF schablonbeskattning** beräkning
6. ✅ **Bolån-förbättring** med bindningstid och belåningsgrad
7. ✅ **CSN-lån** stöd
8. ✅ **E-faktura/Autogiro** metadata
9. ✅ **Reseavdrag** spårning
10. ✅ **Kreditvärdighet** registrering

---

## Nya datamodeller

### TaxDeduction (ROT/RUT-avdrag)

**Syfte:** Hantera ROT- och RUT-avdrag enligt svenska skatteregler.

**Egenskaper:**
- `Type`: "ROT" eller "RUT"
- `Amount`: Totalt betalt belopp
- `DeductibleAmount`: Avdragsgillt belopp (automatiskt beräknat)
- `ServiceProvider`: Namn på tjänsteleverantör
- `OrganizationNumber`: Organisationsnummer (krävs för ROT/RUT)
- `WorkDescription`: Beskrivning av utfört arbete
- `TaxYear`: Beskattningsår

**Användning:**
```csharp
var rotDeduction = new TaxDeduction
{
    TransactionId = transaction.TransactionId,
    Type = "ROT",
    Amount = 20000,
    ServiceProvider = "Bygg AB",
    OrganizationNumber = "556123-4567",
    WorkDescription = "Renovering av badrum",
    WorkDate = DateTime.Now,
    TaxYear = 2025
};

await taxDeductionService.AddDeductionAsync(rotDeduction);
// DeductibleAmount beräknas automatiskt: 20000 * 50% = 10000 SEK
```

**Regler:**
- ROT: 50% avdrag på arbetskostnad, max 50 000 SEK/person/år
- RUT: 50% avdrag på totalkostnad, max 75 000 SEK/person/år

---

### CapitalGain (Kapitalvinster för K4)

**Syfte:** Registrera och beräkna kapitalvinster/förluster för K4-deklaration.

**Egenskaper:**
- `InvestmentId`: Koppling till investering
- `SaleDate`: Försäljningsdatum
- `Quantity`: Antal sålda värdepapper
- `TotalPurchasePrice`: Totalt inköpspris
- `TotalSalePrice`: Totalt försäljningspris
- `Gain`: Vinst/förlust (beräknas automatiskt)
- `SecurityType`: "Stock", "Fund", "Crypto", "Bond"
- `IsISK`: Om försäljning skett från ISK-konto
- `TaxYear`: Beskattningsår

**Användning:**
```csharp
var capitalGain = new CapitalGain
{
    InvestmentId = investment.InvestmentId,
    SaleDate = DateTime.Now,
    Quantity = 100,
    TotalPurchasePrice = 10000,
    TotalSalePrice = 15000,
    Gain = 5000, // Vinst
    SecurityType = "Stock",
    SecurityName = "Volvo B",
    ISIN = "SE0000115446",
    TaxYear = 2025,
    IsISK = false
};

await context.CapitalGains.AddAsync(capitalGain);
```

**K4-rapport:**
```csharp
var k4Report = await k4Generator.GenerateK4ReportAsync(householdId, 2025);
Console.WriteLine($"Totala vinster: {k4Report.TotalGain} SEK");
Console.WriteLine($"Totala förluster: {k4Report.TotalLoss} SEK");
Console.WriteLine($"Nettovinst: {k4Report.NetGain} SEK");
Console.WriteLine($"Skattepliktig vinst (30%): {k4Report.TaxableGain} SEK");

// Exportera till text
var k4Text = await k4Generator.ExportK4ToTextAsync(householdId, 2025);
File.WriteAllText("k4_2025.txt", k4Text);
```

---

### CommuteDeduction (Reseavdrag)

**Syfte:** Spåra arbetsresor för reseavdrag i deklarationen.

**Egenskaper:**
- `FromAddress`: Från-adress (hemadress)
- `ToAddress`: Till-adress (arbetsplats)
- `DistanceKm`: Avstånd i kilometer (en väg)
- `NumberOfTrips`: Antal resor (vanligtvis 2 för tur och retur)
- `TransportMethod`: "Car", "PublicTransport", "Bicycle"
- `Cost`: Faktisk kostnad
- `DeductibleAmount`: Avdragsgillt belopp
- `TaxYear`: Beskattningsår

**Användning:**
```csharp
var commuteDeduction = new CommuteDeduction
{
    Date = DateTime.Now,
    FromAddress = "Hemvägen 1, Stockholm",
    ToAddress = "Kontorsvägen 10, Uppsala",
    DistanceKm = 65,
    NumberOfTrips = 2, // Tur och retur
    TransportMethod = "Car",
    Cost = 200, // Bensin
    DeductibleAmount = CalculateDeduction(65, 2), // Beräknas enligt Skatteverkets regler
    TaxYear = 2025,
    IsRegularCommute = true
};

await context.CommuteDeductions.AddAsync(commuteDeduction);
```

---

### CreditRating (Kreditvärdighet)

**Syfte:** Registrera kreditvärdering från UC eller andra kreditupplysningsföretag.

**Egenskaper:**
- `Provider`: "UC", "Creditsafe", "Bisnode"
- `Rating`: Betyg (t.ex. "AAA", "AA")
- `Score`: Numeriskt poäng
- `CheckedDate`: Datum för kontroll
- `PaymentRemarks`: Antal betalningsanmärkningar
- `TotalDebt`: Total skuld enligt kreditupplysning
- `CreditLimit`: Kreditgräns
- `CreditUtilization`: Procentuell kreditanvändning

**Användning:**
```csharp
var creditRating = new CreditRating
{
    HouseholdId = household.HouseholdId,
    Provider = "UC",
    Rating = "AA",
    Score = 750,
    CheckedDate = DateTime.Now,
    PaymentRemarks = 0,
    TotalDebt = 150000,
    CreditLimit = 200000,
    CreditUtilization = 75
};

await context.CreditRatings.AddAsync(creditRating);
```

---

## Utökade befintliga modeller

### Investment - ISK/KF-stöd

**Nya egenskaper:**
- `AccountType`: "ISK", "KF", "AF", "Depå"
- `SchablonTax`: Beräknad schablonintäkt för ISK/KF
- `SchablonTaxYear`: Beskattningsår för schablonintäkten

**Användning:**
```csharp
var iskInvestment = new Investment
{
    Name = "Volvo B",
    Type = "Aktie",
    Quantity = 100,
    PurchasePrice = 100,
    CurrentPrice = 150,
    AccountType = "ISK", // Investeringssparkonto
    // ...
};

// Beräkna schablonintäkt
var calculator = new ISKTaxCalculator(context);
var totalTax = await calculator.CalculateSchablonTaxAsync(householdId, 2025);

// Schablonintäkt = Kapitalunderlaget * (Statslåneräntan + 1%) * 30%
```

**ISK-beräkning:**
```csharp
// För 2024: Statslåneräntan = 2.84%
// Schablonintäkt = 150000 * (0.0284 + 0.01) * 0.30 = 1731 SEK skatt
```

---

### Transaction - Betalningsmetoder

**Nya egenskaper:**
- `PaymentMethod`: "Swish", "Autogiro", "E-faktura", "Banköverföring", "Kort", "Kontant"
- `RecipientBankgiro`: Bankgironummer
- `RecipientPlusgiro`: Plusgironummer
- `InvoiceNumber`: Fakturanummer
- `OCR`: OCR-nummer
- `IsRecurring`: Om det är en återkommande betalning

**Användning:**
```csharp
var transaction = new Transaction
{
    Amount = -500,
    Description = "Elräkning",
    Date = DateTime.Now,
    PaymentMethod = "Autogiro",
    RecipientBankgiro = "5050-1234",
    InvoiceNumber = "INV-2025-001",
    OCR = "1234567890",
    IsRecurring = true,
    IsIncome = false
};
```

---

### Loan - Bolån och CSN-lån

**Nya egenskaper för bolån:**
- `PropertyAddress`: Fastighetens adress
- `PropertyValue`: Fastighetsvärde
- `LTV`: Belåningsgrad (beräknas automatiskt)
- `LoanProvider`: Bank/långivare
- `IsFixedRate`: Fast eller rörlig ränta
- `RateResetDate`: Datum när bindningstiden löper ut
- `BindingPeriodMonths`: Bindningstid i månader

**Nya egenskaper för CSN-lån:**
- `CSN_LoanType`: "Studiemedel", "Studiemedelsränta"
- `CSN_StudyYear`: Studieår
- `CSN_MonthlyPayment`: Månadsbetalning till CSN
- `CSN_RemainingAmount`: Återstående skuld
- `CSN_LastUpdate`: Senast uppdaterat

**Användning - Bolån:**
```csharp
var mortgageLoan = new Loan
{
    Name = "Bolån villa Storgatan",
    Type = "Bolån",
    Amount = 3000000,
    InterestRate = 5.5m,
    Amortization = 10000,
    PropertyAddress = "Storgatan 1, Stockholm",
    PropertyValue = 4000000,
    // LTV beräknas automatiskt: 3000000 / 4000000 * 100 = 75%
    LoanProvider = "Swedbank",
    IsFixedRate = true,
    RateResetDate = DateTime.Now.AddYears(3),
    BindingPeriodMonths = 36,
    StartDate = DateTime.Now,
    MaturityDate = DateTime.Now.AddYears(30)
};
```

**Användning - CSN-lån:**
```csharp
var csnLoan = new Loan
{
    Name = "CSN-studielån",
    Type = "CSN-lån",
    Amount = 150000,
    InterestRate = 1.5m,
    CSN_LoanType = "Studiemedel",
    CSN_StudyYear = 2020,
    CSN_MonthlyPayment = 1200,
    CSN_RemainingAmount = 145000,
    CSN_LastUpdate = DateTime.Now,
    StartDate = new DateTime(2020, 8, 1),
    Currency = "SEK"
};
```

---

## Nya Services

### SieExporter - SIE-export för bokföring

**Syfte:** Exportera transaktioner i SIE-format för import i bokföringsprogram.

**Användning:**
```csharp
// Injicera service
services.AddScoped<ISieExporter, SieExporter>();

// Använd i controller/service
var sieExporter = serviceProvider.GetService<ISieExporter>();

// Exportera för ett helt år
var sieContent = await sieExporter.ExportToSie4Async(householdId: 1, year: 2025);

// Eller med anpassat datumintervall
var sieContent = await sieExporter.ExportToSie4Async(
    householdId: 1, 
    fromDate: new DateTime(2025, 1, 1), 
    toDate: new DateTime(2025, 12, 31)
);

// Spara till fil
await File.WriteAllTextAsync("bokforing_2025.se", sieContent);
```

**SIE-format:**
```text
#FLAGGA 0
#PROGRAM "Privatekonomi" "1.0"
#FORMAT PC8
#GEN 20250121
#SIETYP 4
#FNR "1"
#RAR 0 20250101 20251231

# Kontoplan
#KONTO 1930 "Bank"
#KONTO 3000 "Lön"
#KONTO 5000 "Mat & Dryck"
#KONTO 5010 "Transport"
...

#VER "A" "1" 20250115 "ICA Maxi Stormarknad"
{
   #TRANS 1930 {} -423.50
   #TRANS 5000 {} 423.50
}
```

**Import i bokföringsprogram:**
1. Fortnox: Importera → SIE-fil
2. Visma: Filimport → SIE
3. Hogia: Importfunktion → SIE-format

---

### K4Generator - Kapitalvinstrapport

**Syfte:** Generera K4-blankett för kapitalvinster i deklarationen.

**Användning:**
```csharp
// Injicera service
services.AddScoped<IK4Generator, K4Generator>();

// Generera rapport
var k4Report = await k4Generator.GenerateK4ReportAsync(householdId: 1, taxYear: 2025);

// Visa sammanfattning
Console.WriteLine($"Totala kapitalvinster: {k4Report.TotalGain:N2} SEK");
Console.WriteLine($"Totala kapitalförluster: {k4Report.TotalLoss:N2} SEK");
Console.WriteLine($"Nettovinst/-förlust: {k4Report.NetGain:N2} SEK");
Console.WriteLine($"Skattepliktig vinst (30%): {k4Report.TaxableGain:N2} SEK");

// Exportera till textformat
var k4Text = await k4Generator.ExportK4ToTextAsync(householdId: 1, taxYear: 2025);
await File.WriteAllTextAsync("K4_2025.txt", k4Text);
```

**Output exempel:**
```text
================================================================================
K4 - BLANKETT FÖR KAPITALINKOMSTER
Beskattningsår: 2025
================================================================================

SAMMANFATTNING
--------------------------------------------------------------------------------
Totala kapitalvinster:    15,000.00 SEK
Totala kapitalförluster:  5,000.00 SEK
Nettovinst/-förlust:      10,000.00 SEK
Skattepliktig vinst (30%): 3,000.00 SEK

SAMMANFATTNING PER VÄRDEPAPPERSTYP
--------------------------------------------------------------------------------
Typ             Antal      Vinst           Förlust         Netto          
--------------------------------------------------------------------------------
Stock           3          12,000.00       2,000.00        10,000.00      
Fund            2          3,000.00        3,000.00        0.00           

DETALJERADE TRANSAKTIONER
--------------------------------------------------------------------------------
Namn                      ISIN         Sälj.datum   Antal      Vinst/Förlust
--------------------------------------------------------------------------------
Volvo B                   SE0000115446 2025-03-15   100        5,000.00
...
```

---

### TaxDeductionService - ROT/RUT-hantering

**Syfte:** Hantera och beräkna ROT- och RUT-avdrag.

**Användning:**
```csharp
// Injicera service
services.AddScoped<ITaxDeductionService, TaxDeductionService>();

// Lägg till ROT-avdrag
var rotDeduction = new TaxDeduction
{
    TransactionId = transaction.TransactionId,
    Type = "ROT",
    Amount = 20000,
    ServiceProvider = "Bygg AB",
    OrganizationNumber = "556123-4567",
    WorkDescription = "Renovering av badrum",
    WorkDate = DateTime.Now,
    TaxYear = 2025
};

await taxDeductionService.AddDeductionAsync(rotDeduction);
// DeductibleAmount sätts automatiskt till 10000 (50% av 20000)

// Hämta alla avdrag för ett år
var deductions = await taxDeductionService.GetDeductionsByYearAsync(2025);

// Beräkna totalt avdrag
var totalDeduction = await taxDeductionService.GetTotalDeductibleAmountAsync(2025);
Console.WriteLine($"Totalt ROT/RUT-avdrag 2025: {totalDeduction:N2} SEK");
```

---

### ISKTaxCalculator - ISK/KF schablonbeskattning

**Syfte:** Beräkna schablonintäkt för ISK- och KF-konton.

**Användning:**
```csharp
// Injicera service
services.AddScoped<IISKTaxCalculator, ISKTaxCalculator>();

// Beräkna schablonintäkt för alla ISK/KF-konton
var totalTax = await iskCalculator.CalculateSchablonTaxAsync(householdId: 1, taxYear: 2025);
Console.WriteLine($"Total schablonintäkt 2025: {totalTax:N2} SEK");

// Beräkna för en specifik investering
var capitalBase = 150000m; // Kapitalunderlag
var tax = iskCalculator.CalculateSchablonTaxForInvestment(capitalBase, 2025);

// Hämta statslåneräntan
var governmentRate = iskCalculator.GetGovernmentLendingRate(2025);
Console.WriteLine($"Statslåneräntan 2025: {governmentRate:P2}");
```

**Beräkningsformel:**
```
Schablonintäkt = Kapitalunderlag * (Statslåneräntan + 1%) * 30%

Exempel för 2024 (statslåneräntan 2.84%):
150 000 * (0.0284 + 0.01) * 0.30 = 1 731 SEK
```

---

## Integration i applikationen

### Registrera services i Program.cs

```csharp
// I Privatekonomi.Web/Program.cs eller Privatekonomi.Api/Program.cs

// Sverige-specifika services
builder.Services.AddScoped<ISieExporter, SieExporter>();
builder.Services.AddScoped<IK4Generator, K4Generator>();
builder.Services.AddScoped<ITaxDeductionService, TaxDeductionService>();
builder.Services.AddScoped<IISKTaxCalculator, ISKTaxCalculator>();
```

### Skapa API-endpoints

Exempel på controller för SIE-export:

```csharp
[ApiController]
[Route("api/[controller]")]
public class ExportController : ControllerBase
{
    private readonly ISieExporter _sieExporter;
    private readonly IK4Generator _k4Generator;
    
    public ExportController(ISieExporter sieExporter, IK4Generator k4Generator)
    {
        _sieExporter = sieExporter;
        _k4Generator = k4Generator;
    }
    
    [HttpGet("sie/{year}")]
    public async Task<IActionResult> ExportSie(int householdId, int year)
    {
        var sieContent = await _sieExporter.ExportToSie4Async(householdId, year);
        var fileName = $"bokforing_{year}.se";
        return File(Encoding.UTF8.GetBytes(sieContent), "text/plain", fileName);
    }
    
    [HttpGet("k4/{year}")]
    public async Task<IActionResult> ExportK4(int householdId, int year)
    {
        var k4Text = await _k4Generator.ExportK4ToTextAsync(householdId, year);
        var fileName = $"K4_{year}.txt";
        return File(Encoding.UTF8.GetBytes(k4Text), "text/plain", fileName);
    }
}
```

### Skapa Blazor-komponenter

Exempel på sida för ROT/RUT-avdrag:

```razor
@page "/tax-deductions"
@inject ITaxDeductionService TaxDeductionService

<MudContainer>
    <MudText Typo="Typo.h4">ROT/RUT-avdrag</MudText>
    
    <MudCard>
        <MudCardContent>
            <MudSelect @bind-Value="selectedYear" Label="Beskattningsår">
                @for (int year = DateTime.Now.Year - 2; year <= DateTime.Now.Year; year++)
                {
                    <MudSelectItem Value="@year">@year</MudSelectItem>
                }
            </MudSelect>
            
            <MudText Typo="Typo.h6" Class="mt-4">
                Totalt avdrag: @totalDeduction.ToString("N2") SEK
            </MudText>
            
            <MudTable Items="@deductions" Hover="true">
                <HeaderContent>
                    <MudTh>Typ</MudTh>
                    <MudTh>Datum</MudTh>
                    <MudTh>Tjänsteleverantör</MudTh>
                    <MudTh>Belopp</MudTh>
                    <MudTh>Avdrag</MudTh>
                </HeaderContent>
                <RowTemplate>
                    <MudTd>@context.Type</MudTd>
                    <MudTd>@context.WorkDate.ToShortDateString()</MudTd>
                    <MudTd>@context.ServiceProvider</MudTd>
                    <MudTd>@context.Amount.ToString("N2")</MudTd>
                    <MudTd>@context.DeductibleAmount.ToString("N2")</MudTd>
                </RowTemplate>
            </MudTable>
        </MudCardContent>
    </MudCard>
</MudContainer>

@code {
    private int selectedYear = DateTime.Now.Year;
    private List<TaxDeduction> deductions = new();
    private decimal totalDeduction;
    
    protected override async Task OnInitializedAsync()
    {
        await LoadDeductions();
    }
    
    private async Task LoadDeductions()
    {
        deductions = await TaxDeductionService.GetDeductionsByYearAsync(selectedYear);
        totalDeduction = await TaxDeductionService.GetTotalDeductibleAmountAsync(selectedYear);
    }
}
```

---

## Databas-migration

För att lägga till de nya tabellerna i databasen:

```bash
# Skapa migration
dotnet ef migrations add AddSwedishIntegrations --project src/Privatekonomi.Core

# Uppdatera databasen
dotnet ef database update --project src/Privatekonomi.Web
```

**OBS:** För InMemory-databas behövs ingen migration, men för SQL Server krävs det.

---

## Testning

Exempel på enhetstester:

```csharp
[Fact]
public void CalculateRotDeduction_ShouldReturn50Percent()
{
    // Arrange
    var service = new TaxDeductionService(context);
    var laborCost = 20000m;
    
    // Act
    var deduction = service.CalculateRotDeduction(laborCost);
    
    // Assert
    Assert.Equal(10000m, deduction); // 50% av 20000
}

[Fact]
public void CalculateRotDeduction_ShouldCapAt50000()
{
    // Arrange
    var service = new TaxDeductionService(context);
    var laborCost = 150000m; // 50% skulle bli 75000, men max är 50000
    
    // Act
    var deduction = service.CalculateRotDeduction(laborCost);
    
    // Assert
    Assert.Equal(50000m, deduction); // Max ROT-avdrag
}

[Fact]
public async Task GenerateK4Report_ShouldCalculateCorrectly()
{
    // Arrange
    var generator = new K4Generator(context);
    // Lägg till testdata...
    
    // Act
    var report = await generator.GenerateK4ReportAsync(householdId: 1, taxYear: 2025);
    
    // Assert
    Assert.Equal(15000m, report.TotalGain);
    Assert.Equal(5000m, report.TotalLoss);
    Assert.Equal(10000m, report.NetGain);
    Assert.Equal(3000m, report.TaxableGain); // 30% av 10000
}
```

---

## Framtida förbättringar

### BankID-integration
För att lägga till BankID-autentisering, se [utvärderingsdokumentet](SWEDISH_INTEGRATIONS_EVALUATION.md) för detaljer.

### Fortnox/Visma-integration
API-integration med bokföringssystem kan läggas till enligt följande struktur:

```csharp
public interface IFortnoxIntegrationService
{
    Task<bool> ExportTransactionsAsync(List<Transaction> transactions);
    Task<List<Customer>> GetCustomersAsync();
    Task<List<Supplier>> GetSuppliersAsync();
}
```

### Grafiskt gränssnitt
Följande sidor kan läggas till i Blazor Web:
- `/tax-deductions` - ROT/RUT-avdrag
- `/capital-gains` - Kapitalvinster (K4)
- `/tax-reports` - Skattedeklarationsunderlag
- `/isk-accounts` - ISK/KF-konton
- `/commute-deductions` - Reseavdrag
- `/export` - Export till SIE, K4, etc.

---

## Support och dokumentation

### Officiella källor
- [Skatteverket ROT/RUT](https://skatteverket.se/privat/fastigheterochbostad/rotochrut.4.2e56d4ba1202f95012080002966.html)
- [Skatteverket K4](https://skatteverket.se/privat/skatter/vardepapper/saljaaktierfondellerandravardepapper.4.15532c7b1442f256bae1158a.html)
- [SIE-format specifikation](https://sie.se/)
- [Fortnox API](https://developer.fortnox.se/)
- [Visma eEkonomi API](https://developer.visma.com/)

### Frågor och problem
För frågor om implementationen:
1. Öppna ett issue på GitHub
2. Kontrollera befintliga issues först
3. Inkludera relevanta logs (utan känslig data)

---

## Licens

Denna implementation är skapad som exempel för privatekonomiapplikationen Privatekonomi.
