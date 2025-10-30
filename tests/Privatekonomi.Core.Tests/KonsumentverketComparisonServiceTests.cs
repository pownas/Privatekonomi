using Xunit;
using Privatekonomi.Core.Services;
using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Tests;

public class KonsumentverketComparisonServiceTests
{
    private readonly IKonsumentverketComparisonService _service;

    public KonsumentverketComparisonServiceTests()
    {
        _service = new KonsumentverketComparisonService();
    }

    [Fact]
    public void CalculateReferenceCosts_EmptyHousehold_ReturnsZeroCosts()
    {
        // Arrange
        var members = new List<KonsumentverketHouseholdMember>();

        // Act
        var result = _service.CalculateReferenceCosts(members);

        // Assert
        Assert.Equal(0, result.ReferenceFoodCosts);
        Assert.Equal(0, result.ReferenceIndividualCosts);
        Assert.Equal(0, result.ReferenceCommonCosts);
        Assert.Equal(0, result.ReferenceTotalCosts);
    }

    [Fact]
    public void CalculateReferenceCosts_SingleAdult_ReturnsCorrectCosts()
    {
        // Arrange
        var members = new List<KonsumentverketHouseholdMember>
        {
            new KonsumentverketHouseholdMember { Age = 35, HasLunchOutOnWeekdays = false }
        };

        // Act
        var result = _service.CalculateReferenceCosts(members);

        // Assert
        // Food cost for 31-60 years, all food at home: 3730 SEK
        Assert.Equal(3730, result.ReferenceFoodCosts);
        
        // Individual costs for 26-49 years: 800+590+120+580 = 2090 SEK
        Assert.Equal(2090, result.ReferenceIndividualCosts);
        
        // Household common costs for 1 person: 150+920+970+630+380+200+150 = 3400 SEK
        Assert.Equal(3400, result.ReferenceCommonCosts);
        
        // Total: 3730 + 2090 + 3400 = 9220 SEK
        Assert.Equal(9220, result.ReferenceTotalCosts);
    }

    [Fact]
    public void CalculateReferenceCosts_SingleAdultWithLunchOut_ReturnsCorrectCosts()
    {
        // Arrange
        var members = new List<KonsumentverketHouseholdMember>
        {
            new KonsumentverketHouseholdMember { Age = 35, HasLunchOutOnWeekdays = true }
        };

        // Act
        var result = _service.CalculateReferenceCosts(members);

        // Assert
        // Food cost for 31-60 years, lunch out on weekdays: 2860 SEK
        Assert.Equal(2860, result.ReferenceFoodCosts);
        
        // Individual costs for 26-49 years: 2090 SEK
        Assert.Equal(2090, result.ReferenceIndividualCosts);
        
        // Household common costs for 1 person: 3400 SEK
        Assert.Equal(3400, result.ReferenceCommonCosts);
        
        // Total: 2860 + 2090 + 3400 = 8350 SEK
        Assert.Equal(8350, result.ReferenceTotalCosts);
    }

    [Fact]
    public void CalculateReferenceCosts_FamilyWithTwoAdultsAndTwoChildren_ReturnsCorrectCosts()
    {
        // Arrange
        var members = new List<KonsumentverketHouseholdMember>
        {
            new KonsumentverketHouseholdMember { Age = 35, HasLunchOutOnWeekdays = true },
            new KonsumentverketHouseholdMember { Age = 38, HasLunchOutOnWeekdays = true },
            new KonsumentverketHouseholdMember { Age = 8, HasLunchOutOnWeekdays = true },
            new KonsumentverketHouseholdMember { Age = 5, HasLunchOutOnWeekdays = true }
        };

        // Act
        var result = _service.CalculateReferenceCosts(members);

        // Assert
        // Food costs with lunch out:
        // Adult 35: 2860 (31-60 years)
        // Adult 38: 2860 (31-60 years)
        // Child 8: 1830 (6-9 years)
        // Child 5: 1240 (2-5 years)
        // Total: 8790 SEK
        Assert.Equal(8790, result.ReferenceFoodCosts);
        
        // Individual costs:
        // Adult 35 (26-49): 2090
        // Adult 38 (26-49): 2090
        // Child 8 (7-10): 2020
        // Child 5 (4-6): 1960
        // Total: 8160 SEK
        Assert.Equal(8160, result.ReferenceIndividualCosts);
        
        // Household common costs for 4 persons: 360+1570+1620+710+830+790+250 = 6130 SEK
        Assert.Equal(6130, result.ReferenceCommonCosts);
        
        // Total: 8790 + 8160 + 6130 = 23080 SEK
        Assert.Equal(23080, result.ReferenceTotalCosts);
    }

