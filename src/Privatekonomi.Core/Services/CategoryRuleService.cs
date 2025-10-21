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
    private readonly ICurrentUserService? _currentUserService;

    public CategoryRuleService(PrivatekonomyContext context, ICurrentUserService? currentUserService = null)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<IEnumerable<CategoryRule>> GetAllRulesAsync(string? userId = null)
    {
        // Use provided userId or current user
        var effectiveUserId = userId ?? _currentUserService?.UserId;
        
        var query = _context.CategoryRules
            .Include(r => r.Category)
            .Include(r => r.OverridesSystemRule)
            .AsQueryable();

        if (!string.IsNullOrEmpty(effectiveUserId))
        {
            // Get system rules not overridden by user + user's own rules (including overrides)
            var userOverriddenSystemRuleIds = await _context.CategoryRules
                .Where(r => r.UserId == effectiveUserId && r.OverridesSystemRuleId != null)
                .Select(r => r.OverridesSystemRuleId!.Value)
                .ToListAsync();

            query = query.Where(r => 
                (r.RuleType == RuleType.System && !userOverriddenSystemRuleIds.Contains(r.CategoryRuleId)) ||
                (r.RuleType == RuleType.User && r.UserId == effectiveUserId));
        }

        return await query
            .OrderByDescending(r => r.Priority)
            .ThenBy(r => r.CategoryRuleId)
            .ToListAsync();
    }

    public async Task<IEnumerable<CategoryRule>> GetActiveRulesAsync(string? userId = null)
    {
        var allRules = await GetAllRulesAsync(userId);
        return allRules.Where(r => r.IsActive);
    }

    public async Task<CategoryRule?> GetRuleByIdAsync(int id)
    {
        var query = _context.CategoryRules
            .Include(r => r.Category)
            .Include(r => r.OverridesSystemRule)
            .AsQueryable();

        // Filter by current user if authenticated (users can only see their own rules and system rules)
        if (_currentUserService?.IsAuthenticated == true && _currentUserService.UserId != null)
        {
            query = query.Where(r => r.RuleType == RuleType.System || r.UserId == _currentUserService.UserId);
        }

        return await query.FirstOrDefaultAsync(r => r.CategoryRuleId == id);
    }

    public async Task<CategoryRule> CreateRuleAsync(CategoryRule rule, string? userId = null, bool isAdmin = false)
    {
        // Use provided userId or current user
        var effectiveUserId = userId ?? _currentUserService?.UserId;
        
        // Set rule type and user ID
        if (isAdmin && rule.RuleType == RuleType.System)
        {
            rule.UserId = null; // System rules have no user
        }
        else
        {
            rule.RuleType = RuleType.User;
            rule.UserId = effectiveUserId;
        }

        rule.CreatedAt = DateTime.UtcNow;
        _context.CategoryRules.Add(rule);
        await _context.SaveChangesAsync();
        return rule;
    }

    public async Task<CategoryRule> UpdateRuleAsync(CategoryRule rule, string? userId = null, bool isAdmin = false)
    {
        var existingRule = await _context.CategoryRules.FindAsync(rule.CategoryRuleId);
        if (existingRule == null)
        {
            throw new ArgumentException($"Rule with ID {rule.CategoryRuleId} not found");
        }

        // Check permissions
        if (existingRule.RuleType == RuleType.System && !isAdmin)
        {
            throw new UnauthorizedAccessException("Only administrators can modify system rules");
        }

        if (existingRule.RuleType == RuleType.User && existingRule.UserId != userId && !isAdmin)
        {
            throw new UnauthorizedAccessException("You can only modify your own rules");
        }

        rule.UpdatedAt = DateTime.UtcNow;
        _context.Entry(rule).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return rule;
    }

    public async Task DeleteRuleAsync(int id, string? userId = null, bool isAdmin = false)
    {
        var rule = await _context.CategoryRules.FindAsync(id);
        if (rule == null)
        {
            return;
        }

        // System rules cannot be deleted, only overridden
        if (rule.RuleType == RuleType.System && !isAdmin)
        {
            throw new UnauthorizedAccessException("System rules cannot be deleted. Create an override instead.");
        }

        // Check permissions for user rules
        if (rule.RuleType == RuleType.User && rule.UserId != userId && !isAdmin)
        {
            throw new UnauthorizedAccessException("You can only delete your own rules");
        }

        _context.CategoryRules.Remove(rule);
        await _context.SaveChangesAsync();
    }

    public async Task<CategoryRule> CreateOverrideAsync(int systemRuleId, CategoryRule overrideRule, string userId)
    {
        var systemRule = await _context.CategoryRules.FindAsync(systemRuleId);
        if (systemRule == null)
        {
            throw new ArgumentException($"System rule with ID {systemRuleId} not found");
        }

        if (systemRule.RuleType != RuleType.System)
        {
            throw new ArgumentException("Can only override system rules");
        }

        // Check if user already has an override for this system rule
        var existingOverride = await _context.CategoryRules
            .FirstOrDefaultAsync(r => r.OverridesSystemRuleId == systemRuleId && r.UserId == userId);

        if (existingOverride != null)
        {
            throw new InvalidOperationException("An override for this system rule already exists");
        }

        overrideRule.RuleType = RuleType.User;
        overrideRule.UserId = userId;
        overrideRule.OverridesSystemRuleId = systemRuleId;
        overrideRule.CreatedAt = DateTime.UtcNow;

        _context.CategoryRules.Add(overrideRule);
        await _context.SaveChangesAsync();
        return overrideRule;
    }

    public async Task RestoreSystemRuleAsync(int systemRuleId, string userId)
    {
        var userOverride = await _context.CategoryRules
            .FirstOrDefaultAsync(r => r.OverridesSystemRuleId == systemRuleId && r.UserId == userId);

        if (userOverride != null)
        {
            _context.CategoryRules.Remove(userOverride);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<CategoryRule?> FindMatchingRuleAsync(string description, string? payee = null, string? userId = null)
    {
        var rules = await GetActiveRulesAsync(userId);
        
        foreach (var rule in rules)
        {
            if (IsMatch(rule, description, payee))
            {
                return rule;
            }
        }
        
        return null;
    }

    public async Task<int?> ApplyCategoryRulesAsync(string description, string? payee = null, string? userId = null)
    {
        var matchingRule = await FindMatchingRuleAsync(description, payee, userId);
        return matchingRule?.CategoryId;
    }

    public async Task<Dictionary<int, int>> ApplyRulesToTransactionsAsync(IEnumerable<int> transactionIds, string? userId = null)
    {
        var results = new Dictionary<int, int>();
        var transactions = await _context.Transactions
            .Where(t => transactionIds.Contains(t.TransactionId))
            .ToListAsync();
        
        var rules = await GetActiveRulesAsync(userId);
        
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
