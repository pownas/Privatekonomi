using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services;

public interface IInvestmentService
{
    Task<IEnumerable<Investment>> GetAllInvestmentsAsync();
    Task<Investment?> GetInvestmentByIdAsync(int id);
    Task<Investment> AddInvestmentAsync(Investment investment);
    Task<Investment> UpdateInvestmentAsync(Investment investment);
    Task DeleteInvestmentAsync(int id);
}
