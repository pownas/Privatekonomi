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
    
    /// <summary>
    /// Exports amortization schedule to CSV format
    /// </summary>
    /// <param name="loan">Loan to generate schedule for</param>
    /// <param name="extraMonthlyPayment">Extra monthly payment amount</param>
    /// <returns>CSV file content as byte array</returns>
    byte[] ExportAmortizationScheduleToCsv(Loan loan, decimal extraMonthlyPayment = 0);
    
    /// <summary>
    /// Exports debt payoff strategy with detailed amortization schedules to CSV format
    /// </summary>
    /// <param name="strategy">Debt payoff strategy to export</param>
    /// <param name="loans">List of loans in the strategy</param>
    /// <returns>CSV file content as byte array</returns>
    byte[] ExportStrategyToCsv(DebtPayoffStrategy strategy, List<Loan> loans);
    
    /// <summary>
    /// Generates detailed amortization schedule for a debt payoff strategy
    /// Shows how all loans are paid off following the strategy
    /// </summary>
    /// <param name="strategy">Strategy type: "Snowball" or "Avalanche"</param>
    /// <param name="availableMonthlyPayment">Total available monthly payment amount</param>
    /// <returns>Detailed strategy with month-by-month payment schedule for all loans</returns>
    Task<DetailedDebtPayoffStrategy> GenerateDetailedStrategy(string strategy, decimal availableMonthlyPayment);
}
