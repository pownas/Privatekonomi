using Microsoft.EntityFrameworkCore;
using Privatekonomi.Core.Data;
using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services;

public class BillService : IBillService
{
    private readonly PrivatekonomyContext _context;
    private readonly IAuditLogService _auditLogService;

    public BillService(PrivatekonomyContext context, IAuditLogService auditLogService)
    {
        _context = context;
        _auditLogService = auditLogService;
    }

    public async Task<List<Bill>> GetBillsAsync(string userId)
    {
        return await _context.Bills
            .Where(b => b.UserId == userId)
            .Include(b => b.Category)
            .Include(b => b.Household)
            .Include(b => b.Transaction)
            .Include(b => b.Reminders)
            .OrderByDescending(b => b.DueDate)
            .ToListAsync();
    }

    public async Task<List<Bill>> GetPendingBillsAsync(string userId)
    {
        return await _context.Bills
            .Where(b => b.UserId == userId && b.Status == "Pending")
            .Include(b => b.Category)
            .Include(b => b.Household)
            .Include(b => b.Reminders)
            .OrderBy(b => b.DueDate)
            .ToListAsync();
    }

    public async Task<List<Bill>> GetOverdueBillsAsync(string userId)
    {
        var today = DateTime.UtcNow.Date;
        return await _context.Bills
            .Where(b => b.UserId == userId && b.Status == "Pending" && b.DueDate < today)
            .Include(b => b.Category)
            .Include(b => b.Household)
            .Include(b => b.Reminders)
            .OrderBy(b => b.DueDate)
            .ToListAsync();
    }

    public async Task<Bill?> GetBillByIdAsync(int billId, string userId)
    {
        return await _context.Bills
            .Where(b => b.BillId == billId && b.UserId == userId)
            .Include(b => b.Category)
            .Include(b => b.Household)
            .Include(b => b.Transaction)
            .Include(b => b.Reminders)
            .FirstOrDefaultAsync();
    }

    public async Task<Bill> CreateBillAsync(Bill bill)
    {
        bill.CreatedAt = DateTime.UtcNow;
        _context.Bills.Add(bill);
        await _context.SaveChangesAsync();
        
        await _auditLogService.LogAsync("Create", "Bill", bill.BillId, 
            $"Created bill: {bill.Name} - {bill.Amount:C} due {bill.DueDate:yyyy-MM-dd}", bill.UserId);
        
        return bill;
    }

    public async Task<Bill> UpdateBillAsync(Bill bill)
    {
        bill.UpdatedAt = DateTime.UtcNow;
        _context.Bills.Update(bill);
        await _context.SaveChangesAsync();
        
        await _auditLogService.LogAsync("Update", "Bill", bill.BillId, 
            $"Updated bill: {bill.Name}", bill.UserId);
        
        return bill;
    }

    public async Task DeleteBillAsync(int billId, string userId)
    {
        var bill = await GetBillByIdAsync(billId, userId);
        if (bill != null)
        {
            _context.Bills.Remove(bill);
            await _context.SaveChangesAsync();
            
            await _auditLogService.LogAsync("Delete", "Bill", billId, 
                $"Deleted bill: {bill.Name}", userId);
        }
    }

    public async Task MarkBillAsPaidAsync(int billId, DateTime paidDate, int? transactionId = null)
    {
        var bill = await _context.Bills.FindAsync(billId);
        if (bill == null)
            throw new InvalidOperationException("Bill not found");

        bill.Status = "Paid";
        bill.PaidDate = paidDate;
        bill.TransactionId = transactionId;
        bill.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        
        await _auditLogService.LogAsync("MarkAsPaid", "Bill", billId, 
            $"Marked bill as paid: {bill.Name}", bill.UserId);
    }

    public async Task<List<Bill>> GetBillsByHouseholdAsync(int householdId, string userId)
    {
        return await _context.Bills
            .Where(b => b.HouseholdId == householdId && b.UserId == userId)
            .Include(b => b.Category)
            .Include(b => b.Household)
            .Include(b => b.Reminders)
            .OrderByDescending(b => b.DueDate)
            .ToListAsync();
    }

    public async Task<List<Bill>> GetBillsDueSoonAsync(string userId, int daysAhead)
    {
        var today = DateTime.UtcNow.Date;
        var cutoffDate = today.AddDays(daysAhead);
        
        return await _context.Bills
            .Where(b => b.UserId == userId && b.Status == "Pending" && 
                       b.DueDate >= today && b.DueDate <= cutoffDate)
            .Include(b => b.Category)
            .Include(b => b.Household)
            .OrderBy(b => b.DueDate)
            .ToListAsync();
    }

    public async Task<List<BillReminder>> GetPendingRemindersAsync()
    {
        var now = DateTime.UtcNow;
        return await _context.BillReminders
            .Where(r => !r.IsSent && r.ReminderDate <= now)
            .Include(r => r.Bill)
            .ToListAsync();
    }

    public async Task AddReminderAsync(int billId, DateTime reminderDate, string reminderMethod = "Notification")
    {
        var reminder = new BillReminder
        {
            BillId = billId,
            ReminderDate = reminderDate,
            ReminderMethod = reminderMethod,
            IsSent = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.BillReminders.Add(reminder);
        await _context.SaveChangesAsync();
    }

    public async Task MarkReminderAsSentAsync(int reminderId)
    {
        var reminder = await _context.BillReminders.FindAsync(reminderId);
        if (reminder != null)
        {
            reminder.IsSent = true;
            reminder.SentDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}
