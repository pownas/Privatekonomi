namespace Privatekonomi.Core.Models;

public class CsvImportResult
{
    public bool Success { get; set; }
    public int TotalRows { get; set; }
    public int ImportedCount { get; set; }
    public int DuplicateCount { get; set; }
    public int ErrorCount { get; set; }
    public List<CsvImportError> Errors { get; set; } = new();
    public List<Transaction> Transactions { get; set; } = new();
    public ImportSummary Summary { get; set; } = new();
}

public class CsvImportError
{
    public int RowNumber { get; set; }
    public string ErrorType { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public string RawData { get; set; } = string.Empty;
}

public class ImportSummary
{
    public decimal TotalAmount { get; set; }
    public decimal IncomeAmount { get; set; }
    public decimal ExpenseAmount { get; set; }
    public int IncomeCount { get; set; }
    public int ExpenseCount { get; set; }
}
