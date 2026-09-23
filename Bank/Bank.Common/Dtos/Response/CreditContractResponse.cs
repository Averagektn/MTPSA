namespace Bank.Common.Dtos.Response;

public sealed record CreditContractResponse(
    int Id,
    string Number,
    int ClientId,
    string ClientName,
    int ProductId,
    string ProductName,
    string RepaymentSchedule,
    int CurrencyId,
    string CurrencyCode,
    DateOnly StartDate,
    DateOnly EndDate,
    int TermMonths,
    decimal Amount,
    decimal RemainingPrincipal,
    decimal AnnualRate,
    string Status,
    int PrincipalAccountId,
    string PrincipalAccountNumber,
    int InterestAccountId,
    string InterestAccountNumber,
    IReadOnlyList<PaymentScheduleItem> Schedule);
