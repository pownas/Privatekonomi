using Microsoft.EntityFrameworkCore;
using Privatekonomi.Core.Data;
using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services;

public class TransactionService : ITransactionService
{
    private readonly PrivatekonomyContext _context;
    private readonly ICategoryRuleService _categoryRuleService;

    public TransactionService(PrivatekonomyContext context, ICategoryRuleService categoryRuleService)
    {
        _context = context;
        _categoryRuleService = categoryRuleService;
    }

    public async Task<IEnumerable<Transaction>> GetAllTransactionsAsync()
    {
        return await _context.Transactions
            .Include(t => t.BankSource)
            .Include(t => t.TransactionCategories)
            .ThenInclude(tc => tc.Category)
            .OrderByDescending(t => t.Date)
            .ToListAsync();
    }

    public async Task<Transaction?> GetTransactionByIdAsync(int id)
    {
        return await _context.Transactions
            .Include(t => t.BankSource)
            .Include(t => t.TransactionCategories)
            .ThenInclude(tc => tc.Category)
            .FirstOrDefaultAsync(t => t.TransactionId == id);
    }

    public async Task<Transaction> CreateTransactionAsync(Transaction transaction)
    {
        // Set audit fields
        transaction.CreatedAt = DateTime.UtcNow;
        
        // Auto-categorize based on rules or similar descriptions if no categories assigned
        if (!transaction.TransactionCategories.Any())
        {
            // First try rule-based categorization
            var ruleCategoryId = await _categoryRuleService.ApplyCategoryRulesAsync(
                transaction.Description, 
                transaction.Payee);
            
            Category? suggestedCategory = null;
            
            if (ruleCategoryId.HasValue)
            {
                suggestedCategory = await _context.Categories.FindAsync(ruleCategoryId.Value);
            }
            else
            {
                // Fall back to similarity-based categorization
                suggestedCategory = await SuggestCategoryAsync(transaction.Description);
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
        return transaction;
    }

    public async Task<Transaction> UpdateTransactionAsync(Transaction transaction)
    {
        transaction.UpdatedAt = DateTime.UtcNow;
        _context.Entry(transaction).State = EntityState.Modified;
        await _context.SaveChangesAsync();
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
        return await _context.Transactions
            .Include(t => t.BankSource)
            .Include(t => t.TransactionCategories)
            .ThenInclude(tc => tc.Category)
            .Where(t => t.Date >= from && t.Date <= to)
            .OrderByDescending(t => t.Date)
            .ToListAsync();
    }

    private async Task<Category?> SuggestCategoryAsync(string description)
    {
        // Find similar transactions and suggest the most common category
        var similarTransactions = await _context.Transactions
            .Include(t => t.TransactionCategories)
            .ThenInclude(tc => tc.Category)
            .Where(t => t.Description.ToLower().Contains(description.ToLower().Substring(0, Math.Min(3, description.Length))))
            .ToListAsync();

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
        return await _context.Transactions
            .Include(t => t.BankSource)
            .Include(t => t.TransactionCategories)
            .ThenInclude(tc => tc.Category)
            .Where(t => !t.TransactionCategories.Any())
            .OrderByDescending(t => t.Date)
            .ToListAsync();
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
}
