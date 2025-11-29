using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services;

public interface IBudgetSuggestionService
{
    /// <summary>
    /// Generate a budget suggestion based on income and distribution model
    /// </summary>
    Task<BudgetSuggestion> GenerateSuggestionAsync(
        decimal totalIncome, 
        BudgetDistributionModel model, 
        string? name = null,
        int? householdId = null);
    
    /// <summary>
    /// Generate a suggestion based on recent transaction history
    /// </summary>
    Task<BudgetSuggestion> GenerateSuggestionFromHistoryAsync(
        BudgetDistributionModel model,
        int monthsToAnalyze = 3,
        int? householdId = null);
    
    /// <summary>
    /// Get all suggestions for the current user
    /// </summary>
    Task<IEnumerable<BudgetSuggestion>> GetAllSuggestionsAsync();
    
    /// <summary>
    /// Get a specific suggestion by ID
    /// </summary>
    Task<BudgetSuggestion?> GetSuggestionByIdAsync(int id);
    
    /// <summary>
    /// Get pending (not yet accepted) suggestions
    /// </summary>
    Task<IEnumerable<BudgetSuggestion>> GetPendingSuggestionsAsync();
    
    /// <summary>
    /// Adjust a suggestion item amount
    /// </summary>
    Task<BudgetSuggestionItem> AdjustSuggestionItemAsync(
        int suggestionId, 
        int categoryId, 
        decimal newAmount,
        string? reason = null);
    
    /// <summary>
    /// Transfer amount between two category items
    /// </summary>
    Task TransferBetweenItemsAsync(
        int suggestionId,
        int fromCategoryId,
        int toCategoryId,
        decimal amount,
        string? reason = null);
    
    /// <summary>
    /// Accept a suggestion and create a budget from it
    /// </summary>
    Task<Budget> AcceptSuggestionAsync(int suggestionId, DateTime startDate, DateTime endDate, BudgetPeriod period);
    
    /// <summary>
    /// Delete a suggestion
    /// </summary>
    Task DeleteSuggestionAsync(int id);
    
    /// <summary>
    /// Calculate the effect of changes compared to the original suggestion
    /// </summary>
    Task<BudgetSuggestionEffects> CalculateEffectsAsync(int suggestionId);
    
    /// <summary>
    /// Get the distribution model description
    /// </summary>
    string GetDistributionModelDescription(BudgetDistributionModel model);
    
    /// <summary>
    /// Get all available distribution models with descriptions
    /// </summary>
    IEnumerable<(BudgetDistributionModel Model, string Name, string Description)> GetAvailableModels();
}

/// <summary>
/// Effects of budget adjustments compared to original suggestion
/// </summary>
public class BudgetSuggestionEffects
{
    public decimal TotalOriginalAmount { get; set; }
    public decimal TotalAdjustedAmount { get; set; }
    public decimal TotalDifference => TotalAdjustedAmount - TotalOriginalAmount;
    public decimal NeedsOriginal { get; set; }
    public decimal NeedsAdjusted { get; set; }
    public decimal NeedsPercentageChange => NeedsOriginal > 0 ? ((NeedsAdjusted - NeedsOriginal) / NeedsOriginal) * 100 : 0;
    public decimal WantsOriginal { get; set; }
    public decimal WantsAdjusted { get; set; }
    public decimal WantsPercentageChange => WantsOriginal > 0 ? ((WantsAdjusted - WantsOriginal) / WantsOriginal) * 100 : 0;
    public decimal SavingsOriginal { get; set; }
    public decimal SavingsAdjusted { get; set; }
    public decimal SavingsPercentageChange => SavingsOriginal > 0 ? ((SavingsAdjusted - SavingsOriginal) / SavingsOriginal) * 100 : 0;
    public int AdjustmentsCount { get; set; }
    public List<CategoryEffect> CategoryEffects { get; set; } = new();
}

public class CategoryEffect
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal OriginalAmount { get; set; }
    public decimal AdjustedAmount { get; set; }
    public decimal Difference => AdjustedAmount - OriginalAmount;
    public decimal PercentageChange => OriginalAmount > 0 ? ((AdjustedAmount - OriginalAmount) / OriginalAmount) * 100 : 0;
    public BudgetAllocationCategory AllocationCategory { get; set; }
}
