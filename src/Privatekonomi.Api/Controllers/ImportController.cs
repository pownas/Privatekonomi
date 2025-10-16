using Microsoft.AspNetCore.Mvc;
using Privatekonomi.Core.Models;
using Privatekonomi.Core.Services;

namespace Privatekonomi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ImportController : ControllerBase
{
    private readonly ICsvImportService _csvImportService;
    private readonly ILogger<ImportController> _logger;
    private static readonly Dictionary<string, byte[]> _tempFiles = new();

    public ImportController(ICsvImportService csvImportService, ILogger<ImportController> logger)
    {
        _csvImportService = csvImportService;
        _logger = logger;
    }

    [HttpPost("upload")]
    public async Task<ActionResult<PreviewResponse>> Upload([FromForm] IFormFile file, [FromForm] string bankName)
    {
        try
        {
            // Validate file
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { error = "Ingen fil vald" });
            }

            if (file.Length > 10 * 1024 * 1024) // 10 MB
            {
                return BadRequest(new { error = "Filen är för stor. Max storlek är 10 MB." });
            }

            var extension = Path.GetExtension(file.FileName).ToLower();
            if (extension != ".csv" && extension != ".txt")
            {
                return BadRequest(new { error = "Filtypen stöds inte. Endast .csv-filer accepteras." });
            }

            // Read file content
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            var fileContent = memoryStream.ToArray();

            // Parse and preview
            memoryStream.Position = 0;
            var result = await _csvImportService.PreviewCsvAsync(memoryStream, bankName);

            if (!result.Success)
            {
                return BadRequest(new { error = "Kunde inte läsa CSV-filen", errors = result.Errors });
            }

            // Store file temporarily
            var fileId = Guid.NewGuid().ToString();
            _tempFiles[fileId] = fileContent;

            // Clean up old temp files (older than 1 hour)
            CleanupOldTempFiles();

            var preview = result.Transactions.Take(5).Select(t => new
            {
                date = t.Date.ToString("yyyy-MM-dd"),
                amount = t.Amount,
                description = t.Description,
                isIncome = t.IsIncome
            }).ToList<object>();

            return Ok(new PreviewResponse
            {
                Success = true,
                FileId = fileId,
                Preview = preview,
                Summary = new SummaryResponse
                {
                    TotalRows = result.TotalRows,
                    ValidTransactions = result.ImportedCount,
                    Duplicates = result.DuplicateCount,
                    Errors = result.ErrorCount,
                    TotalAmount = result.Summary.TotalAmount,
                    IncomeAmount = result.Summary.IncomeAmount,
                    ExpenseAmount = result.Summary.ExpenseAmount
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading CSV file");
            return StatusCode(500, new { error = $"Ett fel uppstod: {ex.Message}" });
        }
    }

    [HttpPost("confirm")]
    public async Task<ActionResult<ImportResponse>> Confirm([FromBody] ConfirmRequest request)
    {
        try
        {
            if (!_tempFiles.TryGetValue(request.FileId, out var fileContent))
            {
                return BadRequest(new { error = "Filen kunde inte hittas. Vänligen ladda upp filen igen." });
            }

            // Import transactions
            using var memoryStream = new MemoryStream(fileContent);
            var result = await _csvImportService.ImportCsvAsync(memoryStream, request.Bank, request.SkipDuplicates);

            // Remove temp file
            _tempFiles.Remove(request.FileId);

            if (!result.Success)
            {
                return BadRequest(new { error = "Import misslyckades", errors = result.Errors });
            }

            return Ok(new ImportResponse
            {
                Success = true,
                Imported = result.ImportedCount,
                Duplicates = result.DuplicateCount,
                Errors = result.ErrorCount,
                ErrorDetails = result.Errors
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming import");
            return StatusCode(500, new { error = $"Ett fel uppstod: {ex.Message}" });
        }
    }

    private void CleanupOldTempFiles()
    {
        // In a production system, you would track creation times and remove old files
        // For simplicity, we'll limit the number of temp files
        if (_tempFiles.Count > 100)
        {
            var oldestKeys = _tempFiles.Keys.Take(_tempFiles.Count - 100).ToList();
            foreach (var key in oldestKeys)
            {
                _tempFiles.Remove(key);
            }
        }
    }
}

public class PreviewResponse
{
    public bool Success { get; set; }
    public string FileId { get; set; } = string.Empty;
    public List<object> Preview { get; set; } = new();
    public SummaryResponse Summary { get; set; } = new();
}

public class SummaryResponse
{
    public int TotalRows { get; set; }
    public int ValidTransactions { get; set; }
    public int Duplicates { get; set; }
    public int Errors { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal IncomeAmount { get; set; }
    public decimal ExpenseAmount { get; set; }
}

public class ConfirmRequest
{
    public string FileId { get; set; } = string.Empty;
    public string Bank { get; set; } = string.Empty;
    public bool SkipDuplicates { get; set; } = true;
}

public class ImportResponse
{
    public bool Success { get; set; }
    public int Imported { get; set; }
    public int Duplicates { get; set; }
    public int Errors { get; set; }
    public List<CsvImportError> ErrorDetails { get; set; } = new();
}
