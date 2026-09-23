using System.Net.Http.Json;
using Bank.Common.Dtos.Response;
using Bank.Common.Errors;
using FluentResults;

namespace Bank.Credits.Http;

public interface IDepositsApi
{
    Task<Result<IReadOnlyList<AccountReportItem>>> GetAccountsAsync(CancellationToken cancellationToken = default);
}

public sealed class HttpDepositsApi(HttpClient http) : IDepositsApi
{
    public async Task<Result<IReadOnlyList<AccountReportItem>>> GetAccountsAsync(
        CancellationToken cancellationToken = default)
    {
        if (http.BaseAddress is null)
        {
            return Result.Ok<IReadOnlyList<AccountReportItem>>([]);
        }

        try
        {
            using var response = await LocaleHttp.GetAsync(http, "api/accounts", cancellationToken);
            response.EnsureSuccessStatusCode();
            var accounts = await response.Content.ReadFromJsonAsync<List<AccountReportItem>>(cancellationToken);
            return Result.Ok<IReadOnlyList<AccountReportItem>>(accounts ?? []);
        }
        catch (HttpRequestException)
        {
            return Result.Fail(new ValidationError("accounts", "depositsUnavailable"));
        }
    }
}
