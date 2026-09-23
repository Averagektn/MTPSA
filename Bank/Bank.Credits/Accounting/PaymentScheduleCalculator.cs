using Bank.Common.Dtos.Response;
using Bank.Credits.Models;

namespace Bank.Credits.Accounting;

public static class PaymentScheduleCalculator
{
    public static IReadOnlyList<PaymentScheduleItem> Build(
        decimal amount,
        decimal annualRatePercent,
        DateOnly startDate,
        int termMonths,
        RepaymentSchedule schedule)
        => schedule == RepaymentSchedule.InterestOnly
            ? InterestOnly(amount, annualRatePercent, startDate, termMonths)
            : Annuity(amount, annualRatePercent, startDate, termMonths);

    public static IReadOnlyList<PaymentScheduleItem> Annuity(
        decimal amount,
        decimal annualRatePercent,
        DateOnly startDate,
        int termMonths)
    {
        if (termMonths <= 0 || amount <= 0)
        {
            return [];
        }

        var monthlyRate = annualRatePercent / 100m / 12m;
        var payment = monthlyRate == 0
            ? Round(amount / termMonths)
            : Round(amount * monthlyRate * Pow(1m + monthlyRate, termMonths)
                / (Pow(1m + monthlyRate, termMonths) - 1m));

        var remaining = amount;
        var items = new List<PaymentScheduleItem>(termMonths);
        for (var period = 1; period <= termMonths; period++)
        {
            var date = startDate.AddMonths(period);
            var interest = Round(remaining * monthlyRate);
            decimal principal;
            decimal pay;
            if (period == termMonths)
            {
                principal = remaining;
                pay = Round(principal + interest);
                remaining = 0m;
            }
            else
            {
                principal = Round(payment - interest);
                if (principal > remaining)
                {
                    principal = remaining;
                }

                pay = Round(principal + interest);
                remaining = Round(remaining - principal);
            }

            items.Add(new PaymentScheduleItem(period, date, principal, interest, pay, remaining));
        }

        return items;
    }

    public static IReadOnlyList<PaymentScheduleItem> InterestOnly(
        decimal amount,
        decimal annualRatePercent,
        DateOnly startDate,
        int termMonths)
    {
        if (termMonths <= 0 || amount <= 0)
        {
            return [];
        }

        var monthlyRate = annualRatePercent / 100m / 12m;
        var items = new List<PaymentScheduleItem>(termMonths);
        for (var period = 1; period <= termMonths; period++)
        {
            var date = startDate.AddMonths(period);
            var interest = Round(amount * monthlyRate);
            var principal = period == termMonths ? amount : 0m;
            var remaining = period == termMonths ? 0m : amount;
            items.Add(new PaymentScheduleItem(period, date, principal, interest, Round(principal + interest), remaining));
        }

        return items;
    }

    public static decimal Round(decimal value)
        => decimal.Round(value, 2, MidpointRounding.AwayFromZero);

    private static decimal Pow(decimal value, int exponent)
    {
        var result = 1m;
        for (var i = 0; i < exponent; i++)
        {
            result *= value;
        }

        return result;
    }
}
