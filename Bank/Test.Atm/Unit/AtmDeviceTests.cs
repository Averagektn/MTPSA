using Bank.Atm.Session;
using Bank.Common.Atm;

namespace Tests.Atm.Unit;

[TestClass]
public sealed class AmountParserTests
{
    [TestMethod]
    public void Accepts_non_negative_integers()
    {
        AmountParser.TryParseNonNegativeInteger("0", out var zero).Should().BeTrue();
        zero.Should().Be(0);
        AmountParser.TryParseNonNegativeInteger("250", out var amount).Should().BeTrue();
        amount.Should().Be(250);
    }

    [TestMethod]
    public void Rejects_decimals_and_text()
    {
        AmountParser.TryParseNonNegativeInteger("10.5", out _).Should().BeFalse();
        AmountParser.TryParseNonNegativeInteger("abc", out _).Should().BeFalse();
        AmountParser.TryParseNonNegativeInteger("", out _).Should().BeFalse();
    }
}

[TestClass]
public sealed class TransactionBufferTests
{
    [TestMethod]
    public void Accumulates_and_rebuilds_the_bank_payload()
    {
        var buffer = new TransactionBuffer();
        buffer.Set("cardNumber", "4277000011112222");
        buffer.Set("pin", "1111");
        buffer.Set("operation", AtmOperation.Withdraw.ToWire());
        buffer.Set("amount", "200");

        var request = buffer.ToRequest(AtmOperation.Withdraw);
        request.CardNumber.Should().Be("4277000011112222");
        request.Pin.Should().Be("1111");
        request.Amount.Should().Be(200m);
        request.Fields.Should().HaveCount(4);

        buffer.Keep("cardNumber", "pin");
        buffer.Fields.Should().HaveCount(2);
        buffer.Visible().Should().Contain(f => f.Key == "pin" && f.Value == "****");
    }
}

[TestClass]
public sealed class PinPolicyTests
{
    [TestMethod]
    public void Locks_after_three_attempts()
    {
        PinPolicy.AttemptsLeft(0).Should().Be(3);
        PinPolicy.AttemptsLeft(3).Should().Be(0);
        PinPolicy.AttemptsLeft(4).Should().Be(0);
    }
}
