using Bank.Common.Dtos.Response;

namespace Bank.Common.Atm;

public static class AtmOperatorCatalog
{
    public static readonly IReadOnlyList<AtmOperatorDto> All =
    [
        new("A1", "A1", "A1"),
        new("MTS", "MTS", "МТС"),
        new("LIFE", "life:)", "life:)")
    ];

    public static bool IsKnown(string? code)
        => !string.IsNullOrWhiteSpace(code)
           && All.Any(item => string.Equals(item.Code, code, StringComparison.OrdinalIgnoreCase));
}
