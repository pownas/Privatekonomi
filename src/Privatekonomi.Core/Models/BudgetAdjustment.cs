namespace Privatekonomi.Core.Models;

/// <summary>
/// Records adjustments made to a budget suggestion before or after acceptance
/// </summary>
public class BudgetAdjustment
{
    public int BudgetAdjustmentId { get; set; }
    
    public int BudgetSuggestionId { get; set; }
    public BudgetSuggestion BudgetSuggestion { get; set; } = null!;
    
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    
    /// <summary>
    /// Amount before the adjustment
    /// </summary>
    public decimal PreviousAmount { get; set; }
    
    /// <summary>
    /// Amount after the adjustment
    /// </summary>
    public decimal NewAmount { get; set; }
    
    /// <summary>
    /// Reason for the adjustment (optional)
    /// </summary>
    public string? Reason { get; set; }
    
    /// <summary>
    /// When the adjustment was made
    /// </summary>
    public DateTime AdjustedAt { get; set; }
    
    /// <summary>
    /// Type of adjustment
    /// </summary>
    public AdjustmentType Type { get; set; }
    
    /// <summary>
    /// Target category for transfer adjustments
    /// </summary>
    public int? TransferToCategoryId { get; set; }
    public Category? TransferToCategory { get; set; }
}

/// <summary>
/// Types of budget adjustments
/// </summary>
public enum AdjustmentType
{
    /// <summary>
    /// Direct modification of amount
    /// </summary>
    Modification = 0,
    
    /// <summary>
    /// Transfer of funds between categories
    /// </summary>
    Transfer = 1,
    
    /// <summary>
    /// Removal of a category from the budget
    /// </summary>
    Removal = 2,
    
    /// <summary>
    /// Addition of a new category to the budget
    /// </summary>
    Addition = 3
}
