using Microsoft.EntityFrameworkCore;
using Privatekonomi.Core.Data;
using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services;

/// <summary>
/// Service for aggregating dashboard data from multiple sources.
/// Provides ekonomisk översikt with balance, budget status, upcoming bills, and insights.
/// </summary>
public class DashboardService : IDashboardService
{
    private readonly PrivatekonomyContext _context;
    private readonly ICurrentUserService? _currentUserService;
    private readonly IBudgetService _budgetService;
    private readonly IBillService _billService;
    private readonly INotificationService _notificationService;

    public DashboardService(
        PrivatekonomyContext context,
        IBudgetService budgetService,
        IBillService billService,
        INotificationService notificationService,
        ICurrentUserService? currentUserService = null)
    {
        _context = context;
        _budgetService = budgetService;
        _billService = billService;
        _notificationService = notificationService;
        _currentUserService = currentUserService;
    }

    /// <inheritdoc />
    public async Task<DashboardData> GetDashboardDataAsync(int[]? accountIds = null)
    {
        var balanceTask = GetBalanceSummaryAsync(accountIds);
        var budgetTask = GetBudgetStatusAsync();
        var billsTask = GetUpcomingBillsAsync();
        var insightsTask = GetRecentInsightsAsync();

        await Task.WhenAll(balanceTask, budgetTask, billsTask, insightsTask);

        return new DashboardData
        {
            Balance = await balanceTask,
            BudgetStatus = await budgetTask,
            UpcomingBills = await billsTask,
            Insights = await insightsTask,
            LastUpdated = DateTime.UtcNow
        };
    }

    /// <inheritdoc />
    public async Task<BalanceSummary> GetBalanceSummaryAsync(int[]? accountIds = null)
    {
        var userId = _currentUserService?.UserId;
        
        var query = _context.BankSources
            .Include(b => b.Transactions)
            .AsQueryable();

        // Filter by user if authenticated
        if (!string.IsNullOrEmpty(userId))
        {
            query = query.Where(b => b.UserId == userId);
        }

        // Filter by specific account IDs if provided
        if (accountIds != null && accountIds.Length > 0)
        {
            query = query.Where(b => accountIds.Contains(b.BankSourceId));
        }

        // Only include active accounts (not closed)
        query = query.Where(b => b.ClosedDate == null);

        var accounts = await query.ToListAsync();

        var accountBalances = accounts.Select(a => new AccountBalanceItem
        {
            AccountId = a.BankSourceId,
            Name = a.Name,
            AccountType = a.AccountType,
            Balance = a.InitialBalance + a.Transactions.Sum(t => t.IsIncome ? t.Amount : -t.Amount),
            Currency = a.Currency,
            Color = a.Color
        }).ToList();

        var totalBalance = accountBalances.Sum(a => a.Balance);

        // Calculate change from previous month
        var (changeFromPrevious, percentageChange) = await CalculateBalanceChangeAsync(accounts, totalBalance);

        return new BalanceSummary
        {
            TotalBalance = totalBalance,
            ChangeFromPrevious = changeFromPrevious,
            PercentageChange = percentageChange,
            Currency = "SEK",
            Accounts = accountBalances
        };
    }

