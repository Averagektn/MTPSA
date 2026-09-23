namespace Bank.Common.Dtos.Response;

public sealed record CreditProductLookup(
    int Id,
    string Code,
    string Name,
    string RepaymentSchedule,
    int TermMonths,
    decimal AnnualRate,
    decimal MinAmount,
    decimal MaxAmount,
    int CurrencyId);
