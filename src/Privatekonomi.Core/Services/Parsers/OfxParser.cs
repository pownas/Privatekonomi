using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services.Parsers;

/// <summary>
/// Parser for OFX (Open Financial Exchange) files.
/// Supports Swedish banks that export in OFX format.
/// </summary>
public class OfxParser : ICsvParser
{
    public string BankName => "OFX (Allmän)";

    public bool CanParse(string content)
    {
        // OFX files typically start with OFXHEADER or <?OFX or <OFX>
        return content.Contains("OFXHEADER:") || 
               content.Contains("<OFX>") || 
               content.Contains("<?OFX");
    }

    public async Task<List<Transaction>> ParseAsync(Stream stream)
    {
        var transactions = new List<Transaction>();
        
        using var reader = new StreamReader(stream, Encoding.UTF8);
        var content = await reader.ReadToEndAsync();
        
        // Convert SGML-style OFX to XML-style OFX
        var xmlContent = ConvertOfxToXml(content);
        
        try
        {
            var doc = XDocument.Parse(xmlContent);
            
            // Find all STMTTRN elements (bank transactions)
            var stmtTransactions = doc.Descendants("STMTTRN");
            
            foreach (var stmtTrn in stmtTransactions)
            {
                try
                {
                    var transaction = ParseTransaction(stmtTrn);
                    if (transaction != null)
                    {
                        transactions.Add(transaction);
                    }
                }
                catch
                {
                    // Skip invalid transactions
                    continue;
                }
            }
            
            // Also check for credit card transactions (CCSTMTTRN)
            var ccTransactions = doc.Descendants("CCSTMTTRN");
            foreach (var ccTrn in ccTransactions)
            {
                try
                {
                    var transaction = ParseTransaction(ccTrn);
                    if (transaction != null)
                    {
                        transactions.Add(transaction);
                    }
                }
                catch
                {
                    // Skip invalid transactions
                    continue;
                }
            }
        }
        catch (Exception)
        {
            // If XML parsing fails, try alternative parsing
            transactions = ParseOfxWithoutXml(content);
        }
        
        return transactions;
    }

    private Transaction? ParseTransaction(XElement stmtTrn)
    {
        // Extract required fields
        var trnTypeElement = stmtTrn.Element("TRNTYPE");
        var dtPostedElement = stmtTrn.Element("DTPOSTED");
        var trnAmtElement = stmtTrn.Element("TRNAMT");
        var nameElement = stmtTrn.Element("NAME") ?? stmtTrn.Element("MEMO");
        
        if (dtPostedElement == null || trnAmtElement == null)
        {
            return null;
        }
        
        // Parse date (format: YYYYMMDD or YYYYMMDDHHMMSS)
        var dateStr = dtPostedElement.Value.Trim();
        if (!TryParseOfxDate(dateStr, out var date))
        {
            return null;
        }
        
        // Parse amount
        var amountStr = trnAmtElement.Value.Trim().Replace(",", ".");
        if (!decimal.TryParse(amountStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var amount))
        {
            return null;
        }
        
        // Build description from NAME and MEMO
        var name = stmtTrn.Element("NAME")?.Value?.Trim() ?? string.Empty;
        var memo = stmtTrn.Element("MEMO")?.Value?.Trim() ?? string.Empty;
        var description = BuildDescription(name, memo);
        
        if (string.IsNullOrWhiteSpace(description))
        {
            description = "Transaktion utan beskrivning";
        }
        
        // Determine if income based on amount sign
        var isIncome = amount > 0;
        
        // Get transaction type for additional context
        var trnType = trnTypeElement?.Value?.Trim().ToUpper() ?? string.Empty;
        if (trnType == "CREDIT" || trnType == "DEP" || trnType == "INT" || trnType == "DIV")
        {
            isIncome = true;
        }
        else if (trnType == "DEBIT" || trnType == "PAYMENT" || trnType == "FEE" || trnType == "SRVCHG")
        {
            isIncome = false;
        }
        
        return new Transaction
        {
            Date = date,
            Amount = Math.Abs(amount),
            IsIncome = isIncome,
            Description = description.Length > 500 ? description.Substring(0, 500) : description,
            Imported = true,
            ImportSource = "OFX Import",
            Currency = "SEK",
            CreatedAt = DateTime.UtcNow,
            ValidFrom = DateTime.UtcNow
        };
    }

