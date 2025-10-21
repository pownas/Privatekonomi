using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services;

public interface ILoanService
{
    Task<IEnumerable<Loan>> GetAllLoansAsync();
    Task<Loan?> GetLoanByIdAsync(int id);
    Task<Loan> CreateLoanAsync(Loan loan);
    Task<Loan> UpdateLoanAsync(Loan loan);
    Task DeleteLoanAsync(int id);
    
    /// <summary>
    /// Gets total debt across all loans
    /// </summary>
    Task<decimal> GetTotalDebtAsync();
    
    /// <summary>
    /// Gets total monthly cost (interest + amortization) across all loans
    /// </summary>
    Task<decimal> GetTotalMonthlyCostAsync();
    
    /// <summary>
    /// Gets loans by type (e.g., "Kreditkort", "Bolån", etc.)
    /// </summary>
    Task<IEnumerable<Loan>> GetLoansByTypeAsync(string type);
    
    /// <summary>
    /// Gets credit cards with their utilization rates
    /// </summary>
    Task<IEnumerable<Loan>> GetCreditCardsAsync();
}
