namespace Privatekonomi.Core.Models;

public class SharedExpense
{
    public int SharedExpenseId { get; set; }
    public int HouseholdId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal TotalAmount { get; set; }
    public ExpenseType Type { get; set; }
    public DateTime ExpenseDate { get; set; }
    public DateTime CreatedDate { get; set; }
    public SplitMethod SplitMethod { get; set; }
    
    public Household? Household { get; set; }
    public ICollection<ExpenseShare> ExpenseShares { get; set; } = new List<ExpenseShare>();
}

public enum ExpenseType
{
    Rent,           // Hyra
    Electricity,    // El
    Internet,       // Bredband
    Water,          // Vatten
    Heating,        // Värme
    Insurance,      // Försäkring
    Other           // Övrigt
}

public enum SplitMethod
{
    Equal,          // Jämnt fördelat
    ByPercentage,   // Efter procent
    ByAmount,       // Efter specifikt belopp
    ByRoomSize      // Efter rumsyta
}
