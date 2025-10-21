namespace Privatekonomi.Core.Models;

public class Loan
{
    public int LoanId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // Bolån, CSN-lån, Privatlån, Kreditkort, BNPL/Avbetalning
    public decimal Amount { get; set; }
    public decimal InterestRate { get; set; } // Ränta i procent
    public decimal Amortization { get; set; } // Amortering per månad
    public string Currency { get; set; } = "SEK";
    public DateTime? StartDate { get; set; }
    public DateTime? MaturityDate { get; set; } // When loan is expected to be fully paid
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Credit card specific fields
    public decimal? CreditLimit { get; set; } // Kreditgräns för kreditkort
    public decimal? MinimumPayment { get; set; } // Minimum månadsinbetalning
    
    // BNPL/Installment specific fields
    public int? InstallmentMonths { get; set; } // Antal månader för avbetalning
    public decimal? InstallmentFee { get; set; } // Fast avgift för avbetalning
    
    // Extra payment tracking
    public decimal? ExtraMonthlyPayment { get; set; } // Extra amortering per månad
    
    // Priority for debt payoff strategies
    public int Priority { get; set; } = 0; // 0 = ingen prioritet, högre nummer = högre prioritet
    
    // Computed properties (not stored in DB)
    public decimal CurrentBalance => Amount; // För framtida implementering av faktisk saldo
    public decimal? UtilizationRate => CreditLimit.HasValue && CreditLimit.Value > 0 
        ? (Amount / CreditLimit.Value) * 100 
        : null;
}
