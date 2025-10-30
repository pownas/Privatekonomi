namespace Privatekonomi.Core.Services;

using Privatekonomi.Core.Models;

/// <summary>
/// Service for KALP (Kvar att leva på) calculations
/// </summary>
public interface IKalpService
{
    /// <summary>
    /// Calculate KALP from user input
    /// </summary>
    /// <param name="input">User's income, fixed expenses, and loans</param>
    /// <returns>KALP calculation result</returns>
    KalpCalculation CalculateKalp(KalpInput input);

    /// <summary>
    /// Calculate KALP from user's current budget and loans
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="budgetId">Budget ID</param>
    /// <returns>KALP calculation with budget comparison</returns>
    Task<KalpBudgetComparison> CalculateKalpFromBudgetAsync(string userId, int budgetId);

    /// <summary>
    /// Get recommended minimum KALP based on household composition
    /// Uses Konsumentverket reference data for variable expenses
    /// </summary>
    /// <param name="householdMembers">Household members</param>
    /// <returns>Recommended minimum KALP amount</returns>
    decimal CalculateRecommendedMinimumKalp(List<KonsumentverketHouseholdMember> householdMembers);

    /// <summary>
    /// Get KALP calculation with Konsumentverket comparison
    /// </summary>
    /// <param name="input">User's KALP input</param>
    /// <returns>KALP with Konsumentverket reference comparison</returns>
    KalpBudgetComparison CalculateKalpWithComparison(KalpInput input);
}