    private string ConvertOfxToXml(string ofxContent)
    {
        // Remove SGML header if present
        var headerEndIndex = ofxContent.IndexOf("<OFX>", StringComparison.OrdinalIgnoreCase);
        if (headerEndIndex == -1)
        {
            headerEndIndex = ofxContent.IndexOf("<OFX ", StringComparison.OrdinalIgnoreCase);
        }
        
        if (headerEndIndex > 0)
        {
            ofxContent = ofxContent.Substring(headerEndIndex);
        }
        
        // Clean up whitespace and newlines
        ofxContent = ofxContent.Trim();
        
        // Convert SGML to XML by adding closing tags
        // OFX SGML uses tags like <TAG>value without closing tags
        var result = new StringBuilder();
        var lines = ofxContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        
        var tagStack = new Stack<string>();
        var selfClosingTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "DTSERVER", "DTSTART", "DTEND", "DTASOF", "DTPOSTED", "DTUSER",
            "TRNTYPE", "TRNAMT", "FITID", "NAME", "MEMO", "CHECKNUM", "REFNUM",
            "BALAMT", "CURDEF", "ACCTID", "ACCTTYPE", "BANKID", "BRANCHID",
            "LANGUAGE", "INTU.BID", "CODE", "SEVERITY", "MESSAGE"
        };
        
        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed)) continue;
            
            // Check if it's a closing tag
            if (trimmed.StartsWith("</"))
            {
                result.AppendLine(trimmed);
                if (tagStack.Count > 0)
                {
                    tagStack.Pop();
                }
                continue;
            }
            
            // Check if it's an opening tag with value
            var match = Regex.Match(trimmed, @"^<([A-Z0-9_.]+)>(.*)$", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                var tagName = match.Groups[1].Value;
                var value = match.Groups[2].Value;
                
                if (!string.IsNullOrEmpty(value))
                {
                    // Self-closing tag with value
                    result.AppendLine($"<{tagName}>{value}</{tagName}>");
                }
                else if (selfClosingTags.Contains(tagName))
                {
                    // Known data tag without value - add empty closing tag
                    result.AppendLine($"<{tagName}></{tagName}>");
                }
                else
                {
                    // Container tag
                    result.AppendLine($"<{tagName}>");
                    tagStack.Push(tagName);
                }
            }
            else
            {
                result.AppendLine(trimmed);
            }
        }
        
        // Close any remaining open tags
        while (tagStack.Count > 0)
        {
            result.AppendLine($"</{tagStack.Pop()}>");
        }
        
        return result.ToString();
    }

    private List<Transaction> ParseOfxWithoutXml(string content)
    {
        var transactions = new List<Transaction>();
        
        // Extract STMTTRN blocks using regex
        var stmtTrnPattern = new Regex(@"<STMTTRN>(.*?)</STMTTRN>", 
            RegexOptions.Singleline | RegexOptions.IgnoreCase);
        
        // Also try pattern without closing tag (SGML style)
        var matches = stmtTrnPattern.Matches(content);
        
        if (matches.Count == 0)
        {
            // Try SGML-style parsing
            var trnBlocks = ExtractSgmlBlocks(content, "STMTTRN");
            foreach (var block in trnBlocks)
            {
                var transaction = ParseSgmlTransaction(block);
                if (transaction != null)
                {
                    transactions.Add(transaction);
                }
            }
        }
        else
        {
            foreach (Match match in matches)
            {
                var transaction = ParseSgmlTransaction(match.Groups[1].Value);
                if (transaction != null)
                {
                    transactions.Add(transaction);
                }
            }
        }
        
        return transactions;
    }

    private List<string> ExtractSgmlBlocks(string content, string tagName)
    {
        var blocks = new List<string>();
        var openTag = $"<{tagName}>";
        var closeTag = $"</{tagName}>";
        
        var startIndex = 0;
        while (true)
        {
            var openIndex = content.IndexOf(openTag, startIndex, StringComparison.OrdinalIgnoreCase);
            if (openIndex == -1) break;
            
            var closeIndex = content.IndexOf(closeTag, openIndex, StringComparison.OrdinalIgnoreCase);
            if (closeIndex == -1)
            {
                // Try to find next opening tag as end
                var nextOpenIndex = content.IndexOf(openTag, openIndex + openTag.Length, StringComparison.OrdinalIgnoreCase);
                if (nextOpenIndex == -1)
                {
                    closeIndex = content.Length;
                }
                else
                {
                    closeIndex = nextOpenIndex;
                }
            }
            
            var blockContent = content.Substring(openIndex + openTag.Length, closeIndex - openIndex - openTag.Length);
            blocks.Add(blockContent);
            
            startIndex = closeIndex + closeTag.Length;
            if (startIndex >= content.Length) break;
        }
        
        return blocks;
    }

    private Transaction? ParseSgmlTransaction(string blockContent)
    {
        var date = ExtractSgmlValue(blockContent, "DTPOSTED");
        var amount = ExtractSgmlValue(blockContent, "TRNAMT");
        var name = ExtractSgmlValue(blockContent, "NAME");
        var memo = ExtractSgmlValue(blockContent, "MEMO");
        var trnType = ExtractSgmlValue(blockContent, "TRNTYPE");
        
        if (string.IsNullOrEmpty(date) || string.IsNullOrEmpty(amount))
        {
            return null;
        }
        
        if (!TryParseOfxDate(date, out var parsedDate))
        {
            return null;
        }
        
        var amountStr = amount.Replace(",", ".");
        if (!decimal.TryParse(amountStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedAmount))
        {
            return null;
        }
        
        var description = BuildDescription(name ?? string.Empty, memo ?? string.Empty);
        if (string.IsNullOrWhiteSpace(description))
        {
            description = "Transaktion utan beskrivning";
        }
        
        var isIncome = parsedAmount > 0;
        if (!string.IsNullOrEmpty(trnType))
        {
            var type = trnType.ToUpper();
            if (type == "CREDIT" || type == "DEP" || type == "INT" || type == "DIV")
            {
                isIncome = true;
            }
            else if (type == "DEBIT" || type == "PAYMENT" || type == "FEE" || type == "SRVCHG")
            {
                isIncome = false;
            }
        }
        
        return new Transaction
        {
            Date = parsedDate,
            Amount = Math.Abs(parsedAmount),
            IsIncome = isIncome,
            Description = description.Length > 500 ? description.Substring(0, 500) : description,
            Imported = true,
            ImportSource = "OFX Import",
            Currency = "SEK",
            CreatedAt = DateTime.UtcNow,
            ValidFrom = DateTime.UtcNow
        };
    }

    private string? ExtractSgmlValue(string content, string tagName)
    {
        var pattern = new Regex($@"<{tagName}>([^<\r\n]+)", RegexOptions.IgnoreCase);
        var match = pattern.Match(content);
        
        if (match.Success)
        {
            return match.Groups[1].Value.Trim();
        }
        
        return null;
    }

    private bool TryParseOfxDate(string dateStr, out DateTime date)
    {
        date = DateTime.MinValue;
        
        if (string.IsNullOrEmpty(dateStr))
        {
            return false;
        }
        
        // Remove timezone offset if present (e.g., [-5:EST])
        var bracketIndex = dateStr.IndexOf('[');
        if (bracketIndex > 0)
        {
            dateStr = dateStr.Substring(0, bracketIndex);
        }
        
        // Remove any non-digit characters except the date part
        dateStr = dateStr.Trim();
        
        // Format: YYYYMMDD or YYYYMMDDHHMMSS or YYYYMMDDHHMMSS.XXX
        var formats = new[]
        {
            "yyyyMMddHHmmss.fff",
            "yyyyMMddHHmmss",
            "yyyyMMdd",
            "yyyy-MM-dd",
            "dd/MM/yyyy"
        };
        
        foreach (var format in formats)
        {
            if (DateTime.TryParseExact(dateStr, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
            {
                return true;
            }
        }
        
        // Try generic parsing
        return DateTime.TryParse(dateStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
    }

    private string BuildDescription(string name, string memo)
    {
        if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(memo))
        {
            // Only combine if they're different
            if (!name.Equals(memo, StringComparison.OrdinalIgnoreCase))
            {
                return $"{name} - {memo}";
            }
            return name;
        }
        else if (!string.IsNullOrWhiteSpace(name))
        {
            return name;
        }
        else
        {
            return memo;
        }
    }
}
