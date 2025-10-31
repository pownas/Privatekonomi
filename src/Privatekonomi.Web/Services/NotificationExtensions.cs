using Privatekonomi.Core.Models;
using Privatekonomi.Core.Services;

namespace Privatekonomi.Web.Services;

/// <summary>
/// Extension methods for common notification scenarios
/// </summary>
public static class NotificationExtensions
{
    /// <summary>
    /// Notify user that a budget has been exceeded
    /// </summary>
    public static async Task NotifyBudgetExceededAsync(
        this INotificationService service,
        string userId,
        string categoryName,
        decimal budgetAmount,
        decimal actualAmount,
        string? actionUrl = null)
    {
        var title = "Budgetöverdrag!";
        var message = $"Din budget för {categoryName} har överskridits. Budget: {budgetAmount:C}, Spenderat: {actualAmount:C}";
        
        await service.SendNotificationAsync(
            userId,
            SystemNotificationType.BudgetExceeded,
            title,
            message,
            NotificationPriority.High,
            data: null,
            actionUrl: actionUrl ?? "/budgets");
    }

    /// <summary>
    /// Notify user of low account balance
    /// </summary>
    public static async Task NotifyLowBalanceAsync(
        this INotificationService service,
        string userId,
        string accountName,
        decimal balance,
        decimal threshold,
        string? actionUrl = null)
    {
        var title = "Låg balans";
        var message = $"Ditt konto {accountName} har låg balans: {balance:C} (tröskel: {threshold:C})";
        
        await service.SendNotificationAsync(
            userId,
            SystemNotificationType.LowBalance,
            title,
            message,
            NotificationPriority.High,
            data: null,
            actionUrl: actionUrl ?? "/transactions");
    }

    /// <summary>
    /// Notify user that a savings goal has been achieved
    /// </summary>
    public static async Task NotifyGoalAchievedAsync(
        this INotificationService service,
        string userId,
        string goalName,
        decimal goalAmount,
        string? actionUrl = null)
    {
        var title = "Grattis! Sparmål uppnått! 🎉";
        var message = $"Du har nått ditt sparmål '{goalName}' på {goalAmount:C}!";
        
        await service.SendNotificationAsync(
            userId,
            SystemNotificationType.GoalAchieved,
            title,
            message,
            NotificationPriority.Normal,
            data: null,
            actionUrl: actionUrl ?? "/goals");
    }

    /// <summary>
    /// Notify user of a bank synchronization error
    /// </summary>
    public static async Task NotifyBankSyncErrorAsync(
        this INotificationService service,
        string userId,
        string bankName,
        string errorMessage,
        string? actionUrl = null)
    {
        var title = "Banksynkronisering misslyckades";
        var message = $"Synkronisering med {bankName} misslyckades: {errorMessage}";
        
        await service.SendNotificationAsync(
            userId,
            SystemNotificationType.BankSyncFailed,
            title,
            message,
            NotificationPriority.High,
            data: null,
            actionUrl: actionUrl ?? "/bank-connections");
    }

    /// <summary>
    /// Notify user of successful bank synchronization
    /// </summary>
    public static async Task NotifyBankSyncSuccessAsync(
        this INotificationService service,
        string userId,
        string bankName,
        int transactionCount,
        string? actionUrl = null)
    {
        var title = "Banksynkronisering lyckades";
        var message = $"{transactionCount} nya transaktioner från {bankName} har importerats";
        
        await service.SendNotificationAsync(
            userId,
            SystemNotificationType.BankSyncSuccess,
            title,
            message,
            NotificationPriority.Normal,
            data: null,
            actionUrl: actionUrl ?? "/transactions");
    }

    /// <summary>
    /// Notify user of an upcoming bill
    /// </summary>
    public static async Task NotifyUpcomingBillAsync(
        this INotificationService service,
        string userId,
        string billName,
        decimal amount,
        DateTime dueDate,
        string? actionUrl = null)
    {
        var daysUntilDue = (dueDate.Date - DateTime.Today).Days;
        var title = "Kommande räkning";
        var message = $"{billName} förfaller om {daysUntilDue} dagar ({dueDate:d}): {amount:C}";
        
        await service.SendNotificationAsync(
            userId,
            SystemNotificationType.UpcomingBill,
            title,
            message,
            NotificationPriority.Normal,
            data: null,
            actionUrl: actionUrl ?? "/bills");
    }

    /// <summary>
    /// Notify user of a large/unusual transaction
    /// </summary>
    public static async Task NotifyLargeTransactionAsync(
        this INotificationService service,
        string userId,
        decimal amount,
        string description,
        DateTime date,
        string? actionUrl = null)
    {
        var title = "Stor transaktion upptäckt";
        var message = $"En ovanligt stor transaktion på {amount:C} registrerades: {description}";
        
        await service.SendNotificationAsync(
            userId,
            SystemNotificationType.LargeTransaction,
            title,
            message,
            NotificationPriority.High,
            data: null,
            actionUrl: actionUrl ?? "/transactions");
    }

    /// <summary>
    /// Notify user of subscription price increase
    /// </summary>
    public static async Task NotifySubscriptionPriceIncreaseAsync(
        this INotificationService service,
        string userId,
        string subscriptionName,
        decimal oldPrice,
        decimal newPrice,
        DateTime effectiveDate,
        string? actionUrl = null)
    {
        var increase = newPrice - oldPrice;
        var increasePercent = (increase / oldPrice) * 100;
        var title = "Prenumerationspris ökat";
        var message = $"{subscriptionName} ökar från {oldPrice:C} till {newPrice:C} (+{increasePercent:F1}%) fr.o.m. {effectiveDate:d}";
        
        await service.SendNotificationAsync(
            userId,
            SystemNotificationType.SubscriptionPriceIncrease,
            title,
            message,
            NotificationPriority.Normal,
            data: null,
            actionUrl: actionUrl ?? "/subscriptions");
    }

    /// <summary>
    /// Notify user of budget warning (approaching limit)
    /// </summary>
    public static async Task NotifyBudgetWarningAsync(
        this INotificationService service,
        string userId,
        string categoryName,
        decimal budgetAmount,
        decimal actualAmount,
        decimal percentUsed,
        string? actionUrl = null)
    {
        var title = "Budgetvarning";
        var message = $"Du har använt {percentUsed:F0}% av din budget för {categoryName} ({actualAmount:C} av {budgetAmount:C})";
        
        await service.SendNotificationAsync(
            userId,
            SystemNotificationType.BudgetWarning,
            title,
            message,
            NotificationPriority.Normal,
            data: null,
            actionUrl: actionUrl ?? "/budgets");
    }

    /// <summary>
    /// Notify user of goal milestone reached
    /// </summary>
    public static async Task NotifyGoalMilestoneAsync(
        this INotificationService service,
        string userId,
        string goalName,
        decimal currentAmount,
        decimal targetAmount,
        decimal percentComplete,
        string? actionUrl = null)
    {
        var title = "Sparmål milstolpe! 🎯";
        var message = $"Du har nått {percentComplete:F0}% av ditt mål '{goalName}' ({currentAmount:C} av {targetAmount:C})";
        
        await service.SendNotificationAsync(
            userId,
            SystemNotificationType.GoalMilestone,
            title,
            message,
            NotificationPriority.Normal,
            data: null,
            actionUrl: actionUrl ?? "/goals");
    }
}
