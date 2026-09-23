namespace Bank.Common.Dtos.Response;

public sealed record AtmTransactionResponse(
    bool Success,
    string MessageKey,
    string Operation,
    decimal? AvailableBalance,
    string? CreditAccountNumber,
    string? ClientName,
    string? CurrencyCode,
    AtmReceiptDto? Receipt,
    IReadOnlyList<AtmDepositBalanceItem>? Deposits = null);