    [Fact]
    public void CalculateReferenceCosts_LargeHousehold_CapsAt7Members()
    {
        // Arrange
        var members = new List<KonsumentverketHouseholdMember>();
        for (int i = 0; i < 10; i++)
        {
            members.Add(new KonsumentverketHouseholdMember { Age = 30, HasLunchOutOnWeekdays = false });
        }

        // Act
        var result = _service.CalculateReferenceCosts(members);

        // Assert
        // Household common costs should use data for 7 persons (max in table)
        Assert.Equal(8670, result.ReferenceCommonCosts);
    }

    [Fact]
    public void CalculateReferenceCosts_InfantAndToddler_ReturnsCorrectCosts()
    {
        // Arrange
        var members = new List<KonsumentverketHouseholdMember>
        {
            new KonsumentverketHouseholdMember { Age = 0, HasLunchOutOnWeekdays = false },
            new KonsumentverketHouseholdMember { Age = 2, HasLunchOutOnWeekdays = false }
        };

        // Act
        var result = _service.CalculateReferenceCosts(members);

        // Assert
        // Food costs:
        // Infant 0 (6-11 months): 1050
        // Toddler 2 (2-5 years): 1610
        // Total: 2660 SEK
        Assert.Equal(2660, result.ReferenceFoodCosts);
        
        // Individual costs:
        // Infant 0 years: 2870
        // Toddler 1-3 years: 2670
        // Total: 5540 SEK
        Assert.Equal(5540, result.ReferenceIndividualCosts);
        
        // Household common costs for 2 persons: 4170 SEK
        Assert.Equal(4170, result.ReferenceCommonCosts);
    }

    [Fact]
    public void CompareWithReference_CalculatesCorrectDifferences()
    {
        // Arrange
        var members = new List<KonsumentverketHouseholdMember>
        {
            new KonsumentverketHouseholdMember { Age = 35, HasLunchOutOnWeekdays = false }
        };

        var userCosts = new UserHouseholdCosts
        {
            FoodCosts = 4000,
            ClothesCosts = 1000,
            LeisureCosts = 500,
            MobileCosts = 150,
            HygieneCosts = 600,
            ConsumablesCosts = 200,
            HomeEquipmentCosts = 1000,
            InternetMobileCosts = 1000,
            OtherMediaCosts = 700,
            ElectricityCosts = 400,
            WaterDrainageCosts = 250,
            HomeInsuranceCosts = 200
        };

        // Act
        var result = _service.CompareWithReference(members, userCosts);

        // Assert
        Assert.NotNull(result.CategoryComparisons);
        Assert.True(result.CategoryComparisons.ContainsKey("Livsmedel"));
        
        var foodComparison = result.CategoryComparisons["Livsmedel"];
        Assert.Equal(4000, foodComparison.UserCost);
        Assert.Equal(3730, foodComparison.ReferenceCost);
        Assert.Equal(270, foodComparison.Difference);
        Assert.True(foodComparison.DifferencePercentage > 0);
    }

