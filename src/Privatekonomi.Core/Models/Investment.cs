namespace Privatekonomi.Core.Models;

public class Investment
{
    public int InvestmentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // "Aktie" or "Fond"
    public decimal Quantity { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal CurrentPrice { get; set; }
    public DateTime PurchaseDate { get; set; }
    public DateTime LastUpdated { get; set; }
    
    public decimal TotalValue => Quantity * CurrentPrice;
    public decimal TotalCost => Quantity * PurchasePrice;
    public decimal ProfitLoss => TotalValue - TotalCost;
    public decimal ProfitLossPercentage => TotalCost > 0 ? (ProfitLoss / TotalCost) * 100 : 0;
}
