namespace Privatekonomi.Core.Models;

public class BudgetCategory
{
    public int BudgetCategoryId { get; set; }
    public int BudgetId { get; set; }
    public int CategoryId { get; set; }
    public decimal PlannedAmount { get; set; }
    
    /// <summary>
    /// Number of months for the recurrence period. 
    /// Default is 1 (monthly). Can be 2 (bi-monthly), 3 (quarterly), 6 (semi-annual), or 12 (annual).
    /// </summary>
    public int RecurrencePeriodMonths { get; set; } = 1;
    
    /// <summary>
    /// Calculated monthly amount based on PlannedAmount and RecurrencePeriodMonths.
    /// For example, if PlannedAmount is 2400 kr and RecurrencePeriodMonths is 12 (annual), 
    /// MonthlyAmount will be 200 kr.
    /// </summary>
    public decimal MonthlyAmount => RecurrencePeriodMonths > 0 ? PlannedAmount / RecurrencePeriodMonths : PlannedAmount;
    
    public Budget Budget { get; set; } = null!;
    public Category Category { get; set; } = null!;
}
