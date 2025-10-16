using Microsoft.EntityFrameworkCore;
using Privatekonomi.Core.Data;
using Privatekonomi.Core.Models;
using Privatekonomi.Core.Services.Parsers;

namespace Privatekonomi.Core.Services;

public class CsvImportService : ICsvImportService
{
    private readonly PrivatekonomyContext _context;
    private readonly List<ICsvParser> _parsers;

    public CsvImportService(PrivatekonomyContext context)
    {
        _context = context;
        _parsers = new List<ICsvParser>
        {
            new SwedbankParser(),
            new IcaBankenParser()
        };
    }

    public async Task<CsvImportResult> PreviewCsvAsync(Stream csvStream, string bankName)
    {
        var result = new CsvImportResult { Success = false };

        try
        {
            // Find the appropriate parser
            var parser = GetParser(bankName);
            if (parser == null)
            {
                result.Errors.Add(new CsvImportError
                {
                    RowNumber = 0,
                    ErrorType = "InvalidBank",
                    ErrorMessage = $"Bank '{bankName}' stöds inte."
                });
                return result;
            }

            // Parse transactions
            var transactions = await parser.ParseAsync(csvStream);
            result.TotalRows = transactions.Count;

            // Validate transactions
            var validTransactions = new List<Transaction>();
            var rowNumber = 1;

            foreach (var transaction in transactions)
            {
                var validationErrors = ValidateTransaction(transaction, rowNumber);
                
                if (validationErrors.Any())
                {
                    result.Errors.AddRange(validationErrors);
                    result.ErrorCount++;
                }
                else
                {
                    validTransactions.Add(transaction);
                }

                rowNumber++;
            }

            // Check for duplicates
            var duplicates = await FindDuplicatesAsync(validTransactions);
            result.DuplicateCount = duplicates.Count;

            // Remove duplicates from valid transactions for preview
            validTransactions = validTransactions
                .Where(t => !duplicates.Any(d => IsDuplicate(t, d)))
                .ToList();

            result.Transactions = validTransactions;
            result.ImportedCount = validTransactions.Count;
            result.Summary = CalculateSummary(validTransactions);
            result.Success = true;
        }
        catch (Exception ex)
        {
            result.Errors.Add(new CsvImportError
            {
                RowNumber = 0,
                ErrorType = "ParseError",
                ErrorMessage = $"Fel vid parsning av CSV-fil: {ex.Message}"
            });
        }

        return result;
    }

    public async Task<CsvImportResult> ImportCsvAsync(Stream csvStream, string bankName, bool skipDuplicates = true)
    {
        var result = await PreviewCsvAsync(csvStream, bankName);
        
        if (!result.Success || result.Transactions.Count == 0)
        {
            return result;
        }

        try
        {
            // Get BankSource by name
            var bankSource = await _context.BankSources
                .FirstOrDefaultAsync(b => b.Name.Equals(bankName, StringComparison.OrdinalIgnoreCase));

            // Import transactions
            foreach (var transaction in result.Transactions)
            {
                transaction.BankSourceId = bankSource?.BankSourceId;
                _context.Transactions.Add(transaction);
            }

            await _context.SaveChangesAsync();
            result.Success = true;
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Errors.Add(new CsvImportError
            {
                RowNumber = 0,
                ErrorType = "DatabaseError",
                ErrorMessage = $"Fel vid sparande till databas: {ex.Message}"
            });
        }

        return result;
    }

    private ICsvParser? GetParser(string bankName)
    {
        return _parsers.FirstOrDefault(p => 
            p.BankName.Equals(bankName, StringComparison.OrdinalIgnoreCase));
    }

    private List<CsvImportError> ValidateTransaction(Transaction transaction, int rowNumber)
    {
        var errors = new List<CsvImportError>();

        // Validate date
        if (transaction.Date > DateTime.Now.AddDays(7))
        {
            errors.Add(new CsvImportError
            {
                RowNumber = rowNumber,
                ErrorType = "InvalidDate",
                ErrorMessage = "Datumet får inte vara senare än 7 dagar framåt i tiden"
            });
        }

        if (transaction.Date < DateTime.Now.AddYears(-10))
        {
            // Warning only, not blocking
        }

        // Validate amount
        if (transaction.Amount == 0)
        {
            // Warning only, not blocking
        }

        if (transaction.Amount > 10_000_000)
        {
            errors.Add(new CsvImportError
            {
                RowNumber = rowNumber,
                ErrorType = "InvalidAmount",
                ErrorMessage = "Beloppet får inte överstiga 10 miljoner"
            });
        }

        // Validate description
        if (string.IsNullOrWhiteSpace(transaction.Description))
        {
            errors.Add(new CsvImportError
            {
                RowNumber = rowNumber,
                ErrorType = "MissingDescription",
                ErrorMessage = "Beskrivning saknas"
            });
        }

        return errors;
    }

    private async Task<List<Transaction>> FindDuplicatesAsync(List<Transaction> transactions)
    {
        var duplicates = new List<Transaction>();
        var existingTransactions = await _context.Transactions.ToListAsync();

        foreach (var transaction in transactions)
        {
            var isDuplicate = existingTransactions.Any(existing => IsDuplicate(transaction, existing));
            if (isDuplicate)
            {
                duplicates.Add(transaction);
            }
        }

        return duplicates;
    }

    private bool IsDuplicate(Transaction t1, Transaction t2)
    {
        return t1.Date.Date == t2.Date.Date &&
               t1.Amount == t2.Amount &&
               t1.IsIncome == t2.IsIncome &&
               string.Equals(t1.Description, t2.Description, StringComparison.OrdinalIgnoreCase);
    }

    private ImportSummary CalculateSummary(List<Transaction> transactions)
    {
        var summary = new ImportSummary();

        foreach (var transaction in transactions)
        {
            if (transaction.IsIncome)
            {
                summary.IncomeAmount += transaction.Amount;
                summary.IncomeCount++;
            }
            else
            {
                summary.ExpenseAmount += transaction.Amount;
                summary.ExpenseCount++;
            }
        }

        summary.TotalAmount = summary.IncomeAmount - summary.ExpenseAmount;

        return summary;
    }
}
