using System.Text;
using Privatekonomi.Core.Services.Parsers;
using Xunit;

namespace Privatekonomi.Core.Tests;

public class OfxParserTests
{
    private readonly OfxParser _parser;

    public OfxParserTests()
    {
        _parser = new OfxParser();
    }

    // Sample OFX file content (SGML style - older format)
    private const string SgmlOfxContent = @"OFXHEADER:100
DATA:OFXSGML
VERSION:102
SECURITY:NONE
ENCODING:USASCII
CHARSET:1252
COMPRESSION:NONE
OLDFILEUID:NONE
NEWFILEUID:NONE

<OFX>
<SIGNONMSGSRSV1>
<SONRS>
<STATUS>
<CODE>0
<SEVERITY>INFO
</STATUS>
<DTSERVER>20250115120000
<LANGUAGE>ENG
</SONRS>
</SIGNONMSGSRSV1>
<BANKMSGSRSV1>
<STMTTRNRS>
<STMTRS>
<CURDEF>SEK
<BANKACCTFROM>
<BANKID>1234
<ACCTID>987654321
<ACCTTYPE>CHECKING
</BANKACCTFROM>
<BANKTRANLIST>
<DTSTART>20250101000000
<DTEND>20250115235959
<STMTTRN>
<TRNTYPE>DEBIT
<DTPOSTED>20250110000000
<TRNAMT>-125.50
<FITID>20250110001
<NAME>ICA MAXI STOCKHOLM
<MEMO>Kortköp
</STMTTRN>
<STMTTRN>
<TRNTYPE>CREDIT
<DTPOSTED>20250115000000
<TRNAMT>3500.00
<FITID>20250115001
<NAME>Lön
<MEMO>Månadslön
</STMTTRN>
<STMTTRN>
<TRNTYPE>DEBIT
<DTPOSTED>20250112000000
<TRNAMT>-89.00
<FITID>20250112001
<NAME>COOP FORUM
</STMTTRN>
</BANKTRANLIST>
<LEDGERBAL>
<BALAMT>8742.30
<DTASOF>20250115120000
</LEDGERBAL>
</STMTRS>
</STMTTRNRS>
</BANKMSGSRSV1>
</OFX>";

    // Sample OFX file content (XML style - newer format)
    private const string XmlOfxContent = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<?OFX OFXHEADER=""200"" VERSION=""220"" SECURITY=""NONE"" OLDFILEUID=""NONE"" NEWFILEUID=""NONE""?>
<OFX>
<SIGNONMSGSRSV1>
<SONRS>
<STATUS>
<CODE>0</CODE>
<SEVERITY>INFO</SEVERITY>
</STATUS>
<DTSERVER>20250115120000</DTSERVER>
<LANGUAGE>ENG</LANGUAGE>
</SONRS>
</SIGNONMSGSRSV1>
<BANKMSGSRSV1>
<STMTTRNRS>
<STMTRS>
<CURDEF>SEK</CURDEF>
<BANKACCTFROM>
<BANKID>1234</BANKID>
<ACCTID>987654321</ACCTID>
<ACCTTYPE>CHECKING</ACCTTYPE>
</BANKACCTFROM>
<BANKTRANLIST>
<DTSTART>20250101000000</DTSTART>
<DTEND>20250115235959</DTEND>
<STMTTRN>
<TRNTYPE>DEBIT</TRNTYPE>
<DTPOSTED>20250110000000</DTPOSTED>
<TRNAMT>-125.50</TRNAMT>
<FITID>20250110001</FITID>
<NAME>ICA MAXI STOCKHOLM</NAME>
<MEMO>Kortköp</MEMO>
</STMTTRN>
<STMTTRN>
<TRNTYPE>CREDIT</TRNTYPE>
<DTPOSTED>20250115000000</DTPOSTED>
<TRNAMT>3500.00</TRNAMT>
<FITID>20250115001</FITID>
<NAME>Lön</NAME>
<MEMO>Månadslön</MEMO>
</STMTTRN>
</BANKTRANLIST>
</STMTRS>
</STMTTRNRS>
</BANKMSGSRSV1>
</OFX>";

    [Fact]
    public void BankName_ReturnsOfxAllman()
    {
        Assert.Equal("OFX (Allmän)", _parser.BankName);
    }

