using Microsoft.EntityFrameworkCore;
using Privatekonomi.Core.Data;
using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services;

public class ReceiptService : IReceiptService
{
    private readonly PrivatekonomyContext _context;
    private readonly IAuditLogService _auditLogService;

    public ReceiptService(PrivatekonomyContext context, IAuditLogService auditLogService)
    {
        _context = context;
        _auditLogService = auditLogService;
    }

    public async Task<List<Receipt>> GetReceiptsAsync(string userId)
    {
        return await _context.Receipts
            .Where(r => r.UserId == userId)
            .Include(r => r.ReceiptLineItems)
            .ThenInclude(li => li.Category)
            .Include(r => r.Transaction)
            .OrderByDescending(r => r.ReceiptDate)
            .ToListAsync();
    }

    public async Task<Receipt?> GetReceiptByIdAsync(int receiptId, string userId)
    {
        return await _context.Receipts
            .Where(r => r.ReceiptId == receiptId && r.UserId == userId)
            .Include(r => r.ReceiptLineItems)
            .ThenInclude(li => li.Category)
            .Include(r => r.Transaction)
            .FirstOrDefaultAsync();
    }

    public async Task<Receipt> CreateReceiptAsync(Receipt receipt)
    {
        receipt.CreatedAt = DateTime.UtcNow;
        _context.Receipts.Add(receipt);
        await _context.SaveChangesAsync();
        
        await _auditLogService.LogAsync("Create", "Receipt", receipt.ReceiptId, 
            $"Created receipt: {receipt.Merchant} - {receipt.TotalAmount:C}", receipt.UserId);
        
        return receipt;
    }

    public async Task<Receipt> UpdateReceiptAsync(Receipt receipt)
    {
        receipt.UpdatedAt = DateTime.UtcNow;
        _context.Receipts.Update(receipt);
        await _context.SaveChangesAsync();
        
        await _auditLogService.LogAsync("Update", "Receipt", receipt.ReceiptId, 
            $"Updated receipt: {receipt.Merchant}", receipt.UserId);
        
        return receipt;
    }

    public async Task DeleteReceiptAsync(int receiptId, string userId)
    {
        var receipt = await GetReceiptByIdAsync(receiptId, userId);
        if (receipt != null)
        {
            _context.Receipts.Remove(receipt);
            await _context.SaveChangesAsync();
            
            await _auditLogService.LogAsync("Delete", "Receipt", receiptId, 
                $"Deleted receipt: {receipt.Merchant}", userId);
        }
    }

    public async Task<List<Receipt>> GetReceiptsByDateRangeAsync(string userId, DateTime startDate, DateTime endDate)
    {
        return await _context.Receipts
            .Where(r => r.UserId == userId && r.ReceiptDate >= startDate && r.ReceiptDate <= endDate)
            .Include(r => r.ReceiptLineItems)
            .ThenInclude(li => li.Category)
            .OrderByDescending(r => r.ReceiptDate)
            .ToListAsync();
    }

    public async Task<List<Receipt>> GetReceiptsByMerchantAsync(string userId, string merchant)
    {
        return await _context.Receipts
            .Where(r => r.UserId == userId && r.Merchant.Contains(merchant))
            .Include(r => r.ReceiptLineItems)
            .ThenInclude(li => li.Category)
            .OrderByDescending(r => r.ReceiptDate)
            .ToListAsync();
    }
}
