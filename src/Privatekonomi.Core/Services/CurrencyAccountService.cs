using Microsoft.EntityFrameworkCore;
using Privatekonomi.Core.Data;
using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services;

public class CurrencyAccountService : ICurrencyAccountService
{
    private readonly PrivatekonomyContext _context;
    private readonly ICurrentUserService? _currentUserService;

    public CurrencyAccountService(PrivatekonomyContext context, ICurrentUserService? currentUserService = null)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<IEnumerable<CurrencyAccount>> GetAllCurrencyAccountsAsync()
    {
        var query = _context.CurrencyAccounts.AsQueryable();

        // Filter by current user if authenticated
        if (_currentUserService?.IsAuthenticated == true && _currentUserService.UserId != null)
        {
            query = query.Where(c => c.UserId == _currentUserService.UserId);
        }

        return await query.OrderBy(c => c.Currency).ToListAsync();
    }

    public async Task<CurrencyAccount?> GetCurrencyAccountByIdAsync(int id)
    {
        var query = _context.CurrencyAccounts.AsQueryable();

        // Filter by current user if authenticated
        if (_currentUserService?.IsAuthenticated == true && _currentUserService.UserId != null)
        {
            query = query.Where(c => c.UserId == _currentUserService.UserId);
        }

        return await query.FirstOrDefaultAsync(c => c.CurrencyAccountId == id);
    }

    public async Task<CurrencyAccount> CreateCurrencyAccountAsync(CurrencyAccount currencyAccount)
    {
        currencyAccount.CreatedAt = DateTime.UtcNow;
        
        // Set user ID for new currency accounts
        if (_currentUserService?.IsAuthenticated == true && _currentUserService.UserId != null)
        {
            currencyAccount.UserId = _currentUserService.UserId;
        }
        
        _context.CurrencyAccounts.Add(currencyAccount);
        await _context.SaveChangesAsync();
        return currencyAccount;
    }

    public async Task UpdateCurrencyAccountAsync(CurrencyAccount currencyAccount)
    {
        currencyAccount.UpdatedAt = DateTime.UtcNow;
        _context.CurrencyAccounts.Update(currencyAccount);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteCurrencyAccountAsync(int id)
    {
        var currencyAccount = await _context.CurrencyAccounts.FindAsync(id);
        if (currencyAccount != null)
        {
            _context.CurrencyAccounts.Remove(currencyAccount);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<decimal> GetTotalValueInSEKAsync()
    {
        var query = _context.CurrencyAccounts.AsQueryable();

        // Filter by current user if authenticated
        if (_currentUserService?.IsAuthenticated == true && _currentUserService.UserId != null)
        {
            query = query.Where(c => c.UserId == _currentUserService.UserId);
        }

        var accounts = await query.ToListAsync();
        return accounts.Sum(c => c.ValueInSEK);
    }
}
