using Microsoft.EntityFrameworkCore;
using Privatekonomi.Core.Data;
using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services;

/// <summary>
/// Service for analyzing mortgage loans according to Swedish regulations
/// </summary>
public class MortgageAnalysisService : IMortgageAnalysisService
{
    private readonly PrivatekonomyContext _context;
    private readonly ICurrentUserService? _currentUserService;

    public MortgageAnalysisService(PrivatekonomyContext context, ICurrentUserService? currentUserService = null)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Calculate required amortization according to Swedish mortgage rules
    /// </summary>
    public AmortizationRequirement CalculateAmortizationRequirement(Loan loan)
    {
        if (loan.Type != "Bolån")
        {
            return new AmortizationRequirement
            {
                Loan = loan,
                LoanToValueRatio = 0,
                RequiredAnnualAmortization = 0,
                RequiredMonthlyAmortization = 0,
                CurrentMonthlyAmortization = loan.Amortization,
                MeetsRequirement = true,
                MonthlyShortage = 0,
                RuleDescription = "Amorteringskrav gäller endast för bolån.",
                ApplicableRule = AmortizationRule.NoRequirement
            };
        }

        var ltv = loan.LTV ?? 0;
        var loanAmount = loan.Amount;
        decimal requiredAnnualAmortization = 0;
        AmortizationRule applicableRule;
        string ruleDescription;

        // Swedish mortgage regulations (Bolånetaket):
        // - LTV > 70%: Amortize 2% of original loan amount per year
        // - 50% < LTV ≤ 70%: Amortize 1% of original loan amount per year
        // - LTV ≤ 50%: No amortization requirement
        
        if (ltv > 70)
        {
            requiredAnnualAmortization = loanAmount * 0.02m;
            applicableRule = AmortizationRule.TwoPercentAnnual;
            ruleDescription = "Vid belåningsgrad över 70% krävs 2% årlig amortering av ursprungligt lånebelopp.";
        }
        else if (ltv > 50)
        {
            requiredAnnualAmortization = loanAmount * 0.01m;
            applicableRule = AmortizationRule.OnePercentAnnual;
            ruleDescription = "Vid belåningsgrad mellan 50-70% krävs 1% årlig amortering av ursprungligt lånebelopp.";
        }
        else
        {
            requiredAnnualAmortization = 0;
            applicableRule = AmortizationRule.NoRequirement;
            ruleDescription = "Vid belåningsgrad under 50% finns inget amorteringskrav.";
        }

        var requiredMonthlyAmortization = requiredAnnualAmortization / 12;
        var currentMonthlyAmortization = loan.Amortization + (loan.ExtraMonthlyPayment ?? 0);
        var meetsRequirement = currentMonthlyAmortization >= requiredMonthlyAmortization;
        var shortage = meetsRequirement ? 0 : requiredMonthlyAmortization - currentMonthlyAmortization;

        decimal? yearsToPayoff = null;
        if (currentMonthlyAmortization > 0)
        {
            var totalMonths = loanAmount / currentMonthlyAmortization;
            yearsToPayoff = Math.Round(totalMonths / 12, 1);
        }

        return new AmortizationRequirement
        {
            Loan = loan,
            LoanToValueRatio = ltv,
            RequiredAnnualAmortization = requiredAnnualAmortization,
            RequiredMonthlyAmortization = requiredMonthlyAmortization,
            CurrentMonthlyAmortization = currentMonthlyAmortization,
            MeetsRequirement = meetsRequirement,
            MonthlyShortage = shortage,
            RuleDescription = ruleDescription,
            ApplicableRule = applicableRule,
            YearsToPayoff = yearsToPayoff
        };
    }

    /// <summary>
    /// Analyze interest rate risk for a mortgage loan
    /// </summary>
    public InterestRateRiskAnalysis AnalyzeInterestRateRisk(Loan loan, decimal[] rateIncreaseScenarios)
    {
        var currentRate = loan.InterestRate;
        var currentCost = CalculateMonthlyCost(loan);
        
        var scenarios = new List<InterestRateScenario>();
        
        foreach (var increase in rateIncreaseScenarios)
        {
            var newRate = currentRate + increase;
            var newCost = CalculateMonthlyCost(loan, newRate);
            var monthlyIncrease = newCost.TotalMonthlyPayment - currentCost.TotalMonthlyPayment;
            
            scenarios.Add(new InterestRateScenario
            {
                ScenarioName = increase > 0 ? $"+{increase:F1}%" : $"{increase:F1}%",
                InterestRate = newRate,
                MonthlyCost = newCost.TotalMonthlyPayment,
                MonthlyIncrease = monthlyIncrease,
                AnnualIncrease = monthlyIncrease * 12
            });
        }

        // Calculate risk level
        var riskLevel = CalculateRiskLevel(loan);
        var riskDescription = GetRiskDescription(loan, riskLevel);

        int? monthsUntilReset = null;
        if (loan.RateResetDate.HasValue)
        {
            monthsUntilReset = (int)Math.Ceiling((loan.RateResetDate.Value - DateTime.Today).TotalDays / 30);
            if (monthsUntilReset < 0) monthsUntilReset = 0;
        }

        return new InterestRateRiskAnalysis
        {
            Loan = loan,
            CurrentInterestRate = currentRate,
            CurrentMonthlyCost = currentCost.TotalMonthlyPayment,
            IsFixedRate = loan.IsFixedRate,
            RateResetDate = loan.RateResetDate,
            MonthsUntilRateReset = monthsUntilReset,
            Scenarios = scenarios,
            RiskLevel = riskLevel,
            RiskDescription = riskDescription
        };
    }