    [Fact]
    public void CompareWithReference_CalculatesNegativeDifference()
    {
        // Arrange
        var members = new List<KonsumentverketHouseholdMember>
        {
            new KonsumentverketHouseholdMember { Age = 35, HasLunchOutOnWeekdays = false }
        };

        var userCosts = new UserHouseholdCosts
        {
            FoodCosts = 3000 // Less than reference 3730
        };

        // Act
        var result = _service.CompareWithReference(members, userCosts);

        // Assert
        var foodComparison = result.CategoryComparisons["Livsmedel"];
        Assert.Equal(3000, foodComparison.UserCost);
        Assert.Equal(3730, foodComparison.ReferenceCost);
        Assert.Equal(-730, foodComparison.Difference);
        Assert.True(foodComparison.DifferencePercentage < 0);
    }

    [Theory]
    [InlineData(0, IndividualAgeGroup.ZeroYears)]
    [InlineData(1, IndividualAgeGroup.OneToThreeYears)]
    [InlineData(3, IndividualAgeGroup.OneToThreeYears)]
    [InlineData(5, IndividualAgeGroup.FourToSixYears)]
    [InlineData(10, IndividualAgeGroup.SevenToTenYears)]
    [InlineData(14, IndividualAgeGroup.ElevenToFourteenYears)]
    [InlineData(17, IndividualAgeGroup.FifteenToSeventeenYears)]
    [InlineData(25, IndividualAgeGroup.EighteenToTwentyFiveYears)]
    [InlineData(40, IndividualAgeGroup.TwentySixToFortyNineYears)]
    [InlineData(60, IndividualAgeGroup.FiftyToSixtyFourYears)]
    [InlineData(70, IndividualAgeGroup.SixtyFivePlusYears)]
    public void GetIndividualAgeGroup_CorrectMapping(int age, IndividualAgeGroup expectedGroup)
    {
        // Arrange
        var member = new KonsumentverketHouseholdMember { Age = age };

        // Act
        var ageGroup = member.GetIndividualAgeGroup();

        // Assert
        Assert.Equal(expectedGroup, ageGroup);
    }

    [Theory]
    [InlineData(0, AgeGroup.SixToElevenMonths)]
    [InlineData(1, AgeGroup.OneYear)]
    [InlineData(3, AgeGroup.TwoToFiveYears)]
    [InlineData(7, AgeGroup.SixToNineYears)]
    [InlineData(12, AgeGroup.TenToThirteenYears)]
    [InlineData(16, AgeGroup.FourteenToSeventeenYears)]
    [InlineData(25, AgeGroup.EighteenToThirtyYears)]
    [InlineData(45, AgeGroup.ThirtyOneToSixtyYears)]
    [InlineData(70, AgeGroup.SixtyOneToSeventyFourYears)]
    [InlineData(80, AgeGroup.SeventyFivePlusYears)]
    public void GetFoodAgeGroup_CorrectMapping(int age, AgeGroup expectedGroup)
    {
        // Arrange
        var member = new KonsumentverketHouseholdMember { Age = age };

        // Act
        var ageGroup = member.GetFoodAgeGroup();

        // Assert
        Assert.Equal(expectedGroup, ageGroup);
    }

    [Fact]
    public void GetAgeGroupLabel_ReturnsSwedishLabels()
    {
        // Act & Assert
        Assert.Equal("6-11 mån", _service.GetAgeGroupLabel(AgeGroup.SixToElevenMonths));
        Assert.Equal("1 år", _service.GetAgeGroupLabel(AgeGroup.OneYear));
        Assert.Equal("18-30 år", _service.GetAgeGroupLabel(AgeGroup.EighteenToThirtyYears));
        Assert.Equal("75 år -", _service.GetAgeGroupLabel(AgeGroup.SeventyFivePlusYears));
    }

    [Fact]
    public void GetIndividualAgeGroupLabel_ReturnsSwedishLabels()
    {
        // Act & Assert
        Assert.Equal("0 år", _service.GetIndividualAgeGroupLabel(IndividualAgeGroup.ZeroYears));
        Assert.Equal("1-3 år", _service.GetIndividualAgeGroupLabel(IndividualAgeGroup.OneToThreeYears));
        Assert.Equal("65 år -", _service.GetIndividualAgeGroupLabel(IndividualAgeGroup.SixtyFivePlusYears));
    }
}
