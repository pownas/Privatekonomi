using System.Text;
using Microsoft.Extensions.Logging;
using Moq;
using Privatekonomi.Core.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Privatekonomi.Core.Tests;

[TestClass]
public class OcrServiceTests
{
    private readonly Mock<ILogger<TesseractOcrService>> _mockLogger;
    private readonly TesseractOcrService _ocrService;

    public OcrServiceTests()
    {
        _mockLogger = new Mock<ILogger<TesseractOcrService>>();
        _ocrService = new TesseractOcrService(_mockLogger.Object);
    }

    [TestMethod]
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
        Assert.IsNotNull(result);
        Assert.AreEqual(100.00m, result.TotalAmount);
    }

    [TestMethod]
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
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Date);
        Assert.AreEqual(new DateTime(2024, 1, 15), result.Date);
    }

    [TestMethod]
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
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Date);
        Assert.AreEqual(new DateTime(2024, 1, 15), result.Date);
    }

    [TestMethod]
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
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Merchant);
        CollectionAssert.Contains(result.Merchant, "ICA Maxi");
    }

    [TestMethod]
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
        Assert.IsNotNull(result);
        Assert.AreEqual("98765", result.ReceiptNumber);
    }

    [TestMethod]
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
        Assert.IsNotNull(result);
        Assert.AreEqual("Swish", result.PaymentMethod);
    }

    [TestMethod]
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
        Assert.IsNotNull(result);
        Assert.AreNotEqual(0, result.LineItems.Count());
        Assert.IsTrue(result.LineItems.Count >= 2);
        
        var milkItem = result.LineItems.FirstOrDefault(i => i.Description.Contains("Mjölk"));
        Assert.IsNotNull(milkItem);
        Assert.AreEqual(29.50m, milkItem.TotalPrice);
    }

    [TestMethod]
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
        Assert.IsNotNull(result);
        Assert.AreNotEqual(0, result.LineItems.Count());
        
        var appleItem = result.LineItems.FirstOrDefault();
        Assert.IsNotNull(appleItem);
        Assert.AreEqual(2, appleItem.Quantity);
        Assert.AreEqual(15.00m, appleItem.UnitPrice);
        Assert.AreEqual(30.00m, appleItem.TotalPrice);
    }

    [TestMethod]
    public void ParseReceiptText_WithEmptyString_ReturnsEmptyData()
    {
        // Arrange
        var ocrText = "";

        // Act
        var result = _ocrService.ParseReceiptText(ocrText);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNull(result.TotalAmount);
        Assert.IsNull(result.Date);
        Assert.IsNull(result.Merchant);
    }

    [TestMethod]
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
        Assert.IsNotNull(result);
        Assert.AreEqual(250.50m, result.TotalAmount);
    }

    [TestMethod]
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
        Assert.IsNotNull(result);
        Assert.AreEqual(150.00m, result.TotalAmount);
    }

    [TestMethod]
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
        Assert.IsNotNull(result);
        Assert.AreEqual("Kort", result.PaymentMethod);
    }

    [TestMethod]
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
        Assert.IsNotNull(result);
        Assert.AreEqual("Kontant", result.PaymentMethod);
    }

    [DataTestMethod]
    [DataRow("Totalt: 100,00 kr", 100.00)]
    [DataRow("Total: 150,50", 150.50)]
    [DataRow("Summa: 75,25 kr", 75.25)]
    [DataRow("Att betala: 200,00", 200.00)]
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
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedAmount, result.TotalAmount);
    }

    [DataTestMethod]
    [DataRow("2024-01-15", 2024, 1, 15)]
    [DataRow("15/01/2024", 2024, 1, 15)]
    [DataRow("15.01.2024", 2024, 1, 15)]
    [DataRow("20240115", 2024, 1, 15)]
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
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Date);
        Assert.AreEqual(new DateTime(year, month, day), result.Date);
    }
}
