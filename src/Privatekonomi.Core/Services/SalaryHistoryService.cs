using Microsoft.EntityFrameworkCore;
using Privatekonomi.Core.Data;
using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services;

public class SalaryHistoryService : ISalaryHistoryService
{
    private readonly PrivatekonomyContext _context;

    public SalaryHistoryService(PrivatekonomyContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<SalaryHistory>> GetAllSalaryHistoriesAsync(string userId)
    {
        return await _context.SalaryHistories
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.Period)
            .ToListAsync();
    }

    public async Task<SalaryHistory?> GetSalaryHistoryByIdAsync(int id, string userId)
    {
        return await _context.SalaryHistories
            .FirstOrDefaultAsync(s => s.SalaryHistoryId == id && s.UserId == userId);
    }

    public async Task<SalaryHistory?> GetCurrentSalaryAsync(string userId)
    {
        return await _context.SalaryHistories
            .Where(s => s.UserId == userId && s.IsCurrent)
            .OrderByDescending(s => s.Period)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<SalaryHistory>> GetSalaryHistoriesByPeriodAsync(string userId, DateTime startPeriod, DateTime endPeriod)
    {
        return await _context.SalaryHistories
            .Where(s => s.UserId == userId && s.Period >= startPeriod && s.Period <= endPeriod)
            .OrderByDescending(s => s.Period)
            .ToListAsync();
    }

    public async Task<SalaryHistory> AddSalaryHistoryAsync(SalaryHistory salaryHistory)
    {
        salaryHistory.CreatedAt = DateTime.UtcNow;
        
        // If this is marked as current, unmark any other current salary for this user
        if (salaryHistory.IsCurrent && salaryHistory.UserId != null)
        {
            var currentSalaries = await _context.SalaryHistories
                .Where(s => s.UserId == salaryHistory.UserId && s.IsCurrent)
                .ToListAsync();
            
            foreach (var salary in currentSalaries)
            {
                salary.IsCurrent = false;
                salary.UpdatedAt = DateTime.UtcNow;
            }
        }
        
        _context.SalaryHistories.Add(salaryHistory);
        await _context.SaveChangesAsync();
        return salaryHistory;
    }

    public async Task<SalaryHistory> UpdateSalaryHistoryAsync(SalaryHistory salaryHistory)
    {
        salaryHistory.UpdatedAt = DateTime.UtcNow;
        
        // If this is marked as current, unmark any other current salary for this user
        if (salaryHistory.IsCurrent && salaryHistory.UserId != null)
        {
            var currentSalaries = await _context.SalaryHistories
                .Where(s => s.UserId == salaryHistory.UserId && s.IsCurrent && s.SalaryHistoryId != salaryHistory.SalaryHistoryId)
                .ToListAsync();
            
            foreach (var salary in currentSalaries)
            {
                salary.IsCurrent = false;
                salary.UpdatedAt = DateTime.UtcNow;
            }
        }
        
        _context.SalaryHistories.Update(salaryHistory);
        await _context.SaveChangesAsync();
        return salaryHistory;
    }

    public async Task DeleteSalaryHistoryAsync(int id, string userId)
    {
        var salaryHistory = await _context.SalaryHistories
            .FirstOrDefaultAsync(s => s.SalaryHistoryId == id && s.UserId == userId);
        
        if (salaryHistory != null)
        {
            _context.SalaryHistories.Remove(salaryHistory);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<decimal> GetAverageSalaryAsync(string userId, int months)
    {
        var startPeriod = DateTime.UtcNow.AddMonths(-months);
        var salaries = await _context.SalaryHistories
            .Where(s => s.UserId == userId && s.Period >= startPeriod)
            .ToListAsync();
        
        return salaries.Any() ? salaries.Average(s => s.MonthlySalary) : 0;
    }

    public async Task<decimal> GetSalaryGrowthPercentageAsync(string userId, int months)
    {
        var startPeriod = DateTime.UtcNow.AddMonths(-months);
        var salaries = await _context.SalaryHistories
            .Where(s => s.UserId == userId && s.Period >= startPeriod)
            .OrderBy(s => s.Period)
            .ToListAsync();
        
        if (salaries.Count < 2)
            return 0;
        
        var firstSalary = salaries.First().MonthlySalary;
        var lastSalary = salaries.Last().MonthlySalary;
        
        if (firstSalary == 0)
            return 0;
        
        return ((lastSalary - firstSalary) / firstSalary) * 100;
    }
}
