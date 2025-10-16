using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services.Parsers;

public interface ICsvParser
{
    string BankName { get; }
    Task<List<Transaction>> ParseAsync(Stream csvStream);
    bool CanParse(string csvContent);
}
