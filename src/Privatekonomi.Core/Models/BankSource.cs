namespace Privatekonomi.Core.Models;

public class BankSource
{
    public int BankSourceId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#000000";
    public string? Logo { get; set; }
    
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
