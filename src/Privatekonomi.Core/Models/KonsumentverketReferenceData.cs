namespace Privatekonomi.Core.Models;

/// <summary>
/// Static reference data from Konsumentverket for household costs 2025
/// </summary>
public static class KonsumentverketReferenceData
{
    /// <summary>
    /// Food costs per person based on age group and cooking pattern
    /// All costs are per month in SEK
    /// </summary>
    public static class FoodCosts
    {
        public static readonly Dictionary<AgeGroup, FoodCostData> AllFoodAtHome = new()
        {
            { AgeGroup.SixToElevenMonths, new FoodCostData(1050) },
            { AgeGroup.OneYear, new FoodCostData(1210) },
            { AgeGroup.TwoToFiveYears, new FoodCostData(1610) },
            { AgeGroup.SixToNineYears, new FoodCostData(2390) },
            { AgeGroup.TenToThirteenYears, new FoodCostData(3000) },
            { AgeGroup.FourteenToSeventeenYears, new FoodCostData(3610) },
            { AgeGroup.EighteenToThirtyYears, new FoodCostData(3960) },
            { AgeGroup.ThirtyOneToSixtyYears, new FoodCostData(3730) },
            { AgeGroup.SixtyOneToSeventyFourYears, new FoodCostData(3350) },
            { AgeGroup.SeventyFivePlusYears, new FoodCostData(3350) } // Using same as 61-74 as marked with *
        };

        public static readonly Dictionary<AgeGroup, FoodCostData> LunchOutOnWeekdays = new()
        {
            { AgeGroup.SixToElevenMonths, new FoodCostData(710) },
            { AgeGroup.OneYear, new FoodCostData(910) },
            { AgeGroup.TwoToFiveYears, new FoodCostData(1240) },
            { AgeGroup.SixToNineYears, new FoodCostData(1830) },
            { AgeGroup.TenToThirteenYears, new FoodCostData(2300) },
            { AgeGroup.FourteenToSeventeenYears, new FoodCostData(2770) },
            { AgeGroup.EighteenToThirtyYears, new FoodCostData(3040) },
            { AgeGroup.ThirtyOneToSixtyYears, new FoodCostData(2860) },
            { AgeGroup.SixtyOneToSeventyFourYears, new FoodCostData(2570) },
            { AgeGroup.SeventyFivePlusYears, new FoodCostData(2570) } // Using same as 61-74 as marked with *
        };
    }

    /// <summary>
    /// Individual costs per person based on age group
    /// All costs are per month in SEK
    /// </summary>
    public static class IndividualCosts
    {
        public static readonly Dictionary<IndividualAgeGroup, IndividualCostData> Costs = new()
        {
            { IndividualAgeGroup.ZeroYears, new IndividualCostData(
                Clothes: 1050, Leisure: 120, Mobile: 0, Hygiene: 500, Insurance: 220, ChildEquipment: 980) },
            { IndividualAgeGroup.OneToThreeYears, new IndividualCostData(
                Clothes: 1030, Leisure: 240, Mobile: 0, Hygiene: 650, Insurance: 220, ChildEquipment: 530) },
            { IndividualAgeGroup.FourToSixYears, new IndividualCostData(
                Clothes: 1140, Leisure: 410, Mobile: 0, Hygiene: 170, Insurance: 220, ChildEquipment: 20) },
            { IndividualAgeGroup.SevenToTenYears, new IndividualCostData(
                Clothes: 1110, Leisure: 430, Mobile: 70, Hygiene: 170, Insurance: 220, ChildEquipment: 20) },
            { IndividualAgeGroup.ElevenToFourteenYears, new IndividualCostData(
                Clothes: 990, Leisure: 400, Mobile: 70, Hygiene: 350, Insurance: 220, ChildEquipment: 0) },
            { IndividualAgeGroup.FifteenToSeventeenYears, new IndividualCostData(
                Clothes: 930, Leisure: 510, Mobile: 120, Hygiene: 550, Insurance: 220, ChildEquipment: 0) },
            { IndividualAgeGroup.EighteenToTwentyFiveYears, new IndividualCostData(
                Clothes: 850, Leisure: 610, Mobile: 120, Hygiene: 560, Insurance: 0, ChildEquipment: 0) },
            { IndividualAgeGroup.TwentySixToFortyNineYears, new IndividualCostData(
                Clothes: 800, Leisure: 590, Mobile: 120, Hygiene: 580, Insurance: 0, ChildEquipment: 0) },
            { IndividualAgeGroup.FiftyToSixtyFourYears, new IndividualCostData(
                Clothes: 770, Leisure: 580, Mobile: 100, Hygiene: 570, Insurance: 0, ChildEquipment: 0) },
            { IndividualAgeGroup.SixtyFivePlusYears, new IndividualCostData(
                Clothes: 700, Leisure: 550, Mobile: 100, Hygiene: 480, Insurance: 0, ChildEquipment: 0) }
        };
    }

