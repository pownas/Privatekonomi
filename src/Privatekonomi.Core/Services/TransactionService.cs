using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Privatekonomi.Core.Data;
using Privatekonomi.Core.Models;
using Privatekonomi.Core.ML;

namespace Privatekonomi.Core.Services;

public class TransactionService : ITransactionService
{
    private readonly PrivatekonomyContext _context;
    private readonly ICurrentUserService? _currentUserService;
    private readonly ICategoryRuleService _categoryRuleService;
    private readonly IAuditLogService _auditLogService;
    private readonly ITransactionMLService? _mlService;
    private readonly IRoundUpService? _roundUpService;
    private readonly ILogger<TransactionService>? _logger;

    public TransactionService(
        PrivatekonomyContext context, 
        ICategoryRuleService categoryRuleService, 
        IAuditLogService auditLogService,
        ICurrentUserService? currentUserService = null,
        ITransactionMLService? mlService = null,
        IRoundUpService? roundUpService = null,
        ILogger<TransactionService>? logger = null)
    {
        _context = context;
        _currentUserService = currentUserService;
        _categoryRuleService = categoryRuleService;
        _auditLogService = auditLogService;
        _mlService = mlService;
        _roundUpService = roundUpService;
        _logger = logger;
    }

    public async Task<IEnumerable<Transaction>> GetAllTransactionsAsync()
    {
        var query = _context.Transactions
            .Include(t => t.BankSource)
            .Include(t => t.TransactionCategories)
            .ThenInclude(tc => tc.Category)
            .Include(t => t.Household)
            .Include(t => t.Receipts)
            .AsQueryable();

        // Filter by current user if authenticated
        if (_currentUserService?.IsAuthenticated == true && _currentUserService.UserId != null)
        {
            query = query.Where(t => t.UserId == _currentUserService.UserId);
        }

        return await query.OrderByDescending(t => t.Date).ToListAsync();
    }

    public async Task<Transaction?> GetTransactionByIdAsync(int id)
    {
        var query = _context.Transactions
            .Include(t => t.BankSource)
            .Include(t => t.TransactionCategories)
            .ThenInclude(tc => tc.Category)
            .Include(t => t.Household)
            .Include(t => t.Receipts)
            .AsQueryable();

        // Filter by current user if authenticated
        if (_currentUserService?.IsAuthenticated == true && _currentUserService.UserId != null)
        {
            query = query.Where(t => t.UserId == _currentUserService.UserId);
        }

        return await query.FirstOrDefaultAsync(t => t.TransactionId == id);
    }

    public async Task<Transaction> CreateTransactionAsync(Transaction transaction)
    {
        // Set audit fields
        transaction.CreatedAt = DateTime.UtcNow;
        
        // Set user ID for new transactions
        if (_currentUserService?.IsAuthenticated == true && _currentUserService.UserId != null)
        {
            transaction.UserId = _currentUserService.UserId;
        }
        
        // Auto-categorize based on ML, rules, or similar descriptions if no categories assigned
        if (!transaction.TransactionCategories.Any())
        {
            Category? suggestedCategory = null;
            
            // First try ML-based categorization if available
            if (_mlService != null && !string.IsNullOrEmpty(transaction.UserId))
            {
                var mlPrediction = await _mlService.PredictCategoryAsync(transaction, transaction.UserId);
                
                // Use ML prediction if confidence is high enough
                if (mlPrediction != null && !mlPrediction.IsUncertain)
                {
                    suggestedCategory = await _context.Categories
                        .FirstOrDefaultAsync(c => c.Name == mlPrediction.Category);
                }
            }
            
            // Fall back to rule-based categorization
            if (suggestedCategory == null)
            {
                var ruleCategoryId = await _categoryRuleService.ApplyCategoryRulesAsync(
                    transaction.Description, 
                    transaction.Payee);
                
                if (ruleCategoryId.HasValue)
                {
                    suggestedCategory = await _context.Categories.FindAsync(ruleCategoryId.Value);
                }
                else
                {
                    // Fall back to similarity-based categorization
                    suggestedCategory = await SuggestCategoryAsync(transaction.Description);
                }
            }
            
            if (suggestedCategory != null)
            {
                transaction.TransactionCategories.Add(new TransactionCategory
                {
                    CategoryId = suggestedCategory.CategoryId,
                    Amount = transaction.Amount
                });
            }
        }

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();
        
        // Process round-up if service is available
        if (_roundUpService != null)
        {
            try
            {
                if (transaction.IsIncome)
                {
                    // Process salary auto-save for income transactions
                    await _roundUpService.ProcessSalaryAutoSaveAsync(transaction.TransactionId);
                }
                else
                {
                    // Process round-up for expense transactions
                    await _roundUpService.ProcessRoundUpForTransactionAsync(transaction.TransactionId);
                }
            }
            catch (Exception ex)
            {
                // Log error but don't fail transaction creation
                _logger?.LogWarning(ex, "Failed to process round-up for transaction {TransactionId}", transaction.TransactionId);
            }
        }
        
        return transaction;
    }

