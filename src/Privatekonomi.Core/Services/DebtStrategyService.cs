using Microsoft.EntityFrameworkCore;
using Privatekonomi.Core.Data;
using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services;

public class DebtStrategyService : IDebtStrategyService
{
    private readonly PrivatekonomyContext _context;

    public DebtStrategyService(PrivatekonomyContext context)
    {
        _context = context;
    }

    public List<AmortizationScheduleEntry> GenerateAmortizationSchedule(Loan loan, decimal extraMonthlyPayment = 0)
    {
        var schedule = new List<AmortizationScheduleEntry>();
        
        if (loan.Amount <= 0)
            return schedule;

        var balance = loan.Amount;
        var monthlyRate = loan.InterestRate / 100 / 12;
        var regularPayment = loan.Amortization + (loan.Amount * monthlyRate);
        var totalPayment = regularPayment + extraMonthlyPayment;
        var currentDate = loan.StartDate ?? DateTime.Today;
        var paymentNumber = 1;
        var totalInterest = 0m;

        while (balance > 0 && paymentNumber <= 600) // Max 50 years
        {
            var interestPayment = balance * monthlyRate;
            var principalPayment = Math.Min(totalPayment - interestPayment, balance);
            
            // Ensure we don't overpay
            if (principalPayment + interestPayment > balance + interestPayment)
            {
                principalPayment = balance;
            }

            var actualPayment = principalPayment + interestPayment;
            balance -= principalPayment;
            totalInterest += interestPayment;

            schedule.Add(new AmortizationScheduleEntry
            {
                PaymentNumber = paymentNumber,
                PaymentDate = currentDate.AddMonths(paymentNumber - 1),
                BeginningBalance = balance + principalPayment,
                Payment = actualPayment,
                Principal = principalPayment,
                Interest = interestPayment,
                ExtraPayment = extraMonthlyPayment,
                EndingBalance = balance,
                TotalInterestPaid = totalInterest
            });

            paymentNumber++;
        }

        return schedule;
    }

    public async Task<DebtPayoffStrategy> CalculateSnowballStrategy(decimal availableMonthlyPayment)
    {
        var loans = await _context.Loans.OrderBy(l => l.Amount).ToListAsync();
        return await CalculatePayoffStrategy(loans, availableMonthlyPayment, "Snöboll", 
            "Betala av minsta skulden först för psykologiska vinster och momentum");
    }

    public async Task<DebtPayoffStrategy> CalculateAvalancheStrategy(decimal availableMonthlyPayment)
    {
        var loans = await _context.Loans.OrderByDescending(l => l.InterestRate).ToListAsync();
        return await CalculatePayoffStrategy(loans, availableMonthlyPayment, "Lavin", 
            "Betala av skulden med högst ränta först för att minimera totala räntekostnader");
    }

    public ExtraPaymentAnalysis AnalyzeExtraPayment(Loan loan, decimal extraMonthlyPayment)
    {
        var originalSchedule = GenerateAmortizationSchedule(loan, 0);
        var newSchedule = GenerateAmortizationSchedule(loan, extraMonthlyPayment);

        return new ExtraPaymentAnalysis
        {
            ExtraMonthlyPayment = extraMonthlyPayment,
            OriginalPayoffDate = originalSchedule.LastOrDefault()?.PaymentDate ?? DateTime.Today,
            NewPayoffDate = newSchedule.LastOrDefault()?.PaymentDate ?? DateTime.Today,
            MonthsSaved = originalSchedule.Count - newSchedule.Count,
            OriginalTotalInterest = originalSchedule.LastOrDefault()?.TotalInterestPaid ?? 0,
            NewTotalInterest = newSchedule.LastOrDefault()?.TotalInterestPaid ?? 0,
            InterestSavings = (originalSchedule.LastOrDefault()?.TotalInterestPaid ?? 0) - 
                            (newSchedule.LastOrDefault()?.TotalInterestPaid ?? 0),
            TotalExtraPayments = extraMonthlyPayment * newSchedule.Count,
            NetSavings = ((originalSchedule.LastOrDefault()?.TotalInterestPaid ?? 0) - 
                         (newSchedule.LastOrDefault()?.TotalInterestPaid ?? 0)) - 
                         (extraMonthlyPayment * newSchedule.Count)
        };
    }

