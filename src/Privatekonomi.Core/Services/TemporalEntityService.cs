using Microsoft.EntityFrameworkCore;
using Privatekonomi.Core.Data;
using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services;

/// <summary>
/// Service for managing temporal entity updates.
/// Handles closing old versions and creating new versions of temporal entities.
/// </summary>
public class TemporalEntityService
{
    private readonly PrivatekonomyContext _context;

    public TemporalEntityService(PrivatekonomyContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Updates a temporal entity by closing the current version and creating a new one.
    /// This preserves the history of changes.
    /// </summary>
    /// <typeparam name="T">The type of temporal entity</typeparam>
    /// <param name="currentEntity">The current version of the entity to be closed</param>
    /// <param name="newEntity">The new version of the entity to be created</param>
    /// <param name="effectiveDate">The date when the change takes effect. Defaults to current UTC time.</param>
    /// <returns>The newly created entity</returns>
    public async Task<T> UpdateTemporalEntityAsync<T>(T currentEntity, T newEntity, DateTime? effectiveDate = null) 
        where T : class, ITemporalEntity
    {
        var changeDate = effectiveDate ?? DateTime.UtcNow;

        // Close the current version
        currentEntity.ValidTo = changeDate;
        _context.Update(currentEntity);

        // Set temporal properties on the new entity
        newEntity.ValidFrom = changeDate;
        newEntity.ValidTo = null; // Current/active version

        // Add the new version
        _context.Add(newEntity);
        
        await _context.SaveChangesAsync();

        return newEntity;
    }

    /// <summary>
    /// Creates a new temporal entity with ValidFrom set to now and ValidTo set to null.
    /// </summary>
    /// <typeparam name="T">The type of temporal entity</typeparam>
    /// <param name="entity">The entity to create</param>
    /// <param name="effectiveDate">The date when the entity becomes valid. Defaults to current UTC time.</param>
    /// <returns>The created entity</returns>
    public async Task<T> CreateTemporalEntityAsync<T>(T entity, DateTime? effectiveDate = null) 
        where T : class, ITemporalEntity
    {
        var startDate = effectiveDate ?? DateTime.UtcNow;

        entity.ValidFrom = startDate;
        entity.ValidTo = null; // Active version

        _context.Add(entity);
        await _context.SaveChangesAsync();

        return entity;
    }

    /// <summary>
    /// Soft deletes a temporal entity by setting its ValidTo to the current time.
    /// </summary>
    /// <typeparam name="T">The type of temporal entity</typeparam>
    /// <param name="entity">The entity to delete</param>
    /// <param name="effectiveDate">The date when the entity should be marked as deleted. Defaults to current UTC time.</param>
    public async Task DeleteTemporalEntityAsync<T>(T entity, DateTime? effectiveDate = null) 
        where T : class, ITemporalEntity
    {
        var endDate = effectiveDate ?? DateTime.UtcNow;

        entity.ValidTo = endDate;
        _context.Update(entity);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Gets the current/active version of an entity by its ID.
    /// </summary>
    /// <typeparam name="T">The type of temporal entity</typeparam>
    /// <param name="predicate">Predicate to find the entity (e.g., e => e.TransactionId == id)</param>
    /// <returns>The current version of the entity or null if not found</returns>
    public async Task<T?> GetCurrentVersionAsync<T>(System.Linq.Expressions.Expression<Func<T, bool>> predicate) 
        where T : class, ITemporalEntity
    {
        return await _context.Set<T>()
            .Where(predicate)
            .Where(e => e.ValidTo == null)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Gets the version of an entity that was valid at a specific point in time.
    /// </summary>
    /// <typeparam name="T">The type of temporal entity</typeparam>
    /// <param name="predicate">Predicate to find the entity</param>
    /// <param name="asOfDate">The date to query for</param>
    /// <returns>The version valid at the specified date or null if not found</returns>
    public async Task<T?> GetVersionAtDateAsync<T>(
        System.Linq.Expressions.Expression<Func<T, bool>> predicate, 
        DateTime asOfDate) 
        where T : class, ITemporalEntity
    {
        return await _context.Set<T>()
            .Where(predicate)
            .Where(e => e.ValidFrom <= asOfDate && (e.ValidTo == null || e.ValidTo > asOfDate))
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Gets all versions of an entity ordered by ValidFrom.
    /// </summary>
    /// <typeparam name="T">The type of temporal entity</typeparam>
    /// <param name="predicate">Predicate to find the entity</param>
    /// <returns>All versions of the entity ordered by ValidFrom</returns>
    public async Task<List<T>> GetAllVersionsAsync<T>(System.Linq.Expressions.Expression<Func<T, bool>> predicate) 
        where T : class, ITemporalEntity
    {
        return await _context.Set<T>()
            .Where(predicate)
            .OrderBy(e => e.ValidFrom)
            .ToListAsync();
    }
}
