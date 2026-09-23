namespace Bank.Common.Dtos.Response;

public sealed record DepositContractResponse(
    int Id,
    string Number,
    int ClientId,
    string ClientName,
    int ProductId,
    string ProductName,
    int CurrencyId,
    string CurrencyCode,
    DateOnly StartDate,
    DateOnly EndDate,
    int TermMonths,
    decimal Amount,
    decimal AnnualRate,
    string Status,
    int PrincipalAccountId,
    string PrincipalAccountNumber,
    int InterestAccountId,
    string InterestAccountNumber);
