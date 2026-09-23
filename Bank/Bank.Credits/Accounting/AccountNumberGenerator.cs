namespace Bank.Credits.Accounting;

public static class AccountNumberGenerator
{
    private static readonly int[] Weights = [7, 1, 3, 7, 1, 3, 7, 1, 3, 7, 1, 3];

    public static int CheckDigit(string twelveDigits)
    {
        if (twelveDigits.Length != 12 || twelveDigits.Any(ch => ch is < '0' or > '9'))
        {
            throw new ArgumentException("Account stem must be 12 digits.", nameof(twelveDigits));
        }

        var sum = 0;
        for (var i = 0; i < 12; i++)
        {
            sum += (twelveDigits[i] - '0') * Weights[i];
        }

        return sum % 10;
    }

    public static string ClientCode(int clientId)
        => (10000 + clientId).ToString("D5");

    public static string Build(string chartCode, string clientCode, int sequence)
    {
        if (chartCode.Length != 4 || chartCode.Any(ch => ch is < '0' or > '9'))
        {
            throw new ArgumentException("Chart code must be 4 digits.", nameof(chartCode));
        }

        if (clientCode.Length != 5 || clientCode.Any(ch => ch is < '0' or > '9'))
        {
            throw new ArgumentException("Client code must be 5 digits.", nameof(clientCode));
        }

        if (sequence is < 0 or > 999)
        {
            throw new ArgumentOutOfRangeException(nameof(sequence));
        }

        var stem = $"{chartCode}{clientCode}{sequence:D3}";
        return stem + CheckDigit(stem);
    }
}
