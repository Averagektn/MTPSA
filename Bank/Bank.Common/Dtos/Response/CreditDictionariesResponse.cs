namespace Bank.Common.Dtos.Response;

public sealed record CreditDictionariesResponse(
    IReadOnlyList<CreditProductLookup> Products,
    IReadOnlyList<LookupItem> Currencies,
    IReadOnlyList<ClientListItem> Clients);
