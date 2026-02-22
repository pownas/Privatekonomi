using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Privatekonomi.Core.Data;
using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services;

public class DataImportService : IDataImportService
{
    private readonly PrivatekonomyContext _context;
    private readonly ICurrentUserService? _currentUserService;
    private readonly ILogger<DataImportService> _logger;

    public DataImportService(
        PrivatekonomyContext context, 
        ILogger<DataImportService> logger,
        ICurrentUserService? currentUserService = null)
    {
        _context = context;
        _logger = logger;
        _currentUserService = currentUserService;
    }

    public async Task<DataImportResult> ImportFullBackupAsync(byte[] backupData, bool mergeMode = false)
    {
        var result = new DataImportResult { Success = false };
        
        try
        {
            // Parse JSON
            var json = Encoding.UTF8.GetString(backupData);
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;

            // Validate backup format
            if (!root.TryGetProperty("version", out var versionProp) || 
                !root.TryGetProperty("data", out var dataProp))
            {
                result.ErrorMessage = "Invalid backup format. Missing version or data property.";
                return result;
            }

            var version = versionProp.GetString();
            _logger.LogInformation("Importing backup version {Version}, merge mode: {MergeMode}", version, mergeMode);

            // If not in merge mode, clear existing data for the current user
            if (!mergeMode)
            {
                await ClearUserDataAsync();
                result.Warnings.Add("All existing data has been replaced with the backup data.");
            }

            // Import data in correct order (respecting foreign key constraints)
            var data = dataProp;
            
            // Import Categories first (no dependencies)
            if (data.TryGetProperty("categories", out var categoriesProp))
            {
                var categories = JsonSerializer.Deserialize<List<Category>>(categoriesProp.GetRawText());
                if (categories != null)
                {
                    result.ImportedCounts["Categories"] = await ImportEntitiesAsync(categories, mergeMode);
                }
            }

            // Import BankSources (no dependencies)
            if (data.TryGetProperty("bankSources", out var bankSourcesProp))
            {
                var bankSources = JsonSerializer.Deserialize<List<BankSource>>(bankSourcesProp.GetRawText());
                if (bankSources != null)
                {
                    result.ImportedCounts["BankSources"] = await ImportEntitiesAsync(bankSources, mergeMode);
                }
            }

            // Import Goals (no dependencies)
            if (data.TryGetProperty("goals", out var goalsProp))
            {
                var goals = JsonSerializer.Deserialize<List<Goal>>(goalsProp.GetRawText());
                if (goals != null)
                {
                    result.ImportedCounts["Goals"] = await ImportEntitiesAsync(goals, mergeMode);
                }
            }

            // Import Investments (no dependencies)
            if (data.TryGetProperty("investments", out var investmentsProp))
            {
                var investments = JsonSerializer.Deserialize<List<Investment>>(investmentsProp.GetRawText());
                if (investments != null)
                {
                    result.ImportedCounts["Investments"] = await ImportEntitiesAsync(investments, mergeMode);
                }
            }

            // Import Loans (no dependencies)
            if (data.TryGetProperty("loans", out var loansProp))
            {
                var loans = JsonSerializer.Deserialize<List<Loan>>(loansProp.GetRawText());
                if (loans != null)
                {
                    result.ImportedCounts["Loans"] = await ImportEntitiesAsync(loans, mergeMode);
                }
            }

            // Import Assets (no dependencies)
            if (data.TryGetProperty("assets", out var assetsProp))
            {
                var assets = JsonSerializer.Deserialize<List<Asset>>(assetsProp.GetRawText());
                if (assets != null)
                {
                    result.ImportedCounts["Assets"] = await ImportEntitiesAsync(assets, mergeMode);
                }
            }

            // Import Transactions (depends on BankSources)
            if (data.TryGetProperty("transactions", out var transactionsProp))
            {
                var transactions = JsonSerializer.Deserialize<List<Transaction>>(transactionsProp.GetRawText());
                if (transactions != null)
                {
                    result.ImportedCounts["Transactions"] = await ImportEntitiesAsync(transactions, mergeMode);
                }
            }

            // Import Budgets (depends on Categories)
            if (data.TryGetProperty("budgets", out var budgetsProp))
            {
                var budgets = JsonSerializer.Deserialize<List<Budget>>(budgetsProp.GetRawText());
                if (budgets != null)
                {
                    result.ImportedCounts["Budgets"] = await ImportEntitiesAsync(budgets, mergeMode);
                }
            }

            // Import BankConnections (depends on BankSources)
            if (data.TryGetProperty("bankConnections", out var bankConnectionsProp))
            {
                var bankConnections = JsonSerializer.Deserialize<List<BankConnection>>(bankConnectionsProp.GetRawText());
                if (bankConnections != null)
                {
                    result.ImportedCounts["BankConnections"] = await ImportEntitiesAsync(bankConnections, mergeMode);
                }
            }

            // Import Households
            if (data.TryGetProperty("households", out var householdsProp))
            {
                var households = JsonSerializer.Deserialize<List<Household>>(householdsProp.GetRawText());
                if (households != null)
                {
                    result.ImportedCounts["Households"] = await ImportEntitiesAsync(households, mergeMode);
                }
            }

            await _context.SaveChangesAsync();
            
            result.Success = true;
            _logger.LogInformation("Backup import completed successfully. Imported counts: {Counts}", 
                string.Join(", ", result.ImportedCounts.Select(kvp => $"{kvp.Key}={kvp.Value}")));
        }
        catch (JsonException ex)
        {
            result.ErrorMessage = $"Invalid JSON format: {ex.Message}";
            _logger.LogError(ex, "JSON parsing error during import");
        }
        catch (Exception ex)
        {
            result.ErrorMessage = $"Import failed: {ex.Message}";
            _logger.LogError(ex, "Error during backup import");
        }

        return result;
    }

