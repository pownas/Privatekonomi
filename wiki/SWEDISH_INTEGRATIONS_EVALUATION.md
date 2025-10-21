# Utvärdering: Sverige-specifika integrationer

Detta dokument utvärderar möjligheterna att implementera de Sverige-specifika integrationer som efterfrågats, inklusive tekniska krav, komplexitet och rekommendationer.

## Sammanfattning

| Funktion | Genomförbarhet | Komplexitet | Prioritet | Anmärkning |
|----------|---------------|-------------|-----------|-----------|
| BankID-inloggning | ✅ Ja | Medel | Hög | Kräver extern identitetsleverantör |
| Swish | ⚠️ Begränsat | Hög | Låg | API endast för företag |
| E-faktura/Autogiro | ✅ Ja | Medel | Medel | Läsning möjlig via bank-API |
| Kivra | ⚠️ Begränsat | Hög | Låg | Kräver affärsavtal |
| Skatteverket-integration | ⚠️ Begränsat | Hög | Medel | Olika API:er för olika tjänster |
| CSN-lån | ✅ Ja | Låg | Medel | Utökad datamodell |
| Bolån | ✅ Ja | Låg | Medel | Redan delvis implementerat |
| UC/Creditsafe | ⚠️ Begränsat | Hög | Låg | Kräver affärsavtal |
| SIE-export | ✅ Ja | Medel | Hög | Bokföringsstandardformat |
| Fortnox/Visma-integration | ✅ Ja | Medel-Hög | Medel | API:er tillgängliga |

---

## 1. Autentisering och betalningar

### 1.1 BankID-inloggning

**Status:** ✅ Genomförbar

**Beskrivning:** Implementera BankID som autentiseringsmetod för användare att logga in i applikationen.

**Tekniska krav:**
- Integration med svensk eID-leverantör (t.ex. GrandID, FreeJa)
- ASP.NET Core Identity för användarhantering
- OpenID Connect/OAuth2 för autentiseringsflöde
- HTTPS/TLS-kryptering

**Implementation:**
1. Registrera applikationen hos en BankID Relying Party-leverantör
2. Implementera OpenID Connect i ASP.NET Core
3. Skapa användarprofiler i databasen
4. Koppla transaktioner/data till användare

**Kostnader:**
- Utvecklingstid: 3-5 dagar
- Månadskostnad: 500-2000 SEK (beroende på leverantör och volym)

**Rekommendation:** ⭐⭐⭐⭐⭐
BankID är den mest använda autentiseringsmetoden i Sverige och bör prioriteras för produktionsmiljö.

**Exempel på leverantörer:**
- GrandID (Svensk E-identitet)
- FreeJa
- CGI (tidigare Bankgirot)

---

### 1.2 Swish

**Status:** ⚠️ Begränsat genomförbar

**Beskrivning:** Integration med Swish för betalningar och transaktionsövervakning.

**Tekniska utmaningar:**
- Swish API är endast tillgängligt för företag med Swish-handelsavtal
- Kräver avtal med bank som erbjuder Swish för företag
- Strikt certifikathantering och säkerhetskrav
- Endast för utbetalningar, inte för att läsa privata Swish-transaktioner

**Möjliga användningsfall:**
1. **Swish-betalningar IN till applikationen** (om det är en tjänst)
   - Användare kan betala för premium-funktioner via Swish
   - Kräver Swish-handelsavtal

2. **Manuell registrering av Swish-transaktioner**
   - Användare registrerar manuellt Swish-betalningar i systemet
   - Ingen API-integration krävs

**Rekommendation:** ⭐⭐
Låg prioritet. Fokusera istället på att hämta Swish-transaktioner via bank-API:er (redan implementerat via PSD2).

---

### 1.3 E-faktura och Autogiro

**Status:** ✅ Genomförbar (läsning)

**Beskrivning:** Visa e-fakturor och autogiro-transaktioner i applikationen.

**Implementation:**
1. **Via bank-API (PSD2)** - Redan delvis implementerat
   - E-fakturor och autogiro-transaktioner syns som vanliga transaktioner
   - Kan utökas med metadata för att identifiera typ av transaktion

