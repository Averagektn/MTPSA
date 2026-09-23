using System.Globalization;
using FluentResults;

namespace Bank.Common.Validation;

public static class DateParsing
{
    private static readonly string[] Formats = ["yyyy-MM-dd", "dd.MM.yyyy", "d.MM.yyyy", "dd.M.yyyy", "d.M.yyyy"];
    private static readonly CultureInfo[] Cultures = [CultureInfo.InvariantCulture, CultureInfo.GetCultureInfo("ru-RU")];

    public static Result<DateOnly> Parse(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return Result.Fail<DateOnly>("empty");
        }

        var value = input.Trim();
        foreach (var culture in Cultures)
        {
            if (DateOnly.TryParseExact(value, Formats, culture, DateTimeStyles.None, out var date))
            {
                return Result.Ok(date);
            }
        }

        return Result.Fail<DateOnly>("invalid");
    }
}
