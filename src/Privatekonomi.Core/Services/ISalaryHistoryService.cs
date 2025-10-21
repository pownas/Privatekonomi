using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services;

public interface ISalaryHistoryService
{
    Task<IEnumerable<SalaryHistory>> GetAllSalaryHistoriesAsync(string userId);
    Task<SalaryHistory?> GetSalaryHistoryByIdAsync(int id, string userId);
    Task<SalaryHistory?> GetCurrentSalaryAsync(string userId);
    Task<IEnumerable<SalaryHistory>> GetSalaryHistoriesByPeriodAsync(string userId, DateTime startPeriod, DateTime endPeriod);
    Task<SalaryHistory> AddSalaryHistoryAsync(SalaryHistory salaryHistory);
    Task<SalaryHistory> UpdateSalaryHistoryAsync(SalaryHistory salaryHistory);
    Task DeleteSalaryHistoryAsync(int id, string userId);
    Task<decimal> GetAverageSalaryAsync(string userId, int months);
    Task<decimal> GetSalaryGrowthPercentageAsync(string userId, int months);
}
