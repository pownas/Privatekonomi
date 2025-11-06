using Microsoft.EntityFrameworkCore;
using Moq;
using Privatekonomi.Core.Data;
using Privatekonomi.Core.Models;
using Privatekonomi.Core.Services;
using Xunit;

namespace Privatekonomi.Core.Tests;

public class ReceiptServiceTests : IDisposable
{
    private readonly PrivatekonomyContext _context;
    private readonly ReceiptService _receiptService;
    private readonly Mock<IAuditLogService> _auditLogServiceMock;
    private const string TestUserId = "test-user-123";

    public ReceiptServiceTests()
    {
        var options = new DbContextOptionsBuilder<PrivatekonomyContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new PrivatekonomyContext(options);
        _auditLogServiceMock = new Mock<IAuditLogService>();
        _receiptService = new ReceiptService(_context, _auditLogServiceMock.Object);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task CreateReceiptAsync_WithLineItems_SavesSuccessfully()
    {
        // Arrange
        var receipt = new Receipt
        {
            UserId = TestUserId,
            Merchant = "Test Store",
            ReceiptDate = DateTime.Today,
            TotalAmount = 150.00m,
            Currency = "SEK",
            ReceiptType = "Physical",
            ReceiptLineItems = new List<ReceiptLineItem>
            {
                new ReceiptLineItem
                {
                    Description = "Product 1",
                    Quantity = 2,
                    UnitPrice = 50.00m,
                    TotalPrice = 100.00m
                },
                new ReceiptLineItem
                {
                    Description = "Product 2",
                    Quantity = 1,
                    UnitPrice = 50.00m,
                    TotalPrice = 50.00m
                }
            }
        };

        // Act
        var result = await _receiptService.CreateReceiptAsync(receipt);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.ReceiptId > 0);
        Assert.Equal("Test Store", result.Merchant);
        Assert.Equal(2, result.ReceiptLineItems.Count);
        Assert.Equal(150.00m, result.TotalAmount);
    }

    [Fact]
    public async Task CreateReceiptAsync_WithImagePath_SavesImagePath()
    {
        // Arrange
        var receipt = new Receipt
        {
            UserId = TestUserId,
            Merchant = "Photo Store",
            ReceiptDate = DateTime.Today,
            TotalAmount = 99.99m,
            Currency = "SEK",
            ReceiptType = "Scanned",
            ImagePath = "receipt-12345.jpg"
        };

        // Act
        var result = await _receiptService.CreateReceiptAsync(receipt);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("receipt-12345.jpg", result.ImagePath);
        Assert.Equal("Scanned", result.ReceiptType);
    }

