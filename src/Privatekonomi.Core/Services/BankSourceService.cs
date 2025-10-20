using Microsoft.EntityFrameworkCore;
using Privatekonomi.Core.Data;
using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services;

public class BankSourceService : IBankSourceService
{
    private readonly PrivatekonomyContext _context;

    public BankSourceService(PrivatekonomyContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<BankSource>> GetAllBankSourcesAsync()
    {
        return await _context.BankSources
            .OrderBy(b => b.Name)
            .ToListAsync();
    }

    public async Task<BankSource?> GetBankSourceByIdAsync(int id)
    {
        return await _context.BankSources.FindAsync(id);
    }

    public async Task<BankSource> CreateBankSourceAsync(BankSource bankSource)
    {
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
