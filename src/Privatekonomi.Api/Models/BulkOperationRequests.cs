using System.ComponentModel.DataAnnotations;

namespace Privatekonomi.Api.Models;

/// <summary>
/// Request model for bulk delete operation
/// </summary>
public class BulkDeleteRequest
{
    [Required(ErrorMessage = "Transaction IDs are required")]
    [MinLength(1, ErrorMessage = "At least one transaction ID is required")]
    public List<int> TransactionIds { get; set; } = new();
}

/// <summary>
/// Request model for bulk categorize operation
/// </summary>
public class BulkCategorizeRequest
{
    [Required(ErrorMessage = "Transaction IDs are required")]
    [MinLength(1, ErrorMessage = "At least one transaction ID is required")]
    public List<int> TransactionIds { get; set; } = new();

    [Required(ErrorMessage = "Categories are required")]
    [MinLength(1, ErrorMessage = "At least one category is required")]
    public List<BulkCategoryDto> Categories { get; set; } = new();
}

/// <summary>
/// Request model for bulk link to household operation
/// </summary>
public class BulkLinkHouseholdRequest
{
    [Required(ErrorMessage = "Transaction IDs are required")]
    [MinLength(1, ErrorMessage = "At least one transaction ID is required")]
    public List<int> TransactionIds { get; set; } = new();

    /// <summary>
    /// Household ID to link to, or null to unlink from household
    /// </summary>
    public int? HouseholdId { get; set; }
}

/// <summary>
/// Request model for bulk export operation
/// </summary>
public class BulkExportRequest
{
    [Required(ErrorMessage = "Transaction IDs are required")]
    [MinLength(1, ErrorMessage = "At least one transaction ID is required")]
    public List<int> TransactionIds { get; set; } = new();

    [Required(ErrorMessage = "Format is required")]
    public ExportFormat Format { get; set; }
}

/// <summary>
/// DTO for category in bulk operations
/// </summary>
public class BulkCategoryDto
{
    [Required]
    public int CategoryId { get; set; }

    /// <summary>
    /// Amount for this category. If null, the full transaction amount will be used.
    /// </summary>
    public decimal? Amount { get; set; }
}

/// <summary>
/// Export format options
/// </summary>
public enum ExportFormat
{
    CSV,
    JSON
}