    [Fact]
    public void CanParse_ReturnsTrueForOfxHeader()
    {
        var result = _parser.CanParse(SgmlOfxContent);
        Assert.True(result);
    }

    [Fact]
    public void CanParse_ReturnsTrueForXmlOfx()
    {
        var result = _parser.CanParse(XmlOfxContent);
        Assert.True(result);
    }

    [Fact]
    public void CanParse_ReturnsTrueForOfxTag()
    {
        var content = "<OFX><SIGNONMSGSRSV1></SIGNONMSGSRSV1></OFX>";
        var result = _parser.CanParse(content);
        Assert.True(result);
    }

    [Fact]
    public void CanParse_ReturnsFalseForCsv()
    {
        var csvContent = "Datum;Belopp;Beskrivning\n2025-01-15;-125,50;ICA";
        var result = _parser.CanParse(csvContent);
        Assert.False(result);
    }

    [Fact]
    public async Task ParseAsync_ParsesSgmlOfxCorrectly()
    {
        // Arrange
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(SgmlOfxContent));

        // Act
        var transactions = await _parser.ParseAsync(stream);

        // Assert
        Assert.NotNull(transactions);
        Assert.Equal(3, transactions.Count);

        // Check first transaction (expense)
        var expense = transactions[0];
        Assert.Equal(new DateTime(2025, 1, 10), expense.Date.Date);
        Assert.Equal(125.50m, expense.Amount);
        Assert.False(expense.IsIncome);
        Assert.Contains("ICA MAXI STOCKHOLM", expense.Description);

