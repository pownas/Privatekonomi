namespace Privatekonomi.Core.Services;

using Privatekonomi.Core.Models;

/// <summary>
/// Service for comparing user household costs with Konsumentverket reference data from 2025
/// </summary>
public class KonsumentverketComparisonService : IKonsumentverketComparisonService
{
    /// <inheritdoc/>
    public KonsumentverketComparisonResult CalculateReferenceCosts(List<KonsumentverketHouseholdMember> members)
    {
        var result = new KonsumentverketComparisonResult
        {
            HouseholdMembers = members
        };

        if (!members.Any())
        {
            return result;
        }

        // Calculate food costs
        decimal totalFoodCosts = 0;
        foreach (var member in members)
        {
            var ageGroup = member.GetFoodAgeGroup();
            var foodCostTable = member.HasLunchOutOnWeekdays 
                ? KonsumentverketReferenceData.FoodCosts.LunchOutOnWeekdays 
                : KonsumentverketReferenceData.FoodCosts.AllFoodAtHome;
            
            if (foodCostTable.TryGetValue(ageGroup, out var foodCost))
            {
                totalFoodCosts += foodCost.MonthlyCost;
            }
        }
        result.ReferenceFoodCosts = totalFoodCosts;

        // Calculate individual costs
        decimal totalIndividualCosts = 0;
        foreach (var member in members)
        {
            var ageGroup = member.GetIndividualAgeGroup();
            if (KonsumentverketReferenceData.IndividualCosts.Costs.TryGetValue(ageGroup, out var individualCost))
            {
                totalIndividualCosts += individualCost.Total;
            }
        }
        result.ReferenceIndividualCosts = totalIndividualCosts;

        // Calculate household common costs
        var householdSize = Math.Min(members.Count, 7); // Max 7 persons in reference table
        if (KonsumentverketReferenceData.HouseholdCommonCosts.Costs.TryGetValue(householdSize, out var commonCosts))
        {
            result.ReferenceCommonCosts = commonCosts.Total;
        }

        return result;
    }

    /// <inheritdoc/>
    public KonsumentverketComparisonResult CompareWithReference(
        List<KonsumentverketHouseholdMember> members, 
        UserHouseholdCosts userCosts)
    {
        var result = CalculateReferenceCosts(members);
        result.UserCosts = userCosts;

        // Build detailed category comparisons
        result.CategoryComparisons = new Dictionary<string, CategoryComparison>
        {
            ["Livsmedel"] = new CategoryComparison 
            { 
                CategoryName = "Livsmedel", 
                UserCost = userCosts.FoodCosts, 
                ReferenceCost = result.ReferenceFoodCosts 
            },
            ["Kläder och skor"] = new CategoryComparison 
            { 
                CategoryName = "Kläder och skor", 
                UserCost = userCosts.ClothesCosts, 
                ReferenceCost = GetReferenceCostByType(members, c => c.Clothes) 
            },
            ["Fritid och lek"] = new CategoryComparison 
            { 
                CategoryName = "Fritid och lek", 
                UserCost = userCosts.LeisureCosts, 
                ReferenceCost = GetReferenceCostByType(members, c => c.Leisure) 
            },
            ["Mobiltelefon"] = new CategoryComparison 
            { 
                CategoryName = "Mobiltelefon", 
                UserCost = userCosts.MobileCosts, 
                ReferenceCost = GetReferenceCostByType(members, c => c.Mobile) 
            },
            ["Personlig hygien"] = new CategoryComparison 
            { 
                CategoryName = "Personlig hygien", 
                UserCost = userCosts.HygieneCosts, 
                ReferenceCost = GetReferenceCostByType(members, c => c.Hygiene) 
            },
            ["Barn- och ungdomsförsäkring"] = new CategoryComparison 
            { 
                CategoryName = "Barn- och ungdomsförsäkring", 
                UserCost = userCosts.InsuranceCosts, 
                ReferenceCost = GetReferenceCostByType(members, c => c.Insurance) 
            },
            ["Barnutrustning"] = new CategoryComparison 
            { 
                CategoryName = "Barnutrustning", 
                UserCost = userCosts.ChildEquipmentCosts, 
                ReferenceCost = GetReferenceCostByType(members, c => c.ChildEquipment) 
            },
            ["Förbrukningsvaror"] = new CategoryComparison 
            { 
                CategoryName = "Förbrukningsvaror", 
                UserCost = userCosts.ConsumablesCosts, 
                ReferenceCost = GetHouseholdCommonCost(members.Count, c => c.Consumables) 
            },
            ["Hemutrustning"] = new CategoryComparison 
            { 
                CategoryName = "Hemutrustning", 
                UserCost = userCosts.HomeEquipmentCosts, 
                ReferenceCost = GetHouseholdCommonCost(members.Count, c => c.HomeEquipment) 
            },
            ["Internet- och mobilabonnemang"] = new CategoryComparison 
            { 
                CategoryName = "Internet- och mobilabonnemang", 
                UserCost = userCosts.InternetMobileCosts, 
                ReferenceCost = GetHouseholdCommonCost(members.Count, c => c.InternetMobile) 
            },
            ["Övriga medietjänster"] = new CategoryComparison 
            { 
                CategoryName = "Övriga medietjänster", 
                UserCost = userCosts.OtherMediaCosts, 
                ReferenceCost = GetHouseholdCommonCost(members.Count, c => c.OtherMedia) 
            },
            ["Hushållsel"] = new CategoryComparison 
            { 
                CategoryName = "Hushållsel", 
                UserCost = userCosts.ElectricityCosts, 
                ReferenceCost = GetHouseholdCommonCost(members.Count, c => c.Electricity) 
            },
            ["Vatten och avlopp"] = new CategoryComparison 
            { 
                CategoryName = "Vatten och avlopp", 
                UserCost = userCosts.WaterDrainageCosts, 
                ReferenceCost = GetHouseholdCommonCost(members.Count, c => c.WaterDrainage) 
            },
            ["Hemförsäkring"] = new CategoryComparison 
            { 
                CategoryName = "Hemförsäkring", 
                UserCost = userCosts.HomeInsuranceCosts, 
                ReferenceCost = GetHouseholdCommonCost(members.Count, c => c.Insurance) 
            }
        };

        return result;
    }

