using Microsoft.EntityFrameworkCore;
using Privatekonomi.Core.Data;
using Privatekonomi.Core.Models;
using Privatekonomi.Core.Services;
using Xunit;

namespace Privatekonomi.Core.Tests;

public class BankSourceServiceTests : IDisposable
{
    private readonly PrivatekonomyContext _context;
    private readonly BankSourceService _bankSourceService;
    private readonly string _testUserId = "test-user-123";

    public BankSourceServiceTests()
    {
        var options = new DbContextOptionsBuilder<PrivatekonomyContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new PrivatekonomyContext(options);
        _bankSourceService = new BankSourceService(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task CreateBankSourceAsync_CreatesSuccessfully()
    {
        // Arrange
        var bankSource = new BankSource
        {
            Name = "Test Checking Account",
            AccountType = "checking",
            Currency = "SEK",
            CreatedAt = DateTime.UtcNow,
            UserId = _testUserId
        };

        // Act
        var result = await _bankSourceService.CreateBankSourceAsync(bankSource);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Checking Account", result.Name);
        Assert.NotEqual(0, result.BankSourceId); // Should have an ID assigned
    }

    [Fact]
    public async Task CreateBankSourceAsync_WithAccountNumber_StoresCorrectly()
    {
        // Arrange
        var bankSource = new BankSource
        {
            Name = "Swedbank Lönekonto",
            AccountType = "checking",
            Currency = "SEK",
            ClearingNumber = "8327",
            AccountNumber = "123456789",
            UserId = _testUserId
        };

        // Act
        var result = await _bankSourceService.CreateBankSourceAsync(bankSource);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("8327", result.ClearingNumber);
        Assert.Equal("123456789", result.AccountNumber);
    }

    [Fact]
    public async Task CreateBankSourceAsync_WithChartOfAccounts_StoresCorrectly()
    {
        // Arrange
        var bankSource = new BankSource
        {
            Name = "Business Account",
            AccountType = "checking",
            Currency = "SEK",
            ChartOfAccountsCode = "1930",
            UserId = _testUserId
        };

        // Act
        var result = await _bankSourceService.CreateBankSourceAsync(bankSource);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("1930", result.ChartOfAccountsCode);
    }

    [Fact]
    public async Task CreateBankSourceAsync_CreditCardAccount_StoresCorrectType()
    {
        // Arrange
        var bankSource = new BankSource
        {
            Name = "Nordea MasterCard",
            AccountType = "credit_card",
            Currency = "SEK",
            Institution = "Nordea",
            UserId = _testUserId
        };

        // Act
        var result = await _bankSourceService.CreateBankSourceAsync(bankSource);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("credit_card", result.AccountType);
        Assert.Equal("Nordea", result.Institution);
    }

    [Fact]
    public async Task CreateBankSourceAsync_PensionAccount_StoresCorrectType()
    {
        // Arrange
        var bankSource = new BankSource
        {
            Name = "SEB Pensionssparande",
            AccountType = "pension",
            Currency = "SEK",
            UserId = _testUserId
        };

        // Act
        var result = await _bankSourceService.CreateBankSourceAsync(bankSource);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("pension", result.AccountType);
    }

    [Fact]
    public async Task CreateBankSourceAsync_LoanAccount_StoresCorrectType()
    {
        // Arrange
        var bankSource = new BankSource
        {
            Name = "Bolån Swedbank",
            AccountType = "loan",
            Currency = "SEK",
            UserId = _testUserId
        };

        // Act
        var result = await _bankSourceService.CreateBankSourceAsync(bankSource);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("loan", result.AccountType);
    }

    [Fact]
    public async Task GetAllBankSourcesAsync_ReturnsAllAccounts()
    {
        // Arrange
        var bankSources = new[]
        {
            new BankSource { Name = "Checking", AccountType = "checking", Currency = "SEK", UserId = _testUserId },
            new BankSource { Name = "Savings", AccountType = "savings", Currency = "SEK", UserId = _testUserId },
            new BankSource { Name = "Credit Card", AccountType = "credit_card", Currency = "SEK", UserId = _testUserId }
        };

        foreach (var bs in bankSources)
        {
            await _bankSourceService.CreateBankSourceAsync(bs);
        }

        // Act
        var result = await _bankSourceService.GetAllBankSourcesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count());
    }

    [Fact]
    public async Task GetBankSourceByIdAsync_ReturnsCorrectAccount()
    {
        // Arrange
        var bankSource = new BankSource
        {
            Name = "Test Account",
            AccountType = "checking",
            Currency = "SEK",
            AccountNumber = "987654321",
            UserId = _testUserId
        };

        var created = await _bankSourceService.CreateBankSourceAsync(bankSource);

        // Act
        var result = await _bankSourceService.GetBankSourceByIdAsync(created.BankSourceId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Account", result.Name);
        Assert.Equal("987654321", result.AccountNumber);
    }

    [Fact]
    public async Task UpdateBankSourceAsync_UpdatesAccountDetails()
    {
        // Arrange
        var bankSource = new BankSource
        {
            Name = "Original Name",
            AccountType = "checking",
            Currency = "SEK",
            UserId = _testUserId
        };

        var created = await _bankSourceService.CreateBankSourceAsync(bankSource);

        // Modify the account
        created.Name = "Updated Name";
        created.AccountNumber = "111222333";
        created.ChartOfAccountsCode = "1920";

        // Act
        var result = await _bankSourceService.UpdateBankSourceAsync(created);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Name", result.Name);
        Assert.Equal("111222333", result.AccountNumber);
        Assert.Equal("1920", result.ChartOfAccountsCode);
    }

    [Fact]
    public async Task DeleteBankSourceAsync_RemovesAccount()
    {
        // Arrange
        var bankSource = new BankSource
        {
            Name = "To Be Deleted",
            AccountType = "checking",
            Currency = "SEK",
            UserId = _testUserId
        };

        var created = await _bankSourceService.CreateBankSourceAsync(bankSource);

        // Act
        await _bankSourceService.DeleteBankSourceAsync(created.BankSourceId);

        // Assert
        var deleted = await _bankSourceService.GetBankSourceByIdAsync(created.BankSourceId);
        Assert.Null(deleted);
    }

    [Fact]
    public async Task CreateBankSourceAsync_WithOpenedDate_StoresCorrectly()
    {
        // Arrange
        var openedDate = new DateTime(2023, 1, 15);
        var bankSource = new BankSource
        {
            Name = "New Account",
            AccountType = "savings",
            Currency = "SEK",
            OpenedDate = openedDate,
            InitialBalance = 10000,
            UserId = _testUserId
        };

        // Act
        var result = await _bankSourceService.CreateBankSourceAsync(bankSource);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(openedDate, result.OpenedDate);
        Assert.Equal(10000, result.InitialBalance);
    }

    [Fact]
    public async Task UpdateBankSourceAsync_CanCloseAccount()
    {
        // Arrange
        var bankSource = new BankSource
        {
            Name = "Active Account",
            AccountType = "checking",
            Currency = "SEK",
            UserId = _testUserId
        };

        var created = await _bankSourceService.CreateBankSourceAsync(bankSource);
        
        // Close the account
        created.ClosedDate = DateTime.UtcNow;

        // Act
        var result = await _bankSourceService.UpdateBankSourceAsync(created);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.ClosedDate);
        Assert.True(result.ClosedDate <= DateTime.UtcNow);
    }
}
