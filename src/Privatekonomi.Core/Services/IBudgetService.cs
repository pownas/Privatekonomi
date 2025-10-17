using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services;

public interface IBudgetService
{
    Task<IEnumerable<Budget>> GetAllBudgetsAsync();
    Task<Budget?> GetBudgetByIdAsync(int id);
    Task<Budget> CreateBudgetAsync(Budget budget);
    Task<Budget> UpdateBudgetAsync(Budget budget);
    Task DeleteBudgetAsync(int id);
    Task<IEnumerable<Budget>> GetActiveBudgetsAsync();
    Task<Dictionary<int, decimal>> GetActualAmountsByCategoryAsync(int budgetId);
}
