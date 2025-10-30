# Implementationsguide: Avanza Bank Integration

**Version:** 1.0  
**Datum:** 2025-10-17

## Översikt

Denna guide beskriver steg-för-steg hur Avanza Bank-integration implementeras i Privatekonomi-applikationen.

## Implementationsordning

### Steg 1: Utöka Investment-modellen

**Fil:** `src/Privatekonomi.Core/Models/Investment.cs`

Lägg till nya egenskaper för att stödja Avanza-data:

```csharp
public class Investment
{
    public int InvestmentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // "Aktie", "Fond", "Certifikat"
    public decimal Quantity { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal CurrentPrice { get; set; }
    public DateTime PurchaseDate { get; set; }
    public DateTime LastUpdated { get; set; }
    
    // Nya egenskaper för Avanza-integration
    public int? BankSourceId { get; set; }
    public BankSource? BankSource { get; set; }
    public string? AccountNumber { get; set; }
    public string? ShortName { get; set; }
    public string? ISIN { get; set; }
    public string? Currency { get; set; }
    public string? Country { get; set; }
    public string? Market { get; set; }
    
    // Beräknade egenskaper (oförändrade)
    public decimal TotalValue => Quantity * CurrentPrice;
    public decimal TotalCost => Quantity * PurchasePrice;
    public decimal ProfitLoss => TotalValue - TotalCost;
    public decimal ProfitLossPercentage => TotalCost > 0 ? (ProfitLoss / TotalCost) * 100 : 0;
}
```

### Steg 2: Uppdatera PrivatekonomyContext

**Fil:** `src/Privatekonomi.Core/Data/PrivatekonomyContext.cs`

Uppdatera Investment entity configuration:

```csharp
modelBuilder.Entity<Investment>(entity =>
{
    entity.HasKey(e => e.InvestmentId);
    entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
    entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
    entity.Property(e => e.Quantity).HasPrecision(18, 4);
    entity.Property(e => e.PurchasePrice).HasPrecision(18, 2);
    entity.Property(e => e.CurrentPrice).HasPrecision(18, 2);
    entity.Property(e => e.PurchaseDate).IsRequired();
    entity.Property(e => e.LastUpdated).IsRequired();
    
    // Nya egenskaper
    entity.Property(e => e.AccountNumber).HasMaxLength(50);
    entity.Property(e => e.ShortName).HasMaxLength(50);
    entity.Property(e => e.ISIN).HasMaxLength(12);
    entity.Property(e => e.Currency).HasMaxLength(3);
    entity.Property(e => e.Country).HasMaxLength(2);
    entity.Property(e => e.Market).HasMaxLength(50);
    
    // Relation till BankSource
    entity.HasOne(e => e.BankSource)
        .WithMany()
        .HasForeignKey(e => e.BankSourceId)
        .OnDelete(DeleteBehavior.SetNull);
        
    // Index för snabbare sökning
    entity.HasIndex(e => e.ISIN);
    entity.HasIndex(e => e.AccountNumber);
});
```

### Steg 3: Skapa IInvestmentCsvParser interface

**Fil:** `src/Privatekonomi.Core/Services/Parsers/IInvestmentCsvParser.cs`

```csharp
using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services.Parsers;

public interface IInvestmentCsvParser
{
    string BankName { get; }
    string FormatType { get; } // "Per konto" eller "Sammanställt"
    Task<List<Investment>> ParseAsync(Stream csvStream);
    bool CanParse(string csvContent);
}
```

### Steg 4: Implementera Avanza CSV Parsers

#### 4.1 AvanzaHoldingsPerAccountParser

**Fil:** `src/Privatekonomi.Core/Services/Parsers/AvanzaHoldingsPerAccountParser.cs`

