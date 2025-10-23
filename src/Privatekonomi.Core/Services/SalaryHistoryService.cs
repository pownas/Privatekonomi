using Microsoft.EntityFrameworkCore;
using Privatekonomi.Core.Data;
using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services;

public class SalaryHistoryService : ISalaryHistoryService
{
    private readonly PrivatekonomyContext _context;
    private readonly ICurrentUserService? _currentUserService;

    public SalaryHistoryService(PrivatekonomyContext context, ICurrentUserService? currentUserService = null)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<IEnumerable<SalaryHistory>> GetAllSalaryHistoriesAsync(string userId)
    {
        var query = _context.SalaryHistories.AsQueryable();

        // Filter by current user if authenticated
        if (_currentUserService?.IsAuthenticated == true && _currentUserService.UserId != null)
        {
            query = query.Where(s => s.UserId == _currentUserService.UserId);
        }

        return await query.OrderByDescending(s => s.Period).ToListAsync();
    }

    public async Task<SalaryHistory?> GetSalaryHistoryByIdAsync(int id, string userId)
    {
        var query = _context.SalaryHistories.AsQueryable();

        // Filter by current user if authenticated
        if (_currentUserService?.IsAuthenticated == true && _currentUserService.UserId != null)
        {
            query = query.Where(s => s.UserId == _currentUserService.UserId);
        }

        return await query.FirstOrDefaultAsync(s => s.SalaryHistoryId == id);
    }

    public async Task<SalaryHistory?> GetCurrentSalaryAsync(string userId)
    {
        var query = _context.SalaryHistories.AsQueryable();

        // Filter by current user if authenticated
        if (_currentUserService?.IsAuthenticated == true && _currentUserService.UserId != null)
        {
            query = query.Where(s => s.UserId == _currentUserService.UserId && s.IsCurrent);
        }

        return await query.OrderByDescending(s => s.Period).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<SalaryHistory>> GetSalaryHistoriesByPeriodAsync(string userId, DateTime startPeriod, DateTime endPeriod)
    {
        var query = _context.SalaryHistories.AsQueryable();

        // Filter by current user if authenticated
        if (_currentUserService?.IsAuthenticated == true && _currentUserService.UserId != null)
        {
            query = query.Where(s => s.UserId == _currentUserService.UserId && s.Period >= startPeriod && s.Period <= endPeriod);
        }

        return await query.OrderByDescending(s => s.Period).ToListAsync();
    }

    public async Task<SalaryHistory> AddSalaryHistoryAsync(SalaryHistory salaryHistory)
    {
        salaryHistory.CreatedAt = DateTime.UtcNow;

        // Set the user ID from the current user
        if (_currentUserService?.IsAuthenticated == true && _currentUserService.UserId != null)
        {
            salaryHistory.UserId = _currentUserService.UserId;
        }
        
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

        // Ensure the user ID is set from the current user
        if (_currentUserService?.IsAuthenticated == true && _currentUserService.UserId != null)
        {
            salaryHistory.UserId = _currentUserService.UserId;
        }
        
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
        var query = _context.SalaryHistories.AsQueryable();

        // Filter by current user if authenticated
        if (_currentUserService?.IsAuthenticated == true && _currentUserService.UserId != null)
        {
            query = query.Where(s => s.UserId == _currentUserService.UserId);
        }

        var salaryHistory = await query.FirstOrDefaultAsync(s => s.SalaryHistoryId == id);
        
        if (salaryHistory != null)
        {
            _context.SalaryHistories.Remove(salaryHistory);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<decimal> GetAverageSalaryAsync(string userId, int months)
    {
        var startPeriod = DateTime.UtcNow.AddMonths(-months);
        var query = _context.SalaryHistories.AsQueryable();

        // Filter by current user if authenticated
        if (_currentUserService?.IsAuthenticated == true && _currentUserService.UserId != null)
        {
            query = query.Where(s => s.UserId == _currentUserService.UserId && s.Period >= startPeriod);
        }

        var salaries = await query.ToListAsync();
        
        return salaries.Any() ? salaries.Average(s => s.MonthlySalary) : 0;
    }

    public async Task<decimal> GetSalaryGrowthPercentageAsync(string userId, int months)
    {
        var startPeriod = DateTime.UtcNow.AddMonths(-months);
        var query = _context.SalaryHistories.AsQueryable();

        // Filter by current user if authenticated
        if (_currentUserService?.IsAuthenticated == true && _currentUserService.UserId != null)
        {
            query = query.Where(s => s.UserId == _currentUserService.UserId && s.Period >= startPeriod);
        }

        var salaries = await query.OrderBy(s => s.Period).ToListAsync();
        
        if (salaries.Count < 2)
            return 0;
        
        var firstSalary = salaries.First().MonthlySalary;
        var lastSalary = salaries.Last().MonthlySalary;
        
        if (firstSalary == 0)
            return 0;
        
        return ((lastSalary - firstSalary) / firstSalary) * 100;
    }
}
