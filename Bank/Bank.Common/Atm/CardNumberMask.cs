namespace Bank.Common.Atm;

public static class CardNumberMask
{
    public static string Mask(string cardNumber)
    {
        var digits = new string(cardNumber.Where(char.IsDigit).ToArray());
        if (digits.Length < 8)
        {
            return "****";
        }

        return $"{digits[..4]}********{digits[^4..]}";
    }

    public static string Digits(string value)
        => new(value.Where(char.IsDigit).ToArray());
}
