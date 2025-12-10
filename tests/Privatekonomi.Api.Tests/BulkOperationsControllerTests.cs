using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Privatekonomi.Api.Controllers;
using Privatekonomi.Api.Models;
using Privatekonomi.Core.Models;
using Privatekonomi.Core.Services;

namespace Privatekonomi.Api.Tests;

[TestClass]
public class BulkOperationsControllerTests
{
    private readonly Mock<ITransactionService> _mockTransactionService;
    private readonly Mock<IExportService> _mockExportService;
    private readonly Mock<ILogger<BulkOperationsController>> _mockLogger;
    private readonly BulkOperationsController _controller;

    public BulkOperationsControllerTests()
    {
        _mockTransactionService = new Mock<ITransactionService>();
        _mockExportService = new Mock<IExportService>();
        _mockLogger = new Mock<ILogger<BulkOperationsController>>();
        _controller = new BulkOperationsController(
            _mockTransactionService.Object,
            _mockExportService.Object,
            _mockLogger.Object);
    }

    [TestMethod]
    public async Task BulkDelete_ShouldReturnOk_WhenSuccessful()
    {
        // Arrange
        var request = new BulkDeleteRequest
        {
            TransactionIds = new List<int> { 1, 2, 3 }
        };

        var snapshot = new BulkOperationSnapshot
        {
            OperationType = BulkOperationType.Delete,
            AffectedTransactionIds = request.TransactionIds
        };

        var expectedResult = new BulkOperationResult
        {
            SuccessCount = 3,
            FailureCount = 0,
            TotalCount = 3
        };

        _mockTransactionService
            .Setup(s => s.CreateOperationSnapshotAsync(It.IsAny<List<int>>(), It.IsAny<BulkOperationType>()))
            .ReturnsAsync(snapshot);

        _mockTransactionService
            .Setup(s => s.BulkDeleteTransactionsAsync(request.TransactionIds))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.BulkDelete(request);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        var okResult = (OkObjectResult)result.Result;
        Assert.IsInstanceOfType(okResult.Value, typeof(BulkOperationResult));
        var bulkResult = (BulkOperationResult)okResult.Value;
        Assert.AreEqual(3, bulkResult.SuccessCount);
        Assert.AreEqual(0, bulkResult.FailureCount);
    }

