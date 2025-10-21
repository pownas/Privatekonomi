using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services;

public class BudgetTemplateService
{
    /// <summary>
    /// Apply a budget template to distribute income across categories
    /// </summary>
    /// <param name="templateType">Type of budget template to apply</param>
    /// <param name="totalIncome">Total monthly/yearly income</param>
    /// <param name="categories">Available categories</param>
    /// <returns>Dictionary of CategoryId to PlannedAmount</returns>
    public static Dictionary<int, decimal> ApplyTemplate(
        BudgetTemplateType templateType, 
        decimal totalIncome, 
        IEnumerable<Category> categories)
    {
        var result = new Dictionary<int, decimal>();
        
        switch (templateType)
        {
            case BudgetTemplateType.FiftyThirtyTwenty:
                return ApplyFiftyThirtyTwentyTemplate(totalIncome, categories);
            
            case BudgetTemplateType.ZeroBased:
                return ApplyZeroBasedTemplate(totalIncome, categories);
            
            case BudgetTemplateType.Envelope:
                return ApplyEnvelopeTemplate(totalIncome, categories);
            
            default:
                // Custom - all zeros, user fills in manually
                foreach (var category in categories)
                {
                    result[category.CategoryId] = 0;
                }
                return result;
        }
    }

    /// <summary>
    /// 50/30/20 Budget Rule:
    /// - 50% Needs (Boende, Transport, Mat)
    /// - 30% Wants (Nöje, Shopping)
    /// - 20% Savings (Sparande)
    /// </summary>
    private static Dictionary<int, decimal> ApplyFiftyThirtyTwentyTemplate(
        decimal totalIncome, 
        IEnumerable<Category> categories)
    {
        var result = new Dictionary<int, decimal>();
        var needs = totalIncome * 0.50m;
        var wants = totalIncome * 0.30m;
        var savings = totalIncome * 0.20m;

        foreach (var category in categories)
        {
            var categoryName = category.Name.ToLower();
            
            // 50% - Needs (Behov)
            if (categoryName.Contains("boende") || 
                categoryName.Contains("transport") || 
                categoryName.Contains("mat") ||
                categoryName.Contains("hälsa") ||
                categoryName.Contains("försäkring"))
            {
                // Distribute needs budget among need categories
                if (categoryName.Contains("boende"))
                    result[category.CategoryId] = needs * 0.40m; // 40% of needs for housing
                else if (categoryName.Contains("mat"))
                    result[category.CategoryId] = needs * 0.35m; // 35% of needs for food
                else if (categoryName.Contains("transport"))
                    result[category.CategoryId] = needs * 0.15m; // 15% of needs for transport
                else
                    result[category.CategoryId] = needs * 0.10m; // 10% for other needs
            }
            // 30% - Wants (Önskemål)
            else if (categoryName.Contains("nöje") || 
                     categoryName.Contains("shopping") ||
                     categoryName.Contains("hobby") ||
                     categoryName.Contains("restaurang"))
            {
                // Distribute wants budget among want categories
                if (categoryName.Contains("nöje"))
                    result[category.CategoryId] = wants * 0.50m;
                else if (categoryName.Contains("shopping"))
                    result[category.CategoryId] = wants * 0.30m;
                else
                    result[category.CategoryId] = wants * 0.20m;
            }
            // 20% - Savings (Sparande)
            else if (categoryName.Contains("sparande") || 
                     categoryName.Contains("investering") ||
                     categoryName.Contains("pension"))
            {
                result[category.CategoryId] = savings;
            }
            else
            {
                // Other categories get minimal allocation
                result[category.CategoryId] = totalIncome * 0.01m;
            }
        }

        return result;
    }

