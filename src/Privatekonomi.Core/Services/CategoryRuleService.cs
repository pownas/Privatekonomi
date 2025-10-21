using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Privatekonomi.Core.Data;
using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services;

/// <summary>
/// Service for managing automatic categorization rules.
/// </summary>
public class CategoryRuleService : ICategoryRuleService
{
    private readonly PrivatekonomyContext _context;

    public CategoryRuleService(PrivatekonomyContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CategoryRule>> GetAllRulesAsync()
    {
        return await _context.CategoryRules
            .Include(r => r.Category)
            .OrderByDescending(r => r.Priority)
            .ThenBy(r => r.CategoryRuleId)
            .ToListAsync();
    }

    public async Task<IEnumerable<CategoryRule>> GetActiveRulesAsync()
    {
        return await _context.CategoryRules
            .Include(r => r.Category)
            .Where(r => r.IsActive)
            .OrderByDescending(r => r.Priority)
            .ThenBy(r => r.CategoryRuleId)
            .ToListAsync();
    }

    public async Task<CategoryRule?> GetRuleByIdAsync(int id)
    {
        return await _context.CategoryRules
            .Include(r => r.Category)
            .FirstOrDefaultAsync(r => r.CategoryRuleId == id);
    }

    public async Task<CategoryRule> CreateRuleAsync(CategoryRule rule)
    {
        rule.CreatedAt = DateTime.UtcNow;
        _context.CategoryRules.Add(rule);
        await _context.SaveChangesAsync();
        return rule;
    }

    public async Task<CategoryRule> UpdateRuleAsync(CategoryRule rule)
    {
        rule.UpdatedAt = DateTime.UtcNow;
        _context.Entry(rule).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return rule;
    }

    public async Task DeleteRuleAsync(int id)
    {
        var rule = await _context.CategoryRules.FindAsync(id);
        if (rule != null)
        {
            _context.CategoryRules.Remove(rule);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<CategoryRule?> FindMatchingRuleAsync(string description, string? payee = null)
    {
        var rules = await GetActiveRulesAsync();
        
        foreach (var rule in rules)
        {
            if (IsMatch(rule, description, payee))
            {
                return rule;
            }
        }
        
        return null;
    }

    public async Task<int?> ApplyCategoryRulesAsync(string description, string? payee = null)
    {
        var matchingRule = await FindMatchingRuleAsync(description, payee);
        return matchingRule?.CategoryId;
    }

    public async Task<Dictionary<int, int>> ApplyRulesToTransactionsAsync(IEnumerable<int> transactionIds)
    {
        var results = new Dictionary<int, int>();
        var transactions = await _context.Transactions
            .Where(t => transactionIds.Contains(t.TransactionId))
            .ToListAsync();
        
        var rules = await GetActiveRulesAsync();
        
        foreach (var transaction in transactions)
        {
            foreach (var rule in rules)
            {
                if (IsMatch(rule, transaction.Description, transaction.Payee))
                {
                    results[transaction.TransactionId] = rule.CategoryId;
                    break; // Use first matching rule (highest priority)
                }
            }
        }
        
        return results;
    }

    /// <summary>
    /// Checks if a rule matches the given description and/or payee.
    /// </summary>
    private bool IsMatch(CategoryRule rule, string description, string? payee)
    {
        bool descriptionMatches = false;
        bool payeeMatches = false;
        
        // Check description if required
        if (rule.Field == MatchField.Description || rule.Field == MatchField.Both)
        {
            descriptionMatches = MatchesPattern(rule, description);
        }
        
        // Check payee if required and available
        if ((rule.Field == MatchField.Payee || rule.Field == MatchField.Both) && !string.IsNullOrEmpty(payee))
        {
            payeeMatches = MatchesPattern(rule, payee);
        }
        
        // Return true if the required field(s) match
        return rule.Field switch
        {
            MatchField.Description => descriptionMatches,
            MatchField.Payee => payeeMatches,
            MatchField.Both => descriptionMatches || payeeMatches,
            _ => false
        };
    }

    /// <summary>
    /// Checks if a pattern matches a given text.
    /// </summary>
    private bool MatchesPattern(CategoryRule rule, string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return false;
        }
        
        var pattern = rule.Pattern;
        var comparison = rule.CaseSensitive 
            ? StringComparison.Ordinal 
            : StringComparison.OrdinalIgnoreCase;
        
        return rule.MatchType switch
        {
            PatternMatchType.Exact => text.Equals(pattern, comparison),
            PatternMatchType.Contains => text.Contains(pattern, comparison),
            PatternMatchType.StartsWith => text.StartsWith(pattern, comparison),
            PatternMatchType.EndsWith => text.EndsWith(pattern, comparison),
            PatternMatchType.Regex => MatchesRegex(pattern, text, rule.CaseSensitive),
            _ => false
        };
    }

    /// <summary>
    /// Checks if a regular expression pattern matches a given text.
    /// </summary>
    private bool MatchesRegex(string pattern, string text, bool caseSensitive)
    {
        try
        {
            var options = caseSensitive 
                ? RegexOptions.None 
                : RegexOptions.IgnoreCase;
            
            return Regex.IsMatch(text, pattern, options);
        }
        catch (ArgumentException)
        {
            // Invalid regex pattern
            return false;
        }
    }
}