        // Check second transaction (income)
        var income = transactions[1];
        Assert.Equal(new DateTime(2025, 1, 15), income.Date.Date);
        Assert.Equal(3500.00m, income.Amount);
        Assert.True(income.IsIncome);
        Assert.Contains("Lön", income.Description);
    }

    [Fact]
    public async Task ParseAsync_ParsesXmlOfxCorrectly()
    {
        // Arrange
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(XmlOfxContent));

        // Act
        var transactions = await _parser.ParseAsync(stream);

        // Assert
        Assert.NotNull(transactions);
        Assert.Equal(2, transactions.Count);

        // Check expense
        var expense = transactions.First(t => !t.IsIncome);
        Assert.Equal(new DateTime(2025, 1, 10), expense.Date.Date);
        Assert.Equal(125.50m, expense.Amount);

        // Check income
        var income = transactions.First(t => t.IsIncome);
        Assert.Equal(new DateTime(2025, 1, 15), income.Date.Date);
        Assert.Equal(3500.00m, income.Amount);
    }

    [Fact]
    public async Task ParseAsync_SetsImportedFlagTrue()
    {
        // Arrange
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(SgmlOfxContent));

        // Act
        var transactions = await _parser.ParseAsync(stream);

        // Assert
        Assert.All(transactions, t => Assert.True(t.Imported));
    }

    [Fact]
    public async Task ParseAsync_SetsImportSourceToOfx()
    {
        // Arrange
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(SgmlOfxContent));

        // Act
        var transactions = await _parser.ParseAsync(stream);

        // Assert
        Assert.All(transactions, t => Assert.Equal("OFX Import", t.ImportSource));
    }

    [Fact]
    public async Task ParseAsync_SetsCurrencyToSek()
    {
        // Arrange
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(SgmlOfxContent));

        // Act
        var transactions = await _parser.ParseAsync(stream);

        // Assert
        Assert.All(transactions, t => Assert.Equal("SEK", t.Currency));
    }

    [Fact]
    public async Task ParseAsync_CombinesNameAndMemo()
    {
        // Arrange
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(SgmlOfxContent));

        // Act
        var transactions = await _parser.ParseAsync(stream);

        // Assert
        var icaTransaction = transactions.First(t => t.Description.Contains("ICA"));
        Assert.Contains("Kortköp", icaTransaction.Description);
    }

    [Fact]
    public async Task ParseAsync_HandlesMissingMemo()
    {
        // Arrange - The COOP transaction has no MEMO
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(SgmlOfxContent));

        // Act
        var transactions = await _parser.ParseAsync(stream);

        // Assert
        var coopTransaction = transactions.FirstOrDefault(t => t.Description.Contains("COOP"));
        Assert.NotNull(coopTransaction);
        Assert.Equal("COOP FORUM", coopTransaction.Description);
    }

    [Fact]
    public async Task ParseAsync_HandlesEmptyFile()
    {
        // Arrange
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(""));

        // Act
        var transactions = await _parser.ParseAsync(stream);

        // Assert
        Assert.Empty(transactions);
    }

    [Fact]
    public async Task ParseAsync_HandlesFileWithNoTransactions()
    {
        // Arrange
        var ofxWithNoTransactions = @"OFXHEADER:100
DATA:OFXSGML
VERSION:102
<OFX>
<SIGNONMSGSRSV1>
<SONRS>
<STATUS>
<CODE>0
<SEVERITY>INFO
</STATUS>
</SONRS>
</SIGNONMSGSRSV1>
</OFX>";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(ofxWithNoTransactions));

        // Act
        var transactions = await _parser.ParseAsync(stream);

        // Assert
        Assert.Empty(transactions);
    }

    [Fact]
    public async Task ParseAsync_CorrectlyIdentifiesIncomeVsExpense()
    {
        // Arrange
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(SgmlOfxContent));

        // Act
        var transactions = await _parser.ParseAsync(stream);

        // Assert
        var expenses = transactions.Where(t => !t.IsIncome).ToList();
        var incomes = transactions.Where(t => t.IsIncome).ToList();

        Assert.Equal(2, expenses.Count); // ICA and COOP
        Assert.Single(incomes); // Lön
    }

    [Fact]
    public async Task ParseAsync_HandlesDateWithTimezone()
    {
        // Arrange
        var ofxWithTimezone = @"OFXHEADER:100
DATA:OFXSGML
VERSION:102
<OFX>
<BANKMSGSRSV1>
<STMTTRNRS>
<STMTRS>
<BANKTRANLIST>
<STMTTRN>
<TRNTYPE>DEBIT
<DTPOSTED>20250110120000[-5:EST]
<TRNAMT>-100.00
<FITID>12345
<NAME>Test Transaction
</STMTTRN>
</BANKTRANLIST>
</STMTRS>
</STMTTRNRS>
</BANKMSGSRSV1>
</OFX>";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(ofxWithTimezone));

        // Act
        var transactions = await _parser.ParseAsync(stream);

        // Assert
        Assert.Single(transactions);
        Assert.Equal(new DateTime(2025, 1, 10), transactions[0].Date.Date);
    }

    [Fact]
    public async Task ParseAsync_HandlesDecimalAmountsWithComma()
    {
        // Arrange - Swedish style with comma decimal separator
        var ofxWithComma = @"OFXHEADER:100
DATA:OFXSGML
VERSION:102
<OFX>
<BANKMSGSRSV1>
<STMTTRNRS>
<STMTRS>
<BANKTRANLIST>
<STMTTRN>
<TRNTYPE>DEBIT
<DTPOSTED>20250110
<TRNAMT>-125,50
<FITID>12345
<NAME>Test
</STMTTRN>
</BANKTRANLIST>
</STMTRS>
</STMTTRNRS>
</BANKMSGSRSV1>
</OFX>";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(ofxWithComma));

        // Act
        var transactions = await _parser.ParseAsync(stream);

        // Assert
        Assert.Single(transactions);
        Assert.Equal(125.50m, transactions[0].Amount);
    }

    [Fact]
    public async Task ParseAsync_SkipsInvalidTransactions()
    {
        // Arrange - Transaction without required fields
        var ofxWithInvalid = @"OFXHEADER:100
DATA:OFXSGML
VERSION:102
<OFX>
<BANKMSGSRSV1>
<STMTTRNRS>
<STMTRS>
<BANKTRANLIST>
<STMTTRN>
<TRNTYPE>DEBIT
<DTPOSTED>invalid-date
<TRNAMT>-100.00
<NAME>Invalid Date Transaction
</STMTTRN>
<STMTTRN>
<TRNTYPE>DEBIT
<DTPOSTED>20250110
<TRNAMT>-100.00
<NAME>Valid Transaction
</STMTTRN>
</BANKTRANLIST>
</STMTRS>
</STMTTRNRS>
</BANKMSGSRSV1>
</OFX>";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(ofxWithInvalid));

        // Act
        var transactions = await _parser.ParseAsync(stream);

        // Assert
        Assert.Single(transactions);
        Assert.Contains("Valid Transaction", transactions[0].Description);
    }
}
