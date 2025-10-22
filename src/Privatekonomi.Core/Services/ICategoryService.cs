using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services;

public interface ICategoryService
{
    Task<IEnumerable<Category>> GetAllCategoriesAsync();
    Task<Category?> GetCategoryByIdAsync(int id);
    Task<Category> CreateCategoryAsync(Category category);
    Task<Category> UpdateCategoryAsync(Category category);
    Task DeleteCategoryAsync(int id);
    Task<IEnumerable<Transaction>> GetTransactionsByCategoryAsync(int categoryId, DateTime? from = null, DateTime? to = null);
    Task<CategoryStatistics> GetCategoryStatisticsAsync(int categoryId, int months);
}
