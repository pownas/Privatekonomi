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

    public async Task<PeriodComparisonReport> GetPeriodComparisonAsync(DateTime? referenceDate = null, int? householdId = null)
    {
        var refDate = referenceDate ?? DateTime.Today;
        
        // Define current period (current month)
        var currentPeriodStart = new DateTime(refDate.Year, refDate.Month, 1);
        var currentPeriodEnd = currentPeriodStart.AddMonths(1).AddDays(-1);
        
        // Define previous period (previous month)
        var previousPeriodStart = currentPeriodStart.AddMonths(-1);
        var previousPeriodEnd = previousPeriodStart.AddMonths(1).AddDays(-1);
        
        // Define year ago period (same month last year)
        var yearAgoPeriodStart = currentPeriodStart.AddYears(-1);
        var yearAgoPeriodEnd = yearAgoPeriodStart.AddMonths(1).AddDays(-1);

        // Get transactions for all periods
        var query = _context.Transactions.AsQueryable();
        
        if (householdId.HasValue)
        {
            query = query.Where(t => t.HouseholdId == householdId.Value);
        }

        var allTransactions = await query
            .Where(t => t.Date >= yearAgoPeriodStart && t.Date <= currentPeriodEnd)
            .ToListAsync();

        // Current period
        var currentTransactions = allTransactions.Where(t => t.Date >= currentPeriodStart && t.Date <= currentPeriodEnd).ToList();
        var currentIncome = currentTransactions.Where(t => t.IsIncome).Sum(t => t.Amount);
        var currentExpenses = currentTransactions.Where(t => !t.IsIncome).Sum(t => t.Amount);
        var currentNetFlow = currentIncome - currentExpenses;

        // Previous period
        var previousTransactions = allTransactions.Where(t => t.Date >= previousPeriodStart && t.Date <= previousPeriodEnd).ToList();
        var previousIncome = previousTransactions.Where(t => t.IsIncome).Sum(t => t.Amount);
        var previousExpenses = previousTransactions.Where(t => !t.IsIncome).Sum(t => t.Amount);
        var previousNetFlow = previousIncome - previousExpenses;

        // Year ago period
        var yearAgoTransactions = allTransactions.Where(t => t.Date >= yearAgoPeriodStart && t.Date <= yearAgoPeriodEnd).ToList();
        var yearAgoIncome = yearAgoTransactions.Where(t => t.IsIncome).Sum(t => t.Amount);
        var yearAgoExpenses = yearAgoTransactions.Where(t => !t.IsIncome).Sum(t => t.Amount);
        var yearAgoNetFlow = yearAgoIncome - yearAgoExpenses;

        // Calculate sparkline data (last 6 months of expenses)
        var sparklineData = new List<double>();
        for (int i = 5; i >= 0; i--)
        {
            var monthStart = currentPeriodStart.AddMonths(-i);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);
            var monthExpenses = allTransactions
                .Where(t => !t.IsIncome && t.Date >= monthStart && t.Date <= monthEnd)
                .Sum(t => t.Amount);
            sparklineData.Add((double)monthExpenses);
        }

        return new PeriodComparisonReport
        {
            Income = CreatePeriodComparison(currentIncome, previousIncome, yearAgoIncome, true),
            Expenses = CreatePeriodComparison(currentExpenses, previousExpenses, yearAgoExpenses, false),
            NetFlow = CreatePeriodComparison(currentNetFlow, previousNetFlow, yearAgoNetFlow, true),
            SparklineData = sparklineData,
            CurrentPeriodStart = currentPeriodStart,
            CurrentPeriodEnd = currentPeriodEnd,
            PreviousPeriodStart = previousPeriodStart,
            PreviousPeriodEnd = previousPeriodEnd,
            YearAgoPeriodStart = yearAgoPeriodStart,
            YearAgoPeriodEnd = yearAgoPeriodEnd
        };
    }

    private PeriodComparison CreatePeriodComparison(decimal current, decimal previous, decimal yearAgo, bool higherIsBetter)
    {
        var changeFromPrevious = current - previous;
        var changeFromYearAgo = current - yearAgo;
        
        var percentageFromPrevious = previous != 0 ? (changeFromPrevious / previous) * 100 : 0;
        var percentageFromYearAgo = yearAgo != 0 ? (changeFromYearAgo / yearAgo) * 100 : 0;

        // Determine trend direction based on whether higher values are better
        string trendDirection;
        if (Math.Abs(percentageFromPrevious) < 5)
        {
            trendDirection = "Stable";
        }
        else if (higherIsBetter)
        {
            trendDirection = changeFromPrevious > 0 ? "Improving" : "Worsening";
        }
        else
        {
            trendDirection = changeFromPrevious < 0 ? "Improving" : "Worsening";
        }

        return new PeriodComparison
        {
            CurrentPeriod = current,
            PreviousPeriod = previous,
            YearAgoPeriod = yearAgo,
            ChangeFromPrevious = changeFromPrevious,
            ChangeFromYearAgo = changeFromYearAgo,
            PercentageChangeFromPrevious = percentageFromPrevious,
            PercentageChangeFromYearAgo = percentageFromYearAgo,
            TrendDirection = trendDirection
        };
    }

    public async Task<HealthScoreReport> GetHealthScoreAsync(int? householdId = null)
    {
        var report = new HealthScoreReport
        {
            CalculatedAt = DateTime.UtcNow
        };

        // Calculate each component
        report.SavingsRate = await CalculateSavingsRateScoreAsync(householdId);
        report.DebtLevel = await CalculateDebtLevelScoreAsync(householdId);
        report.EmergencyFund = await CalculateEmergencyFundScoreAsync(householdId);
        report.BudgetAdherence = await CalculateBudgetAdherenceScoreAsync(householdId);
        report.InvestmentDiversification = await CalculateInvestmentDiversificationScoreAsync(householdId);
        report.IncomeStability = await CalculateIncomeStabilityScoreAsync(householdId);

        // Calculate total score
        report.TotalScore = report.SavingsRate.Score + 
                           report.DebtLevel.Score + 
                           report.EmergencyFund.Score + 
                           report.BudgetAdherence.Score + 
                           report.InvestmentDiversification.Score + 
                           report.IncomeStability.Score;

        // Determine health level
        report.HealthLevel = report.TotalScore switch
        {
            >= 80 => "Utmärkt",
            >= 60 => "Bra",
            >= 40 => "Godkänt",
            _ => "Behöver förbättras"
        };

        // Identify strengths and improvement areas
        var components = new[] 
        { 
            report.SavingsRate, 
            report.DebtLevel, 
            report.EmergencyFund, 
            report.BudgetAdherence, 
            report.InvestmentDiversification, 
            report.IncomeStability 
        };

        foreach (var component in components.OrderByDescending(c => c.Score))
        {
            var percentage = component.MaxScore > 0 ? (component.Score * 100.0 / component.MaxScore) : 0;
            if (percentage >= 80 && report.Strengths.Count < 3)
            {
                var valueText = component.Value.HasValue ? $" ({component.Value.Value:F1}{component.Unit})" : "";
                report.Strengths.Add($"{component.Name}{valueText}");
            }
            else if (percentage < 60 && report.ImprovementAreas.Count < 3)
            {
                report.ImprovementAreas.Add(component.Description);
            }
        }

        // Get historical data (last 12 months)
        var historyStart = DateTime.Today.AddMonths(-12);
        var snapshots = await _context.NetWorthSnapshots
            .Where(s => s.Date >= historyStart)
            .OrderBy(s => s.Date)
            .ToListAsync();

        // For now, we'll add placeholder history points
        // In a real implementation, we'd store historical health scores
        report.History = new List<HealthScoreHistoryPoint>();

        return report;
    }

    private async Task<HealthScoreComponent> CalculateSavingsRateScoreAsync(int? householdId)
    {
        // Calculate savings rate for the last 3 months
        var threeMonthsAgo = DateTime.Today.AddMonths(-3);
        
        var query = _context.Transactions
            .Where(t => t.Date >= threeMonthsAgo);

        if (householdId.HasValue)
        {
            query = query.Where(t => t.HouseholdId == householdId.Value);
        }

        var transactions = await query.ToListAsync();
        
        var totalIncome = transactions.Where(t => t.IsIncome).Sum(t => t.Amount);
        var totalExpenses = transactions.Where(t => !t.IsIncome).Sum(t => t.Amount);
        
        var savingsRate = totalIncome > 0 ? ((totalIncome - totalExpenses) / totalIncome) * 100 : 0;
        
        // Score: 0-20 points based on savings rate
        var score = savingsRate switch
        {
            >= 20 => 20,
            >= 15 => 18,
            >= 10 => 15,
            >= 5 => 10,
            >= 0 => 5,
            _ => 0
        };

        var status = savingsRate switch
        {
            >= 20 => "Utmärkt",
            >= 10 => "Bra",
            >= 5 => "Godkänt",
            _ => "Låg"
        };

        return new HealthScoreComponent
        {
            Name = "Sparprocent",
            Score = score,
            MaxScore = 20,
            Value = savingsRate,
            Unit = "%",
            Status = status,
            Description = savingsRate < 10 ? "Öka sparandet till minst 10%" : ""
        };
    }

    private async Task<HealthScoreComponent> CalculateDebtLevelScoreAsync(int? householdId)
    {
        var loans = await _context.Loans.ToListAsync();
        var totalDebt = loans.Sum(l => l.CurrentBalance);
        
        // Get annual income
        var oneYearAgo = DateTime.Today.AddYears(-1);
        var query = _context.Transactions
            .Where(t => t.Date >= oneYearAgo && t.IsIncome);

        if (householdId.HasValue)
        {
            query = query.Where(t => t.HouseholdId == householdId.Value);
        }

        var annualIncome = await query.SumAsync(t => t.Amount);
        
        var debtToIncomeRatio = annualIncome > 0 ? (totalDebt / annualIncome) * 100 : 0;
        
        // Score: 0-20 points (lower debt is better)
        var score = debtToIncomeRatio switch
        {
            0 => 20,
            <= 100 => 18,
            <= 200 => 15,
            <= 300 => 10,
            <= 400 => 5,
            _ => 0
        };

        var status = debtToIncomeRatio switch
        {
            0 => "Utmärkt",
            <= 150 => "Bra",
            <= 300 => "Godkänt",
            _ => "Hög"
        };

        return new HealthScoreComponent
        {
            Name = "Skuldsättning",
            Score = score,
            MaxScore = 20,
            Value = debtToIncomeRatio,
            Unit = "%",
            Status = status,
            Description = debtToIncomeRatio > 200 ? "Minska skulder i förhållande till inkomst" : ""
        };
    }

    private async Task<HealthScoreComponent> CalculateEmergencyFundScoreAsync(int? householdId)
    {
        // Calculate monthly expenses (average of last 3 months)
        var threeMonthsAgo = DateTime.Today.AddMonths(-3);
        
        var query = _context.Transactions
            .Where(t => t.Date >= threeMonthsAgo && !t.IsIncome);

        if (householdId.HasValue)
        {
            query = query.Where(t => t.HouseholdId == householdId.Value);
        }

        var monthlyExpenses = await query.AverageAsync(t => (decimal?)t.Amount) ?? 0;
        
        // Get liquid assets (bank balances + liquid investments)
        var bankSources = await _context.BankSources.ToListAsync();
        var liquidAssets = bankSources.Sum(b => b.CurrentBalance);
        
        var monthsOfExpenses = monthlyExpenses > 0 ? liquidAssets / monthlyExpenses : 0;
        
        // Score: 0-20 points based on months of emergency fund
        var score = monthsOfExpenses switch
        {
            >= 6 => 20,
            >= 4 => 16,
            >= 3 => 12,
            >= 2 => 8,
            >= 1 => 4,
            _ => 0
        };

        var status = monthsOfExpenses switch
        {
            >= 6 => "Utmärkt",
            >= 3 => "Bra",
            >= 1 => "Godkänt",
            _ => "Låg"
        };

        return new HealthScoreComponent
        {
            Name = "Buffert",
            Score = score,
            MaxScore = 20,
            Value = monthsOfExpenses,
            Unit = " månader",
            Status = status,
            Description = monthsOfExpenses < 3 ? "Bygg upp minst 3 månaders buffert" : ""
        };
    }

    private async Task<HealthScoreComponent> CalculateBudgetAdherenceScoreAsync(int? householdId)
    {
        // Get current month's budget (active budget overlapping current month)
        var currentMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var currentMonthEnd = currentMonth.AddMonths(1).AddDays(-1);
        
        var budget = await _context.Budgets
            .Include(b => b.BudgetCategories)
            .ThenInclude(bc => bc.Category)
            .FirstOrDefaultAsync(b => b.StartDate <= currentMonthEnd && b.EndDate >= currentMonth);

        if (budget == null || !budget.BudgetCategories.Any())
        {
            return new HealthScoreComponent
            {
                Name = "Budgetföljning",
                Score = 0,
                MaxScore = 15,
                Status = "Saknas",
                Description = "Skapa en budget för att spåra utgifter"
            };
        }

        // Calculate budget adherence for each category
        var adherenceScores = new List<decimal>();
        
        foreach (var budgetCategory in budget.BudgetCategories)
        {
            var spent = await _context.Transactions
                .Where(t => t.Date >= currentMonth && 
                           t.Date < currentMonth.AddMonths(1) &&
                           !t.IsIncome &&
                           t.TransactionCategories.Any(tc => tc.CategoryId == budgetCategory.CategoryId))
                .SumAsync(t => t.Amount);

            if (budgetCategory.PlannedAmount > 0)
            {
                var adherence = Math.Min(100, (1 - Math.Abs(spent - budgetCategory.PlannedAmount) / budgetCategory.PlannedAmount) * 100);
                adherenceScores.Add(adherence);
            }
        }

        var averageAdherence = adherenceScores.Any() ? adherenceScores.Average() : 0;
        
        // Score: 0-15 points
        var score = averageAdherence switch
        {
            >= 90 => 15,
            >= 80 => 12,
            >= 70 => 9,
            >= 60 => 6,
            >= 50 => 3,
            _ => 0
        };

        var status = averageAdherence switch
        {
            >= 85 => "Utmärkt",
            >= 70 => "Bra",
            >= 50 => "Godkänt",
            _ => "Låg"
        };

        return new HealthScoreComponent
        {
            Name = "Budgetföljning",
            Score = score,
            MaxScore = 15,
            Value = averageAdherence,
            Unit = "%",
            Status = status,
            Description = averageAdherence < 70 ? "Förbättra budgetdisciplin" : ""
        };
    }

    private async Task<HealthScoreComponent> CalculateInvestmentDiversificationScoreAsync(int? householdId)
    {
        var investments = await _context.Investments.ToListAsync();
        
        if (!investments.Any())
        {
            return new HealthScoreComponent
            {
                Name = "Investeringsdiversifiering",
                Score = 0,
                MaxScore = 15,
                Status = "Saknas",
                Description = "Överväg att diversifiera investeringar"
            };
        }

        // Calculate diversification based on number of different investments
        var totalValue = investments.Sum(i => i.TotalValue);
        var diversificationScore = 0m;

        if (totalValue > 0)
        {
            // Calculate Herfindahl index (lower is more diversified)
            var herfindahl = investments.Sum(i => Math.Pow((double)(i.TotalValue / totalValue), 2));
            
            // Convert to diversification score (0-100, higher is better)
            diversificationScore = (decimal)((1 - herfindahl) * 100);
        }

        // Score: 0-15 points
        var score = diversificationScore switch
        {
            >= 70 => 15,
            >= 50 => 12,
            >= 30 => 9,
            >= 20 => 6,
            >= 10 => 3,
            _ => 0
        };

        var status = diversificationScore switch
        {
            >= 60 => "Utmärkt",
            >= 40 => "Bra",
            >= 20 => "Godkänt",
            _ => "Låg"
        };

        return new HealthScoreComponent
        {
            Name = "Investeringsdiversifiering",
            Score = score,
            MaxScore = 15,
            Value = diversificationScore,
            Unit = "%",
            Status = status,
            Description = diversificationScore < 40 ? "Öka diversifieringen av investeringar" : ""
        };
    }

    private async Task<HealthScoreComponent> CalculateIncomeStabilityScoreAsync(int? householdId)
    {
        // Get last 6 months of income
        var sixMonthsAgo = DateTime.Today.AddMonths(-6);
        
        var query = _context.Transactions
            .Where(t => t.Date >= sixMonthsAgo && t.IsIncome);

        if (householdId.HasValue)
        {
            query = query.Where(t => t.HouseholdId == householdId.Value);
        }

        var incomeTransactions = await query.ToListAsync();
        
        if (!incomeTransactions.Any())
        {
            return new HealthScoreComponent
            {
                Name = "Inkomststabilitet",
                Score = 0,
                MaxScore = 10,
                Status = "Saknas",
                Description = "Ingen inkomstdata registrerad"
            };
        }

        // Group by month and calculate coefficient of variation
        var monthlyIncomes = incomeTransactions
            .GroupBy(t => new { t.Date.Year, t.Date.Month })
            .Select(g => g.Sum(t => t.Amount))
            .ToList();

        if (monthlyIncomes.Count < 2)
        {
            return new HealthScoreComponent
            {
                Name = "Inkomststabilitet",
                Score = 5,
                MaxScore = 10,
                Status = "Begränsad data",
                Description = "Behöver mer inkomsthistorik"
            };
        }

        var mean = monthlyIncomes.Average();
        var variance = monthlyIncomes.Sum(i => Math.Pow((double)(i - mean), 2)) / monthlyIncomes.Count;
        var stdDev = (decimal)Math.Sqrt(variance);
        var coefficientOfVariation = mean > 0 ? (stdDev / mean) * 100 : 100;
        
        // Score: 0-10 points (lower variation is better)
        var score = coefficientOfVariation switch
        {
            <= 10 => 10,
            <= 20 => 8,
            <= 30 => 6,
            <= 40 => 4,
            <= 50 => 2,
            _ => 0
        };

        var status = coefficientOfVariation switch
        {
            <= 15 => "Utmärkt",
            <= 30 => "Bra",
            <= 50 => "Godkänt",
            _ => "Variabel"
        };

        var stability = 100 - coefficientOfVariation;

        return new HealthScoreComponent
        {
            Name = "Inkomststabilitet",
            Score = score,
            MaxScore = 10,
            Value = stability,
            Unit = "%",
            Status = status,
            Description = coefficientOfVariation > 30 ? "Inkomsten varierar mycket mellan månader" : ""
        };
    }
}