    /// <summary>
    /// Household common costs based on number of persons
    /// All costs are per month in SEK
    /// </summary>
    public static class HouseholdCommonCosts
    {
        public static readonly Dictionary<int, HouseholdCommonCostData> Costs = new()
        {
            { 1, new HouseholdCommonCostData(
                Consumables: 150, HomeEquipment: 920, InternetMobile: 970, 
                OtherMedia: 630, Electricity: 380, WaterDrainage: 200, Insurance: 150) },
            { 2, new HouseholdCommonCostData(
                Consumables: 200, HomeEquipment: 1030, InternetMobile: 1190, 
                OtherMedia: 680, Electricity: 510, WaterDrainage: 390, Insurance: 170) },
            { 3, new HouseholdCommonCostData(
                Consumables: 300, HomeEquipment: 1310, InternetMobile: 1410, 
                OtherMedia: 710, Electricity: 670, WaterDrainage: 590, Insurance: 200) },
            { 4, new HouseholdCommonCostData(
                Consumables: 360, HomeEquipment: 1570, InternetMobile: 1620, 
                OtherMedia: 710, Electricity: 830, WaterDrainage: 790, Insurance: 250) },
            { 5, new HouseholdCommonCostData(
                Consumables: 440, HomeEquipment: 1790, InternetMobile: 1840, 
                OtherMedia: 710, Electricity: 960, WaterDrainage: 980, Insurance: 290) },
            { 6, new HouseholdCommonCostData(
                Consumables: 490, HomeEquipment: 1960, InternetMobile: 2060, 
                OtherMedia: 710, Electricity: 1090, WaterDrainage: 1180, Insurance: 330) },
            { 7, new HouseholdCommonCostData(
                Consumables: 550, HomeEquipment: 2070, InternetMobile: 2280, 
                OtherMedia: 830, Electricity: 1220, WaterDrainage: 1380, Insurance: 340) }
        };
    }
}

/// <summary>
/// Age groups for food costs (based on Konsumentverket table structure)
/// </summary>
public enum AgeGroup
{
    SixToElevenMonths,
    OneYear,
    TwoToFiveYears,
    SixToNineYears,
    TenToThirteenYears,
    FourteenToSeventeenYears,
    EighteenToThirtyYears,
    ThirtyOneToSixtyYears,
    SixtyOneToSeventyFourYears,
    SeventyFivePlusYears
}

/// <summary>
/// Age groups for individual costs (based on Konsumentverket table structure)
/// </summary>
public enum IndividualAgeGroup
{
    ZeroYears,
    OneToThreeYears,
    FourToSixYears,
    SevenToTenYears,
    ElevenToFourteenYears,
    FifteenToSeventeenYears,
    EighteenToTwentyFiveYears,
    TwentySixToFortyNineYears,
    FiftyToSixtyFourYears,
    SixtyFivePlusYears
}

/// <summary>
/// Food cost data for a specific age group
/// </summary>
public record FoodCostData(decimal MonthlyCost);

/// <summary>
/// Individual cost data for a specific age group
/// </summary>
public record IndividualCostData(
    decimal Clothes,
    decimal Leisure,
    decimal Mobile,
    decimal Hygiene,
    decimal Insurance,
    decimal ChildEquipment)
{
    public decimal Total => Clothes + Leisure + Mobile + Hygiene + Insurance + ChildEquipment;
}

/// <summary>
/// Household common cost data for a specific number of persons
/// </summary>
public record HouseholdCommonCostData(
    decimal Consumables,
    decimal HomeEquipment,
    decimal InternetMobile,
    decimal OtherMedia,
    decimal Electricity,
    decimal WaterDrainage,
    decimal Insurance)
{
    public decimal Total => Consumables + HomeEquipment + InternetMobile + OtherMedia + 
                           Electricity + WaterDrainage + Insurance;
}

/// <summary>
/// Represents a household member for Konsumentverket comparison
/// </summary>
public class KonsumentverketHouseholdMember
{
    public int Age { get; set; }
    public bool HasLunchOutOnWeekdays { get; set; }

