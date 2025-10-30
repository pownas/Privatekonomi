namespace Privatekonomi.Core.Services;

using Privatekonomi.Core.Models;

/// <summary>
/// Service for comparing user household costs with Konsumentverket reference data
/// </summary>
public interface IKonsumentverketComparisonService
{
    /// <summary>
    /// Calculate reference costs based on household composition
    /// </summary>
    /// <param name="members">List of household members with ages and lunch patterns</param>
    /// <returns>Comparison result with calculated reference costs</returns>
    KonsumentverketComparisonResult CalculateReferenceCosts(List<KonsumentverketHouseholdMember> members);
    
    /// <summary>
    /// Compare user costs with reference costs
    /// </summary>
    /// <param name="members">List of household members</param>
    /// <param name="userCosts">User's actual household costs</param>
    /// <returns>Complete comparison result with differences per category</returns>
    KonsumentverketComparisonResult CompareWithReference(
        List<KonsumentverketHouseholdMember> members, 
        UserHouseholdCosts userCosts);
    
    /// <summary>
    /// Get age group label for display
    /// </summary>
    string GetAgeGroupLabel(AgeGroup ageGroup);
    
    /// <summary>
    /// Get individual age group label for display
    /// </summary>
    string GetIndividualAgeGroupLabel(IndividualAgeGroup ageGroup);
}
