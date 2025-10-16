using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services;

public interface ICsvImportService
{
    Task<CsvImportResult> ImportCsvAsync(Stream csvStream, string bankName, bool skipDuplicates = true);
    Task<CsvImportResult> PreviewCsvAsync(Stream csvStream, string bankName);
}