    [TestMethod]
    public async Task BulkCategorize_ShouldReturnOk_WhenSuccessful()
    {
        // Arrange
        var request = new BulkCategorizeRequest
        {
            TransactionIds = new List<int> { 1, 2 },
            Categories = new List<BulkCategoryDto>
            {
                new() { CategoryId = 1, Amount = null }
            }
        };

        var expectedResult = new BulkOperationResult
        {
            SuccessCount = 2,
            FailureCount = 0,
            TotalCount = 2
        };

        _mockTransactionService
            .Setup(s => s.BulkCategorizeTransactionsAsync(
                It.IsAny<List<int>>(), 
                It.IsAny<List<(int CategoryId, decimal? Amount)>>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.BulkCategorize(request);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        var okResult = (OkObjectResult)result.Result;
        Assert.IsInstanceOfType(okResult.Value, typeof(BulkOperationResult));
        var bulkResult = (BulkOperationResult)okResult.Value;
        Assert.AreEqual(2, bulkResult.SuccessCount);
    }

    [TestMethod]
    public async Task BulkLinkHousehold_ShouldReturnOk_WhenSuccessful()
    {
        // Arrange
        var request = new BulkLinkHouseholdRequest
        {
            TransactionIds = new List<int> { 1, 2, 3 },
            HouseholdId = 5
        };

        var expectedResult = new BulkOperationResult
        {
            SuccessCount = 3,
            FailureCount = 0,
            TotalCount = 3
        };

        _mockTransactionService
            .Setup(s => s.BulkLinkToHouseholdAsync(request.TransactionIds, request.HouseholdId))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.BulkLinkHousehold(request);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        var okResult = (OkObjectResult)result.Result;
        Assert.IsInstanceOfType(okResult.Value, typeof(BulkOperationResult));
        var bulkResult = (BulkOperationResult)okResult.Value;
        Assert.AreEqual(3, bulkResult.SuccessCount);
    }

    [TestMethod]
    public async Task BulkLinkHousehold_ShouldUnlink_WhenHouseholdIdIsNull()
    {
        // Arrange
        var request = new BulkLinkHouseholdRequest
        {
            TransactionIds = new List<int> { 1, 2 },
            HouseholdId = null
        };

        var expectedResult = new BulkOperationResult
        {
            SuccessCount = 2,
            FailureCount = 0,
            TotalCount = 2
        };

        _mockTransactionService
            .Setup(s => s.BulkLinkToHouseholdAsync(request.TransactionIds, null))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.BulkLinkHousehold(request);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        var okResult = (OkObjectResult)result.Result;
        Assert.IsInstanceOfType(okResult.Value, typeof(BulkOperationResult));
        var bulkResult = (BulkOperationResult)okResult.Value;
        Assert.AreEqual(2, bulkResult.SuccessCount);
        
        _mockTransactionService.Verify(
            s => s.BulkLinkToHouseholdAsync(request.TransactionIds, null), 
            Times.Once);
    }

    [TestMethod]
    public async Task BulkExport_ShouldReturnCsvFile_WhenFormatIsCsv()
    {
        // Arrange
        var request = new BulkExportRequest
        {
            TransactionIds = new List<int> { 1, 2, 3 },
            Format = ExportFormat.CSV
        };

        var csvData = new byte[] { 1, 2, 3 };
        _mockExportService
            .Setup(s => s.ExportSelectedTransactionsToCsvAsync(request.TransactionIds))
            .ReturnsAsync(csvData);

        // Act
        var result = await _controller.BulkExport(request);

        // Assert
        Assert.IsInstanceOfType(result, typeof(FileContentResult));
        var fileResult = (FileContentResult)result;
        Assert.AreEqual("text/csv", fileResult.ContentType);
        CollectionAssert.Contains(fileResult.FileDownloadName, "transaktioner_");
        StringAssert.EndsWith(fileResult.FileDownloadName, ".csv");
        Assert.AreEqual(csvData, fileResult.FileContents);
    }

    [TestMethod]
    public async Task BulkExport_ShouldReturnJsonFile_WhenFormatIsJson()
    {
        // Arrange
        var request = new BulkExportRequest
        {
            TransactionIds = new List<int> { 1, 2, 3 },
            Format = ExportFormat.JSON
        };

        var jsonData = new byte[] { 1, 2, 3 };
        _mockExportService
            .Setup(s => s.ExportSelectedTransactionsToJsonAsync(request.TransactionIds))
            .ReturnsAsync(jsonData);

        // Act
        var result = await _controller.BulkExport(request);

        // Assert
        Assert.IsInstanceOfType(result, typeof(FileContentResult));
        var fileResult = (FileContentResult)result;
        Assert.AreEqual("application/json", fileResult.ContentType);
        CollectionAssert.Contains(fileResult.FileDownloadName, "transaktioner_");
        StringAssert.EndsWith(fileResult.FileDownloadName, ".json");
        Assert.AreEqual(jsonData, fileResult.FileContents);
    }

    [TestMethod]
    public async Task CreateSnapshot_ShouldReturnSnapshot()
    {
        // Arrange
        var transactionIds = new List<int> { 1, 2, 3 };
        var operationType = BulkOperationType.Categorize;

        var expectedSnapshot = new BulkOperationSnapshot
        {
            OperationType = operationType,
            AffectedTransactionIds = transactionIds,
            OperationId = "test-id"
        };

        _mockTransactionService
            .Setup(s => s.CreateOperationSnapshotAsync(transactionIds, operationType))
            .ReturnsAsync(expectedSnapshot);

        // Act
        var result = await _controller.CreateSnapshot(operationType, transactionIds);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        var okResult = (OkObjectResult)result.Result;
        Assert.IsInstanceOfType(okResult.Value, typeof(BulkOperationSnapshot));
        var snapshot = (BulkOperationSnapshot)okResult.Value;
        Assert.AreEqual(operationType, snapshot.OperationType);
        Assert.AreEqual(3, snapshot.AffectedTransactionIds.Count);
    }

    [TestMethod]
    public async Task Undo_ShouldReturnOk_WhenSuccessful()
    {
        // Arrange
        var snapshot = new BulkOperationSnapshot
        {
            OperationType = BulkOperationType.Categorize,
            AffectedTransactionIds = new List<int> { 1, 2 },
            OperationId = "test-id"
        };

        var expectedResult = new BulkOperationResult
        {
            SuccessCount = 2,
            FailureCount = 0,
            TotalCount = 2
        };

        _mockTransactionService
            .Setup(s => s.UndoBulkOperationAsync(snapshot))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.Undo(snapshot);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        var okResult = (OkObjectResult)result.Result;
        Assert.IsInstanceOfType(okResult.Value, typeof(BulkOperationResult));
        var undoResult = (BulkOperationResult)okResult.Value;
        Assert.AreEqual(2, undoResult.SuccessCount);
    }

    [TestMethod]
    public async Task BulkCategorize_ShouldMapCategories_Correctly()
    {
        // Arrange
        var request = new BulkCategorizeRequest
        {
            TransactionIds = new List<int> { 1 },
            Categories = new List<BulkCategoryDto>
            {
                new() { CategoryId = 1, Amount = 100m },
                new() { CategoryId = 2, Amount = null }
            }
        };

        List<(int CategoryId, decimal? Amount)>? capturedCategories = null;
        
        _mockTransactionService
            .Setup(s => s.BulkCategorizeTransactionsAsync(
                It.IsAny<List<int>>(), 
                It.IsAny<List<(int CategoryId, decimal? Amount)>>()))
            .Callback<List<int>, List<(int CategoryId, decimal? Amount)>>((ids, cats) => 
            {
                capturedCategories = cats;
            })
            .ReturnsAsync(new BulkOperationResult { SuccessCount = 1 });

        // Act
        await _controller.BulkCategorize(request);

        // Assert
        Assert.IsNotNull(capturedCategories);
        Assert.AreEqual(2, capturedCategories.Count);
        Assert.AreEqual(1, capturedCategories[0].CategoryId);
        Assert.AreEqual(100m, capturedCategories[0].Amount);
        Assert.AreEqual(2, capturedCategories[1].CategoryId);
        Assert.IsNull(capturedCategories[1].Amount);
    }
}
