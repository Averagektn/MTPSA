namespace Bank.Common.Dtos.Response;

public sealed record AtmReceiptDto(
    string TitleKey,
    DateTimeOffset PrintedAt,
    string CardNumberMasked,
    string Operation,
    decimal? Amount,
    decimal? Balance,
    string? OperatorCode,
    string? Phone,
    string? CreditAccountNumber,
    string ClientName,
    string? DepositAccountNumber = null);
