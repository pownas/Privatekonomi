using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services;

public interface IDividendService
{
    Task<IEnumerable<Dividend>> GetAllDividendsAsync();
    Task<IEnumerable<Dividend>> GetDividendsByInvestmentIdAsync(int investmentId);
    Task<Dividend?> GetDividendByIdAsync(int id);
    Task<Dividend> AddDividendAsync(Dividend dividend);
    Task<Dividend> UpdateDividendAsync(Dividend dividend);
    Task DeleteDividendAsync(int id);
    
    // Statistics
    Task<decimal> GetTotalDividendsAsync();
    Task<decimal> GetDividendsForYearAsync(int year);
    Task<decimal> GetDividendsForInvestmentAsync(int investmentId);
}
