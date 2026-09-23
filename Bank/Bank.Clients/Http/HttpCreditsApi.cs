using System.Net.Http.Json;
using Bank.Common.Dtos.Response;
using Bank.Common.Errors;
using FluentResults;

namespace Bank.Clients.Http;

public interface ICreditsApi
{
    Task<Result<IReadOnlyList<CreditContractListItem>>> GetContractsByClientAsync(
        int clientId,
        CancellationToken cancellationToken = default);
}

public sealed class HttpCreditsApi(HttpClient http) : ICreditsApi
{
    public async Task<Result<IReadOnlyList<CreditContractListItem>>> GetContractsByClientAsync(
        int clientId,
        CancellationToken cancellationToken = default)
    {
        if (http.BaseAddress is null)
        {
            return Result.Ok<IReadOnlyList<CreditContractListItem>>([]);
        }

        try
        {
            var contracts = await http.GetFromJsonAsync<List<CreditContractListItem>>(
                $"api/credits/contracts?clientId={clientId}",
                cancellationToken);
            return Result.Ok<IReadOnlyList<CreditContractListItem>>(contracts ?? []);
        }
        catch (HttpRequestException)
        {
            return Result.Fail(new ValidationError("id", "creditsUnavailable"));
        }
    }
}
