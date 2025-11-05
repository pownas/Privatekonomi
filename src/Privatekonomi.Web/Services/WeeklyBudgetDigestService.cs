using Microsoft.EntityFrameworkCore;
using Privatekonomi.Core.Services;

namespace Privatekonomi.Web.Services;

/// <summary>
/// Background service that sends weekly budget digest emails
/// </summary>
public class WeeklyBudgetDigestService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<WeeklyBudgetDigestService> _logger;
    
    // Run every Sunday at 18:00
    private static readonly TimeSpan CheckInterval = TimeSpan.FromHours(1);
    private static readonly DayOfWeek DigestDay = DayOfWeek.Sunday;
    private static readonly int DigestHour = 18;

    public WeeklyBudgetDigestService(
        IServiceProvider serviceProvider,
        ILogger<WeeklyBudgetDigestService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Weekly Budget Digest Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var now = DateTime.Now;
                
                // Check if it's the right day and hour
                if (now.DayOfWeek == DigestDay && now.Hour == DigestHour)
                {
                    // Check if we already sent today
                    var lastSent = await GetLastSentDateAsync();
                    if (lastSent.Date != now.Date)
                    {
                        await SendWeeklyDigestsAsync();
                        await UpdateLastSentDateAsync(now);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending weekly budget digests");
            }

            await Task.Delay(CheckInterval, stoppingToken);
        }

        _logger.LogInformation("Weekly Budget Digest Service stopped");
    }

    private async Task SendWeeklyDigestsAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var budgetService = scope.ServiceProvider.GetRequiredService<IBudgetService>();
        var budgetAlertService = scope.ServiceProvider.GetRequiredService<IBudgetAlertService>();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
        var context = scope.ServiceProvider.GetRequiredService<Core.Data.PrivatekonomyContext>();

        try
        {
            _logger.LogInformation("Starting weekly budget digest at {Time}", DateTime.UtcNow);

            // Get all users who have active budgets
            var usersWithBudgets = await context.Budgets
                .Where(b => b.StartDate <= DateTime.Now && b.EndDate >= DateTime.Now)
                .Select(b => b.UserId)
                .Distinct()
                .ToListAsync();

            foreach (var userId in usersWithBudgets)
            {
                if (string.IsNullOrEmpty(userId)) continue;

                try
                {
                    await SendUserDigestAsync(userId, budgetService, budgetAlertService, notificationService);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending digest for user {UserId}", userId);
                }
            }

            _logger.LogInformation("Completed weekly budget digest at {Time}", DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in SendWeeklyDigestsAsync");
            throw;
        }
    }

    private async Task SendUserDigestAsync(
        string userId,
        IBudgetService budgetService,
        IBudgetAlertService budgetAlertService,
        INotificationService notificationService)
    {
        // Get user's active budgets
        var activeBudgets = await budgetService.GetActiveBudgetsAsync();
        var userBudgets = activeBudgets.Where(b => b.UserId == userId).ToList();

        if (!userBudgets.Any())
        {
            return;
        }

        // Build digest message
        var digestLines = new List<string>
        {
            "# Veckosammanfattning - Budgetar",
            "",
            $"Här är din budgetöversikt för vecka {GetWeekNumber(DateTime.Now)}:",
            ""
        };

        int totalCategories = 0;
        int categoriesOverBudget = 0;
        int categoriesNearLimit = 0;

        foreach (var budget in userBudgets)
        {
            digestLines.Add($"## {budget.Name}");
            digestLines.Add($"Period: {budget.StartDate:d} - {budget.EndDate:d}");
            digestLines.Add("");

            foreach (var budgetCategory in budget.BudgetCategories)
            {
                totalCategories++;
                
                var usage = await budgetAlertService.CalculateBudgetUsagePercentageAsync(
                    budget.BudgetId, budgetCategory.CategoryId);
                
                var spent = usage * budgetCategory.PlannedAmount / 100m;
                var remaining = budgetCategory.PlannedAmount - spent;
                
                var emoji = usage >= 100m ? "🔴" :
                           usage >= 90m ? "🟠" :
                           usage >= 75m ? "🟡" :
                           "🟢";

                if (usage >= 100m) categoriesOverBudget++;
                else if (usage >= 75m) categoriesNearLimit++;

                digestLines.Add($"{emoji} **{budgetCategory.Category?.Name ?? "Okänd"}**: {spent:N0} kr / {budgetCategory.PlannedAmount:N0} kr ({usage:F0}%)");
                
                if (remaining > 0)
                {
                    digestLines.Add($"   Återstående: {remaining:N0} kr");
                }

                // Add forecast if approaching limit
                if (usage >= 75m && usage < 100m)
                {
                    var daysUntilExceeded = await budgetAlertService.CalculateDaysUntilExceededAsync(
                        budget.BudgetId, budgetCategory.CategoryId);
                    
                    if (daysUntilExceeded.HasValue)
                    {
                        var dailyRate = await budgetAlertService.CalculateDailyRateAsync(
                            budget.BudgetId, budgetCategory.CategoryId);
                        digestLines.Add($"   ⚠️ Prognos: Överskrids om {daysUntilExceeded.Value} dagar ({dailyRate:F0} kr/dag)");
                    }
                }
                
                digestLines.Add("");
            }
        }

        // Add summary
        digestLines.Insert(3, "");
        digestLines.Insert(4, $"**Sammanfattning:** {totalCategories} kategorier");
        if (categoriesOverBudget > 0)
        {
            digestLines.Insert(5, $"🔴 {categoriesOverBudget} över budget");
        }
        if (categoriesNearLimit > 0)
        {
            digestLines.Insert(5 + (categoriesOverBudget > 0 ? 1 : 0), 
                $"🟡 {categoriesNearLimit} närmar sig gränsen");
        }
        digestLines.Insert(5 + (categoriesOverBudget > 0 ? 1 : 0) + (categoriesNearLimit > 0 ? 1 : 0), "");

        var digestMessage = string.Join("\n", digestLines);

        // Send as email notification
        await notificationService.SendNotificationAsync(
            userId,
            Core.Models.SystemNotificationType.BudgetWarning,
            "Veckosammanfattning - Budgetar",
            digestMessage,
            Core.Models.NotificationPriority.Normal,
            data: null,
            actionUrl: "/budgets");

        _logger.LogInformation("Sent weekly budget digest to user {UserId}", userId);
    }

    private static int GetWeekNumber(DateTime date)
    {
        var culture = System.Globalization.CultureInfo.CurrentCulture;
        return culture.Calendar.GetWeekOfYear(
            date,
            System.Globalization.CalendarWeekRule.FirstFourDayWeek,
            DayOfWeek.Monday);
    }

    private async Task<DateTime> GetLastSentDateAsync()
    {
        // Store last sent date in a file or database
        // For simplicity, using a file in temp directory
        var filePath = Path.Combine(Path.GetTempPath(), "budget_digest_last_sent.txt");
        
        if (File.Exists(filePath))
        {
            var content = await File.ReadAllTextAsync(filePath);
            if (DateTime.TryParse(content, out var lastSent))
            {
                return lastSent;
            }
        }
        
        return DateTime.MinValue;
    }

    private async Task UpdateLastSentDateAsync(DateTime date)
    {
        var filePath = Path.Combine(Path.GetTempPath(), "budget_digest_last_sent.txt");
        await File.WriteAllTextAsync(filePath, date.ToString("O"));
    }
}
