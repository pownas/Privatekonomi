using Microsoft.EntityFrameworkCore;
using Privatekonomi.Core.Data;
using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services;

public class BankSourceService : IBankSourceService
{
    private readonly PrivatekonomyContext _context;
    private readonly ICurrentUserService? _currentUserService;

    public BankSourceService(PrivatekonomyContext context, ICurrentUserService? currentUserService = null)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<IEnumerable<BankSource>> GetAllBankSourcesAsync()
    {
        var query = _context.BankSources.AsQueryable();

        // Filter by current user if authenticated
        if (_currentUserService?.IsAuthenticated == true && _currentUserService.UserId != null)
        {
            query = query.Where(b => b.UserId == _currentUserService.UserId);
        }

        return await query.OrderBy(b => b.Name).ToListAsync();
    }

    public async Task<BankSource?> GetBankSourceByIdAsync(int id)
    {
        var query = _context.BankSources.AsQueryable();

        // Filter by current user if authenticated
        if (_currentUserService?.IsAuthenticated == true && _currentUserService.UserId != null)
        {
            query = query.Where(b => b.UserId == _currentUserService.UserId);
        }

        return await query.FirstOrDefaultAsync(b => b.BankSourceId == id);
    }

    public async Task<BankSource> CreateBankSourceAsync(BankSource bankSource)
    {
        // Set user ID for new bank sources
        if (_currentUserService?.IsAuthenticated == true && _currentUserService.UserId != null)
        {
            bankSource.UserId = _currentUserService.UserId;
        }
        
        _context.BankSources.Add(bankSource);
        await _context.SaveChangesAsync();
        return bankSource;
    }

    public async Task<BankSource> UpdateBankSourceAsync(BankSource bankSource)
    {
        _context.Entry(bankSource).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return bankSource;
    }

    public async Task DeleteBankSourceAsync(int id)
    {
        var bankSource = await _context.BankSources.FindAsync(id);
        if (bankSource != null)
        {
            _context.BankSources.Remove(bankSource);
            await _context.SaveChangesAsync();
        }
    }
}
