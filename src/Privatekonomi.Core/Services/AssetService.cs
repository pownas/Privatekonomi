using Microsoft.EntityFrameworkCore;
using Privatekonomi.Core.Data;
using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services;

public class AssetService : IAssetService
{
    private readonly PrivatekonomyContext _context;

    public AssetService(PrivatekonomyContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Asset>> GetAllAssetsAsync()
    {
        return await _context.Assets
            .OrderByDescending(a => a.CurrentValue)
            .ToListAsync();
    }

    public async Task<Asset?> GetAssetByIdAsync(int id)
    {
        return await _context.Assets.FindAsync(id);
    }

    public async Task<Asset> CreateAssetAsync(Asset asset)
    {
        asset.CreatedAt = DateTime.UtcNow;
        _context.Assets.Add(asset);
        await _context.SaveChangesAsync();
        return asset;
    }

    public async Task UpdateAssetAsync(Asset asset)
    {
        asset.UpdatedAt = DateTime.UtcNow;
        _context.Assets.Update(asset);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAssetAsync(int id)
    {
        var asset = await _context.Assets.FindAsync(id);
        if (asset != null)
        {
            _context.Assets.Remove(asset);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Asset>> GetAssetsByTypeAsync(string type)
    {
        return await _context.Assets
            .Where(a => a.Type == type)
            .OrderByDescending(a => a.CurrentValue)
            .ToListAsync();
    }

    public async Task<decimal> GetTotalAssetValueAsync()
    {
        return await _context.Assets.SumAsync(a => a.CurrentValue);
    }
}
