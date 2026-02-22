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

    public async Task<BillSchedule> CreateScheduleAsync(BillSchedule schedule)
    {
        schedule.CreatedAt = DateTime.UtcNow;
        _context.BillSchedules.Add(schedule);
        await _context.SaveChangesAsync();
        return schedule;
    }

    public async Task<BillSchedule?> GetScheduleByBillIdAsync(int billId)
    {
        return await _context.BillSchedules
            .Include(s => s.Bill)
            .FirstOrDefaultAsync(s => s.BillId == billId);
    }

    public async Task<BillSchedule> UpdateScheduleAsync(BillSchedule schedule)
    {
        schedule.UpdatedAt = DateTime.UtcNow;
        _context.BillSchedules.Update(schedule);
        await _context.SaveChangesAsync();
        return schedule;
    }

    public async Task DeleteScheduleAsync(int billScheduleId)
    {
        var schedule = await _context.BillSchedules.FindAsync(billScheduleId);
        if (schedule != null)
        {
            _context.BillSchedules.Remove(schedule);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<BillSchedule>> GetDueSchedulesAsync()
    {
        var today = DateTime.UtcNow.Date;
        // Load active schedules where the next occurrence is within the advance creation window
        var candidates = await _context.BillSchedules
            .Include(s => s.Bill)
            .Where(s => s.IsActive && (s.EndDate == null || s.EndDate >= today))
            .ToListAsync();

        // Filter in-memory so each schedule's DaysBeforeCreate is respected
        return candidates
            .Where(s => s.NextOccurrenceDate <= today.AddDays(s.DaysBeforeCreate))
            .ToList();
    }

    public async Task<Bill> GenerateNextOccurrenceAsync(BillSchedule schedule)
    {
        if (schedule.Bill == null)
        {
            schedule = await _context.BillSchedules
                .Include(s => s.Bill)
                .FirstAsync(s => s.BillScheduleId == schedule.BillScheduleId);
        }

        var sourceBill = schedule.Bill!;
        var dueDate = schedule.NextOccurrenceDate;

        var newBill = new Bill
        {
            UserId = sourceBill.UserId,
            Name = sourceBill.Name,
            Description = sourceBill.Description,
            Amount = sourceBill.Amount,
            Currency = sourceBill.Currency,
            IssueDate = DateTime.UtcNow.Date,
            DueDate = dueDate,
            Status = "Pending",
            IsRecurring = true,
            RecurringFrequency = schedule.Frequency,
            PaymentMethod = sourceBill.PaymentMethod,
            OCR = sourceBill.OCR,
            Bankgiro = sourceBill.Bankgiro,
            Plusgiro = sourceBill.Plusgiro,
            Payee = sourceBill.Payee,
            CategoryId = sourceBill.CategoryId,
            HouseholdId = sourceBill.HouseholdId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Bills.Add(newBill);

        // Advance the schedule to the next occurrence
        schedule.NextOccurrenceDate = CalculateNextOccurrence(dueDate, schedule.Frequency);
        schedule.UpdatedAt = DateTime.UtcNow;
        _context.BillSchedules.Update(schedule);

        await _context.SaveChangesAsync();

        // Add automatic reminder if configured
        if (schedule.ReminderDaysBefore > 0)
        {
            var reminderDate = dueDate.AddDays(-schedule.ReminderDaysBefore);
            if (reminderDate > DateTime.UtcNow)
            {
                await AddReminderAsync(newBill.BillId, reminderDate);
            }
        }

        await _auditLogService.LogAsync("Generate", "Bill", newBill.BillId,
            $"Generated recurring bill occurrence: {newBill.Name} due {dueDate:yyyy-MM-dd}", newBill.UserId);

        return newBill;
    }

    private static DateTime CalculateNextOccurrence(DateTime current, string frequency)
    {
        return frequency switch
        {
            "Weekly" => current.AddDays(7),
            "BiMonthly" => current.AddMonths(2),
            "Quarterly" => current.AddMonths(3),
            "Yearly" => current.AddYears(1),
            _ => current.AddMonths(1) // Monthly (default)
        };
    }
}

