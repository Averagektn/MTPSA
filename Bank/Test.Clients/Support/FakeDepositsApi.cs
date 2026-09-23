using Bank.Clients.Http;
using Bank.Common.Dtos.Response;
using Bank.Common.Errors;
using FluentResults;

namespace Tests.Clients.Support;

public sealed class FakeDepositsApi : IDepositsApi
{
    public HashSet<int> ClientIdsWithDeposits { get; } = [];
    public bool Unavailable { get; set; }

    public Task<Result<IReadOnlyList<DepositContractListItem>>> GetContractsByClientAsync(
        int clientId,
        CancellationToken cancellationToken = default)
    {
        if (Unavailable)
        {
            return Task.FromResult(Result.Fail<IReadOnlyList<DepositContractListItem>>(
                new ValidationError("id", "depositsUnavailable")));
        }

        if (!ClientIdsWithDeposits.Contains(clientId))
        {
            return Task.FromResult(Result.Ok<IReadOnlyList<DepositContractListItem>>([]));
        }

        IReadOnlyList<DepositContractListItem> items =
        [
            new(
                1,
                "Д-1",
                clientId,
                "Client",
                "Alfa Safe",
                "BYN",
                default,
                default,
                13,
                1000m,
                5.5m,
                "active",
                "1",
                "2",
                null)
        ];
        return Task.FromResult(Result.Ok(items));
    }
}
