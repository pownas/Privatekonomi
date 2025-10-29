using Microsoft.EntityFrameworkCore;
using Privatekonomi.Core.Data;
using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services;

public class ReportService : IReportService
{
    private readonly PrivatekonomyContext _context;

    public ReportService(PrivatekonomyContext context)
    {
        _context = context;
    }

    public async Task<CashFlowReport> GetCashFlowReportAsync(DateTime fromDate, DateTime toDate, string groupBy = "month", int? householdId = null)
    {
        var query = _context.Transactions
            .Where(t => t.Date >= fromDate && t.Date <= toDate);

        if (householdId.HasValue)
        {
            query = query.Where(t => t.HouseholdId == householdId.Value);
        }

        var transactions = await query.OrderBy(t => t.Date).ToListAsync();

        var periods = new List<CashFlowPeriod>();
        
        if (groupBy.ToLower() == "month")
        {
            var monthlyGroups = transactions.GroupBy(t => new { t.Date.Year, t.Date.Month });
            
            foreach (var group in monthlyGroups.OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month))
            {
                var income = group.Where(t => t.IsIncome).Sum(t => t.Amount);
                var expenses = group.Where(t => !t.IsIncome).Sum(t => t.Amount);
                
                periods.Add(new CashFlowPeriod
                {
                    Period = $"{group.Key.Year}-{group.Key.Month:D2}",
                    StartDate = new DateTime(group.Key.Year, group.Key.Month, 1),
                    EndDate = new DateTime(group.Key.Year, group.Key.Month, DateTime.DaysInMonth(group.Key.Year, group.Key.Month)),
                    Income = income,
                    Expenses = expenses,
                    NetFlow = income - expenses
                });
            }
        }
        else if (groupBy.ToLower() == "week")
        {
            // Group by ISO week
            var weeklyGroups = transactions.GroupBy(t => new 
            { 
                Year = System.Globalization.ISOWeek.GetYear(t.Date),
                Week = System.Globalization.ISOWeek.GetWeekOfYear(t.Date)
            });
            
            foreach (var group in weeklyGroups.OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Week))
            {
                var income = group.Where(t => t.IsIncome).Sum(t => t.Amount);
                var expenses = group.Where(t => !t.IsIncome).Sum(t => t.Amount);
                var firstDate = group.Min(t => t.Date);
                
                periods.Add(new CashFlowPeriod
                {
                    Period = $"{group.Key.Year}-W{group.Key.Week:D2}",
                    StartDate = System.Globalization.ISOWeek.ToDateTime(group.Key.Year, group.Key.Week, DayOfWeek.Monday),
                    EndDate = System.Globalization.ISOWeek.ToDateTime(group.Key.Year, group.Key.Week, DayOfWeek.Sunday),
                    Income = income,
                    Expenses = expenses,
                    NetFlow = income - expenses
                });
            }
        }

        var totalIncome = transactions.Where(t => t.IsIncome).Sum(t => t.Amount);
        var totalExpenses = transactions.Where(t => !t.IsIncome).Sum(t => t.Amount);

        return new CashFlowReport
        {
            Periods = periods,
            TotalIncome = totalIncome,
            TotalExpenses = totalExpenses,
            NetCashFlow = totalIncome - totalExpenses,
            GroupBy = groupBy
        };
    }

    public async Task<NetWorthReport> GetNetWorthReportAsync(string? userId = null)
    {
        // Query investments with optional user filter
        var investmentsQuery = _context.Investments.AsQueryable();
        if (!string.IsNullOrEmpty(userId))
        {
            investmentsQuery = investmentsQuery.Where(i => i.UserId == userId);
        }
        var investments = await investmentsQuery.ToListAsync();

        // Query assets with optional user filter
        var assetsQuery = _context.Assets.AsQueryable();
        if (!string.IsNullOrEmpty(userId))
        {
            assetsQuery = assetsQuery.Where(a => a.UserId == userId);
        }
        var assets = await assetsQuery.ToListAsync();

        // Query loans with optional user filter
        var loansQuery = _context.Loans.AsQueryable();
        if (!string.IsNullOrEmpty(userId))
        {
            loansQuery = loansQuery.Where(l => l.UserId == userId);
        }
        var loans = await loansQuery.ToListAsync();

        var assetItems = new List<AssetItem>();
        var liabilityItems = new List<LiabilityItem>();

        // Add investments
        foreach (var investment in investments)
        {
            assetItems.Add(new AssetItem
            {
                Name = investment.Name,
                Type = "Investment",
                Value = investment.TotalValue
            });
        }

        // Add other assets
        foreach (var asset in assets)
        {
            assetItems.Add(new AssetItem
            {
                Name = asset.Name,
                Type = asset.Type,
                Value = asset.CurrentValue
            });
        }

        // Add loans as liabilities
        foreach (var loan in loans)
        {
            liabilityItems.Add(new LiabilityItem
            {
                Name = loan.Name,
                Type = loan.Type,
                Amount = loan.CurrentBalance
            });
        }

        var totalAssets = assetItems.Sum(a => a.Value);
        var totalInvestments = investments.Sum(i => i.TotalValue);
        var totalLiabilities = liabilityItems.Sum(l => l.Amount);
        var netWorth = totalAssets - totalLiabilities;

        // Create historical data for the last 12 months
        var history = await CreateNetWorthHistory(userId, 12);
        
        // Calculate percentage change from previous month
        var percentageChange = CalculatePercentageChange(history, netWorth);

        return new NetWorthReport
        {
            TotalAssets = totalAssets,
            TotalInvestments = totalInvestments,
            TotalLiabilities = totalLiabilities,
            NetWorth = netWorth,
            PercentageChange = percentageChange,
            Assets = assetItems,
            Liabilities = liabilityItems,
            History = history,
            CalculatedAt = DateTime.UtcNow
        };
    }

    private async Task<List<NetWorthDataPoint>> CreateNetWorthHistory(string? userId, int months)
    {
        var history = new List<NetWorthDataPoint>();
        var endDate = DateTime.Today;
        var startDate = endDate.AddMonths(-months);

        // Get snapshots for the date range
        var snapshotsQuery = _context.NetWorthSnapshots
            .Where(s => s.Date >= startDate && s.Date <= endDate);
        
        if (!string.IsNullOrEmpty(userId))
        {
            snapshotsQuery = snapshotsQuery.Where(s => s.UserId == userId);
        }

        var snapshots = await snapshotsQuery
            .OrderBy(s => s.Date)
            .ToListAsync();

        // Group by month and take the latest snapshot per month
        var monthlySnapshots = snapshots
            .GroupBy(s => new { s.Date.Year, s.Date.Month })
            .Select(g => g.OrderByDescending(s => s.Date).First())
            .OrderBy(s => s.Date)
            .ToList();

        // If we don't have historical snapshots, calculate current values for each month
        if (!monthlySnapshots.Any())
        {
            // Create data points based on current values
            // This is a simplified approach - in production, you'd want actual historical data
            for (int i = months - 1; i >= 0; i--)
            {
                var date = DateTime.Today.AddMonths(-i);
                var monthStart = new DateTime(date.Year, date.Month, 1);
                
                // For now, we'll use current values as approximation
                // In a real scenario, you'd query historical data
                var investmentsQuery = _context.Investments.AsQueryable();
                var assetsQuery = _context.Assets.AsQueryable();
                var loansQuery = _context.Loans.AsQueryable();

                if (!string.IsNullOrEmpty(userId))
                {
                    investmentsQuery = investmentsQuery.Where(inv => inv.UserId == userId);
                    assetsQuery = assetsQuery.Where(a => a.UserId == userId);
                    loansQuery = loansQuery.Where(l => l.UserId == userId);
                }

                // Materialize the queries before using computed properties
                var investments = await investmentsQuery.ToListAsync();
                var assets = await assetsQuery.ToListAsync();
                var loans = await loansQuery.ToListAsync();

                var investmentValue = investments.Sum(inv => inv.TotalValue);
                var assetValue = assets.Sum(a => a.CurrentValue);
                var loanValue = loans.Sum(l => l.CurrentBalance);

                var totalAssets = investmentValue + assetValue;
                var totalLiabilities = loanValue;

                history.Add(new NetWorthDataPoint
                {
                    Date = monthStart,
                    Assets = totalAssets,
                    Liabilities = totalLiabilities,
                    NetWorth = totalAssets - totalLiabilities
                });
            }
        }
        else
        {
            // Use actual snapshots
            foreach (var snapshot in monthlySnapshots)
            {
                history.Add(new NetWorthDataPoint
                {
                    Date = snapshot.Date,
                    Assets = snapshot.TotalAssets,
                    Liabilities = snapshot.TotalLiabilities,
                    NetWorth = snapshot.NetWorth
                });
            }

            // If we don't have a full 12 months, pad with current values
            while (history.Count < months)
            {
                var lastDate = history.Any() ? history[0].Date : DateTime.Today;
                var newDate = new DateTime(lastDate.Year, lastDate.Month, 1).AddMonths(-1);
                
                history.Insert(0, new NetWorthDataPoint
                {
                    Date = newDate,
                    Assets = history.FirstOrDefault()?.Assets ?? 0,
                    Liabilities = history.FirstOrDefault()?.Liabilities ?? 0,
                    NetWorth = history.FirstOrDefault()?.NetWorth ?? 0
                });
            }
        }

        return history.OrderBy(h => h.Date).ToList();
    }

    private decimal CalculatePercentageChange(List<NetWorthDataPoint> history, decimal currentNetWorth)
    {
        if (!history.Any() || history.Count < 2)
            return 0;

        // Get the previous month's net worth (second to last in history)
        var previousNetWorth = history[history.Count - 2].NetWorth;
        
        if (previousNetWorth == 0)
            return 0;

        return ((currentNetWorth - previousNetWorth) / Math.Abs(previousNetWorth)) * 100;
    }

    public async Task<TrendAnalysis> GetTrendAnalysisAsync(int? categoryId, int months = 6, int? householdId = null)
    {
        var fromDate = DateTime.Now.AddMonths(-months);
        
        var query = _context.Transactions
            .Where(t => t.Date >= fromDate);

        if (householdId.HasValue)
        {
            query = query.Where(t => t.HouseholdId == householdId.Value);
        }

        string categoryName = "Alla kategorier";
        
        if (categoryId.HasValue)
        {
            var category = await _context.Categories.FindAsync(categoryId.Value);
            categoryName = category?.Name ?? "Okänd kategori";
            
            query = query.Where(t => t.TransactionCategories.Any(tc => tc.CategoryId == categoryId.Value));
        }

        var transactions = await query.ToListAsync();

        var monthlyGroups = transactions
            .GroupBy(t => new { t.Date.Year, t.Date.Month })
            .OrderBy(g => g.Key.Year)
            .ThenBy(g => g.Key.Month)
            .Select(g => new MonthlyTrend
            {
                Month = $"{g.Key.Year}-{g.Key.Month:D2}",
                Amount = g.Where(t => !t.IsIncome).Sum(t => t.Amount)
            })
            .ToList();

        var average = monthlyGroups.Any() ? monthlyGroups.Average(m => m.Amount) : 0;
        
        // Simple trend calculation: compare first half vs second half
        var trend = 0m;
        var trendDirection = "Stable";
        
        if (monthlyGroups.Count >= 2)
        {
            var halfPoint = monthlyGroups.Count / 2;
            var firstHalfAvg = monthlyGroups.Take(halfPoint).Average(m => m.Amount);
            var secondHalfAvg = monthlyGroups.Skip(halfPoint).Average(m => m.Amount);
            
            trend = secondHalfAvg - firstHalfAvg;
            
            if (Math.Abs(trend) > average * 0.1m) // More than 10% change
            {
                trendDirection = trend > 0 ? "Increasing" : "Decreasing";
            }
        }

        return new TrendAnalysis
        {
            CategoryName = categoryName,
            MonthlyTrends = monthlyGroups,
            AverageMonthly = average,
            Trend = trend,
            TrendDirection = trendDirection
        };
    }

    public async Task<IEnumerable<TopMerchant>> GetTopMerchantsAsync(int limit = 10, DateTime? fromDate = null, DateTime? toDate = null, int? householdId = null)
    {
        var query = _context.Transactions
            .Where(t => !t.IsIncome); // Only expenses

        if (fromDate.HasValue)
        {
            query = query.Where(t => t.Date >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(t => t.Date <= toDate.Value);
        }

        if (householdId.HasValue)
        {
            query = query.Where(t => t.HouseholdId == householdId.Value);
        }

        var transactions = await query.ToListAsync();

        // Group by Description (which often contains merchant name)
        var merchants = transactions
            .GroupBy(t => GetMerchantName(t.Description))
            .Select(g => new TopMerchant
            {
                Name = g.Key,
                TotalSpent = g.Sum(t => t.Amount),
                TransactionCount = g.Count(),
                AverageTransaction = g.Average(t => t.Amount)
            })
            .OrderByDescending(m => m.TotalSpent)
            .Take(limit)
            .ToList();

        return merchants;
    }

    private string GetMerchantName(string description)
    {
        // Extract merchant name from description
        // This is a simple implementation - could be improved with more sophisticated parsing
        if (string.IsNullOrWhiteSpace(description))
            return "Okänd";

        // Remove common transaction words and trim
        var cleaned = description
            .Replace("Köp", "")
            .Replace("Betalning", "")
            .Replace("Till:", "")
            .Trim();

        // Take first part (usually merchant name)
        var parts = cleaned.Split(new[] { ' ', ',', '-' }, StringSplitOptions.RemoveEmptyEntries);
        return parts.Length > 0 ? parts[0] : description;
    }

    public async Task<NetWorthHistoryReport> GetNetWorthHistoryAsync(string groupBy = "month", DateTime? fromDate = null, DateTime? toDate = null)
    {
        // Default date range to last 12 months if not specified
        var endDate = toDate ?? DateTime.Today;
        var startDate = fromDate ?? endDate.AddMonths(-12);

        // Get snapshots within date range
        var snapshots = await _context.NetWorthSnapshots
            .Where(s => s.Date >= startDate && s.Date <= endDate)
            .OrderBy(s => s.Date)
            .ToListAsync();

        var periods = new List<NetWorthHistoryPeriod>();

        if (groupBy.ToLower() == "month")
        {
            // Group snapshots by month and take the latest snapshot per month
            var monthlyGroups = snapshots
                .GroupBy(s => new { s.Date.Year, s.Date.Month })
                .OrderBy(g => g.Key.Year)
                .ThenBy(g => g.Key.Month);

            foreach (var group in monthlyGroups)
            {
                // Take the last snapshot of the month
                var snapshot = group.OrderByDescending(s => s.Date).First();
                
                periods.Add(new NetWorthHistoryPeriod
                {
                    Period = $"{group.Key.Year}-{group.Key.Month:D2}",
                    Date = snapshot.Date,
                    NetWorth = snapshot.NetWorth,
                    TotalAssets = snapshot.TotalAssets,
                    TotalLiabilities = snapshot.TotalLiabilities,
                    BankBalance = snapshot.BankBalance,
                    InvestmentValue = snapshot.InvestmentValue,
                    PhysicalAssetValue = snapshot.PhysicalAssetValue,
                    LoanBalance = snapshot.LoanBalance
                });
            }
        }
        else if (groupBy.ToLower() == "quarter")
        {
            // Group by quarter
            var quarterlyGroups = snapshots
                .GroupBy(s => new { s.Date.Year, Quarter = (s.Date.Month - 1) / 3 + 1 })
                .OrderBy(g => g.Key.Year)
                .ThenBy(g => g.Key.Quarter);

            foreach (var group in quarterlyGroups)
            {
                var snapshot = group.OrderByDescending(s => s.Date).First();
                
                periods.Add(new NetWorthHistoryPeriod
                {
                    Period = $"{group.Key.Year}-Q{group.Key.Quarter}",
                    Date = snapshot.Date,
                    NetWorth = snapshot.NetWorth,
                    TotalAssets = snapshot.TotalAssets,
                    TotalLiabilities = snapshot.TotalLiabilities,
                    BankBalance = snapshot.BankBalance,
                    InvestmentValue = snapshot.InvestmentValue,
                    PhysicalAssetValue = snapshot.PhysicalAssetValue,
                    LoanBalance = snapshot.LoanBalance
                });
            }
        }
        else if (groupBy.ToLower() == "year")
        {
            // Group by year
            var yearlyGroups = snapshots
                .GroupBy(s => s.Date.Year)
                .OrderBy(g => g.Key);

            foreach (var group in yearlyGroups)
            {
                var snapshot = group.OrderByDescending(s => s.Date).First();
                
                periods.Add(new NetWorthHistoryPeriod
                {
                    Period = $"{group.Key}",
                    Date = snapshot.Date,
                    NetWorth = snapshot.NetWorth,
                    TotalAssets = snapshot.TotalAssets,
                    TotalLiabilities = snapshot.TotalLiabilities,
                    BankBalance = snapshot.BankBalance,
                    InvestmentValue = snapshot.InvestmentValue,
                    PhysicalAssetValue = snapshot.PhysicalAssetValue,
                    LoanBalance = snapshot.LoanBalance
                });
            }
        }

        var currentNetWorth = periods.Any() ? periods.Last().NetWorth : 0;
        var startNetWorth = periods.Any() ? periods.First().NetWorth : 0;
        var netWorthChange = currentNetWorth - startNetWorth;
        var netWorthChangePercentage = startNetWorth != 0 ? (netWorthChange / Math.Abs(startNetWorth)) * 100 : 0;

        return new NetWorthHistoryReport
        {
            Periods = periods,
            CurrentNetWorth = currentNetWorth,
            StartNetWorth = startNetWorth,
            NetWorthChange = netWorthChange,
            NetWorthChangePercentage = netWorthChangePercentage,
            StartDate = startDate,
            EndDate = endDate,
            GroupBy = groupBy
        };
    }

    public async Task<NetWorthSnapshot> CreateNetWorthSnapshotAsync(bool isManual = false, string? notes = null)
    {
        // Calculate current net worth
        var investments = await _context.Investments.ToListAsync();
        var assets = await _context.Assets.ToListAsync();
        var loans = await _context.Loans.ToListAsync();
        var bankSources = await _context.BankSources.ToListAsync();

        var bankBalance = bankSources.Sum(b => b.CurrentBalance);
        var investmentValue = investments.Sum(i => i.TotalValue);
        var physicalAssetValue = assets.Sum(a => a.CurrentValue);
        var totalAssets = bankBalance + investmentValue + physicalAssetValue;

        var loanBalance = loans.Sum(l => l.Amount);
        var totalLiabilities = loanBalance;

        var netWorth = totalAssets - totalLiabilities;

        var snapshot = new NetWorthSnapshot
        {
            Date = DateTime.Today,
            TotalAssets = totalAssets,
            TotalLiabilities = totalLiabilities,
            NetWorth = netWorth,
            BankBalance = bankBalance,
            InvestmentValue = investmentValue,
            PhysicalAssetValue = physicalAssetValue,
            LoanBalance = loanBalance,
            IsManual = isManual,
            Notes = notes,
            CreatedAt = DateTime.UtcNow
        };

        _context.NetWorthSnapshots.Add(snapshot);
        await _context.SaveChangesAsync();

        return snapshot;
    }
}
