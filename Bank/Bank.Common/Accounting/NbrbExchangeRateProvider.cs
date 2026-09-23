using System.Collections.Concurrent;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Bank.Common.Accounting;

public sealed class NbrbExchangeRateProvider(HttpClient http) : IExchangeRateProvider
{
    private readonly ConcurrentDictionary<string, decimal> cache = new(StringComparer.OrdinalIgnoreCase);

    public async Task<decimal> GetBynPerUnitAsync(string currencyCode, DateOnly onDate, CancellationToken cancellationToken)
    {
        if (currencyCode.Equals("BYN", StringComparison.OrdinalIgnoreCase))
        {
            return 1m;
        }

        var key = $"{onDate:yyyy-MM-dd}:{currencyCode}";
        if (cache.TryGetValue(key, out var cached))
        {
            return cached;
        }

        var date = $"{onDate.Year}-{onDate.Month}-{onDate.Day}";
        var rates = await http.GetFromJsonAsync<List<NbrbRate>>(
            $"exrates/rates?ondate={date}&periodicity=0",
            cancellationToken) ?? [];

        foreach (var rate in rates)
        {
            if (string.IsNullOrWhiteSpace(rate.Abbreviation) || rate.Scale == 0)
            {
                continue;
            }

            cache[$"{onDate:yyyy-MM-dd}:{rate.Abbreviation}"] = rate.OfficialRate / rate.Scale;
        }

        if (!cache.TryGetValue(key, out var value))
        {
            throw new InvalidOperationException($"NBRB has no rate for {currencyCode} on {onDate:yyyy-MM-dd}.");
        }

        return value;
    }

    private sealed class NbrbRate
    {
        [JsonPropertyName("Cur_Abbreviation")]
        public string Abbreviation { get; set; } = "";

        [JsonPropertyName("Cur_Scale")]
        public decimal Scale { get; set; }

        [JsonPropertyName("Cur_OfficialRate")]
        public decimal OfficialRate { get; set; }
    }
}
