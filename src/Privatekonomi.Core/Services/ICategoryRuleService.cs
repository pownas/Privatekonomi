using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services;

/// <summary>
/// Service interface for managing automatic categorization rules.
/// </summary>
public interface ICategoryRuleService
{
    /// <summary>
    /// Gets all categorization rules, ordered by priority (descending).
    /// </summary>
    Task<IEnumerable<CategoryRule>> GetAllRulesAsync();
    
    /// <summary>
    /// Gets active categorization rules, ordered by priority (descending).
    /// </summary>
    Task<IEnumerable<CategoryRule>> GetActiveRulesAsync();
    
    /// <summary>
    /// Gets a categorization rule by ID.
    /// </summary>
    Task<CategoryRule?> GetRuleByIdAsync(int id);
    
    /// <summary>
    /// Creates a new categorization rule.
    /// </summary>
    Task<CategoryRule> CreateRuleAsync(CategoryRule rule);
    
    /// <summary>
    /// Updates an existing categorization rule.
    /// </summary>
    Task<CategoryRule> UpdateRuleAsync(CategoryRule rule);
    
    /// <summary>
    /// Deletes a categorization rule.
    /// </summary>
    Task DeleteRuleAsync(int id);
    
    /// <summary>
    /// Finds the first matching category rule for a transaction.
    /// Returns null if no rule matches.
    /// </summary>
    Task<CategoryRule?> FindMatchingRuleAsync(string description, string? payee = null);
    
    /// <summary>
    /// Applies categorization rules to a transaction.
    /// Returns the category ID if a rule matches, otherwise null.
    /// </summary>
    Task<int?> ApplyCategoryRulesAsync(string description, string? payee = null);
    
    /// <summary>
    /// Applies categorization rules to multiple transactions in bulk.
    /// Returns a dictionary mapping transaction IDs to suggested category IDs.
    /// </summary>
    Task<Dictionary<int, int>> ApplyRulesToTransactionsAsync(IEnumerable<int> transactionIds);
}
