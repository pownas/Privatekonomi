using System.ComponentModel.DataAnnotations;

namespace Privatekonomi.Core.Models;

/// <summary>
/// Represents an image attached to a household activity
/// </summary>
public class HouseholdActivityImage
{
    public int HouseholdActivityImageId { get; set; }
    
    [Required]
    public int HouseholdActivityId { get; set; }
    public HouseholdActivity? HouseholdActivity { get; set; }
    
    /// <summary>
    /// Path to the uploaded image file
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string ImagePath { get; set; } = string.Empty;
    
    /// <summary>
    /// Optional caption or description for the image
    /// </summary>
    [MaxLength(200)]
    public string? Caption { get; set; }
    
    /// <summary>
    /// Display order of the image (for sorting)
    /// </summary>
    public int DisplayOrder { get; set; }
    
    /// <summary>
    /// File size in bytes
    /// </summary>
    public long FileSize { get; set; }
    
    /// <summary>
    /// MIME type of the image (e.g., image/jpeg, image/png)
    /// </summary>
    [MaxLength(50)]
    public string MimeType { get; set; } = string.Empty;
    
    public DateTime UploadedDate { get; set; } = DateTime.UtcNow;
}
