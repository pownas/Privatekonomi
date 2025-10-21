namespace Privatekonomi.Core.Models;

/// <summary>
/// Represents a tax deduction for ROT/RUT work
/// ROT = Reparation, Ombyggnad, Tillbyggnad (renovation/construction)
/// RUT = Rengöring, Underhåll, Tvätt (cleaning/maintenance)
/// </summary>
public class TaxDeduction
{
    public int TaxDeductionId { get; set; }
    
    /// <summary>
    /// Link to the transaction this deduction relates to
    /// </summary>
    public int TransactionId { get; set; }
    
    /// <summary>
    /// Type of deduction: "ROT" or "RUT"
    /// </summary>
    public string Type { get; set; } = string.Empty;
    
    /// <summary>
    /// Total amount paid
    /// </summary>
    public decimal Amount { get; set; }
    
    /// <summary>
    /// Amount that is deductible (typically 50% of labor cost for ROT/RUT)
    /// </summary>
    public decimal DeductibleAmount { get; set; }
    
    /// <summary>
    /// Name of service provider
    /// </summary>
    public string ServiceProvider { get; set; } = string.Empty;
    
    /// <summary>
    /// Organization number of service provider (required for ROT/RUT)
    /// </summary>
    public string OrganizationNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Tax year this deduction applies to
    /// </summary>
    public int TaxYear { get; set; }
    
    /// <summary>
    /// Whether the deduction has been approved by Skatteverket
    /// </summary>
    public bool Approved { get; set; }
    
    /// <summary>
    /// Description of the work performed
    /// </summary>
    public string? WorkDescription { get; set; }
    
    /// <summary>
    /// Date when the work was performed
    /// </summary>
    public DateTime WorkDate { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation property
    public Transaction? Transaction { get; set; }
}