    private async Task ClearUserDataAsync()
    {
        // Only clear data for the current user if authenticated
        if (_currentUserService?.IsAuthenticated == true && _currentUserService.UserId != null)
        {
            var userId = _currentUserService.UserId;
            _logger.LogInformation("Clearing existing data for user {UserId}", userId);

            // Clear in reverse order of dependencies
            _context.TransactionCategories.RemoveRange(
                await _context.TransactionCategories.Where(tc => tc.Transaction.UserId == userId).ToListAsync());
            _context.Transactions.RemoveRange(
                await _context.Transactions.Where(t => t.UserId == userId).ToListAsync());
            _context.BudgetCategories.RemoveRange(
                await _context.BudgetCategories.Where(bc => bc.Budget.UserId == userId).ToListAsync());
            _context.Budgets.RemoveRange(
                await _context.Budgets.Where(b => b.UserId == userId).ToListAsync());
            _context.Goals.RemoveRange(
                await _context.Goals.Where(g => g.UserId == userId).ToListAsync());
            _context.Investments.RemoveRange(
                await _context.Investments.Where(i => i.UserId == userId).ToListAsync());
            _context.Loans.RemoveRange(
                await _context.Loans.Where(l => l.UserId == userId).ToListAsync());
            _context.Assets.RemoveRange(
                await _context.Assets.Where(a => a.UserId == userId).ToListAsync());
            // BankConnections don't have UserId, so we need to find them through BankSource or skip
            // For now, we'll skip BankConnections as they may be shared across users

            await _context.SaveChangesAsync();
        }
        else
        {
            _logger.LogWarning("User not authenticated, clearing all data");
            // If not authenticated (development mode), clear all data
            _context.TransactionCategories.RemoveRange(_context.TransactionCategories);
            _context.Transactions.RemoveRange(_context.Transactions);
            _context.BudgetCategories.RemoveRange(_context.BudgetCategories);
            _context.Budgets.RemoveRange(_context.Budgets);
            _context.Goals.RemoveRange(_context.Goals);
            _context.Investments.RemoveRange(_context.Investments);
            _context.Loans.RemoveRange(_context.Loans);
            _context.Assets.RemoveRange(_context.Assets);
            _context.BankConnections.RemoveRange(_context.BankConnections);
            
            await _context.SaveChangesAsync();
        }
    }

    private async Task<int> ImportEntitiesAsync<T>(List<T> entities, bool mergeMode) where T : class
    {
        if (entities == null || entities.Count == 0)
            return 0;

        int importedCount = 0;

        foreach (var entity in entities)
        {
            // In merge mode, check if entity already exists
            if (mergeMode)
            {
                // Get primary key property
                var keyProperties = _context.Model.FindEntityType(typeof(T))?.FindPrimaryKey()?.Properties;
                var keyProperty = keyProperties is { Count: > 0 } ? keyProperties[0] : null;
                if (keyProperty != null)
                {
                    var keyValue = keyProperty.PropertyInfo?.GetValue(entity);
                    if (keyValue != null)
                    {
                        var existingEntity = await _context.Set<T>().FindAsync(keyValue);
                        if (existingEntity != null)
                        {
                            // Entity exists, update it
                            _context.Entry(existingEntity).CurrentValues.SetValues(entity);
                            importedCount++;
                            continue;
                        }
                    }
                }
            }

            // Add new entity
            await _context.Set<T>().AddAsync(entity);
            importedCount++;
        }

        return importedCount;
    }
}