    public async Task<Transaction> UpdateTransactionAsync(Transaction transaction)
    {
        transaction.UpdatedAt = DateTime.UtcNow;
        _context.Entry(transaction).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return transaction;
    }

    public async Task<Transaction> UpdateTransactionWithAuditAsync(
        int id,
        decimal amount,
        DateTime date,
        string description,
        string? payee,
        string? notes,
        string? tags,
        List<(int CategoryId, decimal Amount)>? categories,
        DateTime? clientUpdatedAt,
        string? userId,
        string? ipAddress)
    {
        // Fetch the existing transaction with related data
        var transaction = await _context.Transactions
            .Include(t => t.TransactionCategories)
            .FirstOrDefaultAsync(t => t.TransactionId == id);

        if (transaction == null)
        {
            throw new InvalidOperationException($"Transaction with ID {id} not found");
        }

        // Check if transaction is locked
        if (transaction.IsLocked)
        {
            throw new InvalidOperationException($"Transaction {id} is locked and cannot be edited");
        }

        // Optimistic locking check
        if (clientUpdatedAt.HasValue && transaction.UpdatedAt.HasValue)
        {
            // Compare timestamps (allow small tolerance for precision differences)
            var timeDifference = Math.Abs((transaction.UpdatedAt.Value - clientUpdatedAt.Value).TotalSeconds);
            if (timeDifference > 1) // More than 1 second difference indicates concurrent modification
            {
                throw new InvalidOperationException(
                    $"Transaction {id} has been modified by another user. Please refresh and try again.");
            }
        }

        // Input validation
        if (amount <= 0)
        {
            throw new ArgumentException("Amount must be greater than 0", nameof(amount));
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Description is required", nameof(description));
        }

        // Create a copy of the original for audit logging
        var originalTransaction = new Transaction
        {
            TransactionId = transaction.TransactionId,
            Amount = transaction.Amount,
            Date = transaction.Date,
            Description = transaction.Description,
            Payee = transaction.Payee,
            Notes = transaction.Notes,
            Tags = transaction.Tags,
            UpdatedAt = transaction.UpdatedAt
        };

        // Update transaction fields
        transaction.Amount = amount;
        transaction.Date = date;
        transaction.Description = description;
        transaction.Payee = payee;
        transaction.Notes = notes;
        transaction.Tags = tags;
        transaction.UpdatedAt = DateTime.UtcNow;

        // Update categories if provided
        if (categories != null)
        {
            // Remove existing categories
            _context.TransactionCategories.RemoveRange(transaction.TransactionCategories);
            
            // Add new categories
            transaction.TransactionCategories = categories
                .Select(c => new TransactionCategory
                {
                    TransactionId = id,
                    CategoryId = c.CategoryId,
                    Amount = c.Amount
                })
                .ToList();
        }

        await _context.SaveChangesAsync();

        // Create audit log
        await _auditLogService.LogTransactionUpdateAsync(
            originalTransaction,
            transaction,
            userId,
            ipAddress);

        return transaction;
    }