    /// <inheritdoc />
    public async Task<BudgetStatusSummary> GetBudgetStatusAsync()
    {
        var activeBudgets = await _budgetService.GetActiveBudgetsAsync();
        var budgetList = activeBudgets.ToList();

        if (!budgetList.Any())
        {
            return new BudgetStatusSummary
            {
                HasActiveBudgets = false
            };
        }

        var budgetSummaries = new List<BudgetItemSummary>();
        decimal totalPlanned = 0;
        decimal totalSpent = 0;
        var overspentCategories = new List<string>();

        foreach (var budget in budgetList)
        {
            var actualAmounts = await _budgetService.GetActualAmountsByCategoryAsync(budget.BudgetId);
            var planned = budget.BudgetCategories.Sum(bc => bc.PlannedAmount);
            var spent = actualAmounts.Values.Sum();
            var percentageUsed = planned > 0 ? (spent / planned) * 100 : 0;

            var status = percentageUsed switch
            {
                >= 100 => BudgetHealthStatus.Exceeded,
                >= 75 => BudgetHealthStatus.Warning,
                _ => BudgetHealthStatus.OnTrack
            };

            budgetSummaries.Add(new BudgetItemSummary
            {
                BudgetId = budget.BudgetId,
                Name = budget.Name,
                Planned = planned,
                Spent = spent,
                PercentageUsed = percentageUsed,
                Status = status
            });

            totalPlanned += planned;
            totalSpent += spent;

            // Check for overspent categories
            foreach (var category in budget.BudgetCategories)
            {
                if (actualAmounts.TryGetValue(category.CategoryId, out var actualAmount))
                {
                    if (actualAmount > category.PlannedAmount)
                    {
                        overspentCategories.Add(category.Category?.Name ?? $"Kategori {category.CategoryId}");
                    }
                }
            }
        }

        var overallPercentage = totalPlanned > 0 ? (totalSpent / totalPlanned) * 100 : 0;
        var overallStatus = overallPercentage switch
        {
            >= 100 => BudgetHealthStatus.Exceeded,
            >= 75 => BudgetHealthStatus.Warning,
            _ => BudgetHealthStatus.OnTrack
        };

        return new BudgetStatusSummary
        {
            HasActiveBudgets = true,
            TotalPlanned = totalPlanned,
            TotalSpent = totalSpent,
            Remaining = totalPlanned - totalSpent,
            PercentageUsed = overallPercentage,
            Status = overallStatus,
            Budgets = budgetSummaries,
            OverspentCategories = overspentCategories.Distinct().ToList()
        };
    }

    /// <inheritdoc />
    public async Task<List<BillSummary>> GetUpcomingBillsAsync(int daysAhead = 30, int limit = 5)
    {
        var userId = _currentUserService?.UserId;
        
        if (string.IsNullOrEmpty(userId))
        {
            return new List<BillSummary>();
        }

        var upcomingBills = await _billService.GetBillsDueSoonAsync(userId, daysAhead);
        var overdueBills = await _billService.GetOverdueBillsAsync(userId);

        var today = DateTime.Today;
        var allBills = overdueBills.Concat(upcomingBills)
            .DistinctBy(b => b.BillId)
            .OrderBy(b => b.DueDate)
            .Take(limit)
            .Select(b => new BillSummary
            {
                BillId = b.BillId,
                Name = b.Name,
                Amount = b.Amount,
                Currency = b.Currency,
                DueDate = b.DueDate,
                DaysUntilDue = (int)(b.DueDate - today).TotalDays,
                Status = b.Status,
                IsOverdue = b.DueDate < today && b.Status != "Paid",
                Payee = b.Payee,
                CategoryName = b.Category?.Name
            })
            .ToList();

        return allBills;
    }

    /// <inheritdoc />
    public async Task<List<InsightItem>> GetRecentInsightsAsync(int limit = 5)
    {
        var userId = _currentUserService?.UserId;
        
        if (string.IsNullOrEmpty(userId))
        {
            return new List<InsightItem>();
        }

        // Get notifications and convert to insights
        var notifications = await _notificationService.GetActiveNotificationsAsync(userId, unreadOnly: false);
        
        var insights = notifications
            .OrderByDescending(n => n.Priority)
            .ThenByDescending(n => n.CreatedAt)
            .Take(limit)
            .Select(n => new InsightItem
            {
                InsightId = n.NotificationId,
                Type = MapNotificationTypeToInsightType(n.Type),
                Title = n.Title,
                Description = n.Message,
                Priority = MapNotificationPriorityToInsightPriority(n.Priority),
                ActionUrl = n.ActionUrl,
                IconName = GetIconForInsightType(MapNotificationTypeToInsightType(n.Type)),
                CreatedAt = n.CreatedAt,
                IsRead = n.IsRead,
                Data = n.Data
            })
            .ToList();

        // Generate additional insights from spending patterns if we have few notifications
        if (insights.Count < limit)
        {
            var additionalInsights = await GenerateSpendingInsightsAsync(limit - insights.Count);
            insights.AddRange(additionalInsights);
        }

        return insights.Take(limit).ToList();
    }

