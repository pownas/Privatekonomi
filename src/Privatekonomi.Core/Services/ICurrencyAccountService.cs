using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services;

public interface ICurrencyAccountService
{
    Task<IEnumerable<CurrencyAccount>> GetAllCurrencyAccountsAsync();
    Task<CurrencyAccount?> GetCurrencyAccountByIdAsync(int id);
    Task<CurrencyAccount> CreateCurrencyAccountAsync(CurrencyAccount currencyAccount);
    Task UpdateCurrencyAccountAsync(CurrencyAccount currencyAccount);
    Task DeleteCurrencyAccountAsync(int id);
    Task<decimal> GetTotalValueInSEKAsync();
}