2. **Direkt integration** (ej rekommenderat för privatpersoner)
   - Bankgirot/BGC har API:er, men endast för företag
   - E-faktura-leverantörer (Kivra, Min Myndighetspost) har API:er

**Förslag på utökad datamodell:**
```csharp
public class Transaction
{
    // ... befintliga fält ...
    
    // E-faktura/Autogiro-specifika fält
    public string? PaymentMethod { get; set; } // "Swish", "Autogiro", "E-faktura", "Banköverföring"
    public string? RecipientBankgiro { get; set; }
    public string? RecipientPlusgiro { get; set; }
    public string? InvoiceNumber { get; set; }
    public string? OCR { get; set; } // OCR-nummer för e-fakturor
    public bool IsRecurring { get; set; } // Om det är autogiro/återkommande
}
```

**Rekommendation:** ⭐⭐⭐⭐
Utöka befintlig transaktionsmodell för att identifiera och kategorisera e-fakturor och autogiro-transaktioner.

---

## 2. Myndighetstjänster och dokument

### 2.1 Kivra

**Status:** ⚠️ Begränsat genomförbar

**Beskrivning:** Integration med Kivra för att hämta digitala brev och fakturor.

**Tekniska utmaningar:**
- Kivra API kräver företagsavtal
- Ingen officiell API för privatpersoner att hämta sina egna brev
- GDPR och säkerhetskrav

**Alternativ:**
1. **Manuell uppladdning**
   - Användare laddar ner PDF från Kivra och laddar upp i applikationen
   
2. **E-postintegration**
   - Om användare får notifikationer via e-post, kan dessa parsas

**Rekommendation:** ⭐
Låg prioritet. För komplex integration för begränsad nytta i ett privatekonomisystem.

---

### 2.2 Skatteverket

**Status:** ⚠️ Begränsat genomförbar

**Beskrivning:** Integration med olika Skatteverket-tjänster.

#### 2.2.1 K4 (Blankett för kapitalinkomster)

**Möjlighet:** Automatgenerering av K4 baserat på investeringstransaktioner

**Implementation:**
```csharp
public class K4Generator
{
    public K4Report GenerateK4(int year, int householdId)
    {
        // Hämta alla investeringstransaktioner för året
        // Beräkna vinster/förluster
        // Generera K4-blankett i rätt format
    }
}
```

**Datamodell för kapitalvinster:**
```csharp
public class CapitalGain
{
    public int CapitalGainId { get; set; }
    public int InvestmentId { get; set; }
    public DateTime SaleDate { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal SalePrice { get; set; }
    public decimal Gain { get; set; } // Kan vara negativ (förlust)
    public string SecurityType { get; set; } // "Stock", "Fund", "Crypto"
    public int TaxYear { get; set; }
}
```

**Rekommendation:** ⭐⭐⭐⭐
Mycket värdefull funktion för användare med investeringar.

#### 2.2.2 ISK/KF (Investeringssparkonto/Kapitalförsäkring)

**Möjlighet:** Spåra ISK/KF-konton och beräkna schablonintäkt

**Utökad Investment-modell:**
```csharp
public class Investment
{
    // ... befintliga fält ...
    
    public string? AccountType { get; set; } // "ISK", "KF", "AF", "Depå"
    public decimal? SchablonTax { get; set; } // Beräknad schablonintäkt för ISK/KF
}
```

**Rekommendation:** ⭐⭐⭐⭐
Viktig funktion för många svenska investerare.

#### 2.2.3 ROT/RUT-avdrag

**Möjlighet:** Registrera och spåra ROT/RUT-berättigade kostnader

**Datamodell:**
```csharp
public class TaxDeduction
{
    public int TaxDeductionId { get; set; }
    public int TransactionId { get; set; }
    public string Type { get; set; } // "ROT", "RUT"
    public decimal Amount { get; set; }
    public decimal DeductibleAmount { get; set; } // 50% för ROT/RUT
    public string ServiceProvider { get; set; }
    public string OrganizationNumber { get; set; }
    public int TaxYear { get; set; }
    public bool Approved { get; set; }
}
```