    [Fact]
    public async Task GetReceiptByIdAsync_WithLineItemsAndCategories_LoadsAllData()
    {
        // Arrange
        var category = new Category { Name = "Groceries", Color = "#FF5733" };
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var receipt = new Receipt
        {
            UserId = TestUserId,
            Merchant = "Grocery Store",
            ReceiptDate = DateTime.Today,
            TotalAmount = 100.00m,
            Currency = "SEK",
            ReceiptType = "E-Receipt",
            ReceiptNumber = "RCP-001",
            PaymentMethod = "Credit Card",
            Notes = "Weekly shopping",
            ReceiptLineItems = new List<ReceiptLineItem>
            {
                new ReceiptLineItem
                {
                    Description = "Milk",
                    Quantity = 2,
                    UnitPrice = 25.00m,
                    TotalPrice = 50.00m,
                    CategoryId = category.CategoryId
                }
            }
        };
        
        var created = await _receiptService.CreateReceiptAsync(receipt);

        // Act
        var result = await _receiptService.GetReceiptByIdAsync(created.ReceiptId, TestUserId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Grocery Store", result.Merchant);
        Assert.Equal("RCP-001", result.ReceiptNumber);
        Assert.Equal("Credit Card", result.PaymentMethod);
        Assert.Equal("Weekly shopping", result.Notes);
        Assert.Single(result.ReceiptLineItems);
        Assert.NotNull(result.ReceiptLineItems[0].Category);
        Assert.Equal("Groceries", result.ReceiptLineItems[0].Category!.Name);
    }

    [Fact]
    public async Task UpdateReceiptAsync_UpdatesAllFields()
    {
        // Arrange
        var receipt = new Receipt
        {
            UserId = TestUserId,
            Merchant = "Old Store",
            ReceiptDate = DateTime.Today.AddDays(-1),
            TotalAmount = 50.00m,
            Currency = "SEK",
            ReceiptType = "Physical"
        };
        
        var created = await _receiptService.CreateReceiptAsync(receipt);

        // Act
        created.Merchant = "New Store";
        created.TotalAmount = 75.00m;
        created.PaymentMethod = "Cash";
        var result = await _receiptService.UpdateReceiptAsync(created);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New Store", result.Merchant);
        Assert.Equal(75.00m, result.TotalAmount);
        Assert.Equal("Cash", result.PaymentMethod);
        Assert.NotNull(result.UpdatedAt);
    }

    [Fact]
    public async Task GetReceiptsAsync_ReturnsOnlyUserReceipts()
    {
        // Arrange
        var receipt1 = new Receipt
        {
            UserId = TestUserId,
            Merchant = "Store A",
            ReceiptDate = DateTime.Today,
            TotalAmount = 100.00m,
            Currency = "SEK",
            ReceiptType = "Physical"
        };
        
        var receipt2 = new Receipt
        {
            UserId = "other-user",
            Merchant = "Store B",
            ReceiptDate = DateTime.Today,
            TotalAmount = 200.00m,
            Currency = "SEK",
            ReceiptType = "Physical"
        };

        await _receiptService.CreateReceiptAsync(receipt1);
        await _receiptService.CreateReceiptAsync(receipt2);

        // Act
        var results = await _receiptService.GetReceiptsAsync(TestUserId);

        // Assert
        Assert.Single(results);
        Assert.Equal("Store A", results[0].Merchant);
    }

    [Fact]
    public void LineItemTotalCalculation_QuantityTimesUnitPrice_CalculatesCorrectly()
    {
        // Arrange
        var lineItem = new ReceiptLineItem
        {
            Description = "Test Product",
            Quantity = 3.5m,
            UnitPrice = 29.99m,
            TotalPrice = 0
        };

        // Act - Simulate the calculation that happens in the UI
        lineItem.TotalPrice = lineItem.Quantity * lineItem.UnitPrice;

        // Assert
        Assert.Equal(104.965m, lineItem.TotalPrice);
    }

    [Fact]
    public void LineItemTotalCalculation_WhenQuantityChanges_UpdatesTotal()
    {
        // Arrange
        var lineItem = new ReceiptLineItem
        {
            Description = "Test Product",
            Quantity = 2,
            UnitPrice = 50.00m,
            TotalPrice = 100.00m
        };

        // Act - User changes quantity from 2 to 5
        lineItem.Quantity = 5;
        lineItem.TotalPrice = lineItem.Quantity * lineItem.UnitPrice;

        // Assert
        Assert.Equal(250.00m, lineItem.TotalPrice);
    }

    [Fact]
    public async Task DeleteReceiptAsync_RemovesReceiptAndLineItems()
    {
        // Arrange
        var receipt = new Receipt
        {
            UserId = TestUserId,
            Merchant = "Test Store",
            ReceiptDate = DateTime.Today,
            TotalAmount = 100.00m,
            Currency = "SEK",
            ReceiptType = "Physical",
            ReceiptLineItems = new List<ReceiptLineItem>
            {
                new ReceiptLineItem
                {
                    Description = "Item 1",
                    Quantity = 1,
                    UnitPrice = 100.00m,
                    TotalPrice = 100.00m
                }
            }
        };
        
        var created = await _receiptService.CreateReceiptAsync(receipt);

        // Act
        await _receiptService.DeleteReceiptAsync(created.ReceiptId, TestUserId);

        // Assert
        var result = await _receiptService.GetReceiptByIdAsync(created.ReceiptId, TestUserId);
        Assert.Null(result);
    }

    [Fact]
    public async Task GetReceiptsByTransactionIdAsync_ReturnsOnlyReceiptsForTransaction()
    {
        // Arrange
        var transaction = new Transaction
        {
            UserId = TestUserId,
            Description = "Test Transaction",
            Amount = 100.00m,
            Date = DateTime.Today,
            IsIncome = false,
            ValidFrom = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        var receipt1 = new Receipt
        {
            UserId = TestUserId,
            Merchant = "Store A",
            ReceiptDate = DateTime.Today,
            TotalAmount = 100.00m,
            Currency = "SEK",
            ReceiptType = "Physical",
            TransactionId = transaction.TransactionId
        };

        var receipt2 = new Receipt
        {
            UserId = TestUserId,
            Merchant = "Store B",
            ReceiptDate = DateTime.Today,
            TotalAmount = 50.00m,
            Currency = "SEK",
            ReceiptType = "Physical"
            // No TransactionId set
        };

        await _receiptService.CreateReceiptAsync(receipt1);
        await _receiptService.CreateReceiptAsync(receipt2);

        // Act
        var results = await _receiptService.GetReceiptsByTransactionIdAsync(transaction.TransactionId, TestUserId);

        // Assert
        Assert.Single(results);
        Assert.Equal("Store A", results[0].Merchant);
        Assert.Equal(transaction.TransactionId, results[0].TransactionId);
    }

    [Fact]
    public async Task LinkReceiptToTransactionAsync_LinksReceiptSuccessfully()
    {
        // Arrange
        var transaction = new Transaction
        {
            UserId = TestUserId,
            Description = "Test Transaction",
            Amount = 100.00m,
            Date = DateTime.Today,
            IsIncome = false,
            ValidFrom = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        var receipt = new Receipt
        {
            UserId = TestUserId,
            Merchant = "Test Store",
            ReceiptDate = DateTime.Today,
            TotalAmount = 100.00m,
            Currency = "SEK",
            ReceiptType = "Physical"
        };
        var created = await _receiptService.CreateReceiptAsync(receipt);

        // Act
        await _receiptService.LinkReceiptToTransactionAsync(created.ReceiptId, transaction.TransactionId, TestUserId);

        // Assert
        var updated = await _receiptService.GetReceiptByIdAsync(created.ReceiptId, TestUserId);
        Assert.NotNull(updated);
        Assert.Equal(transaction.TransactionId, updated.TransactionId);
        Assert.NotNull(updated.UpdatedAt);
        
        _auditLogServiceMock.Verify(
            x => x.LogAsync("Link", "Receipt", created.ReceiptId, 
                It.IsAny<string>(), TestUserId, null),
            Times.Once);
    }

    [Fact]
    public async Task LinkReceiptToTransactionAsync_ThrowsWhenReceiptNotFound()
    {
        // Arrange
        var transaction = new Transaction
        {
            UserId = TestUserId,
            Description = "Test Transaction",
            Amount = 100.00m,
            Date = DateTime.Today,
            IsIncome = false,
            ValidFrom = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _receiptService.LinkReceiptToTransactionAsync(999, transaction.TransactionId, TestUserId));
    }

    [Fact]
    public async Task LinkReceiptToTransactionAsync_ThrowsWhenTransactionNotFound()
    {
        // Arrange
        var receipt = new Receipt
        {
            UserId = TestUserId,
            Merchant = "Test Store",
            ReceiptDate = DateTime.Today,
            TotalAmount = 100.00m,
            Currency = "SEK",
            ReceiptType = "Physical"
        };
        var created = await _receiptService.CreateReceiptAsync(receipt);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _receiptService.LinkReceiptToTransactionAsync(created.ReceiptId, 999, TestUserId));
    }

    [Fact]
    public async Task UnlinkReceiptFromTransactionAsync_UnlinksReceiptSuccessfully()
    {
        // Arrange
        var transaction = new Transaction
        {
            UserId = TestUserId,
            Description = "Test Transaction",
            Amount = 100.00m,
            Date = DateTime.Today,
            IsIncome = false,
            ValidFrom = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        var receipt = new Receipt
        {
            UserId = TestUserId,
            Merchant = "Test Store",
            ReceiptDate = DateTime.Today,
            TotalAmount = 100.00m,
            Currency = "SEK",
            ReceiptType = "Physical",
            TransactionId = transaction.TransactionId
        };
        var created = await _receiptService.CreateReceiptAsync(receipt);

        // Act
        await _receiptService.UnlinkReceiptFromTransactionAsync(created.ReceiptId, TestUserId);

        // Assert
        var updated = await _receiptService.GetReceiptByIdAsync(created.ReceiptId, TestUserId);
        Assert.NotNull(updated);
        Assert.Null(updated.TransactionId);
        Assert.NotNull(updated.UpdatedAt);
        
        _auditLogServiceMock.Verify(
            x => x.LogAsync("Unlink", "Receipt", created.ReceiptId, 
                It.IsAny<string>(), TestUserId, null),
            Times.Once);
    }

    [Fact]
    public async Task UnlinkReceiptFromTransactionAsync_ThrowsWhenReceiptNotFound()
    {
        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _receiptService.UnlinkReceiptFromTransactionAsync(999, TestUserId));
    }
}
