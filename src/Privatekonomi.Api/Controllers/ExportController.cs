using Microsoft.AspNetCore.Mvc;
using Privatekonomi.Core.Services;

namespace Privatekonomi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExportController : ControllerBase
{
    private readonly IExportService _exportService;
    private readonly ILogger<ExportController> _logger;

    public ExportController(IExportService exportService, ILogger<ExportController> logger)
    {
        _exportService = exportService;
        _logger = logger;
    }

    /// <summary>
    /// Export transactions to CSV format
    /// </summary>
    /// <param name="fromDate">Optional start date (format: yyyy-MM-dd)</param>
    /// <param name="toDate">Optional end date (format: yyyy-MM-dd)</param>
    /// <returns>CSV file download</returns>
    [HttpGet("transactions/csv")]
    public async Task<IActionResult> ExportTransactionsToCsv([FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
    {
        try
        {
            var csvData = await _exportService.ExportTransactionsToCsvAsync(fromDate, toDate);
            var fileName = $"transaktioner_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            
            return File(csvData, "text/csv", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting transactions to CSV");
            return StatusCode(500, new { error = "Ett fel uppstod vid export av transaktioner" });
        }
    }

    /// <summary>
    /// Export transactions to JSON format
    /// </summary>
    /// <param name="fromDate">Optional start date (format: yyyy-MM-dd)</param>
    /// <param name="toDate">Optional end date (format: yyyy-MM-dd)</param>
    /// <returns>JSON file download</returns>
    [HttpGet("transactions/json")]
    public async Task<IActionResult> ExportTransactionsToJson([FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
    {
        try
        {
            var jsonData = await _exportService.ExportTransactionsToJsonAsync(fromDate, toDate);
            var fileName = $"transaktioner_{DateTime.Now:yyyyMMdd_HHmmss}.json";
            
            return File(jsonData, "application/json", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting transactions to JSON");
            return StatusCode(500, new { error = "Ett fel uppstod vid export av transaktioner" });
        }
    }

    /// <summary>
    /// Export a specific budget to CSV format
    /// </summary>
    /// <param name="budgetId">Budget ID to export</param>
    /// <returns>CSV file download</returns>
    [HttpGet("budgets/{budgetId}/csv")]
    public async Task<IActionResult> ExportBudgetToCsv(int budgetId)
    {
        try
        {
            var csvData = await _exportService.ExportBudgetToCsvAsync(budgetId);
            var fileName = $"budget_{budgetId}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            
            return File(csvData, "text/csv", fileName);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting budget to CSV");
            return StatusCode(500, new { error = "Ett fel uppstod vid export av budget" });
        }
    }

    /// <summary>
    /// Create a full backup of all data
    /// </summary>
    /// <returns>JSON backup file download</returns>
    [HttpGet("backup")]
    public async Task<IActionResult> ExportFullBackup()
    {
        try
        {
            var backupData = await _exportService.ExportFullBackupAsync();
            var fileName = $"privatekonomi_backup_{DateTime.Now:yyyyMMdd_HHmmss}.json";
            
            return File(backupData, "application/json", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating full backup");
            return StatusCode(500, new { error = "Ett fel uppstod vid skapande av backup" });
        }
    }
}
