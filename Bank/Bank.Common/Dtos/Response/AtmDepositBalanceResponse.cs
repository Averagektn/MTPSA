namespace Bank.Common.Dtos.Response;

public sealed record AtmDepositBalanceResponse(
    int ClientId,
    string ClientName,
    IReadOnlyList<AtmDepositBalanceItem> Items);
