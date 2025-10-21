namespace Privatekonomi.Core.Models;

/// <summary>
/// Represents a commute/travel deduction (Reseavdrag)
/// For work-related travel expenses in Sweden
/// </summary>
public class CommuteDeduction
{
    public int CommuteDeductionId { get; set; }
    
    /// <summary>
    /// Date of travel
    /// </summary>
    public DateTime Date { get; set; }
    
    /// <summary>
    /// Starting address (typically home)
    /// </summary>
    public string FromAddress { get; set; } = string.Empty;
    
    /// <summary>
    /// Destination address (typically workplace)
    /// </summary>
    public string ToAddress { get; set; } = string.Empty;
    
    /// <summary>
    /// Distance in kilometers (one way)
    /// </summary>
    public decimal DistanceKm { get; set; }
    
    /// <summary>
    /// Number of trips (typically 2 for round trip)
    /// </summary>
    public int NumberOfTrips { get; set; } = 2;
    
    /// <summary>
    /// Total distance for the day
    /// </summary>
    public decimal TotalDistanceKm => DistanceKm * NumberOfTrips;
    
    /// <summary>
    /// Transport method: "Car", "PublicTransport", "Bicycle", "Walking"
    /// </summary>
    public string TransportMethod { get; set; } = string.Empty;
    
    /// <summary>
    /// Actual cost for this trip (e.g., public transport ticket)
    /// </summary>
    public decimal Cost { get; set; }
    
    /// <summary>
    /// Calculated deductible amount based on Swedish tax rules
    /// </summary>
    public decimal DeductibleAmount { get; set; }
    
    /// <summary>
    /// Tax year this deduction applies to
    /// </summary>
    public int TaxYear { get; set; }
    
    /// <summary>
    /// Whether this is a regular commute (daily)
    /// </summary>
    public bool IsRegularCommute { get; set; }
    
    /// <summary>
    /// Notes about the trip
    /// </summary>
    public string? Notes { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
