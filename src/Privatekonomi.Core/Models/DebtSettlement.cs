namespace Privatekonomi.Core.Models;

/// <summary>
/// Represents a debt or balance between two household members that needs settlement
/// </summary>
public class DebtSettlement
{
    public int DebtSettlementId { get; set; }
    public int HouseholdId { get; set; }
    public int DebtorMemberId { get; set; }
    public int CreditorMemberId { get; set; }
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public DebtSettlementStatus Status { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? SettledDate { get; set; }
    public string? SettlementNote { get; set; }
    
    public Household? Household { get; set; }
    public HouseholdMember? DebtorMember { get; set; }
    public HouseholdMember? CreditorMember { get; set; }
}

public enum DebtSettlementStatus
{
    Pending,    // Väntar på betalning
    Settled,    // Betald/avslutad
    Cancelled   // Avbruten
}
