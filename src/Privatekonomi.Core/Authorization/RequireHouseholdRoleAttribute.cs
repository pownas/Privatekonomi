using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Authorization;

/// <summary>
/// Attribute to specify minimum required household role for a method or class
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public class RequireHouseholdRoleAttribute : Attribute
{
    /// <summary>
    /// Minimum role required to access the resource
    /// </summary>
    public HouseholdRoleType MinimumRole { get; }

    /// <summary>
    /// Optional specific permission key required
    /// </summary>
    public string? Permission { get; set; }

    /// <summary>
    /// Optional household ID parameter name to use for validation
    /// </summary>
    public string HouseholdIdParameterName { get; set; } = "householdId";

    public RequireHouseholdRoleAttribute(HouseholdRoleType minimumRole)
    {
        MinimumRole = minimumRole;
    }
}
