using System.Text.Json;
using Bank.Common.Dtos.Request;

namespace Tests.Credits.Support;

public static class CreditSamples
{
    private static int UniqueNumber = 1;

    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public static CreditContractRequest CashLoan(int clientId = 1, Action<CreditContractRequest>? mutate = null)
    {
        var request = new CreditContractRequest
        {
            ClientId = clientId,
            ProductId = 1,
            Number = "",
            CurrencyId = 1,
            StartDate = "20.09.2026",
            EndDate = "20.09.2027",
            TermMonths = 12,
            Amount = 3000m,
            AnnualRate = 18.1m
        };
        mutate?.Invoke(request);
        return request;
    }

    public static CreditContractRequest OnlineLoan(int clientId = 1, Action<CreditContractRequest>? mutate = null)
    {
        var request = new CreditContractRequest
        {
            ClientId = clientId,
            ProductId = 2,
            Number = "",
            CurrencyId = 1,
            StartDate = "20.09.2026",
            EndDate = "20.09.2028",
            TermMonths = 24,
            Amount = 2000m,
            AnnualRate = 18.1m
        };
        mutate?.Invoke(request);
        return request;
    }

    public static CreditContractRequest UniqueCash(int clientId = 6)
    {
        var n = Interlocked.Increment(ref UniqueNumber);
        return CashLoan(clientId, r => r.Number = $"Т-{n:0000}");
    }

    public static StringContent JsonBody(CreditContractRequest request)
        => new(JsonSerializer.Serialize(request, JsonOptions), System.Text.Encoding.UTF8, "application/json");
}

public sealed class ValidationProblemDto
{
    public Dictionary<string, string[]> Errors { get; set; } = [];
}
