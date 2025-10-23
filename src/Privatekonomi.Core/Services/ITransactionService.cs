using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services;

public interface ITransactionService
{
    Task<IEnumerable<Transaction>> GetAllTransactionsAsync();
    Task<Transaction?> GetTransactionByIdAsync(int id);
    Task<Transaction> CreateTransactionAsync(Transaction transaction);
    Task<Transaction> UpdateTransactionAsync(Transaction transaction);
    Task DeleteTransactionAsync(int id);
    Task<IEnumerable<Transaction>> GetTransactionsByDateRangeAsync(DateTime from, DateTime to);
    Task<IEnumerable<Transaction>> GetUnmappedTransactionsAsync();
    Task UpdateTransactionCategoriesAsync(int transactionId, List<TransactionCategory> categories);
    Task<IEnumerable<Transaction>> GetTransactionsByHouseholdAsync(int householdId);
    Task<IEnumerable<Transaction>> GetTransactionsByHouseholdAndDateRangeAsync(int householdId, DateTime from, DateTime to);
}
