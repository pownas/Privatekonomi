using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services;

public interface ICsvImportService
{
    Task<CsvImportResult> ImportCsvAsync(Stream csvStream, string bankName, bool skipDuplicates = true);
    Task<CsvImportResult> PreviewCsvAsync(Stream csvStream, string bankName);
    
    /// <summary>
    /// Import transactions and create an ImportJob record for tracking.
    /// </summary>
    Task<(CsvImportResult Result, ImportJob Job)> ImportWithJobAsync(
        Stream stream, 
        string bankName, 
        string fileName, 
        long fileSize, 
        string? userId = null,
        bool skipDuplicates = true);
    
    /// <summary>
    /// Get an import job by ID.
    /// </summary>
    Task<ImportJob?> GetImportJobAsync(int importJobId);
    
    /// <summary>
    /// Get all import jobs for a user.
    /// </summary>
    Task<List<ImportJob>> GetUserImportJobsAsync(string userId);
}
