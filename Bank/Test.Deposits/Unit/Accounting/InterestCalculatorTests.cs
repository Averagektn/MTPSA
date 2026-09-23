using Bank.Deposits.Accounting;

namespace Tests.Deposits.Unit.Accounting;

[TestClass]
public sealed class InterestCalculatorTests
{
    [TestMethod]
    public void Monthly_interest_for_alfa_safe_is_rounded_away_from_zero()
    {
        InterestCalculator.ForDays(1000m, 5.5m, 30).Should().Be(4.52m);
    }

    [TestMethod]
    public void End_of_term_interest_uses_actual_day_count()
    {
        InterestCalculator.ForDays(5000m, 12m, 395).Should().Be(649.32m);
    }

    [TestMethod]
    public void Zero_days_yields_zero()
    {
        InterestCalculator.ForDays(1000m, 5.5m, 0).Should().Be(0m);
    }
}