    /// <summary>
    /// Zero-Based Budget:
    /// Every krona is assigned a job. Total allocations should equal total income.
    /// This provides suggested starting percentages based on typical Swedish household spending.
    /// </summary>
    private static Dictionary<int, decimal> ApplyZeroBasedTemplate(
        decimal totalIncome, 
        IEnumerable<Category> categories)
    {
        var result = new Dictionary<int, decimal>();

        foreach (var category in categories)
        {
            var categoryName = category.Name.ToLower();
            
            // Typical Swedish household budget percentages
            if (categoryName.Contains("boende"))
                result[category.CategoryId] = totalIncome * 0.30m; // 30% housing
            else if (categoryName.Contains("mat") || categoryName.Contains("dryck"))
                result[category.CategoryId] = totalIncome * 0.15m; // 15% food
            else if (categoryName.Contains("transport"))
                result[category.CategoryId] = totalIncome * 0.10m; // 10% transport
            else if (categoryName.Contains("sparande") || categoryName.Contains("investering"))
                result[category.CategoryId] = totalIncome * 0.15m; // 15% savings
            else if (categoryName.Contains("nöje"))
                result[category.CategoryId] = totalIncome * 0.10m; // 10% entertainment
            else if (categoryName.Contains("shopping"))
                result[category.CategoryId] = totalIncome * 0.05m; // 5% shopping
            else if (categoryName.Contains("hälsa"))
                result[category.CategoryId] = totalIncome * 0.05m; // 5% health
            else if (categoryName.Contains("försäkring"))
                result[category.CategoryId] = totalIncome * 0.05m; // 5% insurance
            else
                result[category.CategoryId] = totalIncome * 0.05m; // 5% other/buffer
        }

        // Ensure total equals income (adjust last category if needed)
        var total = result.Values.Sum();
        if (total != totalIncome && result.Any())
        {
            var lastKey = result.Keys.Last();
            result[lastKey] += (totalIncome - total);
        }

        return result;
    }

    /// <summary>
    /// Envelope Budget:
    /// Each category gets a specific "envelope" of money.
    /// Once the envelope is empty, no more spending in that category.
    /// Similar to zero-based but with emphasis on strict category limits.
    /// </summary>
    private static Dictionary<int, decimal> ApplyEnvelopeTemplate(
        decimal totalIncome, 
        IEnumerable<Category> categories)
    {
        var result = new Dictionary<int, decimal>();

        foreach (var category in categories)
        {
            var categoryName = category.Name.ToLower();
            
            // Envelope budgeting with conservative allocations
            if (categoryName.Contains("boende"))
                result[category.CategoryId] = totalIncome * 0.30m;
            else if (categoryName.Contains("mat"))
                result[category.CategoryId] = totalIncome * 0.12m;
            else if (categoryName.Contains("transport"))
                result[category.CategoryId] = totalIncome * 0.08m;
            else if (categoryName.Contains("sparande"))
                result[category.CategoryId] = totalIncome * 0.20m; // Higher savings emphasis
            else if (categoryName.Contains("nöje"))
                result[category.CategoryId] = totalIncome * 0.08m;
            else if (categoryName.Contains("shopping"))
                result[category.CategoryId] = totalIncome * 0.05m;
            else if (categoryName.Contains("hälsa"))
                result[category.CategoryId] = totalIncome * 0.05m;
            else if (categoryName.Contains("buffert") || categoryName.Contains("övrigt"))
                result[category.CategoryId] = totalIncome * 0.12m; // Emergency buffer
            else
                result[category.CategoryId] = 0m; // No allocation unless specifically planned
        }

        return result;
    }

    /// <summary>
    /// Get template description and recommendations
    /// </summary>
    public static string GetTemplateDescription(BudgetTemplateType templateType)
    {
        return templateType switch
        {
            BudgetTemplateType.FiftyThirtyTwenty => 
                "50/30/20-regeln: 50% behov (boende, mat, transport), 30% önskemål (nöje, shopping), 20% sparande",
            
            BudgetTemplateType.ZeroBased => 
                "Zero-based budgeting: Varje krona tilldelas ett syfte. Alla inkomster fördelas över kategorier.",
            
            BudgetTemplateType.Envelope => 
                "Kuvertbudget: Varje kategori får ett fast belopp. När pengarna är slut, inget mer spenderande i den kategorin.",
            
            _ => "Anpassad budget: Du väljer själv hur mycket som ska budgeteras per kategori"
        };
    }

    /// <summary>
    /// Get suggested monthly income based on historical transactions
    /// </summary>
    public static decimal GetSuggestedIncome(IEnumerable<Transaction> recentTransactions)
    {
        var incomeTransactions = recentTransactions.Where(t => t.IsIncome);
        
        if (!incomeTransactions.Any())
            return 30000m; // Default Swedish median income
        
        // Average of last 3 months income
        return incomeTransactions
            .Where(t => t.Date >= DateTime.Now.AddMonths(-3))
            .Average(t => t.Amount);
    }
}
