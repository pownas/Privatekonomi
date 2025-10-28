using Microsoft.EntityFrameworkCore;
using Privatekonomi.Core.Data;
using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services;

public class DividendService : IDividendService
{
    private readonly PrivatekonomyContext _context;
    private readonly ICurrentUserService? _currentUserService;

    public DividendService(PrivatekonomyContext context, ICurrentUserService? currentUserService = null)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<IEnumerable<Dividend>> GetAllDividendsAsync()
    {
        var query = _context.Dividends
            .Include(d => d.Investment)
            .AsQueryable();

        // Filter by current user if authenticated
        if (_currentUserService?.IsAuthenticated == true && _currentUserService.UserId != null)
        {
            query = query.Where(d => d.UserId == _currentUserService.UserId);
        }

        return await query.OrderByDescending(d => d.PaymentDate).ToListAsync();
    }

    public async Task<IEnumerable<Dividend>> GetDividendsByInvestmentIdAsync(int investmentId)
    {
        var query = _context.Dividends
            .Include(d => d.Investment)
            .Where(d => d.InvestmentId == investmentId)
            .AsQueryable();

        // Filter by current user if authenticated
        if (_currentUserService?.IsAuthenticated == true && _currentUserService.UserId != null)
        {
            query = query.Where(d => d.UserId == _currentUserService.UserId);
        }

        return await query.OrderByDescending(d => d.PaymentDate).ToListAsync();
    }

    public async Task<Dividend?> GetDividendByIdAsync(int id)
    {
        var query = _context.Dividends
            .Include(d => d.Investment)
            .AsQueryable();

        // Filter by current user if authenticated
        if (_currentUserService?.IsAuthenticated == true && _currentUserService.UserId != null)
        {
            query = query.Where(d => d.UserId == _currentUserService.UserId);
        }

        return await query.FirstOrDefaultAsync(d => d.DividendId == id);
    }

    public async Task<Dividend> AddDividendAsync(Dividend dividend)
    {
        dividend.CreatedAt = DateTime.UtcNow;
        
        // Set user ID for new dividends
        if (_currentUserService?.IsAuthenticated == true && _currentUserService.UserId != null)
        {
            dividend.UserId = _currentUserService.UserId;
        }
        
        _context.Dividends.Add(dividend);
        await _context.SaveChangesAsync();
        return dividend;
    }

    public async Task<Dividend> UpdateDividendAsync(Dividend dividend)
    {
        dividend.UpdatedAt = DateTime.UtcNow;
        _context.Dividends.Update(dividend);
        await _context.SaveChangesAsync();
        return dividend;
    }

    public async Task DeleteDividendAsync(int id)
    {
        var dividend = await _context.Dividends.FindAsync(id);
        if (dividend != null)
        {
            _context.Dividends.Remove(dividend);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<decimal> GetTotalDividendsAsync()
    {
        var query = _context.Dividends.AsQueryable();

        // Filter by current user if authenticated
        if (_currentUserService?.IsAuthenticated == true && _currentUserService.UserId != null)
        {
            query = query.Where(d => d.UserId == _currentUserService.UserId);
        }

        return await query.SumAsync(d => d.TotalAmount);
    }

    public async Task<decimal> GetDividendsForYearAsync(int year)
    {
        var query = _context.Dividends.AsQueryable();

        // Filter by current user if authenticated
        if (_currentUserService?.IsAuthenticated == true && _currentUserService.UserId != null)
        {
            query = query.Where(d => d.UserId == _currentUserService.UserId);
        }

        return await query
            .Where(d => d.PaymentDate.Year == year)
            .SumAsync(d => d.TotalAmount);
    }

    public async Task<decimal> GetDividendsForInvestmentAsync(int investmentId)
    {
        var query = _context.Dividends.AsQueryable();

        // Filter by current user if authenticated
        if (_currentUserService?.IsAuthenticated == true && _currentUserService.UserId != null)
        {
            query = query.Where(d => d.UserId == _currentUserService.UserId);
        }

        return await query
            .Where(d => d.InvestmentId == investmentId)
            .SumAsync(d => d.TotalAmount);
    }
}
