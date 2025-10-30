using Privatekonomi.Core.Models;
using Xunit;

namespace Privatekonomi.Core.Tests;

public class BudgetCategoryTests
{
    [Fact]
    public void MonthlyAmount_WithMonthlyPeriod_ReturnsSameAsPlannedAmount()
    {
        // Arrange
        var budgetCategory = new BudgetCategory
        {
            PlannedAmount = 1000m,
            RecurrencePeriodMonths = 1
        };

        // Act
        var monthlyAmount = budgetCategory.MonthlyAmount;

        // Assert
        Assert.Equal(1000m, monthlyAmount);
    }

    [Fact]
    public void MonthlyAmount_WithBiMonthlyPeriod_ReturnsHalfOfPlannedAmount()
    {
        // Arrange
        var budgetCategory = new BudgetCategory
        {
            PlannedAmount = 1300m, // Example from issue: Avfall och vatten 1 300 kr/varannan månad
            RecurrencePeriodMonths = 2
        };

        // Act
        var monthlyAmount = budgetCategory.MonthlyAmount;

        // Assert
        Assert.Equal(650m, monthlyAmount); // 1300 / 2 = 650 kr/månad
    }

    [Fact]
    public void MonthlyAmount_WithQuarterlyPeriod_ReturnsThirdOfPlannedAmount()
    {
        // Arrange
        var budgetCategory = new BudgetCategory
        {
            PlannedAmount = 900m,
            RecurrencePeriodMonths = 3
        };

        // Act
        var monthlyAmount = budgetCategory.MonthlyAmount;

        // Assert
        Assert.Equal(300m, monthlyAmount); // 900 / 3 = 300 kr/månad
    }

    [Fact]
    public void MonthlyAmount_WithSemiAnnualPeriod_ReturnsSixthOfPlannedAmount()
    {
        // Arrange
        var budgetCategory = new BudgetCategory
        {
            PlannedAmount = 850m, // Example from issue: Danskurs 850 kr/halvår
            RecurrencePeriodMonths = 6
        };

        // Act
        var monthlyAmount = budgetCategory.MonthlyAmount;

        // Assert
        Assert.Equal(141.666666666666666666666666667m, monthlyAmount); // 850 / 6 ≈ 141.67 kr/månad
    }

    [Fact]
    public void MonthlyAmount_WithAnnualPeriod_ReturnsTwelfthOfPlannedAmount()
    {
        // Arrange
        var budgetCategory = new BudgetCategory
        {
            PlannedAmount = 2400m, // Example from issue: Fritidsaktivitet 2 400 kr/år
            RecurrencePeriodMonths = 12
        };

        // Act
        var monthlyAmount = budgetCategory.MonthlyAmount;

        // Assert
        Assert.Equal(200m, monthlyAmount); // 2400 / 12 = 200 kr/månad
    }

    [Fact]
    public void MonthlyAmount_WithAnnualGymMembership_Returns150PerMonth()
    {
        // Arrange - Example from issue: Gymkort 1 800 kr/år
        var budgetCategory = new BudgetCategory
        {
            PlannedAmount = 1800m,
            RecurrencePeriodMonths = 12
        };

        // Act
        var monthlyAmount = budgetCategory.MonthlyAmount;

        // Assert
        Assert.Equal(150m, monthlyAmount); // 1800 / 12 = 150 kr/månad
    }

    [Fact]
    public void MonthlyAmount_WithZeroPeriod_ReturnsPlannedAmount()
    {
        // Arrange - Edge case: invalid period should default to planned amount
        var budgetCategory = new BudgetCategory
        {
            PlannedAmount = 1000m,
            RecurrencePeriodMonths = 0
        };

        // Act
        var monthlyAmount = budgetCategory.MonthlyAmount;

        // Assert
        Assert.Equal(1000m, monthlyAmount);
    }

    [Fact]
    public void RecurrencePeriodMonths_DefaultValue_IsOne()
    {
        // Arrange & Act
        var budgetCategory = new BudgetCategory();

        // Assert
        Assert.Equal(1, budgetCategory.RecurrencePeriodMonths);
    }
}
