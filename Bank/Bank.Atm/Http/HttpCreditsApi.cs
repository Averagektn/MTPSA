using System.Net.Http.Json;
using Bank.Common.Dtos.Request;
using Bank.Common.Dtos.Response;
using Bank.Common.Errors;
using FluentResults;
using Microsoft.AspNetCore.Http;

namespace Bank.Atm.Http;

public interface ICreditsApi
{
    Task<Result<AtmAuthorizeResponse>> AuthorizeAsync(
        AtmAuthorizeRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<AtmTransactionResponse>> ExecuteAsync(
        AtmTransactionRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class HttpCreditsApi(HttpClient http) : ICreditsApi
{
    public async Task<Result<AtmAuthorizeResponse>> AuthorizeAsync(
        AtmAuthorizeRequest request,
        CancellationToken cancellationToken = default)
    {
        if (http.BaseAddress is null)
        {
            return Result.Fail(new ValidationError("cardNumber", "creditsUnavailable"));
        }

        try
        {
            using var response = await LocaleHttp.PostAsJsonAsync(
                http,
                "api/credits/atm/authorize",
                request,
                cancellationToken);
            return await ReadAsync<AtmAuthorizeResponse>(response, "cardNumber", cancellationToken);
        }
        catch (HttpRequestException)
        {
            return Result.Fail(new ValidationError("cardNumber", "creditsUnavailable"));
        }
    }

    public async Task<Result<AtmTransactionResponse>> ExecuteAsync(
        AtmTransactionRequest request,
        CancellationToken cancellationToken = default)
    {
        if (http.BaseAddress is null)
        {
            return Result.Fail(new ValidationError("operation", "creditsUnavailable"));
        }

        try
        {
            using var response = await LocaleHttp.PostAsJsonAsync(
                http,
                "api/credits/atm/transactions",
                request,
                cancellationToken);
            return await ReadAsync<AtmTransactionResponse>(response, "operation", cancellationToken);
        }
        catch (HttpRequestException)
        {
            return Result.Fail(new ValidationError("operation", "creditsUnavailable"));
        }
    }

    private static async Task<Result<T>> ReadAsync<T>(
        HttpResponseMessage response,
        string fallbackField,
        CancellationToken cancellationToken)
    {
        if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
        {
            var problem = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>(cancellationToken);
            if (problem?.Errors is { Count: > 0 })
            {
                var first = problem.Errors.First();
                return Result.Fail(new ValidationError(first.Key, first.Value.FirstOrDefault() ?? "requestFailed"));
            }

            return Result.Fail(new ValidationError(fallbackField, "requestFailed"));
        }

        if (!response.IsSuccessStatusCode)
        {
            return Result.Fail(new ValidationError(fallbackField, "creditsUnavailable"));
        }

        var body = await response.Content.ReadFromJsonAsync<T>(cancellationToken);
        return body is null
            ? Result.Fail(new ValidationError(fallbackField, "requestFailed"))
            : Result.Ok(body);
    }
}
