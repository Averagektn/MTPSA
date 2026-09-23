namespace Bank.Common.Localization;

public static class RequestLocale
{
    public const string English = "en";
    public const string Russian = "ru";

    private static readonly AsyncLocal<string> CurrentValue = new();

    public static string Current => CurrentValue.Value == Russian ? Russian : English;

    public static bool IsRussian => Current == Russian;

    public static void SetFromAcceptLanguage(string? header)
    {
        CurrentValue.Value = Parse(header);
    }

    public static string Parse(string? header)
    {
        if (string.IsNullOrWhiteSpace(header))
        {
            return English;
        }

        foreach (var part in header.Split(','))
        {
            var tag = part.Split(';')[0].Trim();
            if (tag.StartsWith("ru", StringComparison.OrdinalIgnoreCase))
            {
                return Russian;
            }

            if (tag.StartsWith("en", StringComparison.OrdinalIgnoreCase))
            {
                return English;
            }
        }

        return English;
    }

    public static string Pick(string nameEn, string nameRu)
        => IsRussian ? nameRu : nameEn;
}