```csharp
using System.Globalization;
using System.Text;
using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services.Parsers;

public class AvanzaHoldingsPerAccountParser : IInvestmentCsvParser
{
    public string BankName => "Avanza";
    public string FormatType => "Per konto";

    public bool CanParse(string csvContent)
    {
        var lines = csvContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length < 2) return false;

        var header = lines[0].ToLower();
        return header.Contains("kontonummer") && 
               header.Contains("namn") && 
               header.Contains("volym") && 
               header.Contains("marknadsvärde");
    }

    public async Task<List<Investment>> ParseAsync(Stream csvStream)
    {
        var investments = new List<Investment>();
        
        using var reader = new StreamReader(csvStream, Encoding.UTF8);
        var content = await reader.ReadToEndAsync();
        
        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length < 2) return investments;

        // Detect separator (tab or pipe for table format)
        var separator = lines[0].Contains('|') ? '|' : 
                       lines[0].Contains('\t') ? '\t' : 
                       lines[0].Contains(';') ? ';' : ',';
        
        // Parse header
        var header = lines[0].Split(separator);
        var accountIndex = FindColumnIndex(header, new[] { "kontonummer", "account number" });
        var nameIndex = FindColumnIndex(header, new[] { "namn", "name" });
        var shortNameIndex = FindColumnIndex(header, new[] { "kortnamn", "short name", "ticker" });
        var volumeIndex = FindColumnIndex(header, new[] { "volym", "volume", "quantity" });
        var marketValueIndex = FindColumnIndex(header, new[] { "marknadsvärde", "market value" });
        var gavSekIndex = FindColumnIndex(header, new[] { "gav (sek)", "gav" });
        var currencyIndex = FindColumnIndex(header, new[] { "valuta", "currency" });
        var countryIndex = FindColumnIndex(header, new[] { "land", "country" });
        var isinIndex = FindColumnIndex(header, new[] { "isin" });
        var marketIndex = FindColumnIndex(header, new[] { "marknad", "market", "exchange" });
        var typeIndex = FindColumnIndex(header, new[] { "typ", "type" });

        if (nameIndex == -1 || volumeIndex == -1 || marketValueIndex == -1)
        {
            throw new InvalidOperationException("Kunde inte hitta nödvändiga kolumner i CSV-filen");
        }

        // Parse data rows
        for (int i = 1; i < lines.Length; i++)
        {
            try
            {
                var columns = lines[i].Split(separator);
                
                // Skip header separators in markdown tables
                if (columns.All(c => c.Trim().All(ch => ch == '-' || ch == ' ')))
                    continue;
                    
                if (columns.Length <= Math.Max(nameIndex, Math.Max(volumeIndex, marketValueIndex)))
                    continue;

                var name = GetColumnValue(columns, nameIndex);
                if (string.IsNullOrWhiteSpace(name))
                    continue;

                var volumeStr = GetColumnValue(columns, volumeIndex).Replace(",", ".").Replace(" ", "");
                var marketValueStr = GetColumnValue(columns, marketValueIndex).Replace(",", ".").Replace(" ", "");
                var gavStr = gavSekIndex >= 0 ? GetColumnValue(columns, gavSekIndex).Replace(",", ".").Replace(" ", "") : "";

                if (!decimal.TryParse(volumeStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var volume))
                    continue;
                    
                if (!decimal.TryParse(marketValueStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var marketValue))
                    continue;

                decimal gav = 0;
                if (!string.IsNullOrEmpty(gavStr))
                {
                    decimal.TryParse(gavStr, NumberStyles.Any, CultureInfo.InvariantCulture, out gav);
                }

                var currentPrice = volume > 0 ? marketValue / volume : 0;
                var type = MapAvanzaTypeToInvestmentType(GetColumnValue(columns, typeIndex));

                var investment = new Investment
                {
                    Name = name,
                    ShortName = GetColumnValue(columns, shortNameIndex),
                    Type = type,
                    Quantity = volume,
                    PurchasePrice = gav,
                    CurrentPrice = currentPrice,
                    AccountNumber = GetColumnValue(columns, accountIndex),
                    ISIN = GetColumnValue(columns, isinIndex),
                    Currency = GetColumnValue(columns, currencyIndex),
                    Country = GetColumnValue(columns, countryIndex),
                    Market = GetColumnValue(columns, marketIndex),
                    PurchaseDate = DateTime.Now, // We don't have this in CSV
                    LastUpdated = DateTime.Now
                };

                investments.Add(investment);
            }
            catch
            {
                // Skip invalid rows
                continue;
            }
        }

        return investments;
    }

    private int FindColumnIndex(string[] header, string[] possibleNames)
    {
        for (int i = 0; i < header.Length; i++)
        {
            var columnName = header[i].Trim().ToLower();
            foreach (var name in possibleNames)
            {
                if (columnName.Contains(name.ToLower()))
                    return i;
            }
        }
        return -1;
    }

    private string GetColumnValue(string[] columns, int index)
    {
        if (index < 0 || index >= columns.Length)
            return string.Empty;
        return columns[index].Trim();
    }

    private string MapAvanzaTypeToInvestmentType(string avanzaType)
    {
        return avanzaType.ToUpper() switch
        {
            "STOCK" => "Aktie",
            "FUND" => "Fond",
            "EXCHANGE_TRADED_FUND" => "Fond",
            "CERTIFICATE" => "Certifikat",
            _ => "Övrigt"
        };
    }
}
```

