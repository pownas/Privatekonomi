using System.Globalization;
using System.Text;
using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services.Parsers;

public class IcaBankenParser : ICsvParser
{
    public string BankName => "ICA-banken";

    public bool CanParse(string csvContent)
    {
        var lines = csvContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length < 2) return false;

        var header = lines[0].ToLower();
        // Accept both 'beskrivning' and 'text' as description column
        return header.Contains("datum") && header.Contains("belopp") && (header.Contains("beskrivning") || header.Contains("text"));
    }

    public async Task<List<Transaction>> ParseAsync(Stream csvStream)
    {
        var transactions = new List<Transaction>();
        
        using var reader = new StreamReader(csvStream, Encoding.UTF8);
        var content = await reader.ReadToEndAsync();
        
        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length < 2) return transactions;

        // Detect separator (semicolon or comma)
        var separator = lines[0].Contains(';') ? ';' : ',';

        // Parse header to find column indices
        var header = lines[0].Split(separator);
        var dateIndex = FindColumnIndex(header, new[] { "datum", "date" });
        var amountIndex = FindColumnIndex(header, new[] { "belopp", "amount" });
        var descriptionIndex = FindColumnIndex(header, new[] { "beskrivning", "text", "beskrivning / text", "description" });

        if (dateIndex == -1 || amountIndex == -1 || descriptionIndex == -1)
        {
            throw new InvalidOperationException("Kunde inte hitta nödvändiga kolumner i CSV-filen");
        }

        // Parse data rows
        for (int i = 1; i < lines.Length; i++)
        {
            try
            {
                var columns = lines[i].Split(separator);
                if (columns.Length <= Math.Max(dateIndex, Math.Max(amountIndex, descriptionIndex)))
                    continue;

                var dateStr = columns[dateIndex].Trim();
                var rawAmount = columns[amountIndex].Trim();
                // Remove currency (kr), spaces (thousand separator), and handle decimal comma
                var amountStr = rawAmount
                    .Replace("kr", "", StringComparison.OrdinalIgnoreCase)
                    .Replace(" ", "")
                    .Replace(".", "") // Remove thousand separator if dot
                    .Replace(",", "."); // Convert decimal comma to dot

                var description = columns[descriptionIndex].Trim();

                if (string.IsNullOrWhiteSpace(description))
                    continue;

                // Parse date - support multiple formats
                DateTime date;
                if (!TryParseDate(dateStr, out date))
                    continue;

                // Parse amount
                if (!decimal.TryParse(amountStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var amount))
                    continue;

                var transaction = new Transaction
                {
                    Date = date,
                    Amount = Math.Abs(amount),
                    IsIncome = amount > 0,
                    Description = description.Length > 500 ? description.Substring(0, 500) : description
                };

                transactions.Add(transaction);
            }
            catch
            {
                // Skip invalid rows
                continue;
            }
        }

        return transactions;
    }

    private int FindColumnIndex(string[] header, string[] possibleNames)
    {
        for (int i = 0; i < header.Length; i++)
        {
            var columnName = header[i].Trim().ToLower();
            foreach (var name in possibleNames)
            {
                if (columnName.Contains(name.ToLower()))
                    return i;
            }
        }
        return -1;
    }

    private bool TryParseDate(string dateStr, out DateTime date)
    {
        var formats = new[]
        {
            "yyyy-MM-dd",
            "dd.MM.yyyy",
            "dd-MM-yyyy",
            "yyyy/MM/dd"
        };

        foreach (var format in formats)
        {
            if (DateTime.TryParseExact(dateStr, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                return true;
        }

        date = DateTime.MinValue;
        return false;
    }
}
