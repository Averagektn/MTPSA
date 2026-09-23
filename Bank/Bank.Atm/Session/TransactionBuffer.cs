using System.Text.Json;
using Bank.Common.Atm;
using Bank.Common.Dtos.Request;
using Bank.Common.Dtos.Response;

namespace Bank.Atm.Session;

public sealed class TransactionBuffer
{
    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    private readonly List<AtmTransactionField> _fields;

    public TransactionBuffer(IEnumerable<AtmTransactionField>? fields = null)
        => _fields = fields?.Select(f => new AtmTransactionField { Key = f.Key, Value = f.Value }).ToList() ?? [];

    public IReadOnlyList<AtmTransactionField> Fields => _fields;

    public static TransactionBuffer Parse(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new TransactionBuffer();
        }

        var fields = JsonSerializer.Deserialize<List<AtmTransactionField>>(json, Json);
        return new TransactionBuffer(fields);
    }

    public string ToJson() => JsonSerializer.Serialize(_fields, Json);

    public void Set(string key, string value)
    {
        var existing = _fields.FirstOrDefault(f => string.Equals(f.Key, key, StringComparison.OrdinalIgnoreCase));
        if (existing is null)
        {
            _fields.Add(new AtmTransactionField { Key = key, Value = value });
            return;
        }

        existing.Value = value;
    }

    public string? Get(string key)
        => _fields.FirstOrDefault(f => string.Equals(f.Key, key, StringComparison.OrdinalIgnoreCase))?.Value;

    public void Keep(params string[] keys)
    {
        var keep = keys.ToHashSet(StringComparer.OrdinalIgnoreCase);
        _fields.RemoveAll(f => !keep.Contains(f.Key));
    }

    public IReadOnlyList<AtmTransactionFieldView> Visible()
        => _fields
            .Select(f => new AtmTransactionFieldView(
                f.Key,
                string.Equals(f.Key, "pin", StringComparison.OrdinalIgnoreCase) ? "****" : f.Value))
            .ToList();

    public AtmTransactionRequest ToRequest(AtmOperation operation)
    {
        var amountRaw = Get("amount");
        decimal? amount = decimal.TryParse(amountRaw, out var parsed) ? parsed : null;
        return new AtmTransactionRequest
        {
            CardNumber = Get("cardNumber") ?? "",
            Pin = Get("pin") ?? "",
            Operation = operation.ToWire(),
            Amount = amount,
            OperatorCode = Get("operatorCode"),
            Phone = Get("phone"),
            Fields = _fields.Select(f => new AtmTransactionField { Key = f.Key, Value = f.Value }).ToList()
        };
    }
}

public static class AmountParser
{
    public static bool TryParseNonNegativeInteger(string value, out int amount)
    {
        amount = 0;
        var digits = CardNumberMask.Digits(value);
        if (digits.Length == 0 || digits.Length != value.Trim().Length)
        {
            return false;
        }

        if (!int.TryParse(digits, out amount) || amount < 0)
        {
            amount = 0;
            return false;
        }

        return true;
    }
}
