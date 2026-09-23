namespace Bank.Common.Dtos.Response;

public sealed record AtmAuthorizeResponse(
    string CardNumber,
    string CardNumberMasked,
    string ClientName,
    int ClientId,
    int ContractId,
    string ContractNumber,
    string CreditAccountNumber,
    decimal AvailableBalance,
    string CurrencyCode);