**Rekommendation:** ⭐⭐⭐⭐
Användbar funktion för att spåra avdragsgilla kostnader.

#### 2.2.4 Reseavdrag

**Möjlighet:** Beräkna och spåra reseavdrag för arbetsresor

**Datamodell:**
```csharp
public class CommuteDeduction
{
    public int CommuteDeductionId { get; set; }
    public DateTime Date { get; set; }
    public string FromAddress { get; set; } = string.Empty;
    public string ToAddress { get; set; } = string.Empty;
    public decimal DistanceKm { get; set; }
    public string TransportMethod { get; set; } // "Car", "PublicTransport"
    public decimal Cost { get; set; }
    public decimal DeductibleAmount { get; set; }
    public int TaxYear { get; set; }
}
```

**Rekommendation:** ⭐⭐⭐
Användbar för personer med långa arbetsresor.

#### 2.2.5 Trängselskatt

**Möjlighet:** Spåra och kategorisera trängselskattkostnader

**Implementation:**
- Lägg till kategori "Trängselskatt"
- Automatisk kategorisering baserat på beskrivning (t.ex. "Trängselskattemyndigheten")
- Sammanställning per månad/år

**Rekommendation:** ⭐⭐
Enkel att implementera, men begränsad användarbas.

---

### 2.3 CSN-lån

**Status:** ✅ Genomförbar

**Beskrivning:** Spåra CSN-lån och amorteringar

**Implementation:**
Utöka befintlig `Loan`-modell (redan delvis implementerad):

```csharp
public class Loan
{
    // ... befintliga fält ...
    
    // CSN-specifika fält
    public string? CSN_LoanType { get; set; } // "Studiemedel", "Studiemedelsränta"
    public int? CSN_StudyYear { get; set; }
    public decimal? CSN_MonthlyPayment { get; set; }
    public decimal? CSN_RemainingAmount { get; set; }
    public DateTime? CSN_LastUpdate { get; set; }
}
```

**CSN API-integration:**
CSN har INGET offentligt API för privatpersoner. Alternativ:
1. Manuell inmatning av låneuppgifter
2. Import från CSN PDF-utdrag (parsing)
3. Manuell uppdatering från csn.se

**Rekommendation:** ⭐⭐⭐⭐
Relevant för många unga vuxna. Fokusera på manuell inmatning och spårning.

---

### 2.4 Bolån

**Status:** ✅ Genomförbar (delvis implementerat)

**Beskrivning:** Spåra bolån med amortering och ränta

**Befintlig implementation:**
`Loan`-modellen finns redan med stöd för:
- Lånebelopp
- Ränta
- Amortering
- Löptid

**Förbättringsförslag:**
```csharp
public class MortgageLoan : Loan
{
    public string? PropertyAddress { get; set; }
    public decimal PropertyValue { get; set; }
    public decimal LTV { get; set; } // Loan-to-Value ratio (belåningsgrad)
    public string? LoanProvider { get; set; } // Bank
    public bool IsFixedRate { get; set; }
    public DateTime? RateResetDate { get; set; } // När räntan bindningstiden löper ut
    public int BindingPeriodMonths { get; set; } // 3 mån, 1 år, 5 år, etc.
}
```

**Rekommendation:** ⭐⭐⭐⭐⭐
Mycket relevant för många svenska hushåll. Utöka befintlig funktionalitet.

---

### 2.5 UC/Creditsafe (Kreditupplysning)

**Status:** ⚠️ Begränsat genomförbar

**Beskrivning:** Visa kreditvärdighet och kreditupplysningar

**Tekniska utmaningar:**
- UC och Creditsafe kräver företagsavtal
- Kostnader per sökning
- GDPR-krav för att lagra kreditupplysningar

**Alternativ:**
1. **Manuell inmatning**
   - Användare anger sin UC-rating manuellt
   
2. **Länkning**
   - Länka till UC/Creditsafe för att kontrollera kreditvärdighet

