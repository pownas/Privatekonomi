namespace Privatekonomi.Core.Models;

public class SavingsChallengeProgress
{
    public int SavingsChallengeProgressId { get; set; }
    public int SavingsChallengeId { get; set; }
    public SavingsChallenge? SavingsChallenge { get; set; }
    public DateTime Date { get; set; }
    public bool Completed { get; set; }
    public decimal AmountSaved { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}
