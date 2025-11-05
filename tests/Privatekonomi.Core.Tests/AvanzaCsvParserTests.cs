using System.Text;
using Privatekonomi.Core.Services.Parsers;
using Xunit;

namespace Privatekonomi.Core.Tests;

public class AvanzaCsvParserTests
{
    private const string PerAccountCsvContent = @"Kontonummer|Namn|Kortnamn|Volym|Marknadsvärde|GAV (SEK)|GAV|Valuta|Land|ISIN|Marknad|Typ
1111-2223332|Telia Company|TELIA|5|180,90|54,78|54,78|SEK|SE|SE0000667925|XSTO|STOCK
2222-3333444|Oneflow|ONEF|100|2730,00|44,71|44,71|SEK|SE|SE0017564461|FNSE|STOCK
2222-3333444|BEAR NASD X22 VT17|BEAR NASD X22 VT17|75000|75,00|0,02|0,02|SEK|SE|DE000VK03YN7|NMTF|CERTIFICATE
2222-3333444|Montrose Global Monthly Dividend MSCI World UCITS ETF|MONTDIV|20|1821,20|89,68|89,68|SEK|SE|IE000DMPF2D5|XSTO|EXCHANGE_TRADED_FUND";

    private const string ConsolidatedCsvContent = @"Namn|Kortnamn|Volym|Marknadsvärde|GAV (SEK)|GAV|Valuta|Land|ISIN|Marknad|Typ
AFRY|AFRY|7|1164,10|159,59|159,59|SEK|SE|SE0005999836|XSTO|STOCK
Aktiespararna Direktavkastning A|Aktiespararna Direktavkastning A|24,519705|6006,05|208,23|208,23|SEK|SE|SE0008321780|FUND|FUND
Aktiespararna Direktavkastning B|Aktiespararna Direktavkastning B|42,457|6170,70|133,49|133,49|SEK|SE|SE0013720000|FUND|FUND
Amundi S&P 500 Screened INDEX AE Acc|Amundi S&P 500 Screened INDEX AE Acc|0,7245|3891,64|5038,52|5038,52|SEK|LU|LU0996179007|FUND|FUND
Angler Gaming|ANGL|650|1917,50|9,21|9,21|SEK|SE|MT0000650102|XSAT|STOCK
Avanza Bank Holding|AZA|6|2248,20|216,25|216,25|SEK|SE|SE0012454072|XSTO|STOCK
Avanza Emerging Markets|Avanza Emerging Markets|25,015468|3789,59|124,74|124,74|SEK|SE|SE0012454338|FUND|FUND
Avanza Europa|Avanza Europa|36,634425|5592,24|145,77|145,77|SEK|SE|SE0013718699|||";