**Datamodell:**
```csharp
public class CreditRating
{
    public int CreditRatingId { get; set; }
    public int HouseholdId { get; set; }
    public string Provider { get; set; } // "UC", "Creditsafe"
    public string Rating { get; set; } // "AAA", "AA", etc.
    public int Score { get; set; }
    public DateTime CheckedDate { get; set; }
    public string? Notes { get; set; }
}
```

**Rekommendation:** ⭐⭐
Begränsat värde. Implementera endast om det finns stark efterfrågan.

---

## 3. Bokföring och affärssystem

### 3.1 SIE-export

**Status:** ✅ Genomförbar

**Beskrivning:** Exportera transaktioner i SIE-format (Standard Import Export) för bokföringsprogram

**SIE-format:**
SIE är ett svenskt standardformat för att utbyta ekonomisk data mellan system.

**Versioner:**
- **SIE 1**: Enkel export av kontotransaktioner
- **SIE 2**: Export av kontotransaktioner med dimensioner
- **SIE 3**: Export av budget
- **SIE 4**: Export av fullständig bokföring (vanligaste)

**Implementation:**
```csharp
public class SieExporter
{
    public string ExportToSie4(int householdId, int year)
    {
        var sie = new StringBuilder();
        
        // Header
        sie.AppendLine("#FLAGGA 0");
        sie.AppendLine("#PROGRAM \"Privatekonomi\" \"1.0\"");
        sie.AppendLine($"#FORMAT PC8");
        sie.AppendLine($"#GEN {DateTime.Now:yyyyMMdd}");
        sie.AppendLine($"#SIETYP 4");
        sie.AppendLine($"#FNR {householdId}"); // Företagsnummer (använd household ID)
        
        // Räkenskapsår
        sie.AppendLine($"#RAR 0 {year}0101 {year}1231");
        
        // Kontoplan
        sie.AppendLine("#KONTO 1930 \"Bank\"");
        sie.AppendLine("#KONTO 3000 \"Inkomster\"");
        sie.AppendLine("#KONTO 5000 \"Utgifter\"");
        // ... fler konton baserat på kategorier
        
        // Transaktioner
        sie.AppendLine("#VER \"\" \"\" {date} \"{description}\"");
        sie.AppendLine("{");
        sie.AppendLine($"#TRANS 1930 {{}} {amount}");
        sie.AppendLine($"#TRANS 3000 {{}} {-amount}");
        sie.AppendLine("}");
        
        return sie.ToString();
    }
}
```

**Mappning mellan kategorier och SIE-konton:**
```csharp
public class CategoryToSieAccountMapper
{
    private static readonly Dictionary<string, string> Mapping = new()
    {
        { "Mat & Dryck", "5000" },
        { "Transport", "5100" },
        { "Boende", "5200" },
        { "Lön", "3000" },
        // ... etc
    };
}
```

**Rekommendation:** ⭐⭐⭐⭐⭐
Mycket värdefull för egenföretagare och de som vill använda professionell bokföringsprogramvara.

---

### 3.2 Fortnox-integration

**Status:** ✅ Genomförbar

**Beskrivning:** Integration med Fortnox bokföringssystem

**Fortnox API:**
- REST API med OAuth2
- Gratis för utvecklare (sandbox)
- Produktionsnycklar kräver Fortnox-konto

**Implementation:**
```csharp
public class FortnoxIntegrationService
{
    public async Task<bool> ExportTransactionsToFortnox(List<Transaction> transactions)
    {
        foreach (var transaction in transactions)
        {
            var voucher = new FortnoxVoucher
            {
                Description = transaction.Description,
                VoucherRows = new List<FortnoxVoucherRow>
                {
                    new FortnoxVoucherRow
                    {
                        Account = GetAccountNumber(transaction.CategoryId),
                        Debit = transaction.Amount > 0 ? transaction.Amount : 0,
                        Credit = transaction.Amount < 0 ? Math.Abs(transaction.Amount) : 0,
                        Description = transaction.Description
                    }
                }
            };
            
            await _fortnoxClient.CreateVoucher(voucher);
        }
        
        return true;
    }
}
```

