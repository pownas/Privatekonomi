using Microsoft.EntityFrameworkCore;
using Privatekonomi.Core.Data;
using Privatekonomi.Core.Models;
using Privatekonomi.Core.Services;
using Xunit;

namespace Privatekonomi.Core.Tests;

public class ReportServiceTests : IDisposable
{
    private readonly PrivatekonomyContext _context;
    private readonly ReportService _reportService;
    private const string TestUserId = "test-user-123";

    public ReportServiceTests()
    {
        var options = new DbContextOptionsBuilder<PrivatekonomyContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new PrivatekonomyContext(options);
        _reportService = new ReportService(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task GetNetWorthReportAsync_WithNoData_ReturnsZeroNetWorth()
    {
        // Act
        var report = await _reportService.GetNetWorthReportAsync(TestUserId);

        // Assert
        Assert.NotNull(report);
        Assert.Equal(0, report.TotalAssets);
        Assert.Equal(0, report.TotalInvestments);
        Assert.Equal(0, report.TotalLiabilities);
        Assert.Equal(0, report.NetWorth);
        Assert.Empty(report.Assets);
        Assert.Empty(report.Liabilities);
    }

    [Fact]
    public async Task GetNetWorthReportAsync_WithAssets_CalculatesCorrectNetWorth()
    {
        // Arrange
        var asset = new Asset
        {
            UserId = TestUserId,
            Name = "Test Asset",
            Type = "Property",
            CurrentValue = 500000m,
            PurchaseValue = 400000m,
            CreatedAt = DateTime.UtcNow,
            ValidFrom = DateTime.UtcNow
        };
        _context.Assets.Add(asset);
        await _context.SaveChangesAsync();

        // Act
        var report = await _reportService.GetNetWorthReportAsync(TestUserId);

        // Assert
        Assert.Equal(500000m, report.TotalAssets);
        Assert.Equal(0, report.TotalLiabilities);
        Assert.Equal(500000m, report.NetWorth);
        Assert.Single(report.Assets);
        Assert.Equal("Test Asset", report.Assets[0].Name);
    }

    [Fact]
    public async Task GetNetWorthReportAsync_WithInvestments_IncludesInTotalAssets()
    {
        // Arrange
        var investment = new Investment
        {
            UserId = TestUserId,
            Name = "Test Stock",
            Type = "Aktie",
            Quantity = 100,
            PurchasePrice = 50m,
            CurrentPrice = 75m,
            PurchaseDate = DateTime.UtcNow.AddMonths(-6),
            LastUpdated = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            ValidFrom = DateTime.UtcNow
        };
        _context.Investments.Add(investment);
        await _context.SaveChangesAsync();

        // Act
        var report = await _reportService.GetNetWorthReportAsync(TestUserId);

        // Assert
        Assert.Equal(7500m, report.TotalAssets); // 100 * 75
        Assert.Equal(7500m, report.TotalInvestments);
        Assert.Equal(7500m, report.NetWorth);
        Assert.Single(report.Assets);
        Assert.Equal("Investment", report.Assets[0].Type);
    }

    [Fact]
    public async Task GetNetWorthReportAsync_WithLoans_SubtractsFromNetWorth()
    {
        // Arrange
        var asset = new Asset
        {
            UserId = TestUserId,
            Name = "Property",
            Type = "Fastighet",
            CurrentValue = 3000000m,
            PurchaseValue = 2500000m,
            CreatedAt = DateTime.UtcNow,
            ValidFrom = DateTime.UtcNow
        };
        var loan = new Loan
        {
            UserId = TestUserId,
            Name = "Mortgage",
            Type = "Bolån",
            Amount = 2000000m,
            InterestRate = 3.5m,
            Amortization = 5000m,
            CreatedAt = DateTime.UtcNow,
            ValidFrom = DateTime.UtcNow
        };
        _context.Assets.Add(asset);
        _context.Loans.Add(loan);
        await _context.SaveChangesAsync();

        // Act
        var report = await _reportService.GetNetWorthReportAsync(TestUserId);

        // Assert
        Assert.Equal(3000000m, report.TotalAssets);
        Assert.Equal(2000000m, report.TotalLiabilities);
        Assert.Equal(1000000m, report.NetWorth);
        Assert.Single(report.Liabilities);
        Assert.Equal("Mortgage", report.Liabilities[0].Name);
    }

    [Fact]
    public async Task GetNetWorthReportAsync_WithMultipleUsers_FiltersCorrectly()
    {
        // Arrange
        var userAsset = new Asset
        {
            UserId = TestUserId,
            Name = "User Asset",
            Type = "Property",
            CurrentValue = 100000m,
            PurchaseValue = 80000m,
            CreatedAt = DateTime.UtcNow,
            ValidFrom = DateTime.UtcNow
        };
        var otherAsset = new Asset
        {
            UserId = "other-user",
            Name = "Other Asset",
            Type = "Property",
            CurrentValue = 500000m,
            PurchaseValue = 400000m,
            CreatedAt = DateTime.UtcNow,
            ValidFrom = DateTime.UtcNow
        };
        _context.Assets.AddRange(userAsset, otherAsset);
        await _context.SaveChangesAsync();

        // Act
        var report = await _reportService.GetNetWorthReportAsync(TestUserId);

        // Assert
        Assert.Equal(100000m, report.TotalAssets);
        Assert.Single(report.Assets);
        Assert.Equal("User Asset", report.Assets[0].Name);
    }

    [Fact]
    public async Task GetNetWorthReportAsync_ReturnsHistoricalData()
    {
        // Arrange
        var asset = new Asset
        {
            UserId = TestUserId,
            Name = "Test Asset",
            Type = "Property",
            CurrentValue = 100000m,
            PurchaseValue = 80000m,
            CreatedAt = DateTime.UtcNow,
            ValidFrom = DateTime.UtcNow
        };
        _context.Assets.Add(asset);
        await _context.SaveChangesAsync();

        // Act
        var report = await _reportService.GetNetWorthReportAsync(TestUserId);

        // Assert
        Assert.NotNull(report.History);
        Assert.NotEmpty(report.History);
        // History should contain 12 months of data
        Assert.Equal(12, report.History.Count);
    }

    [Fact]
    public async Task GetNetWorthReportAsync_WithSnapshots_UsesSnapshotData()
    {
        // Arrange
        var snapshot = new NetWorthSnapshot
        {
            UserId = TestUserId,
            Date = DateTime.Today.AddMonths(-1),
            TotalAssets = 50000m,
            TotalLiabilities = 10000m,
            NetWorth = 40000m,
            CreatedAt = DateTime.UtcNow
        };
        _context.NetWorthSnapshots.Add(snapshot);
        await _context.SaveChangesAsync();

        // Act
        var report = await _reportService.GetNetWorthReportAsync(TestUserId);

        // Assert
        Assert.NotNull(report.History);
        Assert.NotEmpty(report.History);
    }

    [Fact]
    public async Task GetNetWorthReportAsync_CalculatesPercentageChange()
    {
        // Arrange - Create two snapshots for different months
        var oldSnapshot = new NetWorthSnapshot
        {
            UserId = TestUserId,
            Date = DateTime.Today.AddMonths(-2).AddDays(-DateTime.Today.Day + 1),
            TotalAssets = 100000m,
            TotalLiabilities = 20000m,
            NetWorth = 80000m,
            CreatedAt = DateTime.UtcNow
        };
        var recentSnapshot = new NetWorthSnapshot
        {
            UserId = TestUserId,
            Date = DateTime.Today.AddMonths(-1).AddDays(-DateTime.Today.Day + 1),
            TotalAssets = 120000m,
            TotalLiabilities = 20000m,
            NetWorth = 100000m,
            CreatedAt = DateTime.UtcNow
        };
        _context.NetWorthSnapshots.AddRange(oldSnapshot, recentSnapshot);
        
        // Add current assets
        var asset = new Asset
        {
            UserId = TestUserId,
            Name = "Current Asset",
            Type = "Property",
            CurrentValue = 120000m,
            PurchaseValue = 100000m,
            CreatedAt = DateTime.UtcNow,
            ValidFrom = DateTime.UtcNow
        };
        var loan = new Loan
        {
            UserId = TestUserId,
            Name = "Loan",
            Type = "Lån",
            Amount = 20000m,
            InterestRate = 5m,
            Amortization = 500m,
            CreatedAt = DateTime.UtcNow,
            ValidFrom = DateTime.UtcNow
        };
        _context.Assets.Add(asset);
        _context.Loans.Add(loan);
        await _context.SaveChangesAsync();

        // Act
        var report = await _reportService.GetNetWorthReportAsync(TestUserId);

        // Assert
        Assert.NotEqual(0, report.PercentageChange);
    }

    [Fact]
    public async Task GetNetWorthReportAsync_WithNullUserId_ReturnsAllUsersData()
    {
        // Arrange
        var asset1 = new Asset
        {
            UserId = "user1",
            Name = "Asset 1",
            Type = "Property",
            CurrentValue = 100000m,
            PurchaseValue = 80000m,
            CreatedAt = DateTime.UtcNow,
            ValidFrom = DateTime.UtcNow
        };
        var asset2 = new Asset
        {
            UserId = "user2",
            Name = "Asset 2",
            Type = "Property",
            CurrentValue = 200000m,
            PurchaseValue = 180000m,
            CreatedAt = DateTime.UtcNow,
            ValidFrom = DateTime.UtcNow
        };
        _context.Assets.AddRange(asset1, asset2);
        await _context.SaveChangesAsync();

        // Act
        var report = await _reportService.GetNetWorthReportAsync(null);

        // Assert
        Assert.Equal(300000m, report.TotalAssets); // Sum of both users
        Assert.Equal(2, report.Assets.Count);
    }
}
