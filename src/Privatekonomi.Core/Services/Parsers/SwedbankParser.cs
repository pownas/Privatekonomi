using System.Globalization;
using System.Text;
using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services.Parsers;

public class SwedbankParser : ICsvParser
{
    public string BankName => "Swedbank";

    public bool CanParse(string csvContent)
    {
        var lines = csvContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length < 2) return false;

        var header = lines[0].ToLower();
        
        // Check for new CSN format (Swedish column names, tab-separated)
        if (header.Contains("radnummer") && header.Contains("bokföringsdag") && 
            header.Contains("belopp") && header.Contains("beskrivning"))
        {
            return true;
        }
        
        // Check for old format (English column names, semicolon-separated)
        return header.Contains("row type") && header.Contains("debit/credit") && 
               (header.Contains("client account") || header.Contains("details"));
    }

    public async Task<List<Transaction>> ParseAsync(Stream csvStream)
    {
        var transactions = new List<Transaction>();
        
        using var reader = new StreamReader(csvStream, Encoding.UTF8);
        var content = await reader.ReadToEndAsync();
        
        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length < 2) return transactions;

        var header = lines[0].ToLower();
        
        // Detect format and parse accordingly
        if (header.Contains("radnummer") && header.Contains("bokföringsdag"))
        {
            // New CSN format with Swedish column names (tab-separated)
            return await ParseCsnFormatAsync(lines);
        }
        else
        {
            // Old format with English column names (semicolon-separated)
            return await ParseOldFormatAsync(lines);
        }
    }

    private async Task<List<Transaction>> ParseCsnFormatAsync(string[] lines)
    {
        var transactions = new List<Transaction>();
        
        // Detect separator (comma, tab, or semicolon) from header
        var separator = DetectSeparator(lines[0]);
        
        // Parse header - handle quoted fields
        var header = ParseCsvLine(lines[0], separator).ToArray();
        var dateIndex = FindColumnIndex(header, new[] { "bokföringsdag", "transaktionsdag" });
        var amountIndex = FindColumnIndex(header, new[] { "belopp" });
        var descriptionIndex = FindColumnIndex(header, new[] { "beskrivning" });
        var referenceIndex = FindColumnIndex(header, new[] { "referens" });
        var currencyIndex = FindColumnIndex(header, new[] { "valuta" });

        if (dateIndex == -1 || amountIndex == -1)
        {
            throw new InvalidOperationException("Kunde inte hitta nödvändiga kolumner i Swedbank CSV-filen");
        }

        // Parse data rows
        for (int i = 1; i < lines.Length; i++)
        {
            try
            {
                var columns = ParseCsvLine(lines[i], separator).ToArray();
                if (columns.Length <= Math.Max(dateIndex, amountIndex))
                    continue;

                // Check currency if available (only process SEK for now)
                if (currencyIndex != -1 && columns.Length > currencyIndex)
                {
                    var currency = columns[currencyIndex].Trim();
                    if (!string.IsNullOrEmpty(currency) && currency.ToUpper() != "SEK")
                        continue; // Skip non-SEK transactions
                }

                var dateStr = columns[dateIndex].Trim();
                var amountStr = columns[amountIndex].Trim().Replace(",", ".");

                // Build description from available fields
                var description = string.Empty;
                if (descriptionIndex != -1 && columns.Length > descriptionIndex)
                {
                    description = columns[descriptionIndex].Trim();
                }
                if (string.IsNullOrWhiteSpace(description) && referenceIndex != -1 && columns.Length > referenceIndex)
                {
                    description = columns[referenceIndex].Trim();
                }

                if (string.IsNullOrWhiteSpace(description))
                    continue;

                // Parse date (YYYY-MM-DD format for CSN)
                if (!TryParseDate(dateStr, out var date))
                    continue;

                // Parse amount
                if (!decimal.TryParse(amountStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var amount))
                    continue;

                // Determine if income based on sign (negative = expense, positive = income)
                var isIncome = amount > 0;

                var transaction = new Transaction
                {
                    Date = date,
                    Amount = Math.Abs(amount),
                    IsIncome = isIncome,
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

        return await Task.FromResult(transactions);
    }

    private async Task<List<Transaction>> ParseOldFormatAsync(string[] lines)
    {
        var transactions = new List<Transaction>();
        
        // Swedbank uses semicolon separator and quotes
        var separator = ';';
        
        // Parse header
        var header = ParseCsvLine(lines[0], separator).ToArray();
        var rowTypeIndex = FindColumnIndex(header, new[] { "row type" });
        var dateIndex = FindColumnIndex(header, new[] { "date" });
        var amountIndex = FindColumnIndex(header, new[] { "amount" });
        var debitCreditIndex = FindColumnIndex(header, new[] { "debit/credit" });
        var detailsIndex = FindColumnIndex(header, new[] { "details" });
        var beneficiaryIndex = FindColumnIndex(header, new[] { "beneficiary/payer" });
        var currencyIndex = FindColumnIndex(header, new[] { "currency" });

        if (rowTypeIndex == -1 || dateIndex == -1 || amountIndex == -1 || debitCreditIndex == -1 || detailsIndex == -1)
        {
            throw new InvalidOperationException("Kunde inte hitta nödvändiga kolumner i Swedbank CSV-filen");
        }

        // Parse data rows
        for (int i = 1; i < lines.Length; i++)
        {
            try
            {
                var columns = ParseCsvLine(lines[i], separator).ToArray();
                if (columns.Length <= Math.Max(rowTypeIndex, Math.Max(dateIndex, Math.Max(amountIndex, debitCreditIndex))))
                    continue;

                var rowType = columns[rowTypeIndex].Trim();
                
                // Skip non-transaction rows (10=opening balance, 82=turnover, 86=closing balance)
                if (rowType != "20")
                    continue;

                // Check currency if available (only process SEK for now)
                if (currencyIndex != -1 && columns.Length > currencyIndex)
                {
                    var currency = columns[currencyIndex].Trim();
                    if (!string.IsNullOrEmpty(currency) && currency != "SEK")
                        continue; // Skip non-SEK transactions
                }

                var dateStr = columns[dateIndex].Trim();
                var amountStr = columns[amountIndex].Trim().Replace(",", ".");
                var debitCredit = columns[debitCreditIndex].Trim().ToUpper();
                var details = columns[detailsIndex].Trim();
                var beneficiary = beneficiaryIndex != -1 && columns.Length > beneficiaryIndex 
                    ? columns[beneficiaryIndex].Trim() 
                    : string.Empty;

                // Parse date (DD.MM.YYYY format)
                if (!TryParseDate(dateStr, out var date))
                    continue;

                // Parse amount
                if (!decimal.TryParse(amountStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var amount))
                    continue;

                // Determine if income based on Debit/Credit flag
                var isIncome = debitCredit == "K"; // K = Kredit (income), D = Debet (expense)

                // Build description from beneficiary and details
                var description = BuildDescription(beneficiary, details);
                if (string.IsNullOrWhiteSpace(description))
                    continue;

                var transaction = new Transaction
                {
                    Date = date,
                    Amount = Math.Abs(amount),
                    IsIncome = isIncome,
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

        return await Task.FromResult(transactions);
    }

    private string BuildDescription(string beneficiary, string details)
    {
        if (!string.IsNullOrWhiteSpace(beneficiary) && !string.IsNullOrWhiteSpace(details))
        {
            return $"{beneficiary} - {details}";
        }
        else if (!string.IsNullOrWhiteSpace(beneficiary))
        {
            return beneficiary;
        }
        else
        {
            return details;
        }
    }

    private List<string> ParseCsvLine(string line, char separator)
    {
        var result = new List<string>();
        var currentField = new StringBuilder();
        var insideQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            var c = line[i];

            if (c == '"')
            {
                insideQuotes = !insideQuotes;
            }
            else if (c == separator && !insideQuotes)
            {
                result.Add(currentField.ToString());
                currentField.Clear();
            }
            else
            {
                currentField.Append(c);
            }
        }

        result.Add(currentField.ToString());
        return result;
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
            "dd.MM.yyyy",
            "yyyy-MM-dd",
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

    private char DetectSeparator(string headerLine)
    {
        // Count occurrences of potential separators (excluding those inside quotes)
        var commaCount = 0;
        var tabCount = 0;
        var semicolonCount = 0;
        var insideQuotes = false;

        for (int i = 0; i < headerLine.Length; i++)
        {
            var c = headerLine[i];
            
            if (c == '"')
            {
                // Check for escaped quote (two consecutive quotes)
                if (i + 1 < headerLine.Length && headerLine[i + 1] == '"')
                {
                    i++; // Skip the next quote as it's escaped
                }
                else
                {
                    insideQuotes = !insideQuotes;
                }
            }
            else if (!insideQuotes)
            {
                if (c == ',') commaCount++;
                else if (c == '\t') tabCount++;
                else if (c == ';') semicolonCount++;
            }
        }

        // Return the most common separator
        if (commaCount >= tabCount && commaCount >= semicolonCount)
            return ',';
        else if (tabCount >= semicolonCount)
            return '\t';
        else
            return ';';
    }
}
