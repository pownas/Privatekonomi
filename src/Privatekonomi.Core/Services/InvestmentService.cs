using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Privatekonomi.Core.Data;
using Privatekonomi.Core.Models;
using Privatekonomi.Core.Services.Parsers;

namespace Privatekonomi.Core.Services;

public class InvestmentService : IInvestmentService
{
    private readonly PrivatekonomyContext _context;
    private readonly List<IInvestmentCsvParser> _parsers;

    public InvestmentService(PrivatekonomyContext context)
    {
        _context = context;
        _parsers = new List<IInvestmentCsvParser>
        {
            new AvanzaHoldingsPerAccountParser(),
            new AvanzaConsolidatedHoldingsParser()
        };
    }

    public async Task<IEnumerable<Investment>> GetAllInvestmentsAsync()
    {
        return await _context.Investments
            .Include(i => i.BankSource)
            .OrderByDescending(i => i.LastUpdated)
            .ToListAsync();
    }

    public async Task<Investment?> GetInvestmentByIdAsync(int id)
    {
        return await _context.Investments
            .Include(i => i.BankSource)
            .FirstOrDefaultAsync(i => i.InvestmentId == id);
    }

    public async Task<Investment> AddInvestmentAsync(Investment investment)
    {
        investment.LastUpdated = DateTime.Now;
        _context.Investments.Add(investment);
        await _context.SaveChangesAsync();
        return investment;
    }

    public async Task<Investment> UpdateInvestmentAsync(Investment investment)
    {
        investment.LastUpdated = DateTime.Now;
        _context.Investments.Update(investment);
        await _context.SaveChangesAsync();
        return investment;
    }

    public async Task DeleteInvestmentAsync(int id)
    {
        var investment = await _context.Investments.FindAsync(id);
        if (investment != null)
        {
            _context.Investments.Remove(investment);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<CsvImportResult> ImportFromCsvAsync(Stream csvStream, int bankSourceId)
    {
        var result = new CsvImportResult();
        
        try
        {
            // Read CSV content to detect format
            csvStream.Position = 0;
            using var reader = new StreamReader(csvStream, Encoding.UTF8, leaveOpen: true);
            var content = await reader.ReadToEndAsync();
            
            // Find appropriate parser
            IInvestmentCsvParser? parser = null;
            foreach (var p in _parsers)
            {
                if (p.CanParse(content))
                {
                    parser = p;
                    break;
                }
            }
            
            if (parser == null)
            {
                result.Errors.Add(new CsvImportError 
                { 
                    ErrorType = "Format", 
                    ErrorMessage = "Kunde inte identifiera CSV-format. Kontrollera att filen kommer från Avanza." 
                });
                result.ErrorCount++;
                return result;
            }
            
            // Parse investments
            csvStream.Position = 0;
            var parsedInvestments = await parser.ParseAsync(csvStream);
            
            result.TotalRows = parsedInvestments.Count;
            
            // Process each investment
            foreach (var parsedInvestment in parsedInvestments)
            {
                parsedInvestment.BankSourceId = bankSourceId;
                
                // Check for duplicates
                Investment? existing = null;
                
                if (!string.IsNullOrEmpty(parsedInvestment.ISIN) && !string.IsNullOrEmpty(parsedInvestment.AccountNumber))
                {
                    existing = await _context.Investments
                        .FirstOrDefaultAsync(i => i.ISIN == parsedInvestment.ISIN && 
                                                  i.AccountNumber == parsedInvestment.AccountNumber);
                }
                else if (!string.IsNullOrEmpty(parsedInvestment.ISIN))
                {
                    existing = await _context.Investments
                        .FirstOrDefaultAsync(i => i.ISIN == parsedInvestment.ISIN);
                }
                else
                {
                    existing = await _context.Investments
                        .FirstOrDefaultAsync(i => i.Name == parsedInvestment.Name && 
                                                  i.Type == parsedInvestment.Type &&
                                                  i.BankSourceId == bankSourceId);
                }
                
                if (existing != null)
                {
                    // Update existing investment
                    existing.Name = parsedInvestment.Name;
                    existing.ShortName = parsedInvestment.ShortName;
                    existing.Type = parsedInvestment.Type;
                    existing.Quantity = parsedInvestment.Quantity;
                    existing.PurchasePrice = parsedInvestment.PurchasePrice;
                    existing.CurrentPrice = parsedInvestment.CurrentPrice;
                    existing.AccountNumber = parsedInvestment.AccountNumber;
                    existing.ISIN = parsedInvestment.ISIN;
                    existing.Currency = parsedInvestment.Currency;
                    existing.Country = parsedInvestment.Country;
                    existing.Market = parsedInvestment.Market;
                    existing.BankSourceId = bankSourceId;
                    existing.LastUpdated = DateTime.Now;
                    
                    result.DuplicateCount++;
                }
                else
                {
                    // Add new investment
                    _context.Investments.Add(parsedInvestment);
                    result.ImportedCount++;
                }
            }
            
            await _context.SaveChangesAsync();
            result.Success = true;
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Errors.Add(new CsvImportError 
            { 
                ErrorType = "Import", 
                ErrorMessage = $"Ett fel uppstod vid import: {ex.Message}" 
            });
            result.ErrorCount++;
        }
        
        return result;
    }

    public Task<string> ExportToCsvAsync(IEnumerable<Investment> investments)
    {
        var sb = new StringBuilder();
        var culture = new CultureInfo("sv-SE");
        
        // Header
        sb.AppendLine("Namn;Kortnamn;Typ;Bank;Konto;ISIN;Antal;Köpkurs;Aktuell kurs;Totalt värde;Kostnad;Vinst/Förlust;Avkastning %;Valuta;Land;Marknad");
        
        // Data rows
        foreach (var inv in investments)
        {
            sb.AppendLine($"{EscapeCsv(inv.Name)};{EscapeCsv(inv.ShortName ?? "")};{EscapeCsv(inv.Type)};" +
                         $"{EscapeCsv(inv.BankSource?.Name ?? "")};{EscapeCsv(inv.AccountNumber ?? "")};{EscapeCsv(inv.ISIN ?? "")};" +
                         $"{inv.Quantity.ToString("N4", culture)};{inv.PurchasePrice.ToString("N2", culture)};" +
                         $"{inv.CurrentPrice.ToString("N2", culture)};{inv.TotalValue.ToString("N2", culture)};" +
                         $"{inv.TotalCost.ToString("N2", culture)};{inv.ProfitLoss.ToString("N2", culture)};" +
                         $"{inv.ProfitLossPercentage.ToString("N2", culture)};{EscapeCsv(inv.Currency ?? "")};" +
                         $"{EscapeCsv(inv.Country ?? "")};{EscapeCsv(inv.Market ?? "")}");
        }
        
        return Task.FromResult(sb.ToString());
    }

    public async Task<IEnumerable<Investment>> GetInvestmentsByBankAsync(int bankSourceId)
    {
        return await _context.Investments
            .Include(i => i.BankSource)
            .Where(i => i.BankSourceId == bankSourceId)
            .OrderByDescending(i => i.LastUpdated)
            .ToListAsync();
    }

    public async Task<IEnumerable<Investment>> GetInvestmentsByAccountAsync(string accountNumber)
    {
        return await _context.Investments
            .Include(i => i.BankSource)
            .Where(i => i.AccountNumber == accountNumber)
            .OrderByDescending(i => i.LastUpdated)
            .ToListAsync();
    }

    private string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value))
            return "";
            
        if (value.Contains(';') || value.Contains('"') || value.Contains('\n'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }
        
        return value;
    }
}

