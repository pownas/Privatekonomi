using System.Text;
using Privatekonomi.Core.Services.Parsers;
using Xunit;

namespace Privatekonomi.Core.Tests;

public class SwedbankParserTests
{
    // Example from the issue - Swedish format with comma separator and quoted fields
    private const string SwedishFormatCommaSeparated = @"Radnummer,Clearingnummer,Kontonummer,Produkt,Valuta,Bokföringsdag,Transaktionsdag,Valutadag,Referens,Beskrivning,Belopp,Bokfört saldo
1,84525,1234567891,""e-sparkonto"",SEK,2025-11-12,2025-11-12,2025-11-12,""50kr/onsda"",""50kr/onsda"",-50.00,9989.74
2,84525,1234567891,""e-sparkonto"",SEK,2025-11-12,2025-11-12,2025-11-12,""CSN Centrala stu"",""CSN Centrala stu"",-3277.00,10039.74
3,84525,1234567891,""e-sparkonto"",SEK,2025-11-11,2025-11-11,2025-11-12,""CSN LÅN NOV"",""CSN LÅN NOV"",3280.00,13316.74";

    // Swedish format with tab separator
    private const string SwedishFormatTabSeparated = @"Radnummer	Clearingnummer	Kontonummer	Produkt	Valuta	Bokföringsdag	Transaktionsdag	Valutadag	Referens	Beskrivning	Belopp	Bokfört saldo
1	84525	1234567891	e-sparkonto	SEK	2025-11-12	2025-11-12	2025-11-12	50kr/onsda	50kr/onsda	-50.00	9989.74
2	84525	1234567891	e-sparkonto	SEK	2025-11-12	2025-11-12	2025-11-12	CSN Centrala stu	CSN Centrala stu	-3277.00	10039.74
3	84525	1234567891	e-sparkonto	SEK	2025-11-11	2025-11-11	2025-11-12	CSN LÅN NOV	CSN LÅN NOV	3280.00	13316.74";

    // Old English format with semicolon separator
    private const string EnglishFormatSemicolonSeparated = @"""Row Type"";""Date"";""Debit/Credit"";""Details"";""Beneficiary/Payer"";""Amount"";""Currency"";""Balance"";""Client Account""
""20"";""15.11.2025"";""D"";""Matinköp ICA"";""ICA SUPERMARKET"";""245.50"";""SEK"";""8500.00"";""1111222333""
""20"";""14.11.2025"";""K"";""Lön"";""FÖRETAG AB"";""25000.00"";""SEK"";""33500.00"";""1111222333""";

