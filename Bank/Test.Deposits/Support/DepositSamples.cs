using System.Text.Json;
using Bank.Common.Dtos.Request;

namespace Tests.Deposits.Support;

public static class DepositSamples
{
    private static int UniqueNumber = 1;

    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public static DepositContractRequest AlfaSafe(int clientId = 1, Action<DepositContractRequest>? mutate = null)
    {
        var request = new DepositContractRequest
        {
            ClientId = clientId,
            ProductId = 1,
            Number = "",
            CurrencyId = 1,
            StartDate = "20.09.2026",
            EndDate = "20.10.2027",
            TermMonths = 13,
            Amount = 1000m,
            AnnualRate = 5.5m
        };
        mutate?.Invoke(request);
        return request;
    }

    public static DepositContractRequest AlfaVklad(int clientId = 1, Action<DepositContractRequest>? mutate = null)
    {
        var request = new DepositContractRequest
        {
            ClientId = clientId,
            ProductId = 2,
            Number = "",
            CurrencyId = 1,
            StartDate = "20.09.2026",
            EndDate = "20.10.2027",
            TermMonths = 13,
            Amount = 5000m,
            AnnualRate = 12m
        };
        mutate?.Invoke(request);
        return request;
    }

    public static DepositContractRequest UniqueSafe(int clientId = 4)
    {
        var n = Interlocked.Increment(ref UniqueNumber);
        return AlfaSafe(clientId, r => r.Number = $"Т-{n:0000}");
    }

    public static StringContent JsonBody(DepositContractRequest request)
        => new(JsonSerializer.Serialize(request, JsonOptions), System.Text.Encoding.UTF8, "application/json");
}

public sealed class ValidationProblemDto
{
    public Dictionary<string, string[]> Errors { get; set; } = [];
}
