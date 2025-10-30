using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services;

public interface IReceiptService
{
    Task<List<Receipt>> GetReceiptsAsync(string userId);
    Task<Receipt?> GetReceiptByIdAsync(int receiptId, string userId);
    Task<Receipt> CreateReceiptAsync(Receipt receipt);
    Task<Receipt> UpdateReceiptAsync(Receipt receipt);
    Task DeleteReceiptAsync(int receiptId, string userId);
    Task<List<Receipt>> GetReceiptsByDateRangeAsync(string userId, DateTime startDate, DateTime endDate);
    Task<List<Receipt>> GetReceiptsByMerchantAsync(string userId, string merchant);
}
