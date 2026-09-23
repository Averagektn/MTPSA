namespace Bank.Common.Accounting;

public static class ExchangeConversion
{
    public readonly record struct MoneyLeg(decimal Amount, decimal AmountByn, decimal BynPerUnit);

    public static decimal ToByn(decimal amount, decimal bynPerUnit)
        => decimal.Round(amount * bynPerUnit, 2, MidpointRounding.AwayFromZero);

    public static MoneyLeg ForSide(
        string sideCurrency,
        decimal amount,
        string amountCurrency,
        decimal amountByn,
        decimal sideBynPerUnit)
    {
        if (sideCurrency.Equals(amountCurrency, StringComparison.OrdinalIgnoreCase))
        {
            return new MoneyLeg(amount, amountByn, sideBynPerUnit);
        }

        if (sideCurrency.Equals("BYN", StringComparison.OrdinalIgnoreCase))
        {
            return new MoneyLeg(amountByn, amountByn, 1m);
        }

        var sideAmount = sideBynPerUnit == 0
            ? 0m
            : decimal.Round(amountByn / sideBynPerUnit, 2, MidpointRounding.AwayFromZero);
        return new MoneyLeg(sideAmount, amountByn, sideBynPerUnit);
    }
}
