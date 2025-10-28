namespace Privatekonomi.Core.Models;

/// <summary>
/// Interface for entities that support temporal tracking (bi-temporal data).
/// Enables viewing historical data by filtering on ValidFrom and ValidTo dates.
/// </summary>
public interface ITemporalEntity
{
    /// <summary>
    /// The date and time when this version of the record became valid.
    /// </summary>
    DateTime ValidFrom { get; set; }
    
    /// <summary>
    /// The date and time when this version of the record became invalid (was superseded).
    /// Null indicates the current/active version.
    /// </summary>
    DateTime? ValidTo { get; set; }
}