    /// <inheritdoc/>
    public string GetAgeGroupLabel(AgeGroup ageGroup)
    {
        return ageGroup switch
        {
            AgeGroup.SixToElevenMonths => "6-11 mån",
            AgeGroup.OneYear => "1 år",
            AgeGroup.TwoToFiveYears => "2-5 år",
            AgeGroup.SixToNineYears => "6-9 år",
            AgeGroup.TenToThirteenYears => "10-13 år",
            AgeGroup.FourteenToSeventeenYears => "14-17 år",
            AgeGroup.EighteenToThirtyYears => "18-30 år",
            AgeGroup.ThirtyOneToSixtyYears => "31-60 år",
            AgeGroup.SixtyOneToSeventyFourYears => "61-74 år",
            AgeGroup.SeventyFivePlusYears => "75 år -",
            _ => ageGroup.ToString()
        };
    }

    /// <inheritdoc/>
    public string GetIndividualAgeGroupLabel(IndividualAgeGroup ageGroup)
    {
        return ageGroup switch
        {
            IndividualAgeGroup.ZeroYears => "0 år",
            IndividualAgeGroup.OneToThreeYears => "1-3 år",
            IndividualAgeGroup.FourToSixYears => "4-6 år",
            IndividualAgeGroup.SevenToTenYears => "7-10 år",
            IndividualAgeGroup.ElevenToFourteenYears => "11-14 år",
            IndividualAgeGroup.FifteenToSeventeenYears => "15-17 år",
            IndividualAgeGroup.EighteenToTwentyFiveYears => "18-25 år",
            IndividualAgeGroup.TwentySixToFortyNineYears => "26-49 år",
            IndividualAgeGroup.FiftyToSixtyFourYears => "50-64 år",
            IndividualAgeGroup.SixtyFivePlusYears => "65 år -",
            _ => ageGroup.ToString()
        };
    }

    private decimal GetReferenceCostByType(
        List<KonsumentverketHouseholdMember> members, 
        Func<IndividualCostData, decimal> costSelector)
    {
        decimal total = 0;
        foreach (var member in members)
        {
            var ageGroup = member.GetIndividualAgeGroup();
            if (KonsumentverketReferenceData.IndividualCosts.Costs.TryGetValue(ageGroup, out var costs))
            {
                total += costSelector(costs);
            }
        }
        return total;
    }

    private decimal GetHouseholdCommonCost(
        int householdSize, 
        Func<HouseholdCommonCostData, decimal> costSelector)
    {
        var size = Math.Min(householdSize, 7); // Max 7 persons in reference table
        if (KonsumentverketReferenceData.HouseholdCommonCosts.Costs.TryGetValue(size, out var costs))
        {
            return costSelector(costs);
        }
        return 0;
    }
}
