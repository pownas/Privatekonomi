using Microsoft.EntityFrameworkCore;
using Privatekonomi.Core.Data;
using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services;

public class LoanService : ILoanService
{
    private readonly PrivatekonomyContext _context;

    public LoanService(PrivatekonomyContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Loan>> GetAllLoansAsync()
    {
        return await _context.Loans
            .OrderBy(l => l.Name)
            .ToListAsync();
    }

    public async Task<Loan?> GetLoanByIdAsync(int id)
    {
        return await _context.Loans.FindAsync(id);
    }

    public async Task<Loan> CreateLoanAsync(Loan loan)
    {
        loan.CreatedAt = DateTime.UtcNow;
        _context.Loans.Add(loan);
        await _context.SaveChangesAsync();
        return loan;
    }

    public async Task<Loan> UpdateLoanAsync(Loan loan)
    {
        loan.UpdatedAt = DateTime.UtcNow;
        _context.Entry(loan).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return loan;
    }

    public async Task DeleteLoanAsync(int id)
    {
        var loan = await _context.Loans.FindAsync(id);
        if (loan != null)
        {
            _context.Loans.Remove(loan);
            await _context.SaveChangesAsync();
        }
    }
}
