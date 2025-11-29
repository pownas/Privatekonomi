namespace Privatekonomi.Core.Models;

/// <summary>
/// Represents a budget suggestion generated based on income and budget distribution model
/// </summary>
public class BudgetSuggestion
{
    public int BudgetSuggestionId { get; set; }
    
    /// <summary>
    /// Name of the suggestion (e.g., "50/30/20 förslag för januari 2025")
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Total income used for calculating the suggestion
    /// </summary>
    public decimal TotalIncome { get; set; }
    
    /// <summary>
    /// The budget distribution model used for this suggestion
    /// </summary>
    public BudgetDistributionModel DistributionModel { get; set; }
    
    /// <summary>
    /// Date when the suggestion was created
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Whether the suggestion has been accepted/applied to create a budget
    /// </summary>
    public bool IsAccepted { get; set; }
    
    /// <summary>
    /// Date when the suggestion was accepted (if applicable)
    /// </summary>
    public DateTime? AcceptedAt { get; set; }
    
    /// <summary>
    /// The budget created from this suggestion (if accepted)
    /// </summary>
    public int? AppliedBudgetId { get; set; }
    public Budget? AppliedBudget { get; set; }
    
    /// <summary>
    /// User who owns this suggestion
    /// </summary>
    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }
    
    /// <summary>
    /// Household this suggestion is for (if applicable)
    /// </summary>
    public int? HouseholdId { get; set; }
    public Household? Household { get; set; }
    
    /// <summary>
    /// Individual category suggestions that make up this suggestion
    /// </summary>
    public ICollection<BudgetSuggestionItem> Items { get; set; } = new List<BudgetSuggestionItem>();
    
    /// <summary>
    /// Adjustments made to this suggestion before accepting
    /// </summary>
    public ICollection<BudgetAdjustment> Adjustments { get; set; } = new List<BudgetAdjustment>();
}

/// <summary>
/// Individual category allocation within a budget suggestion
/// </summary>
public class BudgetSuggestionItem
{
    public int BudgetSuggestionItemId { get; set; }
    
    public int BudgetSuggestionId { get; set; }
    public BudgetSuggestion BudgetSuggestion { get; set; } = null!;
    
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    
    /// <summary>
    /// Original suggested amount based on the distribution model
    /// </summary>
    public decimal SuggestedAmount { get; set; }
    
    /// <summary>
    /// Adjusted amount after user modifications (if any)
    /// </summary>
    public decimal AdjustedAmount { get; set; }
    
    /// <summary>
    /// Percentage of total income allocated to this category
    /// </summary>
    public decimal Percentage { get; set; }
    
    /// <summary>
    /// Budget allocation type (Needs, Wants, or Savings for 50/30/20)
    /// </summary>
    public BudgetAllocationCategory AllocationCategory { get; set; }
    
    /// <summary>
    /// Recurrence period in months (1 = monthly, 12 = yearly)
    /// </summary>
    public int RecurrencePeriodMonths { get; set; } = 1;
    
    /// <summary>
    /// Whether this item has been manually adjusted
    /// </summary>
    public bool IsManuallyAdjusted { get; set; }
}

/// <summary>
/// Budget distribution models supported by the suggestion service
/// </summary>
public enum BudgetDistributionModel
{
    /// <summary>
    /// 50% Needs, 30% Wants, 20% Savings
    /// </summary>
    FiftyThirtyTwenty = 0,
    
    /// <summary>
    /// Every krona is assigned a job
    /// </summary>
    ZeroBased = 1,
    
    /// <summary>
    /// Strict spending limits per category
    /// </summary>
    Envelope = 2,
    
    /// <summary>
    /// Swedish household budget for families
    /// </summary>
    SwedishFamily = 3,
    
    /// <summary>
    /// Swedish household budget for singles
    /// </summary>
    SwedishSingle = 4,
    
    /// <summary>
    /// 80/20 rule - 80% for expenses, 20% for savings
    /// </summary>
    EightyTwenty = 5,
    
    /// <summary>
    /// 70/20/10 rule - 70% needs/wants, 20% savings, 10% giving
    /// </summary>
    SeventyTwentyTen = 6
}

/// <summary>
/// Budget allocation categories for the 50/30/20 rule
/// </summary>
public enum BudgetAllocationCategory
{
    /// <summary>
    /// Essential expenses: housing, food, transportation, utilities
    /// </summary>
    Needs = 0,
    
    /// <summary>
    /// Non-essential expenses: entertainment, dining out, shopping
    /// </summary>
    Wants = 1,
    
    /// <summary>
    /// Savings and debt repayment
    /// </summary>
    Savings = 2,
    
    /// <summary>
    /// Charitable giving (for 70/20/10 model)
    /// </summary>
    Giving = 3
}
