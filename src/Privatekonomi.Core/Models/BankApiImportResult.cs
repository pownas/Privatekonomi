namespace Privatekonomi.Core.Models;

/// <summary>
/// Result of importing transactions from a bank API
/// </summary>
public class BankApiImportResult
{
    public bool Success { get; set; }
    public int ImportedCount { get; set; }
    public int DuplicateCount { get; set; }
    public int ErrorCount { get; set; }
    public List<Transaction> Transactions { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public DateTime? LastTransactionDate { get; set; }
}
