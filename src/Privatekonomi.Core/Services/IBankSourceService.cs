using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services;

public interface IBankSourceService
{
    Task<IEnumerable<BankSource>> GetAllBankSourcesAsync();
    Task<BankSource?> GetBankSourceByIdAsync(int id);
}
