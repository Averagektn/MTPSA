using Bank.Credits.Accounting;

namespace Tests.Credits.Unit.Accounting;

[TestClass]
public sealed class AccountNumberGeneratorTests
{
    [TestMethod]
    public void Check_digit_matches_weighted_sum_modulo_10()
    {
        AccountNumberGenerator.CheckDigit("101000000001").Should().Be(3);
        AccountNumberGenerator.CheckDigit("120100000001").Should().Be(9);
        AccountNumberGenerator.CheckDigit("732700000001").Should().Be(0);
    }

    [TestMethod]
    public void Builds_13_digit_client_account()
    {
        var number = AccountNumberGenerator.Build("2410", AccountNumberGenerator.ClientCode(1), 1);
        number.Should().HaveLength(13);
        number[..4].Should().Be("2410");
        number[4..9].Should().Be("10001");
        number[9..12].Should().Be("001");
    }
}
