namespace Bank.Common.Dtos.Request;

public sealed class AtmTransactionRequest
{
    public string CardNumber { get; set; } = "";
    public string Pin { get; set; } = "";
    public string Operation { get; set; } = "";
    public decimal? Amount { get; set; }
    public string? OperatorCode { get; set; }
    public string? Phone { get; set; }
    public List<AtmTransactionField> Fields { get; set; } = [];

    public void Normalize()
    {
        CardNumber = Digits(CardNumber);
        Pin = Digits(Pin);
        Operation = Operation.Trim().ToLowerInvariant();
        OperatorCode = string.IsNullOrWhiteSpace(OperatorCode) ? null : OperatorCode.Trim().ToUpperInvariant();
        Phone = string.IsNullOrWhiteSpace(Phone) ? null : Digits(Phone);

        foreach (var field in Fields)
        {
            if (string.Equals(field.Key, "cardNumber", StringComparison.OrdinalIgnoreCase) && CardNumber.Length == 0)
            {
                CardNumber = Digits(field.Value);
            }
            else if (string.Equals(field.Key, "pin", StringComparison.OrdinalIgnoreCase) && Pin.Length == 0)
            {
                Pin = Digits(field.Value);
            }
            else if (string.Equals(field.Key, "operation", StringComparison.OrdinalIgnoreCase) && Operation.Length == 0)
            {
                Operation = field.Value.Trim().ToLowerInvariant();
            }
            else if (string.Equals(field.Key, "amount", StringComparison.OrdinalIgnoreCase) && Amount is null
                     && decimal.TryParse(field.Value, out var amount))
            {
                Amount = amount;
            }
            else if (string.Equals(field.Key, "operatorCode", StringComparison.OrdinalIgnoreCase) && OperatorCode is null)
            {
                OperatorCode = field.Value.Trim().ToUpperInvariant();
            }
            else if (string.Equals(field.Key, "phone", StringComparison.OrdinalIgnoreCase) && Phone is null)
            {
                Phone = Digits(field.Value);
            }
        }
    }

    private static string Digits(string value)
        => new(value.Where(char.IsDigit).ToArray());
}
