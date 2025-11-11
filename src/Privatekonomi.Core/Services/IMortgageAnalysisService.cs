using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services;

/// <summary>
/// Service for analyzing mortgage loans according to Swedish regulations
/// </summary>
public interface IMortgageAnalysisService
{
    /// <summary>
    /// Calculate required amortization according to Swedish mortgage rules
    /// </summary>
    /// <param name="loan">The mortgage loan to analyze</param>
    /// <returns>Analysis result with required amortization</returns>
    AmortizationRequirement CalculateAmortizationRequirement(Loan loan);
    
    /// <summary>
    /// Analyze interest rate risk for a mortgage loan
    /// </summary>
    /// <param name="loan">The mortgage loan to analyze</param>
    /// <param name="rateIncreaseScenarios">Interest rate increase scenarios (in percentage points)</param>
    /// <returns>Interest rate risk analysis</returns>
    InterestRateRiskAnalysis AnalyzeInterestRateRisk(Loan loan, decimal[] rateIncreaseScenarios);
    
    /// <summary>
    /// Get all mortgages with upcoming rate resets
    /// </summary>
    /// <param name="withinMonths">Number of months to look ahead</param>
    /// <returns>List of mortgages with rate resets</returns>
    Task<IEnumerable<Loan>> GetUpcomingRateResetsAsync(int withinMonths = 6);
    
    /// <summary>
    /// Calculate monthly cost including interest and amortization
    /// </summary>
    /// <param name="loan">The mortgage loan</param>
    /// <param name="customInterestRate">Optional custom interest rate for simulation</param>
    /// <returns>Monthly cost breakdown</returns>
    MonthlyCostBreakdown CalculateMonthlyCost(Loan loan, decimal? customInterestRate = null);
}