    public async Task DeleteTransactionAsync(int id)
    {
        var transaction = await _context.Transactions.FindAsync(id);
        if (transaction != null)
        {
            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Transaction>> GetTransactionsByDateRangeAsync(DateTime from, DateTime to)
    {
        var query = _context.Transactions
            .Include(t => t.BankSource)
            .Include(t => t.TransactionCategories)
            .ThenInclude(tc => tc.Category)
            .Include(t => t.Household)
            .Where(t => t.Date >= from && t.Date <= to)
            .AsQueryable();

        // Filter by current user if authenticated
        if (_currentUserService?.IsAuthenticated == true && _currentUserService.UserId != null)
        {
            query = query.Where(t => t.UserId == _currentUserService.UserId);
        }

        return await query.OrderByDescending(t => t.Date).ToListAsync();
    }

    private async Task<Category?> SuggestCategoryAsync(string description)
    {
        // Find similar transactions and suggest the most common category
        var query = _context.Transactions
            .Include(t => t.TransactionCategories)
            .ThenInclude(tc => tc.Category)
            .Where(t => t.Description.ToLower().Contains(description.ToLower().Substring(0, Math.Min(3, description.Length))))
            .AsQueryable();

        // Filter by current user if authenticated
        if (_currentUserService?.IsAuthenticated == true && _currentUserService.UserId != null)
        {
            query = query.Where(t => t.UserId == _currentUserService.UserId);
        }

        var similarTransactions = await query.ToListAsync();

        if (similarTransactions.Any())
        {
            var mostCommonCategory = similarTransactions
                .SelectMany(t => t.TransactionCategories)
                .GroupBy(tc => tc.CategoryId)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault();

            if (mostCommonCategory != null)
            {
                return await _context.Categories.FindAsync(mostCommonCategory.Key);
            }
        }

        return null;
    }

    public async Task<IEnumerable<Transaction>> GetUnmappedTransactionsAsync()
    {
        var query = _context.Transactions
            .Include(t => t.BankSource)
            .Include(t => t.TransactionCategories)
            .ThenInclude(tc => tc.Category)
            .Include(t => t.Household)
            .Where(t => !t.TransactionCategories.Any())
            .AsQueryable();

        // Filter by current user if authenticated
        if (_currentUserService?.IsAuthenticated == true && _currentUserService.UserId != null)
        {
            query = query.Where(t => t.UserId == _currentUserService.UserId);
        }

        return await query.OrderByDescending(t => t.Date).ToListAsync();
    }

    public async Task UpdateTransactionCategoriesAsync(int transactionId, List<TransactionCategory> categories)
    {
        var transaction = await _context.Transactions
            .Include(t => t.TransactionCategories)
            .FirstOrDefaultAsync(t => t.TransactionId == transactionId);

        if (transaction == null)
        {
            throw new ArgumentException($"Transaction with ID {transactionId} not found");
        }

        // Remove existing categories
        _context.TransactionCategories.RemoveRange(transaction.TransactionCategories);

        // Add new categories
        foreach (var category in categories)
        {
            category.TransactionId = transactionId;
            _context.TransactionCategories.Add(category);
        }

        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Transaction>> GetTransactionsByHouseholdAsync(int householdId)
    {
        var query = _context.Transactions
            .Include(t => t.BankSource)
            .Include(t => t.TransactionCategories)
            .ThenInclude(tc => tc.Category)
            .Include(t => t.Household)
            .Where(t => t.HouseholdId == householdId)
            .AsQueryable();

        // Filter by current user if authenticated
        if (_currentUserService?.IsAuthenticated == true && _currentUserService.UserId != null)
        {
            query = query.Where(t => t.UserId == _currentUserService.UserId);
        }

        return await query.OrderByDescending(t => t.Date).ToListAsync();
    }

    public async Task<IEnumerable<Transaction>> GetTransactionsByHouseholdAndDateRangeAsync(int householdId, DateTime from, DateTime to)
    {
        var query = _context.Transactions
            .Include(t => t.BankSource)
            .Include(t => t.TransactionCategories)
            .ThenInclude(tc => tc.Category)
            .Include(t => t.Household)
            .Where(t => t.HouseholdId == householdId && t.Date >= from && t.Date <= to)
            .AsQueryable();

        // Filter by current user if authenticated
        if (_currentUserService?.IsAuthenticated == true && _currentUserService.UserId != null)
        {
            query = query.Where(t => t.UserId == _currentUserService.UserId);
        }

        return await query.OrderByDescending(t => t.Date).ToListAsync();
    }
}
