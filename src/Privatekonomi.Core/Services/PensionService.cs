using Microsoft.EntityFrameworkCore;
using Privatekonomi.Core.Data;
using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services;

public class PensionService : IPensionService
{
    private readonly PrivatekonomyContext _context;
    private readonly ICurrentUserService? _currentUserService;

    public PensionService(PrivatekonomyContext context, ICurrentUserService? currentUserService = null)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<IEnumerable<Pension>> GetAllPensionsAsync()
    {
        var query = _context.Pensions.AsQueryable();

        // Filter by current user if authenticated
        if (_currentUserService?.IsAuthenticated == true && _currentUserService.UserId != null)
        {
            query = query.Where(p => p.UserId == _currentUserService.UserId);
        }

        return await query.OrderByDescending(p => p.LastUpdated).ToListAsync();
    }

    public async Task<Pension?> GetPensionByIdAsync(int id)
    {
        var query = _context.Pensions.AsQueryable();

        // Filter by current user if authenticated
        if (_currentUserService?.IsAuthenticated == true && _currentUserService.UserId != null)
        {
            query = query.Where(p => p.UserId == _currentUserService.UserId);
        }

        return await query.FirstOrDefaultAsync(p => p.PensionId == id);
    }

    public async Task<Pension> AddPensionAsync(Pension pension)
    {
        pension.LastUpdated = DateTime.UtcNow;
        pension.CreatedAt = DateTime.UtcNow;
        
        // Set user ID for new pensions
        if (_currentUserService?.IsAuthenticated == true && _currentUserService.UserId != null)
        {
            pension.UserId = _currentUserService.UserId;
        }
        
        _context.Pensions.Add(pension);
        await _context.SaveChangesAsync();
        return pension;
    }

    public async Task<Pension> UpdatePensionAsync(Pension pension)
    {
        pension.LastUpdated = DateTime.UtcNow;
        pension.UpdatedAt = DateTime.UtcNow;
        _context.Pensions.Update(pension);
        await _context.SaveChangesAsync();
        return pension;
    }

    public async Task DeletePensionAsync(int id)
    {
        var pension = await _context.Pensions.FindAsync(id);
        if (pension != null)
        {
            _context.Pensions.Remove(pension);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<decimal> GetTotalPensionValueAsync()
    {
        var query = _context.Pensions.AsQueryable();

        // Filter by current user if authenticated
        if (_currentUserService?.IsAuthenticated == true && _currentUserService.UserId != null)
        {
            query = query.Where(p => p.UserId == _currentUserService.UserId);
        }

        return await query.SumAsync(p => p.CurrentValue);
    }

    public async Task<decimal> GetTotalPensionContributionsAsync()
    {
        var query = _context.Pensions.AsQueryable();

        // Filter by current user if authenticated
        if (_currentUserService?.IsAuthenticated == true && _currentUserService.UserId != null)
        {
            query = query.Where(p => p.UserId == _currentUserService.UserId);
        }

        return await query.SumAsync(p => p.TotalContributions);
    }

    public async Task<Dictionary<string, decimal>> GetPensionByTypeAsync()
    {
        var query = _context.Pensions.AsQueryable();

        // Filter by current user if authenticated
        if (_currentUserService?.IsAuthenticated == true && _currentUserService.UserId != null)
        {
            query = query.Where(p => p.UserId == _currentUserService.UserId);
        }

        return await query
            .GroupBy(p => p.PensionType)
            .Select(g => new { Type = g.Key, Value = g.Sum(p => p.CurrentValue) })
            .ToDictionaryAsync(x => x.Type, x => x.Value);
    }

    public async Task<Dictionary<string, decimal>> GetPensionByProviderAsync()
    {
        var query = _context.Pensions.AsQueryable();

        // Filter by current user if authenticated
        if (_currentUserService?.IsAuthenticated == true && _currentUserService.UserId != null)
        {
            query = query.Where(p => p.UserId == _currentUserService.UserId);
        }

        return await query
            .Where(p => p.Provider != null)
            .GroupBy(p => p.Provider!)
            .Select(g => new { Provider = g.Key, Value = g.Sum(p => p.CurrentValue) })
            .ToDictionaryAsync(x => x.Provider, x => x.Value);
    }
}
