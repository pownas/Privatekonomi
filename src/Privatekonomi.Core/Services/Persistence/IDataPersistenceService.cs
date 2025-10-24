using Privatekonomi.Core.Data;

namespace Privatekonomi.Core.Services.Persistence;

/// <summary>
/// Interface for database persistence services
/// </summary>
public interface IDataPersistenceService
{
    /// <summary>
    /// Saves the current state of the database to persistent storage
    /// </summary>
    Task SaveAsync(PrivatekonomyContext context);
    
    /// <summary>
    /// Loads data from persistent storage into the database
    /// </summary>
    Task LoadAsync(PrivatekonomyContext context);
    
    /// <summary>
    /// Checks if persistent storage exists
    /// </summary>
    bool Exists();
}
