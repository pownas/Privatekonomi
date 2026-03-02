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

        // Look for account number in metadata lines before the header
        // ICA-banken sometimes includes "Kontonummer: XXXXXXXX" or similar in header lines
        string? accountNumber = null;
        string? clearingNumber = null;
        int headerLineIndex = 0;
        for (int i = 0; i < Math.Min(lines.Length, 5); i++)
        {
            var lower = lines[i].ToLowerInvariant();
            if (lower.Contains("datum") && (lower.Contains("belopp") || lower.Contains("amount")))
            {
                headerLineIndex = i;
                break;
            }
            // Try to extract account info from metadata lines
            ExtractAccountFromMetadataLine(lines[i], ref clearingNumber, ref accountNumber);
        }

        // Detect separator (semicolon or comma)
        var separator = lines[headerLineIndex].Contains(';') ? ';' : ',';

        // Parse header to find column indices
        var header = lines[headerLineIndex].Split(separator);
        var dateIndex = FindColumnIndex(header, new[] { "datum", "date" });
        var amountIndex = FindColumnIndex(header, new[] { "belopp", "amount" });
        var descriptionIndex = FindColumnIndex(header, new[] { "beskrivning", "text", "beskrivning / text", "description" });

        if (dateIndex == -1 || amountIndex == -1 || descriptionIndex == -1)
        {
            throw new InvalidOperationException("Kunde inte hitta nödvändiga kolumner i CSV-filen");
        }

        // Parse data rows
        for (int i = headerLineIndex + 1; i < lines.Length; i++)
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
                    Description = description.Length > 500 ? description.Substring(0, 500) : description,
                    ClearingNumber = clearingNumber,
                    AccountNumber = accountNumber
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

    /// <summary>
    /// Tries to extract clearing number and account number from a metadata line.
    /// ICA-banken may export lines like "Kontonummer;9270;1234567" or "Konto: 9270-1234567".
    /// </summary>
    private static void ExtractAccountFromMetadataLine(string line, ref string? clearingNumber, ref string? accountNumber)
    {
        // Pattern: "Kontonummer;9270;1234567" (semicolon-separated)
        var parts = line.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 3)
        {
            var label = parts[0].Trim();
            if (label.Contains("kontonummer", StringComparison.OrdinalIgnoreCase) ||
                label.Contains("konto", StringComparison.OrdinalIgnoreCase))
            {
                var potential1 = parts[1].Trim();
                var potential2 = parts[2].Trim();
                // If both look numeric, treat first as clearing and second as account
                if (potential1.All(char.IsDigit) && potential2.All(char.IsDigit))
                {
                    clearingNumber = potential1;
                    accountNumber = potential2;
                    return;
                }
                // If only second is numeric and longer, treat as full account number
                if (potential2.All(char.IsDigit) && potential2.Length >= 4)
                {
                    accountNumber = potential2;
                    return;
                }
            }
        }

        // Pattern: "Kontonummer: 9270-1234567" or "Konto: 12345678"
        if (parts.Length >= 2)
        {
            var label = parts[0].Trim();
            if (label.Contains("kontonummer", StringComparison.OrdinalIgnoreCase) ||
                label.Contains("konto", StringComparison.OrdinalIgnoreCase))
            {
                var value = parts[1].Trim().Replace(" ", "");
                var dashIndex = value.IndexOf('-');
                if (dashIndex > 0)
                {
                    clearingNumber = value.Substring(0, dashIndex);
                    accountNumber = value.Substring(dashIndex + 1);
                }
                else if (value.All(char.IsDigit) && value.Length >= 4)
                {
                    accountNumber = value;
                }
            }
        }
    }

    private int FindColumnIndex(string[] header, string[] possibleNames)
    {
        for (int i = 0; i < header.Length; i++)
        {
            var columnName = header[i].Trim();
            foreach (var name in possibleNames)
            {
                if (columnName.Contains(name, StringComparison.OrdinalIgnoreCase))
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
