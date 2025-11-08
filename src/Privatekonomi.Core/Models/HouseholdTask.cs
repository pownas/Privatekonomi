namespace Privatekonomi.Core.Models;

/// <summary>
/// Represents a task in the household to-do list
/// </summary>
public class HouseholdTask
{
    public int HouseholdTaskId { get; set; }
    public int HouseholdId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public bool IsCompleted { get; set; }
    public HouseholdTaskPriority Priority { get; set; } = HouseholdTaskPriority.Normal;
    public HouseholdActivityType Category { get; set; }
    
    // Optional: Assigned to a specific member
    public int? AssignedToMemberId { get; set; }
    public HouseholdMember? AssignedToMember { get; set; }
    
    // Optional: Completed by member
    public int? CompletedByMemberId { get; set; }
    public HouseholdMember? CompletedByMember { get; set; }
    
    public Household? Household { get; set; }
}

public enum HouseholdTaskPriority
{
    Low,      // Låg
    Normal,   // Normal
    High      // Hög
}
