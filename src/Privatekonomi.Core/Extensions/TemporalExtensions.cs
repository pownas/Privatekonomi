using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Extensions;

/// <summary>
/// Extension methods for querying temporal entities.
/// </summary>
public static class TemporalExtensions
{
    /// <summary>
    /// Filters temporal entities to return only those that were valid at the specified date.
    /// </summary>
    /// <typeparam name="T">The type of temporal entity</typeparam>
    /// <param name="query">The queryable collection</param>
    /// <param name="asOfDate">The date to query for. If null, returns current/active records.</param>
    /// <returns>Filtered queryable with only records valid at the specified date</returns>
    public static IQueryable<T> AsOf<T>(this IQueryable<T> query, DateTime? asOfDate = null) 
        where T : ITemporalEntity
    {
        if (!asOfDate.HasValue)
        {
            // Return only current/active records (ValidTo is null)
            return query.Where(e => e.ValidTo == null);
        }

        // Return records that were valid at the specified date
        return query.Where(e => e.ValidFrom <= asOfDate.Value && 
                               (e.ValidTo == null || e.ValidTo > asOfDate.Value));
    }

    /// <summary>
    /// Gets only the current/active version of temporal entities.
    /// </summary>
    /// <typeparam name="T">The type of temporal entity</typeparam>
    /// <param name="query">The queryable collection</param>
    /// <returns>Filtered queryable with only current records</returns>
    public static IQueryable<T> CurrentOnly<T>(this IQueryable<T> query) 
        where T : ITemporalEntity
    {
        return query.Where(e => e.ValidTo == null);
    }

    /// <summary>
    /// Checks if a temporal entity is currently active.
    /// </summary>
    /// <param name="entity">The temporal entity to check</param>
    /// <returns>True if the entity is currently active (ValidTo is null)</returns>
    public static bool IsActive(this ITemporalEntity entity)
    {
        return entity.ValidTo == null;
    }

    /// <summary>
    /// Checks if a temporal entity was active at a specific date.
    /// </summary>
    /// <param name="entity">The temporal entity to check</param>
    /// <param name="asOfDate">The date to check</param>
    /// <returns>True if the entity was active at the specified date</returns>
    public static bool IsActiveAt(this ITemporalEntity entity, DateTime asOfDate)
    {
        return entity.ValidFrom <= asOfDate && 
               (entity.ValidTo == null || entity.ValidTo > asOfDate);
    }
}
