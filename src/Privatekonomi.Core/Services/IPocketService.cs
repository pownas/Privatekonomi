using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services;

public interface IPocketService
{
    Task<IEnumerable<Pocket>> GetAllPocketsAsync();
    Task<IEnumerable<Pocket>> GetPocketsByBankSourceAsync(int bankSourceId);
    Task<Pocket?> GetPocketByIdAsync(int id);
    Task<Pocket> CreatePocketAsync(Pocket pocket);
    Task<Pocket> UpdatePocketAsync(Pocket pocket);
    Task DeletePocketAsync(int id);
    Task<Pocket> AllocateMoneyAsync(int pocketId, decimal amount, string? description = null);
    Task<Pocket> WithdrawMoneyAsync(int pocketId, decimal amount, string? description = null);
    Task TransferMoneyAsync(int fromPocketId, int toPocketId, decimal amount, string? description = null);
    Task<IEnumerable<PocketTransaction>> GetPocketTransactionsAsync(int pocketId);
    Task<decimal> GetTotalAllocatedForBankSourceAsync(int bankSourceId);
}
