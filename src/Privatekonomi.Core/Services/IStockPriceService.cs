using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services;

public interface IStockPriceService
{
    /// <summary>
    /// Updates the current price for a single investment
    /// </summary>
    /// <param name="investment">The investment to update</param>
    /// <returns>True if the price was successfully updated, false otherwise</returns>
    Task<bool> UpdatePriceAsync(Investment investment);
    
    /// <summary>
    /// Updates the current prices for multiple investments
    /// </summary>
    /// <param name="investments">The investments to update</param>
    /// <returns>A result containing the number of successful and failed updates</returns>
    Task<StockPriceUpdateResult> UpdatePricesAsync(IEnumerable<Investment> investments);
}

public class StockPriceUpdateResult
{
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
    public List<string> Errors { get; set; } = new();
}
