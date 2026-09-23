using System.Net.Http.Json;
using Bank.Common.Dtos.Response;
using Bank.Common.Errors;
using FluentResults;

namespace Bank.Clients.Http;

public interface IDepositsApi
{
    Task<Result<IReadOnlyList<DepositContractListItem>>> GetContractsByClientAsync(
        int clientId,
        CancellationToken cancellationToken = default);
}

public sealed class HttpDepositsApi(HttpClient http) : IDepositsApi
{
    public async Task<Result<IReadOnlyList<DepositContractListItem>>> GetContractsByClientAsync(
        int clientId,
        CancellationToken cancellationToken = default)
    {
        if (http.BaseAddress is null)
        {
            return Result.Ok<IReadOnlyList<DepositContractListItem>>([]);
        }

        try
        {
            var contracts = await http.GetFromJsonAsync<List<DepositContractListItem>>(
                $"api/deposits/contracts?clientId={clientId}",
                cancellationToken);
            return Result.Ok<IReadOnlyList<DepositContractListItem>>(contracts ?? []);
        }
        catch (HttpRequestException)
        {
            return Result.Fail(new ValidationError("id", "depositsUnavailable"));
        }
    }
}
