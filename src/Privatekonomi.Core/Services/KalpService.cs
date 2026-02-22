namespace Privatekonomi.Core.Services;

using Microsoft.EntityFrameworkCore;
using Privatekonomi.Core.Data;
using Privatekonomi.Core.Models;

/// <summary>
/// Service for KALP (Kvar att leva på) calculations
/// </summary>
public class KalpService : IKalpService
{
    private readonly PrivatekonomyContext _context;
    private readonly IKonsumentverketComparisonService _konsumentverketService;

    public KalpService(
        PrivatekonomyContext context,
        IKonsumentverketComparisonService konsumentverketService)
    {
        _context = context;
        _konsumentverketService = konsumentverketService;
    }

    /// <inheritdoc/>
    public KalpCalculation CalculateKalp(KalpInput input)
    {
        var calculation = new KalpCalculation
        {
            TotalIncome = input.MonthlyIncome,
            FixedExpenses = input.FixedExpenses.Values.Sum(),
            LoanPayments = input.Loans.Sum(l => l.MonthlyPayment),
            FixedExpenseBreakdown = new Dictionary<string, decimal>(input.FixedExpenses),
            LoanPaymentBreakdown = input.Loans.GroupBy(l => l.LoanType)
                .ToDictionary(g => g.Key, g => g.Sum(l => l.MonthlyPayment))
        };

        // Calculate recommended minimum if household members are provided
        if (input.HouseholdMembers != null && input.HouseholdMembers.Any())
        {
            calculation.RecommendedMinimumKalp = CalculateRecommendedMinimumKalp(input.HouseholdMembers);
        }

        return calculation;
    }

    /// <inheritdoc/>
    public async Task<KalpBudgetComparison> CalculateKalpFromBudgetAsync(string userId, int budgetId)
    {
        // Get budget with categories
        var budget = await _context.Budgets
            .Include(b => b.BudgetCategories)
            .ThenInclude(bc => bc.Category)
            .FirstOrDefaultAsync(b => b.BudgetId == budgetId && b.UserId == userId);

        if (budget == null)
        {
            throw new InvalidOperationException($"Budget with ID {budgetId} not found for user {userId}");
        }

        // Get user's loans
        var loans = await _context.Loans
            .Where(l => l.UserId == userId && (l.MaturityDate == null || l.MaturityDate > DateTime.Now))
            .ToListAsync();

        // Get user's income from transactions (last month's income)
        var lastMonth = DateTime.Now.AddMonths(-1);
        var startOfLastMonth = new DateTime(lastMonth.Year, lastMonth.Month, 1);
        var endOfLastMonth = startOfLastMonth.AddMonths(1).AddDays(-1);

        var monthlyIncome = await _context.Transactions
            .Where(t => t.UserId == userId && 
                       t.Date >= startOfLastMonth && 
                       t.Date <= endOfLastMonth && 
                       t.IsIncome)
            .SumAsync(t => t.Amount);

        // If no income found in last month, use a default or calculate from budget period
        if (monthlyIncome == 0 && budget.Period == BudgetPeriod.Monthly)
        {
            // Try to find income in budget period
            monthlyIncome = await _context.Transactions
                .Where(t => t.UserId == userId && 
                           t.Date >= budget.StartDate && 
                           t.Date <= budget.EndDate && 
                           t.IsIncome)
                .SumAsync(t => t.Amount);
        }

        // Separate fixed and variable expenses from budget
        var fixedExpenseCategories = new[] { "Boende", "Försäkringar", "Abonnemang", "Transport", "Elkostnader" };
        var fixedExpenses = budget.BudgetCategories
            .Where(bc => fixedExpenseCategories.Contains(bc.Category.Name))
            .ToDictionary(bc => bc.Category.Name, bc => bc.PlannedAmount);

        var variableExpenses = budget.BudgetCategories
            .Where(bc => !fixedExpenseCategories.Contains(bc.Category.Name))
            .Sum(bc => bc.PlannedAmount);

        // Create KALP input
        var kalpInput = new KalpInput
        {
            MonthlyIncome = monthlyIncome,
            FixedExpenses = fixedExpenses,
            Loans = loans.Select(l => new LoanPayment
            {
                LoanName = l.Name,
                LoanType = l.Type,
                MonthlyPayment = l.Amortization + (l.Amount * (l.InterestRate / 100 / 12))
            }).ToList()
        };

        var kalpCalculation = CalculateKalp(kalpInput);

        return new KalpBudgetComparison
        {
            KalpCalculation = kalpCalculation,
            Budget = budget,
            BudgetedVariableExpenses = variableExpenses
        };
    }

    /// <inheritdoc/>
    public decimal CalculateRecommendedMinimumKalp(List<KonsumentverketHouseholdMember> householdMembers)
    {
        if (!householdMembers.Any())
        {
            return 0;
        }

        // Get Konsumentverket reference costs for variable expenses
        var referenceResult = _konsumentverketService.CalculateReferenceCosts(householdMembers);

        // KALP should at minimum cover:
        // - Food costs (essential)
        // - Individual costs (clothes, hygiene, leisure - essential)
        // We don't include common costs as those are typically fixed expenses
        return referenceResult.ReferenceFoodCosts + referenceResult.ReferenceIndividualCosts;
    }

    /// <inheritdoc/>
    public KalpBudgetComparison CalculateKalpWithComparison(KalpInput input)
    {
        var kalpCalculation = CalculateKalp(input);

        var comparison = new KalpBudgetComparison
        {
            KalpCalculation = kalpCalculation,
            Budget = null,
            BudgetedVariableExpenses = 0
        };

        // Add Konsumentverket comparison if household members provided
        if (input.HouseholdMembers != null && input.HouseholdMembers.Any())
        {
            var userCosts = new UserHouseholdCosts
            {
                FoodCosts = input.FixedExpenses.TryGetValue("Mat", out var foodCosts) ? foodCosts : 0
            };

            comparison.KonsumentverketComparison = _konsumentverketService.CompareWithReference(
                input.HouseholdMembers, 
                userCosts);
        }

        return comparison;
    }
}
