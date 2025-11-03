using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using Privatekonomi.Core.Data;
using Privatekonomi.Core.Models;
using Xunit;

namespace Privatekonomi.Core.Tests;

public class TestDataSeederTests : IDisposable
{
    private readonly PrivatekonomyContext _context;
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;

    public TestDataSeederTests()
    {
        var options = new DbContextOptionsBuilder<PrivatekonomyContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new PrivatekonomyContext(options);

        // Setup mock UserManager
        var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
        _mockUserManager = new Mock<UserManager<ApplicationUser>>(
            userStoreMock.Object, null, null, null, null, null, null, null, null);
        
        // Setup FindByEmailAsync to return null (user doesn't exist)
        _mockUserManager.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null);
        
        // Setup CreateAsync to succeed and set the user ID
        _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success)
            .Callback<ApplicationUser, string>((user, password) => 
            {
                user.Id = Guid.NewGuid().ToString();
            });
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task SeedTestDataAsync_GeneratesTransactionsWithin18MonthsDateRange()
    {
        // Arrange
        var today = DateTime.UtcNow;
        var expectedMinDate = today.AddDays(-550); // Approximately 18 months ago
        
        // Act
        await TestDataSeeder.SeedTestDataAsync(_context, _mockUserManager.Object);
        
        // Assert
        var transactions = await _context.Transactions.ToListAsync();
        
        // Should have generated more than 50 transactions (original was 50 for 3 months)
        Assert.True(transactions.Count >= 300, 
            $"Expected at least 300 transactions, but got {transactions.Count}");
        
        // Verify all transaction dates are within the expected range
        foreach (var transaction in transactions)
        {
            Assert.True(transaction.Date >= expectedMinDate, 
                $"Transaction date {transaction.Date:yyyy-MM-dd} is before expected minimum {expectedMinDate:yyyy-MM-dd}");
            Assert.True(transaction.Date <= today, 
                $"Transaction date {transaction.Date:yyyy-MM-dd} is after today {today:yyyy-MM-dd}");
        }
        
        // Verify we have transactions spread across the date range
        var oldestTransaction = transactions.Min(t => t.Date);
        var newestTransaction = transactions.Max(t => t.Date);
        var dateRangeDays = (newestTransaction - oldestTransaction).TotalDays;
        
        // We expect the range to be close to 550 days (with some margin for randomness)
        Assert.True(dateRangeDays >= 500, 
            $"Date range should be at least 500 days, but was {dateRangeDays:F0} days");
    }

    [Fact]
    public async Task SeedTestDataAsync_GeneratesCorrectNumberOfTransactions()
    {
        // Act
        await TestDataSeeder.SeedTestDataAsync(_context, _mockUserManager.Object);
        
        // Assert
        var transactions = await _context.Transactions.ToListAsync();
        
        // Should have 300 categorized transactions + 5 unmapped = 305 total
        Assert.Equal(305, transactions.Count);
    }

    [Fact]
    public async Task SeedTestDataAsync_TransactionDatesAreRelativeToCurrentDate()
    {
        // Arrange
        var testStartTime = DateTime.UtcNow;
        
        // Act
        await TestDataSeeder.SeedTestDataAsync(_context, _mockUserManager.Object);
        
        // Assert
        var transactions = await _context.Transactions.ToListAsync();
        var testEndTime = DateTime.UtcNow;
        
        // All transactions should have dates between approximately 18 months ago and now
        var expectedMinDate = testStartTime.AddDays(-550);
        var expectedMaxDate = testEndTime;
        
        foreach (var transaction in transactions)
        {
            Assert.True(transaction.Date >= expectedMinDate.AddDays(-1), // Small margin for test execution time
                $"Transaction date {transaction.Date:yyyy-MM-dd HH:mm:ss} should be after {expectedMinDate:yyyy-MM-dd HH:mm:ss}");
            Assert.True(transaction.Date <= expectedMaxDate.AddDays(1), // Small margin for test execution time
                $"Transaction date {transaction.Date:yyyy-MM-dd HH:mm:ss} should be before {expectedMaxDate:yyyy-MM-dd HH:mm:ss}");
        }
    }
}
