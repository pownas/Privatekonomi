namespace Privatekonomi.Core.Models;

/// <summary>
/// Represents a role assigned to a household member
/// </summary>
public class HouseholdRole
{
    public int HouseholdRoleId { get; set; }
    public int HouseholdMemberId { get; set; }
    public HouseholdRoleType RoleType { get; set; }
    
    // Delegering (Delegation)
    public bool IsDelegated { get; set; }
    public string? DelegatedBy { get; set; }  // UserId som delegerade
    public DateTime? DelegationStartDate { get; set; }
    public DateTime? DelegationEndDate { get; set; }
    
    // Metadata
    public DateTime AssignedDate { get; set; } = DateTime.UtcNow;
    public string? AssignedBy { get; set; }  // UserId
    public DateTime? RevokedDate { get; set; }
    public string? RevokedBy { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Navigation
    public HouseholdMember? HouseholdMember { get; set; }
}

/// <summary>
/// Defines the hierarchy of household roles
/// </summary>
public enum HouseholdRoleType
{
    /// <summary>
    /// Full control over household (only one per household)
    /// </summary>
    Admin = 1,
    
    /// <summary>
    /// Almost all permissions except member/role management
    /// </summary>
    FullAccess = 2,
    
    /// <summary>
    /// Can create and edit own data, with amount limits
    /// </summary>
    Editor = 3,
    
    /// <summary>
    /// Read-only access to all household data
    /// </summary>
    ViewOnly = 4,
    
    /// <summary>
    /// Can only see and manage own transactions/budgets
    /// </summary>
    Limited = 5,
    
    /// <summary>
    /// Special role for children with age-based restrictions
    /// </summary>
    Child = 6
}