    /// <summary>
    /// Get all mortgages with upcoming rate resets
    /// </summary>
    public async Task<IEnumerable<Loan>> GetUpcomingRateResetsAsync(int withinMonths = 6)
    {
        var cutoffDate = DateTime.Today.AddMonths(withinMonths);
        var query = _context.Loans
            .Where(l => l.Type == "Bolån" 
                && l.IsFixedRate 
                && l.RateResetDate.HasValue 
                && l.RateResetDate.Value <= cutoffDate
                && l.RateResetDate.Value >= DateTime.Today);

        // Filter by current user if authenticated
        if (_currentUserService?.IsAuthenticated == true && _currentUserService.UserId != null)
        {
            query = query.Where(l => l.UserId == _currentUserService.UserId);
        }

        return await query.OrderBy(l => l.RateResetDate).ToListAsync();
    }

    /// <summary>
    /// Calculate monthly cost including interest and amortization
    /// </summary>
    public MonthlyCostBreakdown CalculateMonthlyCost(Loan loan, decimal? customInterestRate = null)
    {
        var rate = customInterestRate ?? loan.InterestRate;
        var principal = loan.Amount;
        var monthlyInterest = (principal * rate / 100) / 12;
        var monthlyAmortization = loan.Amortization + (loan.ExtraMonthlyPayment ?? 0);
        var totalMonthlyPayment = monthlyInterest + monthlyAmortization;

        return new MonthlyCostBreakdown
        {
            Principal = principal,
            MonthlyInterest = monthlyInterest,
            MonthlyAmortization = monthlyAmortization,
            TotalMonthlyPayment = totalMonthlyPayment,
            InterestRate = rate,
            AnnualInterestCost = monthlyInterest * 12,
            AnnualAmortization = monthlyAmortization * 12,
            TotalAnnualPayment = totalMonthlyPayment * 12
        };
    }

    private RiskLevel CalculateRiskLevel(Loan loan)
    {
        var ltv = loan.LTV ?? 0;
        
        // Variable rate or no fixed rate information
        if (!loan.IsFixedRate || !loan.RateResetDate.HasValue)
        {
            return ltv > 70 ? RiskLevel.High : RiskLevel.Medium;
        }

        var monthsUntilReset = (loan.RateResetDate.Value - DateTime.Today).TotalDays / 30;

        // Fixed rate with long binding period
        if (monthsUntilReset > 36) // More than 3 years
        {
            return RiskLevel.Low;
        }
        else if (monthsUntilReset > 12) // 1-3 years
        {
            return ltv > 70 ? RiskLevel.Medium : RiskLevel.Low;
        }
        else // Less than 1 year
        {
            return ltv > 70 ? RiskLevel.High : RiskLevel.Medium;
        }
    }

    private string GetRiskDescription(Loan loan, RiskLevel riskLevel)
    {
        var ltv = loan.LTV ?? 0;
        var isFixedRate = loan.IsFixedRate;
        
        if (riskLevel == RiskLevel.Low)
        {
            if (isFixedRate && loan.RateResetDate.HasValue)
            {
                var monthsUntilReset = (int)Math.Ceiling((loan.RateResetDate.Value - DateTime.Today).TotalDays / 30);
                return $"Låg risk: Bunden ränta i {monthsUntilReset} månader till och belåningsgrad {ltv:F1}%.";
            }
            return $"Låg risk: Belåningsgrad {ltv:F1}% och stabil ränta.";
        }
        else if (riskLevel == RiskLevel.Medium)
        {
            if (isFixedRate && loan.RateResetDate.HasValue)
            {
                var monthsUntilReset = (int)Math.Ceiling((loan.RateResetDate.Value - DateTime.Today).TotalDays / 30);
                return $"Måttlig risk: Räntebindning löper ut om {monthsUntilReset} månader. Överväg att förlänga bindningstiden.";
            }
            return $"Måttlig risk: Rörlig ränta med belåningsgrad {ltv:F1}%. Överväg räntebindning.";
        }
        else // High
        {
            if (isFixedRate && loan.RateResetDate.HasValue)
            {
                var monthsUntilReset = (int)Math.Ceiling((loan.RateResetDate.Value - DateTime.Today).TotalDays / 30);
                return $"Hög risk: Räntebindning löper ut snart ({monthsUntilReset} månader) och hög belåningsgrad ({ltv:F1}%). Planera för högre kostnader.";
            }
            return $"Hög risk: Rörlig ränta och hög belåningsgrad ({ltv:F1}%). Starkt rekommenderat att binda räntan.";
        }
    }
}
