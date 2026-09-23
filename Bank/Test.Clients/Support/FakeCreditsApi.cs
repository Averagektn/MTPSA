using Bank.Clients.Http;
using Bank.Common.Dtos.Response;
using Bank.Common.Errors;
using FluentResults;

namespace Tests.Clients.Support;

public sealed class FakeCreditsApi : ICreditsApi
{
    public HashSet<int> ClientIdsWithCredits { get; } = [];
    public bool Unavailable { get; set; }

    public Task<Result<IReadOnlyList<CreditContractListItem>>> GetContractsByClientAsync(
        int clientId,
        CancellationToken cancellationToken = default)
    {
        if (Unavailable)
        {
            return Task.FromResult(Result.Fail<IReadOnlyList<CreditContractListItem>>(
                new ValidationError("id", "creditsUnavailable")));
        }

        if (!ClientIdsWithCredits.Contains(clientId))
        {
            return Task.FromResult(Result.Ok<IReadOnlyList<CreditContractListItem>>([]));
        }

        IReadOnlyList<CreditContractListItem> items =
        [
            new(
                1,
                "К-1",
                clientId,
                "Client",
                "Cash loan",
                "annuity",
                "BYN",
                default,
                default,
                12,
                3000m,
                3000m,
                18.1m,
                "active",
                "1",
                "2",
                null)
        ];
        return Task.FromResult(Result.Ok(items));
    }
}
