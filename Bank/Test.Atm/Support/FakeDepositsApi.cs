using Bank.Atm.Http;
using Bank.Common.Dtos.Response;
using Bank.Common.Errors;
using FluentResults;

namespace Tests.Atm.Support;

public sealed class FakeDepositsApi : IDepositsApi
{
    public List<AtmDepositBalanceItem> Items { get; } =
    [
        new("Д-2026-0003", "3404100040018", "Alfa Safe (revocable)", 1200m, "BYN")
    ];

    public bool Unavailable { get; set; }

    public Task<Result<AtmDepositBalanceResponse>> GetBalancesAsync(
        int clientId,
        CancellationToken cancellationToken = default)
    {
        if (Unavailable)
        {
            return Task.FromResult(Result.Fail<AtmDepositBalanceResponse>(
                new ValidationError("operation", "depositsUnavailable")));
        }

        IReadOnlyList<AtmDepositBalanceItem> items = clientId == 4 ? Items : [];
        return Task.FromResult(Result.Ok(new AtmDepositBalanceResponse(clientId, "Client4 Test X", items)));
    }
}
