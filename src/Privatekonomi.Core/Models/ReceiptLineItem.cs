using System.ComponentModel.DataAnnotations;

namespace Privatekonomi.Core.Models;

/// <summary>
/// Represents an individual line item on a receipt
/// </summary>
public class ReceiptLineItem
{
    public int ReceiptLineItemId { get; set; }
    
    public int ReceiptId { get; set; }
    public Receipt? Receipt { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Description { get; set; } = string.Empty;
    
    public decimal Quantity { get; set; } = 1;
    
    [Required]
    public decimal UnitPrice { get; set; }
    
    [Required]
    public decimal TotalPrice { get; set; }
    
    /// <summary>
    /// VAT/Tax percentage
    /// </summary>
    public decimal? TaxRate { get; set; }
    
    /// <summary>
    /// Category for this line item
    /// </summary>
    public int? CategoryId { get; set; }
    public Category? Category { get; set; }
    
    [MaxLength(500)]
    public string? Notes { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
