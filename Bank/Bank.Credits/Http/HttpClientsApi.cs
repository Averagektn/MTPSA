using System.Net;
using System.Net.Http.Json;
using Bank.Common.Dtos.Response;
using Bank.Common.Errors;
using FluentResults;

namespace Bank.Credits.Http;

public interface IClientsApi
{
    Task<Result<ClientResponse>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<ClientListItem>>> ListAsync(CancellationToken cancellationToken = default);
}

public sealed class HttpClientsApi(HttpClient http) : IClientsApi
{
    public async Task<Result<ClientResponse>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        if (http.BaseAddress is null)
        {
            return Result.Fail(new ValidationError("clientId", "clientNotFound"));
        }

        try
        {
            using var response = await LocaleHttp.GetAsync(http, $"api/clients/{id}", cancellationToken);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return Result.Fail(new ValidationError("clientId", "clientNotFound"));
            }

            response.EnsureSuccessStatusCode();
            var client = await response.Content.ReadFromJsonAsync<ClientResponse>(cancellationToken);
            return client is null
                ? Result.Fail(new ValidationError("clientId", "clientNotFound"))
                : Result.Ok(client);
        }
        catch (HttpRequestException)
        {
            return Result.Fail(new ValidationError("clientId", "clientsUnavailable"));
        }
    }

    public async Task<Result<IReadOnlyList<ClientListItem>>> ListAsync(CancellationToken cancellationToken = default)
    {
        if (http.BaseAddress is null)
        {
            return Result.Ok<IReadOnlyList<ClientListItem>>([]);
        }

        try
        {
            using var response = await LocaleHttp.GetAsync(http, "api/clients", cancellationToken);
            response.EnsureSuccessStatusCode();
            var clients = await response.Content.ReadFromJsonAsync<List<ClientListItem>>(cancellationToken);
            return Result.Ok<IReadOnlyList<ClientListItem>>(clients ?? []);
        }
        catch (HttpRequestException)
        {
            return Result.Fail(new ValidationError("clients", "clientsUnavailable"));
        }
    }
}
