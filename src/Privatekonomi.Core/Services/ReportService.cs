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

    public async Task<CashFlowReport> GetCashFlowReportAsync(DateTime fromDate, DateTime toDate, string groupBy = "month")
    {
        var transactions = await _context.Transactions
            .Where(t => t.Date >= fromDate && t.Date <= toDate)
            .OrderBy(t => t.Date)
            .ToListAsync();

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

    public async Task<NetWorthReport> GetNetWorthReportAsync()
    {
        var investments = await _context.Investments.ToListAsync();
        var assets = await _context.Assets.ToListAsync();
        var loans = await _context.Loans.ToListAsync();

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
        var totalLiabilities = liabilityItems.Sum(l => l.Amount);

        return new NetWorthReport
        {
            TotalAssets = totalAssets,
            TotalLiabilities = totalLiabilities,
            NetWorth = totalAssets - totalLiabilities,
            Assets = assetItems,
            Liabilities = liabilityItems,
            CalculatedAt = DateTime.UtcNow
        };
    }

    public async Task<TrendAnalysis> GetTrendAnalysisAsync(int? categoryId, int months = 6)
    {
        var fromDate = DateTime.Now.AddMonths(-months);
        
        var query = _context.Transactions
            .Where(t => t.Date >= fromDate);

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

    public async Task<IEnumerable<TopMerchant>> GetTopMerchantsAsync(int limit = 10, DateTime? fromDate = null, DateTime? toDate = null)
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
}
