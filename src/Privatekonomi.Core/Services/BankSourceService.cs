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
}