    [Fact]
    public void AvanzaHoldingsPerAccountParser_CanParse_ReturnsTrueForPerAccountFormat()
    {
        // Arrange
        var parser = new AvanzaHoldingsPerAccountParser();

        // Act
        var result = parser.CanParse(PerAccountCsvContent);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void AvanzaHoldingsPerAccountParser_CanParse_ReturnsFalseForConsolidatedFormat()
    {
        // Arrange
        var parser = new AvanzaHoldingsPerAccountParser();

        // Act
        var result = parser.CanParse(ConsolidatedCsvContent);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void AvanzaConsolidatedHoldingsParser_CanParse_ReturnsTrueForConsolidatedFormat()
    {
        // Arrange
        var parser = new AvanzaConsolidatedHoldingsParser();

        // Act
        var result = parser.CanParse(ConsolidatedCsvContent);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void AvanzaConsolidatedHoldingsParser_CanParse_ReturnsFalseForPerAccountFormat()
    {
        // Arrange
        var parser = new AvanzaConsolidatedHoldingsParser();

        // Act
        var result = parser.CanParse(PerAccountCsvContent);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task AvanzaHoldingsPerAccountParser_ParseAsync_ParsesCorrectly()
    {
        // Arrange
        var parser = new AvanzaHoldingsPerAccountParser();
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(PerAccountCsvContent));

        // Act
        var investments = await parser.ParseAsync(stream);

        // Assert
        Assert.NotNull(investments);
        Assert.Equal(4, investments.Count);

        // Check first investment (Telia Company)
        var telia = investments[0];
        Assert.Equal("Telia Company", telia.Name);
        Assert.Equal("TELIA", telia.ShortName);
        Assert.Equal("Aktie", telia.Type);
        Assert.Equal(5, telia.Quantity);
        Assert.Equal(54.78m, telia.PurchasePrice);
        Assert.Equal(36.18m, telia.CurrentPrice); // 180.90 / 5
        Assert.Equal("1111-2223332", telia.AccountNumber);
        Assert.Equal("SE0000667925", telia.ISIN);
        Assert.Equal("SEK", telia.Currency);
        Assert.Equal("SE", telia.Country);
        Assert.Equal("XSTO", telia.Market);

        // Check certificate investment
        var certificate = investments[2];
        Assert.Equal("BEAR NASD X22 VT17", certificate.Name);
        Assert.Equal("Certifikat", certificate.Type);
        Assert.Equal(75000, certificate.Quantity);
        Assert.Equal(0.02m, certificate.PurchasePrice);
        Assert.Equal(0.001m, certificate.CurrentPrice); // 75.00 / 75000

        // Check ETF/Fund
        var etf = investments[3];
        Assert.Equal("Montrose Global Monthly Dividend MSCI World UCITS ETF", etf.Name);
        Assert.Equal("Fond", etf.Type); // EXCHANGE_TRADED_FUND maps to "Fond"
    }

    [Fact]
    public async Task AvanzaConsolidatedHoldingsParser_ParseAsync_ParsesCorrectly()
    {
        // Arrange
        var parser = new AvanzaConsolidatedHoldingsParser();
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(ConsolidatedCsvContent));

        // Act
        var investments = await parser.ParseAsync(stream);

        // Assert
        Assert.NotNull(investments);
        Assert.Equal(8, investments.Count);

        // Check first investment (AFRY)
        var afry = investments[0];
        Assert.Equal("AFRY", afry.Name);
        Assert.Equal("AFRY", afry.ShortName);
        Assert.Equal("Aktie", afry.Type);
        Assert.Equal(7, afry.Quantity);
        Assert.Equal(159.59m, afry.PurchasePrice);
        Assert.Equal(166.30m, Math.Round(afry.CurrentPrice, 2)); // 1164.10 / 7
        Assert.Null(afry.AccountNumber); // Consolidated format doesn't have account numbers
        Assert.Equal("SE0005999836", afry.ISIN);

        // Check fund with decimal quantity
        var fund = investments[1];
        Assert.Equal("Aktiespararna Direktavkastning A", fund.Name);
        Assert.Equal("Fond", fund.Type);
        Assert.Equal(24.519705m, fund.Quantity);
        Assert.Equal(208.23m, fund.PurchasePrice);

        // Check investment with empty type
        var europa = investments[7];
        Assert.Equal("Avanza Europa", europa.Name);
        Assert.Equal("Övrigt", europa.Type); // Empty type maps to "Övrigt"
    }

    [Fact]
    public async Task AvanzaHoldingsPerAccountParser_ParseAsync_HandlesDecimalCommasCorrectly()
    {
        // Arrange
        var parser = new AvanzaHoldingsPerAccountParser();
        var csvWithCommas = @"Kontonummer|Namn|Kortnamn|Volym|Marknadsvärde|GAV (SEK)|GAV|Valuta|Land|ISIN|Marknad|Typ
1111-2223332|Test Fund|TEST|10,5|1234,56|100,00|100,00|SEK|SE|SE0000000001|XSTO|FUND";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvWithCommas));

        // Act
        var investments = await parser.ParseAsync(stream);

        // Assert
        Assert.Single(investments);
        var investment = investments[0];
        Assert.Equal(10.5m, investment.Quantity);
        Assert.Equal(100.00m, investment.PurchasePrice);
        Assert.Equal(117.58m, Math.Round(investment.CurrentPrice, 2)); // 1234.56 / 10.5
    }

    [Fact]
    public async Task AvanzaConsolidatedHoldingsParser_ParseAsync_SkipsInvalidRows()
    {
        // Arrange
        var parser = new AvanzaConsolidatedHoldingsParser();
        var csvWithInvalidRows = @"Namn|Kortnamn|Volym|Marknadsvärde|GAV (SEK)|GAV|Valuta|Land|ISIN|Marknad|Typ
AFRY|AFRY|7|1164,10|159,59|159,59|SEK|SE|SE0005999836|XSTO|STOCK
|EMPTY|10|100|10|10|SEK|SE|SE0000000002|XSTO|STOCK
Test|TEST|invalid|100|10|10|SEK|SE|SE0000000003|XSTO|STOCK
Valid Fund|VALID|5|500,00|100,00|100,00|SEK|SE|SE0000000004|XSTO|FUND";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvWithInvalidRows));

        // Act
        var investments = await parser.ParseAsync(stream);

        // Assert
        Assert.Equal(2, investments.Count); // Only valid rows
        Assert.Equal("AFRY", investments[0].Name);
        Assert.Equal("Valid Fund", investments[1].Name);
    }

    [Fact]
    public void AvanzaHoldingsPerAccountParser_BankName_ReturnsAvanza()
    {
        // Arrange
        var parser = new AvanzaHoldingsPerAccountParser();

        // Act & Assert
        Assert.Equal("Avanza", parser.BankName);
    }

    [Fact]
    public void AvanzaHoldingsPerAccountParser_FormatType_ReturnsPerKonto()
    {
        // Arrange
        var parser = new AvanzaHoldingsPerAccountParser();

        // Act & Assert
        Assert.Equal("Per konto", parser.FormatType);
    }

    [Fact]
    public void AvanzaConsolidatedHoldingsParser_FormatType_ReturnsSammanställt()
    {
        // Arrange
        var parser = new AvanzaConsolidatedHoldingsParser();

        // Act & Assert
        Assert.Equal("Sammanställt", parser.FormatType);
    }

    [Fact]
    public async Task AvanzaHoldingsPerAccountParser_ParseAsync_HandlesTabSeparator()
    {
        // Arrange
        var parser = new AvanzaHoldingsPerAccountParser();
        var csvWithTabs = "Kontonummer\tNamn\tKortnamn\tVolym\tMarknadsvärde\tGAV (SEK)\tGAV\tValuta\tLand\tISIN\tMarknad\tTyp\n" +
                         "1111-2223332\tTelia Company\tTELIA\t5\t180,90\t54,78\t54,78\tSEK\tSE\tSE0000667925\tXSTO\tSTOCK";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvWithTabs));

        // Act
        var investments = await parser.ParseAsync(stream);

        // Assert
        Assert.Single(investments);
        Assert.Equal("Telia Company", investments[0].Name);
    }

    [Fact]
    public async Task AvanzaConsolidatedHoldingsParser_ParseAsync_HandlesSemicolonSeparator()
    {
        // Arrange
        var parser = new AvanzaConsolidatedHoldingsParser();
        var csvWithSemicolon = "Namn;Kortnamn;Volym;Marknadsvärde;GAV (SEK);GAV;Valuta;Land;ISIN;Marknad;Typ\n" +
                              "AFRY;AFRY;7;1164,10;159,59;159,59;SEK;SE;SE0005999836;XSTO;STOCK";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvWithSemicolon));

        // Act
        var investments = await parser.ParseAsync(stream);

        // Assert
        Assert.Single(investments);
        Assert.Equal("AFRY", investments[0].Name);
    }
}