    [Fact]
    public void SwedbankParser_CanParse_ReturnsTrueForSwedishCommaSeparated()
    {
        // Arrange
        var parser = new SwedbankParser();

        // Act
        var result = parser.CanParse(SwedishFormatCommaSeparated);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void SwedbankParser_CanParse_ReturnsTrueForSwedishTabSeparated()
    {
        // Arrange
        var parser = new SwedbankParser();

        // Act
        var result = parser.CanParse(SwedishFormatTabSeparated);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void SwedbankParser_CanParse_ReturnsTrueForEnglishFormat()
    {
        // Arrange
        var parser = new SwedbankParser();

        // Act
        var result = parser.CanParse(EnglishFormatSemicolonSeparated);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void SwedbankParser_CanParse_ReturnsFalseForInvalidFormat()
    {
        // Arrange
        var parser = new SwedbankParser();
        var invalidCsv = "Name,Amount\nTest,100";

        // Act
        var result = parser.CanParse(invalidCsv);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task SwedbankParser_ParseAsync_ParsesSwedishCommaSeparatedCorrectly()
    {
        // Arrange
        var parser = new SwedbankParser();
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(SwedishFormatCommaSeparated));

        // Act
        var transactions = await parser.ParseAsync(stream);

        // Assert
        Assert.NotNull(transactions);
        Assert.Equal(3, transactions.Count);

        // Check first transaction (expense)
        var transaction1 = transactions[0];
        Assert.Equal(new DateTime(2025, 11, 12), transaction1.Date);
        Assert.Equal(50.00m, transaction1.Amount);
        Assert.False(transaction1.IsIncome); // Negative amount = expense
        Assert.Equal("50kr/onsda", transaction1.Description);

        // Check second transaction (expense)
        var transaction2 = transactions[1];
        Assert.Equal(new DateTime(2025, 11, 12), transaction2.Date);
        Assert.Equal(3277.00m, transaction2.Amount);
        Assert.False(transaction2.IsIncome);
        Assert.Equal("CSN Centrala stu", transaction2.Description);

        // Check third transaction (income)
        var transaction3 = transactions[2];
        Assert.Equal(new DateTime(2025, 11, 11), transaction3.Date);
        Assert.Equal(3280.00m, transaction3.Amount);
        Assert.True(transaction3.IsIncome); // Positive amount = income
        Assert.Equal("CSN LÅN NOV", transaction3.Description);
    }

    [Fact]
    public async Task SwedbankParser_ParseAsync_ParsesSwedishTabSeparatedCorrectly()
    {
        // Arrange
        var parser = new SwedbankParser();
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(SwedishFormatTabSeparated));

        // Act
        var transactions = await parser.ParseAsync(stream);

        // Assert
        Assert.NotNull(transactions);
        Assert.Equal(3, transactions.Count);

        // Verify same data as comma-separated
        Assert.Equal(50.00m, transactions[0].Amount);
        Assert.Equal(3277.00m, transactions[1].Amount);
        Assert.Equal(3280.00m, transactions[2].Amount);
    }

    [Fact]
    public async Task SwedbankParser_ParseAsync_ParsesEnglishFormatCorrectly()
    {
        // Arrange
        var parser = new SwedbankParser();
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(EnglishFormatSemicolonSeparated));

        // Act
        var transactions = await parser.ParseAsync(stream);

        // Assert
        Assert.NotNull(transactions);
        Assert.Equal(2, transactions.Count);

        // Check expense transaction
        var expense = transactions[0];
        Assert.Equal(new DateTime(2025, 11, 15), expense.Date);
        Assert.Equal(245.50m, expense.Amount);
        Assert.False(expense.IsIncome); // D = Debit = expense
        Assert.Equal("ICA SUPERMARKET - Matinköp ICA", expense.Description);

        // Check income transaction
        var income = transactions[1];
        Assert.Equal(new DateTime(2025, 11, 14), income.Date);
        Assert.Equal(25000.00m, income.Amount);
        Assert.True(income.IsIncome); // K = Kredit = income
        Assert.Equal("FÖRETAG AB - Lön", income.Description);
    }

    [Fact]
    public async Task SwedbankParser_ParseAsync_SkipsNonSEKTransactions()
    {
        // Arrange
        var parser = new SwedbankParser();
        var csvWithMultipleCurrencies = @"Radnummer,Clearingnummer,Kontonummer,Produkt,Valuta,Bokföringsdag,Transaktionsdag,Valutadag,Referens,Beskrivning,Belopp,Bokfört saldo
1,84525,1234567891,""e-sparkonto"",SEK,2025-11-12,2025-11-12,2025-11-12,""SEK Transaction"",""SEK Transaction"",-50.00,9989.74
2,84525,1234567891,""e-sparkonto"",USD,2025-11-12,2025-11-12,2025-11-12,""USD Transaction"",""USD Transaction"",-100.00,10039.74
3,84525,1234567891,""e-sparkonto"",SEK,2025-11-11,2025-11-11,2025-11-12,""Another SEK"",""Another SEK"",3280.00,13316.74";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvWithMultipleCurrencies));

        // Act
        var transactions = await parser.ParseAsync(stream);

        // Assert
        Assert.Equal(2, transactions.Count); // Only SEK transactions
        Assert.All(transactions, t => Assert.NotEqual("USD Transaction", t.Description));
    }

    [Fact]
    public async Task SwedbankParser_ParseAsync_HandlesDecimalCommasCorrectly()
    {
        // Arrange
        var parser = new SwedbankParser();
        var csvWithDecimalComma = @"Radnummer,Clearingnummer,Kontonummer,Produkt,Valuta,Bokföringsdag,Transaktionsdag,Valutadag,Referens,Beskrivning,Belopp,Bokfört saldo
1,84525,1234567891,""e-sparkonto"",SEK,2025-11-12,2025-11-12,2025-11-12,""Test"",""Test"",""1234,56"",9989.74";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvWithDecimalComma));

        // Act
        var transactions = await parser.ParseAsync(stream);

        // Assert
        Assert.Single(transactions);
        Assert.Equal(1234.56m, transactions[0].Amount);
    }

    [Fact]
    public async Task SwedbankParser_ParseAsync_SkipsRowsWithMissingDescription()
    {
        // Arrange
        var parser = new SwedbankParser();
        var csvWithMissingDescription = @"Radnummer,Clearingnummer,Kontonummer,Produkt,Valuta,Bokföringsdag,Transaktionsdag,Valutadag,Referens,Beskrivning,Belopp,Bokfört saldo
1,84525,1234567891,""e-sparkonto"",SEK,2025-11-12,2025-11-12,2025-11-12,"""","""",50.00,9989.74
2,84525,1234567891,""e-sparkonto"",SEK,2025-11-11,2025-11-11,2025-11-12,""Valid"",""Valid"",100.00,10039.74";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvWithMissingDescription));

        // Act
        var transactions = await parser.ParseAsync(stream);

        // Assert
        Assert.Single(transactions); // Only the row with valid description
        Assert.Equal("Valid", transactions[0].Description);
    }

    [Fact]
    public async Task SwedbankParser_ParseAsync_FallbackToReferenceIfDescriptionEmpty()
    {
        // Arrange
        var parser = new SwedbankParser();
        var csvWithReferenceOnly = @"Radnummer,Clearingnummer,Kontonummer,Produkt,Valuta,Bokföringsdag,Transaktionsdag,Valutadag,Referens,Beskrivning,Belopp,Bokfört saldo
1,84525,1234567891,""e-sparkonto"",SEK,2025-11-12,2025-11-12,2025-11-12,""Reference Text"","""",50.00,9989.74";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvWithReferenceOnly));

        // Act
        var transactions = await parser.ParseAsync(stream);

        // Assert
        Assert.Single(transactions);
        Assert.Equal("Reference Text", transactions[0].Description);
    }

    [Fact]
    public async Task SwedbankParser_ParseAsync_TruncatesLongDescriptions()
    {
        // Arrange
        var parser = new SwedbankParser();
        var longDescription = new string('X', 600); // 600 characters
        var csvWithLongDescription = $@"Radnummer,Clearingnummer,Kontonummer,Produkt,Valuta,Bokföringsdag,Transaktionsdag,Valutadag,Referens,Beskrivning,Belopp,Bokfört saldo
1,84525,1234567891,""e-sparkonto"",SEK,2025-11-12,2025-11-12,2025-11-12,""{longDescription}"",""{longDescription}"",50.00,9989.74";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvWithLongDescription));

        // Act
        var transactions = await parser.ParseAsync(stream);

        // Assert
        Assert.Single(transactions);
        Assert.Equal(500, transactions[0].Description.Length); // Truncated to 500
    }

    [Fact]
    public void SwedbankParser_BankName_ReturnsSwedbank()
    {
        // Arrange
        var parser = new SwedbankParser();

        // Act & Assert
        Assert.Equal("Swedbank", parser.BankName);
    }

    [Fact]
    public async Task SwedbankParser_ParseAsync_ThrowsExceptionForInvalidFormat()
    {
        // Arrange
        var parser = new SwedbankParser();
        var invalidCsv = "Invalid,Header\nValue1,Value2";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(invalidCsv));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await parser.ParseAsync(stream));
    }

    [Fact]
    public async Task SwedbankParser_ParseAsync_HandlesEscapedQuotesInDescription()
    {
        // Arrange
        var parser = new SwedbankParser();
        var csvWithEscapedQuotes = @"Radnummer,Clearingnummer,Kontonummer,Produkt,Valuta,Bokföringsdag,Transaktionsdag,Valutadag,Referens,Beskrivning,Belopp,Bokfört saldo
1,84525,1234567891,""e-sparkonto"",SEK,2025-11-12,2025-11-12,2025-11-12,""Test """"quoted"""" text"",""Test """"quoted"""" text"",50.00,9989.74";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvWithEscapedQuotes));

        // Act
        var transactions = await parser.ParseAsync(stream);

        // Assert
        Assert.Single(transactions);
        Assert.Contains("quoted", transactions[0].Description);
    }
}
