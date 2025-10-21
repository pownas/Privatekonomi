using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services;

public interface IBankSourceService
{
    Task<IEnumerable<BankSource>> GetAllBankSourcesAsync();
    Task<BankSource?> GetBankSourceByIdAsync(int id);
    Task<BankSource> CreateBankSourceAsync(BankSource bankSource);
    Task<BankSource> UpdateBankSourceAsync(BankSource bankSource);
    Task DeleteBankSourceAsync(int id);
}
