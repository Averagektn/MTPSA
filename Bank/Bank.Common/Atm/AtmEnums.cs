namespace Bank.Common.Atm;

public enum AtmOperation
{
    Withdraw = 1,
    Balance = 2,
    DepositBalance = 3,
    Payment = 4,
    Eject = 5
}

public enum AtmScreen
{
    Card = 1,
    Pin = 2,
    Locked = 3,
    Menu = 4,
    WithdrawAmount = 5,
    PaymentOperator = 6,
    PaymentPhone = 7,
    PaymentAmount = 8,
    PaymentConfirm = 9,
    ReceiptChoice = 10,
    Result = 11
}

public enum AtmInputKind
{
    Pin = 1,
    Menu = 2,
    Amount = 3,
    Operator = 4,
    Phone = 5,
    Confirm = 6,
    Receipt = 7,
    Continue = 8,
    Eject = 9
}

public enum AtmChoice
{
    No = 1,
    Yes = 2
}

public static class AtmEnum
{
    public static bool TryParse<T>(string? value, out T result) where T : struct, Enum
    {
        result = default;
        if (string.IsNullOrWhiteSpace(value) || int.TryParse(value.Trim(), out _))
        {
            return false;
        }

        return Enum.TryParse(value, ignoreCase: true, out result) && Enum.IsDefined(result);
    }

    public static T Parse<T>(string value) where T : struct, Enum
        => TryParse<T>(value, out var result)
            ? result
            : throw new ArgumentOutOfRangeException(nameof(value), value, $"Unknown {typeof(T).Name}.");

    public static string ToWire(this Enum value)
    {
        var name = value.ToString();
        return name.Length switch
        {
            0 => name,
            1 => name.ToLowerInvariant(),
            _ => char.ToLowerInvariant(name[0]) + name[1..]
        };
    }
}
