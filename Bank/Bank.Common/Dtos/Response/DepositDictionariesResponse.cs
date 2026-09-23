namespace Bank.Common.Dtos.Response;

public sealed record DepositDictionariesResponse(
    IReadOnlyList<DepositProductLookup> Products,
    IReadOnlyList<LookupItem> Currencies,
    IReadOnlyList<ClientListItem> Clients);