    public AgeGroup GetFoodAgeGroup()
    {
        return Age switch
        {
            < 1 => AgeGroup.SixToElevenMonths,
            1 => AgeGroup.OneYear,
            >= 2 and <= 5 => AgeGroup.TwoToFiveYears,
            >= 6 and <= 9 => AgeGroup.SixToNineYears,
            >= 10 and <= 13 => AgeGroup.TenToThirteenYears,
            >= 14 and <= 17 => AgeGroup.FourteenToSeventeenYears,
            >= 18 and <= 30 => AgeGroup.EighteenToThirtyYears,
            >= 31 and <= 60 => AgeGroup.ThirtyOneToSixtyYears,
            >= 61 and <= 74 => AgeGroup.SixtyOneToSeventyFourYears,
            _ => AgeGroup.SeventyFivePlusYears
        };
    }

    public IndividualAgeGroup GetIndividualAgeGroup()
    {
        return Age switch
        {
            0 => IndividualAgeGroup.ZeroYears,
            >= 1 and <= 3 => IndividualAgeGroup.OneToThreeYears,
            >= 4 and <= 6 => IndividualAgeGroup.FourToSixYears,
            >= 7 and <= 10 => IndividualAgeGroup.SevenToTenYears,
            >= 11 and <= 14 => IndividualAgeGroup.ElevenToFourteenYears,
            >= 15 and <= 17 => IndividualAgeGroup.FifteenToSeventeenYears,
            >= 18 and <= 25 => IndividualAgeGroup.EighteenToTwentyFiveYears,
            >= 26 and <= 49 => IndividualAgeGroup.TwentySixToFortyNineYears,
            >= 50 and <= 64 => IndividualAgeGroup.FiftyToSixtyFourYears,
            _ => IndividualAgeGroup.SixtyFivePlusYears
        };
    }
}

/// <summary>
/// User's actual household costs for comparison
/// </summary>
public class UserHouseholdCosts
{
    public decimal FoodCosts { get; set; }
    public decimal ClothesCosts { get; set; }
    public decimal LeisureCosts { get; set; }
    public decimal MobileCosts { get; set; }
    public decimal HygieneCosts { get; set; }
    public decimal InsuranceCosts { get; set; }
    public decimal ChildEquipmentCosts { get; set; }
    public decimal ConsumablesCosts { get; set; }
    public decimal HomeEquipmentCosts { get; set; }
    public decimal InternetMobileCosts { get; set; }
    public decimal OtherMediaCosts { get; set; }
    public decimal ElectricityCosts { get; set; }
    public decimal WaterDrainageCosts { get; set; }
    public decimal HomeInsuranceCosts { get; set; }

    public decimal TotalIndividualCosts => ClothesCosts + LeisureCosts + MobileCosts + 
                                          HygieneCosts + InsuranceCosts + ChildEquipmentCosts;
    
    public decimal TotalCommonCosts => ConsumablesCosts + HomeEquipmentCosts + InternetMobileCosts + 
                                      OtherMediaCosts + ElectricityCosts + WaterDrainageCosts + HomeInsuranceCosts;
    
    public decimal TotalCosts => FoodCosts + TotalIndividualCosts + TotalCommonCosts;
}

/// <summary>
/// Comparison result between user costs and Konsumentverket reference
/// </summary>
public class KonsumentverketComparisonResult
{
    public List<KonsumentverketHouseholdMember> HouseholdMembers { get; set; } = new();
    public UserHouseholdCosts UserCosts { get; set; } = new();
    public decimal ReferenceFoodCosts { get; set; }
    public decimal ReferenceIndividualCosts { get; set; }
    public decimal ReferenceCommonCosts { get; set; }
    public decimal ReferenceTotalCosts => ReferenceFoodCosts + ReferenceIndividualCosts + ReferenceCommonCosts;
    
    public Dictionary<string, CategoryComparison> CategoryComparisons { get; set; } = new();
}

/// <summary>
/// Comparison for a specific cost category
/// </summary>
public class CategoryComparison
{
    public string CategoryName { get; set; } = string.Empty;
    public decimal UserCost { get; set; }
    public decimal ReferenceCost { get; set; }
    public decimal Difference => UserCost - ReferenceCost;
    public decimal DifferencePercentage => ReferenceCost > 0 ? (Difference / ReferenceCost) * 100 : 0;
}
