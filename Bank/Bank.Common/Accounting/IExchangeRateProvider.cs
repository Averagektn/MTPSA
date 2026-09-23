namespace Bank.Common.Accounting;

public interface IExchangeRateProvider
{
    Task<decimal> GetBynPerUnitAsync(string currencyCode, DateOnly onDate, CancellationToken cancellationToken);
}
