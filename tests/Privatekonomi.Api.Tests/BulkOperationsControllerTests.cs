using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Privatekonomi.Api.Controllers;
using Privatekonomi.Api.Models;
using Privatekonomi.Core.Models;
using Privatekonomi.Core.Services;

namespace Privatekonomi.Api.Tests;

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

    [Fact]
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
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var bulkResult = Assert.IsType<BulkOperationResult>(okResult.Value);
        Assert.Equal(3, bulkResult.SuccessCount);
        Assert.Equal(0, bulkResult.FailureCount);
    }

    [Fact]
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
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var bulkResult = Assert.IsType<BulkOperationResult>(okResult.Value);
        Assert.Equal(2, bulkResult.SuccessCount);
    }

    [Fact]
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
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var bulkResult = Assert.IsType<BulkOperationResult>(okResult.Value);
        Assert.Equal(3, bulkResult.SuccessCount);
    }

    [Fact]
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
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var bulkResult = Assert.IsType<BulkOperationResult>(okResult.Value);
        Assert.Equal(2, bulkResult.SuccessCount);
        
        _mockTransactionService.Verify(
            s => s.BulkLinkToHouseholdAsync(request.TransactionIds, null), 
            Times.Once);
    }

    [Fact]
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
        var fileResult = Assert.IsType<FileContentResult>(result);
        Assert.Equal("text/csv", fileResult.ContentType);
        Assert.Contains("transaktioner_", fileResult.FileDownloadName);
        Assert.EndsWith(".csv", fileResult.FileDownloadName);
        Assert.Equal(csvData, fileResult.FileContents);
    }

    [Fact]
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
        var fileResult = Assert.IsType<FileContentResult>(result);
        Assert.Equal("application/json", fileResult.ContentType);
        Assert.Contains("transaktioner_", fileResult.FileDownloadName);
        Assert.EndsWith(".json", fileResult.FileDownloadName);
        Assert.Equal(jsonData, fileResult.FileContents);
    }

    [Fact]
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
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var snapshot = Assert.IsType<BulkOperationSnapshot>(okResult.Value);
        Assert.Equal(operationType, snapshot.OperationType);
        Assert.Equal(3, snapshot.AffectedTransactionIds.Count);
    }

    [Fact]
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
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var undoResult = Assert.IsType<BulkOperationResult>(okResult.Value);
        Assert.Equal(2, undoResult.SuccessCount);
    }

    [Fact]
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
        Assert.NotNull(capturedCategories);
        Assert.Equal(2, capturedCategories.Count);
        Assert.Equal(1, capturedCategories[0].CategoryId);
        Assert.Equal(100m, capturedCategories[0].Amount);
        Assert.Equal(2, capturedCategories[1].CategoryId);
        Assert.Null(capturedCategories[1].Amount);
    }
}
