using Microsoft.AspNetCore.Identity;

namespace Privatekonomi.Core.Models;

public class ApplicationUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the onboarding process has been completed.
    /// </summary>
    /// <remarks>This property is used to track the completion status of the onboarding process. It defaults
    /// to <see langword="false"/>, indicating that onboarding has not yet been completed.</remarks>
    public bool OnboardingCompleted { get; set; }
    public DateTime? OnboardingCompletedAt { get; set; }

    /// <summary>
    /// System-level admin flag for platform administration
    /// </summary>
    public bool IsSystemAdmin { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the household member associated with the user. This property is optional and may
    /// be null if the user is not part of any household.
    /// </summary>
    /// <remarks>If the value is null, it indicates that the user is not linked to any household.</remarks>
    public int? HouseholdMemberId { get; set; }
    public HouseholdMember? HouseholdMember { get; set; }
}
