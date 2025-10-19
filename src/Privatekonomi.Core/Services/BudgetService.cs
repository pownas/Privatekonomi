using Microsoft.EntityFrameworkCore;
using Privatekonomi.Core.Data;
using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services;

public class BudgetService : IBudgetService
{
    private readonly PrivatekonomyContext _context;

    public BudgetService(PrivatekonomyContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Budget>> GetAllBudgetsAsync()
    {
        return await _context.Budgets
            .Include(b => b.BudgetCategories)
            .ThenInclude(bc => bc.Category)
            .OrderByDescending(b => b.StartDate)
            .ToListAsync();
    }

    public async Task<Budget?> GetBudgetByIdAsync(int id)
    {
        return await _context.Budgets
            .Include(b => b.BudgetCategories)
            .ThenInclude(bc => bc.Category)
            .FirstOrDefaultAsync(b => b.BudgetId == id);
    }

    public async Task<Budget> CreateBudgetAsync(Budget budget)
    {
        budget.CreatedAt = DateTime.UtcNow;
        _context.Budgets.Add(budget);
        await _context.SaveChangesAsync();
        return budget;
    }

    public async Task<Budget> UpdateBudgetAsync(Budget budget)
    {
        budget.UpdatedAt = DateTime.UtcNow;
        _context.Entry(budget).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return budget;
    }

    public async Task DeleteBudgetAsync(int id)
    {
        var budget = await _context.Budgets.FindAsync(id);
        if (budget != null)
        {
            _context.Budgets.Remove(budget);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Budget>> GetActiveBudgetsAsync()
    {
        var now = DateTime.Now;
        return await _context.Budgets
            .Include(b => b.BudgetCategories)
            .ThenInclude(bc => bc.Category)
            .Where(b => b.StartDate <= now && b.EndDate >= now)
            .ToListAsync();
    }

    public async Task<Dictionary<int, decimal>> GetActualAmountsByCategoryAsync(int budgetId)
    {
        var budget = await _context.Budgets.FindAsync(budgetId);
        if (budget == null)
        {
            return new Dictionary<int, decimal>();
        }

        var transactions = await _context.Transactions
            .Include(t => t.TransactionCategories)
            .Where(t => t.Date >= budget.StartDate && t.Date <= budget.EndDate)
            .ToListAsync();

        var actualAmounts = new Dictionary<int, decimal>();
        
        foreach (var transaction in transactions)
        {
            foreach (var tc in transaction.TransactionCategories)
            {
                if (!actualAmounts.ContainsKey(tc.CategoryId))
                {
                    actualAmounts[tc.CategoryId] = 0;
                }
                
                // For expenses, use positive amounts; for income, use negative amounts
                actualAmounts[tc.CategoryId] += transaction.IsIncome ? -tc.Amount : tc.Amount;
            }
        }

        return actualAmounts;
    }

    public async Task<IEnumerable<Transaction>> GetTransactionsForBudgetAsync(int budgetId)
    {
        var budget = await _context.Budgets.FindAsync(budgetId);
        if (budget == null)
        {
            return new List<Transaction>();
        }

        return await _context.Transactions
            .Include(t => t.TransactionCategories)
            .ThenInclude(tc => tc.Category)
            .Include(t => t.BankSource)
            .Where(t => t.Date >= budget.StartDate && t.Date <= budget.EndDate)
            .OrderByDescending(t => t.Date)
            .ToListAsync();
    }

    public async Task<Dictionary<int, List<Transaction>>> GetTransactionsByCategoryForBudgetAsync(int budgetId)
    {
        var budget = await _context.Budgets
            .Include(b => b.BudgetCategories)
            .ThenInclude(bc => bc.Category)
            .FirstOrDefaultAsync(b => b.BudgetId == budgetId);
            
        if (budget == null)
        {
            return new Dictionary<int, List<Transaction>>();
        }

        var transactions = await _context.Transactions
            .Include(t => t.TransactionCategories)
            .ThenInclude(tc => tc.Category)
            .Include(t => t.BankSource)
            .Where(t => t.Date >= budget.StartDate && t.Date <= budget.EndDate && !t.IsIncome)
            .OrderByDescending(t => t.Date)
            .ToListAsync();

        var transactionsByCategory = new Dictionary<int, List<Transaction>>();
        
        // Group transactions by their categories
        foreach (var transaction in transactions)
        {
            foreach (var tc in transaction.TransactionCategories)
            {
                if (!transactionsByCategory.ContainsKey(tc.CategoryId))
                {
                    transactionsByCategory[tc.CategoryId] = new List<Transaction>();
                }
                
                // Only add if not already in the list (avoid duplicates)
                if (!transactionsByCategory[tc.CategoryId].Contains(transaction))
                {
                    transactionsByCategory[tc.CategoryId].Add(transaction);
                }
            }
        }

        return transactionsByCategory;
    }
}
