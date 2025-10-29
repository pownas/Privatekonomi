using System.ComponentModel.DataAnnotations;

namespace Privatekonomi.Core.Models;

/// <summary>
/// Represents a receipt with OCR support and line items
/// </summary>
public class Receipt
{
    public int ReceiptId { get; set; }
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Merchant { get; set; } = string.Empty;
    
    [Required]
    public DateTime ReceiptDate { get; set; }
    
    [Required]
    public decimal TotalAmount { get; set; }
    
    [MaxLength(3)]
    public string Currency { get; set; } = "SEK";
    
    /// <summary>
    /// Type of receipt: Physical, E-Receipt, Scanned
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string ReceiptType { get; set; } = "Physical";
    
    /// <summary>
    /// Path to scanned/uploaded receipt image
    /// </summary>
    [MaxLength(500)]
    public string? ImagePath { get; set; }
    
    /// <summary>
    /// OCR extracted text from receipt
    /// </summary>
    public string? OcrText { get; set; }
    
    /// <summary>
    /// Receipt number from merchant
    /// </summary>
    [MaxLength(100)]
    public string? ReceiptNumber { get; set; }
    
    /// <summary>
    /// Payment method used
    /// </summary>
    [MaxLength(50)]
    public string? PaymentMethod { get; set; }
    
    [MaxLength(500)]
    public string? Notes { get; set; }
    
    /// <summary>
    /// Related transaction if linked
    /// </summary>
    public int? TransactionId { get; set; }
    public Transaction? Transaction { get; set; }
    
    /// <summary>
    /// Individual line items from receipt
    /// </summary>
    public List<ReceiptLineItem> ReceiptLineItems { get; set; } = new();
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
