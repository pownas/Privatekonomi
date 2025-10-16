using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services;

public interface ILoanService
{
    Task<IEnumerable<Loan>> GetAllLoansAsync();
    Task<Loan?> GetLoanByIdAsync(int id);
    Task<Loan> CreateLoanAsync(Loan loan);
    Task<Loan> UpdateLoanAsync(Loan loan);
    Task DeleteLoanAsync(int id);
}