#### 4.2 AvanzaConsolidatedHoldingsParser

**Fil:** `src/Privatekonomi.Core/Services/Parsers/AvanzaConsolidatedHoldingsParser.cs`

```csharp
// Samma implementation som AvanzaHoldingsPerAccountParser men utan accountIndex
// och FormatType = "Sammanställt"
```

### Steg 5: Utöka InvestmentService

**Fil:** `src/Privatekonomi.Core/Services/IInvestmentService.cs`

Lägg till nya metoder:

```csharp
Task<CsvImportResult> ImportFromCsvAsync(Stream csvStream, int bankSourceId);
Task<IEnumerable<Investment>> GetInvestmentsByBankAsync(int bankSourceId);
Task<IEnumerable<Investment>> GetInvestmentsByAccountAsync(string accountNumber);
Task<string> ExportToCsvAsync(IEnumerable<Investment> investments);
```

**Fil:** `src/Privatekonomi.Core/Services/InvestmentService.cs`

Implementera metoderna.

### Steg 6: Skapa ImportInvestments.razor

**Fil:** `src/Privatekonomi.Web/Components/Pages/ImportInvestments.razor`

Skapa en ny sida för import av investeringar, liknande Import.razor men för investeringar istället för transaktioner.

### Steg 7: Uppdatera Investments.razor

Lägg till:
- Filterkomponenter för bank och konto
- Bank/konto-kolumner i tabellen
- Export-knapp
- Gruppering per bank/konto (valfritt)

### Steg 8: Lägg till navigeringslänk

**Fil:** `src/Privatekonomi.Web/Components/Layout/NavMenu.razor`

Lägg till länk till "Importera Investeringar".

## Testning

### Testfiler

Skapa testfiler baserade på exemplen i issue:

**test-avanza-per-account.csv:**
```
Kontonummer|Namn|Kortnamn|Volym|Marknadsvärde|GAV (SEK)|GAV|Valuta|Land|ISIN|Marknad|Typ
1111-2223332|Telia Company|TELIA|5|180,90|54,78|54,78|SEK|SE|SE0000667925|XSTO|STOCK
2222-3333444|Oneflow|ONEF|100|2730,00|44,71|44,71|SEK|SE|SE0017564461|FNSE|STOCK
```

**test-avanza-consolidated.csv:**
```
Namn|Kortnamn|Volym|Marknadsvärde|GAV (SEK)|GAV|Valuta|Land|ISIN|Marknad|Typ
AFRY|AFRY|7|1164,10|159,59|159,59|SEK|SE|SE0005999836|XSTO|STOCK
Avanza Bank Holding|AZA|6|2248,20|216,25|216,25|SEK|SE|SE0012454072|XSTO|STOCK
```

### Manuell testning

1. Starta applikationen
2. Navigera till "Importera Investeringar"
3. Välj Avanza
4. Ladda upp testfil
5. Verifiera förhandsvisning
6. Bekräfta import
7. Kontrollera att data visas korrekt i investeringslistan
8. Testa filtrering per bank
9. Testa export till CSV

## Vanliga problem och lösningar

### Problem: CSV-filen parsas inte korrekt

**Lösning:** 
- Kontrollera filens encoding (UTF-8)
- Kontrollera separator (| för tabellformat, ; för CSV)
- Kontrollera att kolumnnamnen matchar

### Problem: Decimal-värden parsas fel

**Lösning:**
- Ersätt komma med punkt innan parsing
- Ta bort mellanslag i numeriska värden
- Använd InvariantCulture vid parsing

## Dokumentation att uppdatera

Efter implementering:
- README.md: Lägg till sektion om Avanza-import
- Skapa AVANZA_IMPORT_GUIDE.md i docs/
