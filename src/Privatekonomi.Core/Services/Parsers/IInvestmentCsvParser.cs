using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services.Parsers;

public interface IInvestmentCsvParser
{
    string BankName { get; }
    string FormatType { get; }
    Task<List<Investment>> ParseAsync(Stream csvStream);
    bool CanParse(string csvContent);
}