    private async Task<(decimal changeFromPrevious, decimal percentageChange)> CalculateBalanceChangeAsync(
        List<BankSource> accounts, decimal currentBalance)
    {
        var firstDayOfCurrentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        var firstDayOfPreviousMonth = firstDayOfCurrentMonth.AddMonths(-1);
        
        // Calculate previous month end balance
        decimal previousBalance = 0;
        foreach (var account in accounts)
        {
            var previousMonthTransactions = account.Transactions
                .Where(t => t.Date < firstDayOfCurrentMonth)
                .Sum(t => t.IsIncome ? t.Amount : -t.Amount);
            previousBalance += account.InitialBalance + previousMonthTransactions;
        }

        var change = currentBalance - previousBalance;
        var percentage = previousBalance != 0 ? (change / Math.Abs(previousBalance)) * 100 : 0;

        return (change, percentage);
    }

    private InsightType MapNotificationTypeToInsightType(SystemNotificationType notificationType)
    {
        return notificationType switch
        {
            SystemNotificationType.BudgetExceeded or SystemNotificationType.BudgetWarning => InsightType.BudgetAlert,
            SystemNotificationType.UpcomingBill or SystemNotificationType.BillDue or SystemNotificationType.BillOverdue => InsightType.BillReminder,
            SystemNotificationType.GoalAchieved or SystemNotificationType.GoalMilestone => InsightType.GoalProgress,
            SystemNotificationType.UnusualTransaction or SystemNotificationType.LargeTransaction => InsightType.UnusualTransaction,
            SystemNotificationType.SubscriptionPriceIncrease or SystemNotificationType.SubscriptionRenewal => InsightType.SubscriptionAlert,
            _ => InsightType.SystemNotification
        };
    }

    private InsightPriority MapNotificationPriorityToInsightPriority(NotificationPriority priority)
    {
        return priority switch
        {
            NotificationPriority.Critical => InsightPriority.Critical,
            NotificationPriority.High => InsightPriority.High,
            NotificationPriority.Normal => InsightPriority.Medium,
            _ => InsightPriority.Low
        };
    }

    private string GetIconForInsightType(InsightType type)
    {
        return type switch
        {
            InsightType.BudgetAlert => "PieChart",
            InsightType.SpendingTrend => "TrendingUp",
            InsightType.SavingsOpportunity => "Savings",
            InsightType.BillReminder => "Receipt",
            InsightType.GoalProgress => "Flag",
            InsightType.UnusualTransaction => "Warning",
            InsightType.SubscriptionAlert => "Subscriptions",
            InsightType.SystemNotification => "Info",
            _ => "Info"
        };
    }

    private async Task<List<InsightItem>> GenerateSpendingInsightsAsync(int limit)
    {
        var insights = new List<InsightItem>();
        var userId = _currentUserService?.UserId;

        if (string.IsNullOrEmpty(userId))
        {
            return insights;
        }

        // Check for overspent budget categories
        var budgetStatus = await GetBudgetStatusAsync();
        if (budgetStatus.OverspentCategories.Any())
        {
            insights.Add(new InsightItem
            {
                InsightId = -1,
                Type = InsightType.BudgetAlert,
                Title = "Budgetvarning",
                Description = $"Du har överskridit budgeten för {string.Join(", ", budgetStatus.OverspentCategories.Take(3))}",
                Priority = InsightPriority.High,
                ActionUrl = "/economy/budgets",
                IconName = "PieChart",
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            });
        }

        // Add savings opportunity insight if spending is significantly reduced
        if (budgetStatus.HasActiveBudgets && budgetStatus.PercentageUsed < 50 && budgetStatus.Remaining > 0)
        {
            insights.Add(new InsightItem
            {
                InsightId = -2,
                Type = InsightType.SavingsOpportunity,
                Title = "Sparmöjlighet",
                Description = $"Du har {budgetStatus.Remaining:C0} kvar av din budget denna månad!",
                Priority = InsightPriority.Low,
                ActionUrl = "/goals",
                IconName = "Savings",
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            });
        }

        return insights.Take(limit).ToList();
    }
}
