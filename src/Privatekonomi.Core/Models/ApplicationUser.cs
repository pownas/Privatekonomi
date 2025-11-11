using Microsoft.AspNetCore.Identity;

namespace Privatekonomi.Core.Models;

public class ApplicationUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
    
    // Onboarding tracking
    public bool OnboardingCompleted { get; set; } = false;
    public DateTime? OnboardingCompletedAt { get; set; }
    
    // System-level admin flag for platform administration
    public bool IsSystemAdmin { get; set; } = false;
    
    // Link to household member (optional - user may not be part of any household)
    public int? HouseholdMemberId { get; set; }
    public HouseholdMember? HouseholdMember { get; set; }
}
