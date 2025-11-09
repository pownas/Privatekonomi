using System.Text;
using Microsoft.Extensions.Logging;
using Moq;
using Privatekonomi.Core.Services;
using Xunit;

namespace Privatekonomi.Core.Tests;

public class OcrServiceTests
{
    private readonly Mock<ILogger<TesseractOcrService>> _mockLogger;
    private readonly TesseractOcrService _ocrService;

    public OcrServiceTests()
    {
        _mockLogger = new Mock<ILogger<TesseractOcrService>>();
        _ocrService = new TesseractOcrService(_mockLogger.Object);
    }

    [Fact]
    public void ParseReceiptText_WithTotalAmount_ExtractsTotalAmount()
    {
        // Arrange
        var ocrText = @"
ICA Maxi
Storvägen 123
123 45 Stockholm

Mjölk 3%              29,50 kr
Bröd                  25,00 kr
Smör                  45,50 kr

Totalt:              100,00 kr
Kort                 100,00 kr

2024-01-15 14:30
Kvitto: 12345
";

        // Act
        var result = _ocrService.ParseReceiptText(ocrText);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(100.00m, result.TotalAmount);
    }

    [Fact]
    public void ParseReceiptText_WithDate_ExtractsDate()
    {
        // Arrange
        var ocrText = @"
ICA Maxi
2024-01-15
Totalt: 100,00 kr
";

        // Act
        var result = _ocrService.ParseReceiptText(ocrText);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Date);
        Assert.Equal(new DateTime(2024, 1, 15), result.Date);
    }

    [Fact]
    public void ParseReceiptText_WithSwedishDateFormat_ExtractsDate()
    {
        // Arrange
        var ocrText = @"
Willys
15.01.2024
Summa: 50,00
";

        // Act
        var result = _ocrService.ParseReceiptText(ocrText);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Date);
        Assert.Equal(new DateTime(2024, 1, 15), result.Date);
    }

    [Fact]
    public void ParseReceiptText_WithMerchant_ExtractsMerchant()
    {
        // Arrange
        var ocrText = @"
ICA Maxi Västerås
Storvägen 123
2024-01-15
Totalt: 100,00
";

        // Act
        var result = _ocrService.ParseReceiptText(ocrText);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Merchant);
        Assert.Contains("ICA Maxi", result.Merchant);
    }

    [Fact]
    public void ParseReceiptText_WithReceiptNumber_ExtractsReceiptNumber()
    {
        // Arrange
        var ocrText = @"
Hemköp
Kvitto: 98765
Datum: 2024-01-15
Total: 75,50 kr
";

        // Act
        var result = _ocrService.ParseReceiptText(ocrText);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("98765", result.ReceiptNumber);
    }

    [Fact]
    public void ParseReceiptText_WithPaymentMethod_ExtractsPaymentMethod()
    {
        // Arrange
        var ocrText = @"
Coop
Totalt: 100,00 kr
Swish
";

        // Act
        var result = _ocrService.ParseReceiptText(ocrText);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Swish", result.PaymentMethod);
    }

    [Fact]
    public void ParseReceiptText_WithLineItems_ExtractsLineItems()
    {
        // Arrange
        var ocrText = @"
ICA Maxi
Mjölk 3%              29,50
Bröd                  25,00
2x Smör               90,00
Totalt:              144,50 kr
";

        // Act
        var result = _ocrService.ParseReceiptText(ocrText);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.LineItems);
        Assert.True(result.LineItems.Count >= 2);
        
        var milkItem = result.LineItems.FirstOrDefault(i => i.Description.Contains("Mjölk"));
        Assert.NotNull(milkItem);
        Assert.Equal(29.50m, milkItem.TotalPrice);
    }

    [Fact]
    public void ParseReceiptText_WithQuantityInLineItem_ExtractsQuantity()
    {
        // Arrange
        var ocrText = @"
ICA
2x Äpplen             30,00
Totalt:               30,00
";

        // Act
        var result = _ocrService.ParseReceiptText(ocrText);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.LineItems);
        
        var appleItem = result.LineItems.FirstOrDefault();
        Assert.NotNull(appleItem);
        Assert.Equal(2, appleItem.Quantity);
        Assert.Equal(15.00m, appleItem.UnitPrice);
        Assert.Equal(30.00m, appleItem.TotalPrice);
    }

    [Fact]
    public void ParseReceiptText_WithEmptyString_ReturnsEmptyData()
    {
        // Arrange
        var ocrText = "";

        // Act
        var result = _ocrService.ParseReceiptText(ocrText);

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.TotalAmount);
        Assert.Null(result.Date);
        Assert.Null(result.Merchant);
    }

    [Fact]
    public void ParseReceiptText_WithAlternativeTotalFormat_ExtractsTotalAmount()
    {
        // Arrange
        var ocrText = @"
Willys
Att betala: 250,50 kr
Kort
";

        // Act
        var result = _ocrService.ParseReceiptText(ocrText);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(250.50m, result.TotalAmount);
    }

    [Fact]
    public void ParseReceiptText_WithSEKCurrency_ExtractsTotalAmount()
    {
        // Arrange
        var ocrText = @"
Coop
Summa: 150,00 SEK
";

        // Act
        var result = _ocrService.ParseReceiptText(ocrText);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(150.00m, result.TotalAmount);
    }

    [Fact]
    public void ParseReceiptText_WithKortPayment_ExtractsPaymentMethod()
    {
        // Arrange
        var ocrText = @"
Hemköp
Total: 100,00
Bankkort
";

        // Act
        var result = _ocrService.ParseReceiptText(ocrText);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Kort", result.PaymentMethod);
    }

    [Fact]
    public void ParseReceiptText_WithKontantPayment_ExtractsPaymentMethod()
    {
        // Arrange
        var ocrText = @"
ICA
Total: 50,00
Kontant
";

        // Act
        var result = _ocrService.ParseReceiptText(ocrText);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Kontant", result.PaymentMethod);
    }

    [Theory]
    [InlineData("Totalt: 100,00 kr", 100.00)]
    [InlineData("Total: 150,50", 150.50)]
    [InlineData("Summa: 75,25 kr", 75.25)]
    [InlineData("Att betala: 200,00", 200.00)]
    public void ParseReceiptText_WithVariousTotalFormats_ExtractsTotalAmount(string totalLine, decimal expectedAmount)
    {
        // Arrange
        var ocrText = $@"
Test Store
{totalLine}
";

        // Act
        var result = _ocrService.ParseReceiptText(ocrText);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedAmount, result.TotalAmount);
    }

    [Theory]
    [InlineData("2024-01-15", 2024, 1, 15)]
    [InlineData("15/01/2024", 2024, 1, 15)]
    [InlineData("15.01.2024", 2024, 1, 15)]
    [InlineData("20240115", 2024, 1, 15)]
    public void ParseReceiptText_WithVariousDateFormats_ExtractsDate(string dateStr, int year, int month, int day)
    {
        // Arrange
        var ocrText = $@"
Store
{dateStr}
Total: 100
";

        // Act
        var result = _ocrService.ParseReceiptText(ocrText);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Date);
        Assert.Equal(new DateTime(year, month, day), result.Date);
    }
}