**Funktioner:**
1. Export av transaktioner till verifikationer
2. Synkronisering av kunder/leverantörer
3. Fakturering (om relevant)

**Rekommendation:** ⭐⭐⭐⭐
Mycket värdefull för egenföretagare som använder Fortnox.

---

### 3.3 Visma-integration

**Status:** ✅ Genomförbar

**Beskrivning:** Integration med Visma eEkonomi/Visma Business

**Visma API:**
- REST API med OAuth2
- Olika API:er för olika produkter (eEkonomi, Business, etc.)

**Implementation:**
Liknande struktur som Fortnox-integration:

```csharp
public class VismaIntegrationService
{
    public async Task<bool> ExportToVismaEconomy(List<Transaction> transactions)
    {
        // Liknande implementation som Fortnox
    }
}
```

**Rekommendation:** ⭐⭐⭐⭐
Värdefullt alternativ till Fortnox för egenföretagare.

---

## Implementationsplan

### Fas 1: Användarautentisering (Hög prioritet)
**Tid:** 1 vecka

1. Implementera ASP.NET Core Identity
2. Integrera BankID via GrandID/FreeJa
3. Migrera till persistent databas (SQL Server)
4. Koppla användare till hushåll och transaktioner

### Fas 2: Utökade datamodeller (Medel prioritet)
**Tid:** 3-5 dagar

1. Utöka `Transaction` med betalningsmetod, OCR, etc.
2. Förbättra `Loan`-modellen för CSN och bolån
3. Lägg till `TaxDeduction` för ROT/RUT
4. Lägg till `CapitalGain` för K4-underlag
5. Lägg till `CommuteDeduction` för reseavdrag

### Fas 3: Skattedeklaration-stöd (Medel prioritet)
**Tid:** 1-2 veckor

1. K4-generator för kapitalvinster
2. ISK/KF schablonintäktberäkning
3. ROT/RUT-sammanställning
4. Reseavdrag-beräkning
5. Export-funktioner för deklarationsunderlag

### Fas 4: Bokföringssystem-integration (Medel prioritet)
**Tid:** 2-3 veckor

1. SIE-export (format 4)
2. Fortnox API-integration
3. Visma API-integration
4. Kontoplan-mappning
5. Automatisk synkronisering

### Fas 5: Övriga funktioner (Låg prioritet)
**Tid:** Varierar

1. Trängselskatt-spårning
2. Kreditvärdighet-visning (manuell)
3. Förbättrad lånhantering

---

## Tekniska krav för implementation

### Databas-migration
För att stödja flera användare och utökade funktioner behöver applikationen:

1. **Migrera från InMemory till SQL Server/PostgreSQL**
   ```bash
   dotnet ef migrations add SwedishIntegrations
   dotnet ef database update
   ```

2. **Lägg till user management**
   ```bash
   dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore
   ```

### Nya NuGet-paket
```xml
<!-- BankID -->
<PackageReference Include="ActiveLogin.Authentication.BankId.AspNetCore" Version="7.0.0" />

<!-- Fortnox -->
<PackageReference Include="Fortnox.SDK" Version="3.0.0" />

<!-- Visma eEkonomi -->
<!-- Inget officiellt paket, implementera egen HTTP-klient -->

<!-- PDF-parsing för CSN/fakturor -->
<PackageReference Include="iTextSharp" Version="5.5.13" />
```

### Nya Services
```csharp
// Identity
services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<PrivatekonomyContext>()
    .AddDefaultTokenProviders();

// BankID
services.AddAuthentication()
    .AddBankId(options => {
        options.ClientId = configuration["BankID:ClientId"];
        options.ClientSecret = configuration["BankID:ClientSecret"];
    });

// Bokföringssystem
services.AddScoped<ISieExporter, SieExporter>();
services.AddScoped<IFortnoxService, FortnoxIntegrationService>();
services.AddScoped<IVismaService, VismaIntegrationService>();

// Skattetjänster
services.AddScoped<IK4Generator, K4Generator>();
services.AddScoped<ITaxDeductionService, TaxDeductionService>();
```

