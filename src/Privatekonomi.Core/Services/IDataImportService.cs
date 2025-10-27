namespace Privatekonomi.Core.Services;

public interface IDataImportService
{
    /// <summary>
    /// Import full backup from JSON format
    /// </summary>
    /// <param name="backupData">JSON backup file content</param>
    /// <param name="mergeMode">If true, merges with existing data; if false, replaces all data</param>
    /// <returns>Import result with statistics</returns>
    Task<DataImportResult> ImportFullBackupAsync(byte[] backupData, bool mergeMode = false);
}

public class DataImportResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public Dictionary<string, int> ImportedCounts { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}
