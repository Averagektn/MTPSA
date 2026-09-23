namespace Bank.Common.Accounting;

public sealed class HardcodedExchangeRateProvider : IExchangeRateProvider
{
    private static readonly Dictionary<string, decimal> Rates = new(StringComparer.OrdinalIgnoreCase)
    {
        ["BYN"] = 1m,
        ["USD"] = 3.25m,
        ["EUR"] = 3.55m
    };

    public Task<decimal> GetBynPerUnitAsync(string currencyCode, DateOnly onDate, CancellationToken cancellationToken)
    {
        if (!Rates.TryGetValue(currencyCode, out var rate))
        {
            throw new InvalidOperationException($"No exchange rate for {currencyCode}.");
        }

        return Task.FromResult(rate);
    }
}
