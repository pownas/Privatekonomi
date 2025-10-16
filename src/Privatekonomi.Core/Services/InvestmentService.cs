using Microsoft.EntityFrameworkCore;
using Privatekonomi.Core.Data;
using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services;

public class InvestmentService : IInvestmentService
{
    private readonly PrivatekonomyContext _context;

    public InvestmentService(PrivatekonomyContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Investment>> GetAllInvestmentsAsync()
    {
        return await _context.Investments
            .OrderByDescending(i => i.LastUpdated)
            .ToListAsync();
    }

    public async Task<Investment?> GetInvestmentByIdAsync(int id)
    {
        return await _context.Investments.FindAsync(id);
    }

    public async Task<Investment> AddInvestmentAsync(Investment investment)
    {
        investment.LastUpdated = DateTime.Now;
        _context.Investments.Add(investment);
        await _context.SaveChangesAsync();
        return investment;
    }

    public async Task<Investment> UpdateInvestmentAsync(Investment investment)
    {
        investment.LastUpdated = DateTime.Now;
        _context.Investments.Update(investment);
        await _context.SaveChangesAsync();
        return investment;
    }

    public async Task DeleteInvestmentAsync(int id)
    {
        var investment = await _context.Investments.FindAsync(id);
        if (investment != null)
        {
            _context.Investments.Remove(investment);
            await _context.SaveChangesAsync();
        }
    }
}