---

## Säkerhetsöverväganden

### GDPR-compliance
- **Användarsamtycke**: Tydlig information om dataanvändning
- **Dataminimering**: Endast samla nödvändig data
- **Rätt till radering**: Möjlighet att ta bort konto och all data
- **Dataskydd**: Kryptering av känslig data

### Kryptering
- **Transit**: HTTPS/TLS för all kommunikation
- **Rest**: Kryptera känsliga fält i databasen (BankID-info, API-nycklar)

### API-nyckelhantering
```csharp
// Använd Azure Key Vault eller User Secrets
builder.Configuration.AddAzureKeyVault(
    new Uri($"https://{keyVaultName}.vault.azure.net/"),
    new DefaultAzureCredential());
```

---

## Kostnadsuppskattning

### Utvecklingskostnader
- **Fas 1 (BankID)**: 40 timmar
- **Fas 2 (Datamodeller)**: 24 timmar  
- **Fas 3 (Skatt)**: 60 timmar
- **Fas 4 (Bokföring)**: 80 timmar
- **Fas 5 (Övrigt)**: 40 timmar

**Totalt:** ~240 timmar (6 veckor heltid)

### Driftkostnader (månatliga)
- **BankID**: 500-2000 SEK (beroende på volym)
- **Hosting**: 500-1500 SEK (Azure/AWS)
- **Databas**: Ingår i hosting
- **SSL-certifikat**: Gratis (Let's Encrypt)
- **Fortnox API**: Gratis (användaren har eget konto)
- **Visma API**: Gratis (användaren har eget konto)

**Totalt:** ~1000-3500 SEK/månad

---

## Slutsats

### Rekommenderade implementationer (Högt värde / Genomförbarhet)

1. ⭐⭐⭐⭐⭐ **BankID-inloggning** - Nödvändig för multi-user
2. ⭐⭐⭐⭐⭐ **SIE-export** - Högt värde för egenföretagare
3. ⭐⭐⭐⭐⭐ **Bolån-förbättring** - Relevant för många
4. ⭐⭐⭐⭐ **K4-generator** - Högt värde för investerare
5. ⭐⭐⭐⭐ **ISK/KF-stöd** - Mycket vanligt i Sverige
6. ⭐⭐⭐⭐ **ROT/RUT-spårning** - Användbart vid deklaration
7. ⭐⭐⭐⭐ **CSN-lån** - Relevant för unga vuxna
8. ⭐⭐⭐⭐ **Fortnox-integration** - Värdefullt för egenföretagare
9. ⭐⭐⭐⭐ **Visma-integration** - Alternativ till Fortnox
10. ⭐⭐⭐⭐ **E-faktura/Autogiro-metadata** - Enkel förbättring

### Ej rekommenderade implementationer

1. ❌ **Swish-integration** - Kräver företagsavtal, begränsat värde
2. ❌ **Kivra-integration** - Kräver företagsavtal, svår integration
3. ❌ **UC/Creditsafe** - Kräver företagsavtal, höga kostnader

---

## Nästa steg

1. **Prioritera funktioner** baserat på användarbehov
2. **Migrera till persistent databas** (SQL Server/PostgreSQL)
3. **Implementera BankID** för användarautentisering
4. **Utöka datamodeller** enligt förslag ovan
5. **Implementera SIE-export** för egenföretagare
6. **Lägg till skattefunktioner** stegvis
7. **Integrera med Fortnox/Visma** vid behov

---

## Referenser

- [BankID för privatpersoner](https://www.bankid.com/)
- [GrandID dokumentation](https://www.grandid.com/)
- [Fortnox API](https://developer.fortnox.se/)
- [Visma eEkonomi API](https://developer.visma.com/)
- [SIE-format specifikation](https://sie.se/)
- [Skatteverket för utvecklare](https://www.skatteverket.se/foretag/etjansterochblanketterforetag/systemforetagochmyndigheter.4.18e1b10334ebe8bc80008000.html)
