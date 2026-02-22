using System.Globalization;
using System.Text;
using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services.Parsers;

/// <summary>
/// Parser for Avanza transaction history CSV exports.
/// Handles the format exported from Avanza's "Transaktioner" page.
/// </summary>
public class AvanzaTransactionParser : ICsvParser
{
    public string BankName => "Avanza";

    public bool CanParse(string csvContent)
    {
        var lines = csvContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length < 2) return false;

        // Detect separator
        var firstLine = lines[0];
        var separator = DetectSeparator(firstLine);
        var header = firstLine.ToLower();

        // Avanza transaction format has "datum", "typ av transaktion" and "belopp"
        return header.Contains("datum") &&
               header.Contains("typ av transaktion") &&
               header.Contains("belopp");
    }

    public async Task<List<Transaction>> ParseAsync(Stream csvStream)
    {
        var transactions = new List<Transaction>();

        using var reader = new StreamReader(csvStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        var content = await reader.ReadToEndAsync();

        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length < 2) return transactions;

        var separator = DetectSeparator(lines[0]);
        var header = SplitLine(lines[0], separator);

        var dateIndex = FindColumnIndex(header, new[] { "datum" });
        var typeIndex = FindColumnIndex(header, new[] { "typ av transaktion" });
        var descriptionIndex = FindColumnIndex(header, new[] { "värdepapper/beskrivning", "värdepapper", "beskrivning" });
        var amountIndex = FindColumnIndex(header, new[] { "belopp" });
        var currencyIndex = FindColumnIndex(header, new[] { "valuta" });

        if (dateIndex == -1 || typeIndex == -1 || amountIndex == -1)
        {
            throw new InvalidOperationException("Kunde inte hitta nödvändiga kolumner (Datum, Typ av transaktion, Belopp) i CSV-filen.");
        }

        for (int i = 1; i < lines.Length; i++)
        {
            try
            {
                var columns = SplitLine(lines[i], separator);
                int maxIndex = Math.Max(dateIndex, Math.Max(typeIndex, amountIndex));
                if (columns.Length <= maxIndex)
                    continue;

                var dateStr = columns[dateIndex].Trim().Trim('"');
                var transactionType = columns[typeIndex].Trim().Trim('"');
                var amountStr = columns[amountIndex].Trim().Trim('"');

                // Build description from type and security/description
                string description;
                if (descriptionIndex != -1 && descriptionIndex < columns.Length)
                {
                    var securityDesc = columns[descriptionIndex].Trim().Trim('"');
                    description = string.IsNullOrWhiteSpace(securityDesc)
                        ? transactionType
                        : $"{transactionType}: {securityDesc}";
                }
                else
                {
                    description = transactionType;
                }

                if (string.IsNullOrWhiteSpace(description) || string.IsNullOrWhiteSpace(dateStr))
                    continue;

                if (!TryParseDate(dateStr, out var date))
                    continue;

                // Normalize amount: remove spaces (thousand separators) and convert decimal comma
                var normalizedAmount = amountStr
                    .Replace("\u00a0", "") // non-breaking space
                    .Replace(" ", "")
                    .Replace(".", "")     // remove potential thousand separator dot
                    .Replace(",", ".");   // decimal comma to dot

                if (!decimal.TryParse(normalizedAmount, NumberStyles.Any, CultureInfo.InvariantCulture, out var amount))
                    continue;

                var currency = "SEK";
                if (currencyIndex != -1 && currencyIndex < columns.Length)
                {
                    var c = columns[currencyIndex].Trim().Trim('"');
                    if (!string.IsNullOrWhiteSpace(c))
                        currency = c;
                }

                var transaction = new Transaction
                {
                    Date = date,
                    Amount = Math.Abs(amount),
                    IsIncome = amount > 0,
                    Description = description.Length > 500 ? description[..500] : description,
                    Currency = currency
                };

                transactions.Add(transaction);
            }
            catch
            {
                // Skip invalid rows
            }
        }

        return transactions;
    }

    private static char DetectSeparator(string line)
    {
        if (line.Contains(';')) return ';';
        if (line.Contains('\t')) return '\t';
        return ',';
    }

    private static string[] SplitLine(string line, char separator)
    {
        // Handle quoted fields
        var result = new List<string>();
        var current = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == separator && !inQuotes)
            {
                result.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }
        result.Add(current.ToString());
        return result.ToArray();
    }

    private static int FindColumnIndex(string[] header, string[] possibleNames)
    {
        for (int i = 0; i < header.Length; i++)
        {
            var columnName = header[i].Trim().Trim('"').ToLower();
            foreach (var name in possibleNames)
            {
                if (columnName.Contains(name.ToLower()))
                    return i;
            }
        }
        return -1;
    }

    private static bool TryParseDate(string dateStr, out DateTime date)
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
