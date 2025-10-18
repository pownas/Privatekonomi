namespace Privatekonomi.Core.Models;

public class ExpenseShare
{
    public int ExpenseShareId { get; set; }
    public int SharedExpenseId { get; set; }
    public int HouseholdMemberId { get; set; }
    public decimal ShareAmount { get; set; }
    public decimal? SharePercentage { get; set; }
    public decimal? RoomSize { get; set; }
    
    public SharedExpense? SharedExpense { get; set; }
    public HouseholdMember? HouseholdMember { get; set; }
}
