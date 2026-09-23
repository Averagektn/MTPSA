using System.Text.Json;
using Bank.Common.Dtos.Request;

namespace Tests.Atm.Support;

public static class AtmSamples
{
    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public const string Card = "4277000011112222";
    public const string Pin = "1111";
    public const string WrongPin = "9999";

    public static StringContent JsonBody<T>(T value)
        => new(JsonSerializer.Serialize(value, JsonOptions), System.Text.Encoding.UTF8, "application/json");

    public static StringContent Insert(string cardNumber)
        => JsonBody(new AtmInsertCardRequest { CardNumber = cardNumber });

    public static StringContent Input(string kind, string value)
        => JsonBody(new AtmInputRequest { Kind = kind, Value = value });
}

public sealed class ValidationProblemDto
{
    public Dictionary<string, string[]> Errors { get; set; } = [];
}
