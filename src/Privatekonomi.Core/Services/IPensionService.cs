using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services;

public interface IPensionService
{
    Task<IEnumerable<Pension>> GetAllPensionsAsync();
    Task<Pension?> GetPensionByIdAsync(int id);
    Task<Pension> AddPensionAsync(Pension pension);
    Task<Pension> UpdatePensionAsync(Pension pension);
    Task DeletePensionAsync(int id);
    
    // Aggregation and statistics
    Task<decimal> GetTotalPensionValueAsync();
    Task<decimal> GetTotalPensionContributionsAsync();
    Task<Dictionary<string, decimal>> GetPensionByTypeAsync();
    Task<Dictionary<string, decimal>> GetPensionByProviderAsync();
}
