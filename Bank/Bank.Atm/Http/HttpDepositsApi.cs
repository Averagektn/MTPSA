using System.Net.Http.Json;
using Bank.Common.Dtos.Response;
using Bank.Common.Errors;
using FluentResults;

namespace Bank.Atm.Http;

public interface IDepositsApi
{
    Task<Result<AtmDepositBalanceResponse>> GetBalancesAsync(
        int clientId,
        CancellationToken cancellationToken = default);
}

public sealed class HttpDepositsApi(HttpClient http) : IDepositsApi
{
    public async Task<Result<AtmDepositBalanceResponse>> GetBalancesAsync(
        int clientId,
        CancellationToken cancellationToken = default)
    {
        if (http.BaseAddress is null)
        {
            return Result.Fail(new ValidationError("operation", "depositsUnavailable"));
        }

        try
        {
            using var response = await LocaleHttp.GetAsync(
                http,
                $"api/deposits/atm/balances?clientId={clientId}",
                cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return Result.Fail(new ValidationError("operation", "depositsUnavailable"));
            }

            var body = await response.Content.ReadFromJsonAsync<AtmDepositBalanceResponse>(cancellationToken);
            return body is null
                ? Result.Fail(new ValidationError("operation", "depositsUnavailable"))
                : Result.Ok(body);
        }
        catch (HttpRequestException)
        {
            return Result.Fail(new ValidationError("operation", "depositsUnavailable"));
        }
    }
}
