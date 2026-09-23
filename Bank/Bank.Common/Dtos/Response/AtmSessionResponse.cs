namespace Bank.Common.Dtos.Response;

public sealed record AtmSessionResponse(
    Guid Id,
    string Screen,
    string? CardNumberMasked,
    int PinAttemptsLeft,
    string? MessageKey,
    IReadOnlyList<AtmTransactionFieldView> Fields,
    AtmAuthorizeResponse? Account,
    AtmTransactionResponse? LastResult,
    AtmReceiptDto? Receipt,
    IReadOnlyList<AtmOperatorDto> Operators);
