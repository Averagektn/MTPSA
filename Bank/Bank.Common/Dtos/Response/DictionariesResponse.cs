namespace Bank.Common.Dtos.Response;

public sealed record DictionariesResponse(
    IReadOnlyList<LookupItem> Cities,
    IReadOnlyList<LookupItem> MaritalStatuses,
    IReadOnlyList<LookupItem> Citizenships,
    IReadOnlyList<LookupItem> Disabilities);
