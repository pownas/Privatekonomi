namespace Privatekonomi.Core.Models;

/// <summary>
/// Represents a completed activity or event in the household timeline
/// </summary>
public class HouseholdActivity
{
    public int HouseholdActivityId { get; set; }
    public int HouseholdId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CompletedDate { get; set; }
    public DateTime CreatedDate { get; set; }
    public HouseholdActivityType Type { get; set; }
    
    // Optional: Link to member who completed the activity
    public int? CompletedByMemberId { get; set; }
    public HouseholdMember? CompletedByMember { get; set; }
    
    public Household? Household { get; set; }
}

public enum HouseholdActivityType
{
    General,        // Allmänt
    Cleaning,       // Städning
    Maintenance,    // Underhåll
    Shopping,       // Inköp
    Cooking,        // Matlagning
    Repair,         // Reparation
    Other           // Övrigt
}
