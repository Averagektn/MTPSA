namespace Bank.Common.Dtos.Response;

public sealed record AtmDepositBalanceItem(
    string ContractNumber,
    string AccountNumber,
    string ProductName,
    decimal Amount,
    string CurrencyCode);
