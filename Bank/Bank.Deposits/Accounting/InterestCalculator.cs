namespace Bank.Deposits.Accounting;

public static class InterestCalculator
{
    public static decimal ForDays(decimal principal, decimal annualRatePercent, int days)
    {
        if (days <= 0 || principal <= 0 || annualRatePercent <= 0)
        {
            return 0m;
        }

        return decimal.Round(principal * annualRatePercent / 100m * days / 365m, 2, MidpointRounding.AwayFromZero);
    }
}