    public async Task<DateTime?> CalculateDebtFreeDate()
    {
        var loans = await _context.Loans.ToListAsync();
        
        if (!loans.Any())
            return null;

        var latestPayoffDate = DateTime.Today;

        foreach (var loan in loans)
        {
            var schedule = GenerateAmortizationSchedule(loan, loan.ExtraMonthlyPayment ?? 0);
            var payoffDate = schedule.LastOrDefault()?.PaymentDate;
            
            if (payoffDate.HasValue && payoffDate.Value > latestPayoffDate)
            {
                latestPayoffDate = payoffDate.Value;
            }
        }

        return latestPayoffDate;
    }

    public async Task<(DebtPayoffStrategy Snowball, DebtPayoffStrategy Avalanche)> CompareStrategies(decimal availableMonthlyPayment)
    {
        var snowball = await CalculateSnowballStrategy(availableMonthlyPayment);
        var avalanche = await CalculateAvalancheStrategy(availableMonthlyPayment);
        
        return (snowball, avalanche);
    }

    private async Task<DebtPayoffStrategy> CalculatePayoffStrategy(List<Loan> loans, decimal availableMonthlyPayment, 
        string strategyName, string description)
    {
        var strategy = new DebtPayoffStrategy
        {
            StrategyName = strategyName,
            Description = description,
            PayoffPlans = new List<DebtPayoffPlan>()
        };

        if (!loans.Any())
            return strategy;

        // Clone loans to simulate payoff
        var remainingLoans = loans.Select(l => new LoanSimulation
        {
            Loan = l,
            Balance = l.Amount,
            MinPayment = l.Amortization
        }).ToList();

        var currentDate = DateTime.Today;
        var totalInterest = 0m;
        var payoffOrder = 1;
        var availableExtra = availableMonthlyPayment - remainingLoans.Sum(l => l.MinPayment);

        if (availableExtra < 0)
        {
            // Not enough money to cover minimum payments
            strategy.DebtFreeDate = DateTime.MaxValue;
            return strategy;
        }

        while (remainingLoans.Any())
        {
            var targetLoan = remainingLoans.First();
            var monthlyRate = targetLoan.Loan.InterestRate / 100 / 12;

            // Apply minimum payments to all loans
            foreach (var loan in remainingLoans)
            {
                var interest = loan.Balance * (loan.Loan.InterestRate / 100 / 12);
                var principal = Math.Min(loan.MinPayment, loan.Balance);
                loan.Balance -= principal;
                totalInterest += interest;
            }

            // Apply extra payment to target loan
            if (availableExtra > 0 && targetLoan.Balance > 0)
            {
                var interest = targetLoan.Balance * monthlyRate;
                var principal = Math.Min(availableExtra, targetLoan.Balance);
                targetLoan.Balance -= principal;
                totalInterest += interest;
            }

            // Check if target loan is paid off
            if (targetLoan.Balance <= 0.01m) // Small tolerance for rounding
            {
                strategy.PayoffPlans.Add(new DebtPayoffPlan
                {
                    LoanId = targetLoan.Loan.LoanId,
                    LoanName = targetLoan.Loan.Name,
                    PayoffOrder = payoffOrder++,
                    PayoffDate = currentDate,
                    TotalInterestPaid = totalInterest,
                    MonthsToPayoff = (currentDate.Year - DateTime.Today.Year) * 12 + 
                                    (currentDate.Month - DateTime.Today.Month)
                });

                availableExtra += targetLoan.MinPayment; // Add freed up money to extra
                remainingLoans.Remove(targetLoan);
            }

            currentDate = currentDate.AddMonths(1);

            // Safety check to prevent infinite loop
            if (currentDate > DateTime.Today.AddYears(50))
            {
                break;
            }
        }

        strategy.DebtFreeDate = currentDate;
        strategy.TotalInterestPaid = totalInterest;
        strategy.TotalAmountPaid = loans.Sum(l => l.Amount) + totalInterest;
        strategy.TotalMonths = (currentDate.Year - DateTime.Today.Year) * 12 + 
                              (currentDate.Month - DateTime.Today.Month);

        return strategy;
    }

    private class LoanSimulation
    {
        public Loan Loan { get; set; } = null!;
        public decimal Balance { get; set; }
        public decimal MinPayment { get; set; }
    }
}
