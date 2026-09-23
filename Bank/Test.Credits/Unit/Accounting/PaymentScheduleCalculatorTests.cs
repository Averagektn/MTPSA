using Bank.Credits.Accounting;
using Bank.Credits.Models;

namespace Tests.Credits.Unit.Accounting;

[TestClass]
public sealed class PaymentScheduleCalculatorTests
{
    [TestMethod]
    public void Annuity_has_one_row_per_month_and_clears_principal()
    {
        var schedule = PaymentScheduleCalculator.Annuity(1000m, 12m, new DateOnly(2026, 9, 20), 12);
        schedule.Should().HaveCount(12);
        schedule[^1].Remaining.Should().Be(0m);
        schedule[0].Date.Should().Be(new DateOnly(2026, 10, 20));
        schedule[^1].Date.Should().Be(new DateOnly(2027, 9, 20));
        schedule.Sum(item => item.Principal).Should().Be(1000m);
        schedule[0].Interest.Should().BeGreaterThan(0m);
        schedule[0].Payment.Should().Be(schedule[0].Principal + schedule[0].Interest);
    }

    [TestMethod]
    public void Interest_only_pays_principal_on_the_last_period()
    {
        var schedule = PaymentScheduleCalculator.InterestOnly(2000m, 18.1m, new DateOnly(2026, 9, 20), 24);
        schedule.Should().HaveCount(24);
        schedule[0].Principal.Should().Be(0m);
        schedule[0].Remaining.Should().Be(2000m);
        schedule[^1].Principal.Should().Be(2000m);
        schedule[^1].Remaining.Should().Be(0m);
        schedule[0].Interest.Should().Be(schedule[1].Interest);
    }

    [TestMethod]
    public void Build_dispatches_by_repayment_schedule()
    {
        var annuity = PaymentScheduleCalculator.Build(3000m, 18.1m, new DateOnly(2026, 9, 20), 12, RepaymentSchedule.Annuity);
        var balloon = PaymentScheduleCalculator.Build(2000m, 18.1m, new DateOnly(2026, 9, 20), 24, RepaymentSchedule.InterestOnly);
        annuity.Should().HaveCount(12);
        annuity[0].Principal.Should().BeGreaterThan(0m);
        balloon[0].Principal.Should().Be(0m);
    }
}
