using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services;

public interface IDebtStrategyService
{
    /// <summary>
    /// Generates an amortization schedule for a loan
    /// </summary>
    List<AmortizationScheduleEntry> GenerateAmortizationSchedule(Loan loan, decimal extraMonthlyPayment = 0);
    
    /// <summary>
    /// Calculates debt payoff using the snowball method (smallest balance first)
    /// </summary>
    Task<DebtPayoffStrategy> CalculateSnowballStrategy(decimal availableMonthlyPayment);
    
    /// <summary>
    /// Calculates debt payoff using the avalanche method (highest interest rate first)
    /// </summary>
    Task<DebtPayoffStrategy> CalculateAvalancheStrategy(decimal availableMonthlyPayment);
    
    /// <summary>
    /// Analyzes the impact of extra payments on a loan
    /// </summary>
    ExtraPaymentAnalysis AnalyzeExtraPayment(Loan loan, decimal extraMonthlyPayment);
    
    /// <summary>
    /// Calculates the debt-free date for current loans
    /// </summary>
    Task<DateTime?> CalculateDebtFreeDate();
    
    /// <summary>
    /// Compares snowball and avalanche strategies
    /// </summary>
    Task<(DebtPayoffStrategy Snowball, DebtPayoffStrategy Avalanche)> CompareStrategies(decimal availableMonthlyPayment);
}
