namespace Bank.Common.Dtos.Response;

public sealed record CreditContractListItem(
    int Id,
    string Number,
    int ClientId,
    string ClientName,
    string ProductName,
    string RepaymentSchedule,
    string CurrencyCode,
    DateOnly StartDate,
    DateOnly EndDate,
    int TermMonths,
    decimal Amount,
    decimal RemainingPrincipal,
    decimal AnnualRate,
    string Status,
    string PrincipalAccountNumber,
    string InterestAccountNumber,
    DateOnly? NextPaymentDate);
