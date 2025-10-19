namespace Privatekonomi.Core.Models;

public class Loan
{
    public int LoanId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // Bolån, CSN-lån, Privatlån, Kreditkort
    public decimal Amount { get; set; }
    public decimal InterestRate { get; set; } // Ränta i procent
    public decimal Amortization { get; set; } // Amortering per månad
    public string Currency { get; set; } = "SEK";
    public DateTime? StartDate { get; set; }
    public DateTime? MaturityDate { get; set; } // When loan is expected to be fully paid
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
