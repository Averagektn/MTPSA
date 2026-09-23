namespace Bank.Common.Dtos.Response;

public sealed record DepositProductLookup(
    int Id,
    string Code,
    string Name,
    bool Revocable,
    string InterestSchedule,
    int TermMonths,
    decimal AnnualRate,
    decimal MinAmount,
    decimal MaxAmount,
    int CurrencyId);
