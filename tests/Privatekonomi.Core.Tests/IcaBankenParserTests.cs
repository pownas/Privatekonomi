using System.Text;
using Privatekonomi.Core.Services.Parsers;

namespace Privatekonomi.Core.Tests;

[TestClass]
public class IcaBankenParserTests
{
    private const string IcaCsvContent =
        "Datum;Text;Typ;Belopp;Saldo\n" +
        "2025-12-24;Ica Faktura;Insättning;8 800,00 kr;587,48 kr\n" +
        "2025-12-23;Willys Mariestad Hag   Mariestad      Se ;Reserverat belopp;-256,70 kr;\n" +
        "2025-12-23;St1 Hova               Hova           Se ;Reserverat belopp;-304,23 kr;\n" +
        "2025-12-22;Kumla Biltvatt                ;Korttransaktion;-66,00 kr;-8 212,52 kr\n" +
        "2025-12-22;Lekia Orebro                  ;Korttransaktion;-399,00 kr;-8 146,52 kr";

    [TestMethod]
    public void IcaBankenParser_CanParse_ReturnsTrueForIcaFormat()
    {
        var parser = new IcaBankenParser();
        Assert.IsTrue(parser.CanParse(IcaCsvContent));
    }

    [TestMethod]
    public async Task IcaBankenParser_ParseAsync_ParsesAmountsAndIncomeCorrectly()
    {
        var parser = new IcaBankenParser();
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(IcaCsvContent));

        var transactions = await parser.ParseAsync(stream);

        Assert.AreEqual(5, transactions.Count);

        var deposit = transactions[0];
        Assert.AreEqual(new DateTime(2025, 12, 24), deposit.Date);
        Assert.AreEqual(8800.00m, deposit.Amount);
        Assert.IsTrue(deposit.IsIncome);

        var reserved = transactions[1];
        Assert.AreEqual(new DateTime(2025, 12, 23), reserved.Date);
        Assert.AreEqual(256.70m, reserved.Amount);
        Assert.IsFalse(reserved.IsIncome);
    }
}
