using Microsoft.EntityFrameworkCore;
using Privatekonomi.Core.Data;
using Privatekonomi.Core.Models;
using Privatekonomi.Core.Services.Parsers;
using System.Text.Json;

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
            new AvanzaTransactionParser(),
            new SwedbankParser(),
            new IcaBankenParser(),
            new OfxParser()
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
                ErrorMessage = $"Fel vid parsning av fil: {ex.Message}"
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

            // Determine file type based on bank/parser
            var fileType = bankName.Contains("OFX", StringComparison.OrdinalIgnoreCase) ? "OFX" : "CSV";
            var importSource = $"{bankName} {fileType} (manuell)";

            // Import transactions with source set to 'manual'
            foreach (var transaction in result.Transactions)
            {
                transaction.BankSourceId = bankSource?.BankSourceId;
                transaction.Imported = true;
                transaction.ImportSource = importSource;
                transaction.CreatedAt = DateTime.UtcNow;
                transaction.ValidFrom = DateTime.UtcNow;
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
    
    /// <summary>
    /// Import transactions and create an ImportJob record for tracking.
    /// </summary>
    public async Task<(CsvImportResult Result, ImportJob Job)> ImportWithJobAsync(
        Stream stream, 
        string bankName, 
        string fileName, 
        long fileSize, 
        string? userId = null,
        bool skipDuplicates = true)
    {
        // Create import job
        var importJob = new ImportJob
        {
            BankName = bankName,
            FileType = bankName.Contains("OFX", StringComparison.OrdinalIgnoreCase) ? "OFX" : "CSV",
            FileName = fileName,
            FileSize = fileSize,
            Source = "manual",
            UserId = userId,
            Status = "Processing",
            CreatedAt = DateTime.UtcNow,
            StartedAt = DateTime.UtcNow
        };
        
        _context.ImportJobs.Add(importJob);
        await _context.SaveChangesAsync();
        
        try
        {
            var result = await PreviewCsvAsync(stream, bankName);
            
            importJob.TotalRows = result.TotalRows;
            importJob.DuplicateCount = result.DuplicateCount;
            importJob.ErrorCount = result.ErrorCount;
            
            if (!result.Success || result.Transactions.Count == 0)
            {
                importJob.Status = result.Transactions.Count == 0 && result.Success ? "Completed" : "Failed";
                importJob.ErrorMessages = result.Errors.Any() 
                    ? JsonSerializer.Serialize(result.Errors) 
                    : null;
                importJob.CompletedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return (result, importJob);
            }

            // Get BankSource by name
            var bankSource = await _context.BankSources
                .FirstOrDefaultAsync(b => b.Name.Equals(bankName, StringComparison.OrdinalIgnoreCase));

            var importSource = $"{bankName} {importJob.FileType} (manuell)";

            // Import transactions with source set to 'manual'
            foreach (var transaction in result.Transactions)
            {
                transaction.BankSourceId = bankSource?.BankSourceId;
                transaction.Imported = true;
                transaction.ImportSource = importSource;
                transaction.UserId = userId;
                transaction.CreatedAt = DateTime.UtcNow;
                transaction.ValidFrom = DateTime.UtcNow;
                _context.Transactions.Add(transaction);
            }

            await _context.SaveChangesAsync();
            
            importJob.ImportedCount = result.Transactions.Count;
            importJob.Status = "Completed";
            importJob.CompletedAt = DateTime.UtcNow;
            
            if (result.Errors.Any())
            {
                importJob.ErrorMessages = JsonSerializer.Serialize(result.Errors);
            }
            
            await _context.SaveChangesAsync();
            
            result.Success = true;
            return (result, importJob);
        }
        catch (Exception ex)
        {
            importJob.Status = "Failed";
            importJob.CompletedAt = DateTime.UtcNow;
            importJob.ErrorMessages = JsonSerializer.Serialize(new[]
            {
                new CsvImportError
                {
                    RowNumber = 0,
                    ErrorType = "ImportError",
                    ErrorMessage = ex.Message
                }
            });
            
            await _context.SaveChangesAsync();
            
            var failedResult = new CsvImportResult
            {
                Success = false,
                Errors = { new CsvImportError { RowNumber = 0, ErrorType = "ImportError", ErrorMessage = ex.Message } }
            };
            
            return (failedResult, importJob);
        }
    }
    
    /// <summary>
    /// Get an import job by ID.
    /// </summary>
    public async Task<ImportJob?> GetImportJobAsync(int importJobId)
    {
        return await _context.ImportJobs.FindAsync(importJobId);
    }
    
    /// <summary>
    /// Get all import jobs for a user.
    /// </summary>
    public async Task<List<ImportJob>> GetUserImportJobsAsync(string userId)
    {
        return await _context.ImportJobs
            .Where(j => j.UserId == userId)
            .OrderByDescending(j => j.CreatedAt)
            .ToListAsync();
    }

    private ICsvParser? GetParser(string bankName)
    {
        // First try exact match
        var parser = _parsers.FirstOrDefault(p => 
            p.BankName.Equals(bankName, StringComparison.OrdinalIgnoreCase));
        
        // If not found and it's OFX, use the OFX parser
        if (parser == null && bankName.Contains("OFX", StringComparison.OrdinalIgnoreCase))
        {
            parser = _parsers.FirstOrDefault(p => p is OfxParser);
        }
        
        return parser;
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
