using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services;

public interface IBillService
{
    Task<List<Bill>> GetBillsAsync(string userId);
    Task<List<Bill>> GetPendingBillsAsync(string userId);
    Task<List<Bill>> GetOverdueBillsAsync(string userId);
    Task<Bill?> GetBillByIdAsync(int billId, string userId);
    Task<Bill> CreateBillAsync(Bill bill);
    Task<Bill> UpdateBillAsync(Bill bill);
    Task DeleteBillAsync(int billId, string userId);
    Task MarkBillAsPaidAsync(int billId, DateTime paidDate, int? transactionId = null);
    Task<List<Bill>> GetBillsByHouseholdAsync(int householdId, string userId);
    Task<List<Bill>> GetBillsDueSoonAsync(string userId, int daysAhead);
    Task<List<BillReminder>> GetPendingRemindersAsync();
    Task AddReminderAsync(int billId, DateTime reminderDate, string reminderMethod = "Notification");
    Task MarkReminderAsSentAsync(int reminderId);

    // Schedule management
    Task<BillSchedule> CreateScheduleAsync(BillSchedule schedule);
    Task<BillSchedule?> GetScheduleByBillIdAsync(int billId);
    Task<BillSchedule> UpdateScheduleAsync(BillSchedule schedule);
    Task DeleteScheduleAsync(int billScheduleId);
    Task<List<BillSchedule>> GetDueSchedulesAsync();
    Task<Bill> GenerateNextOccurrenceAsync(BillSchedule schedule);
}
