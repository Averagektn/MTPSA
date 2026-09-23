namespace Bank.Clients.Validation;

public static class ValidationPatterns
{
    public const string PersonName = @"^[A-Za-zА-Яа-яЁёІіЎў''-]+$";
    public const string PassportSeries = @"^[A-Za-z]{2}$";
    public const string PassportNumber = @"^\d{7}$";
    public const string IdentificationNumber = @"^\d{7}[A-Za-z]\d{3}[A-Za-z]{2}\d$";
    public const string HomePhone = @"^\+375 \(\d{2}\) \d{3}-\d{2}-\d{2}$";
    public const string MobilePhone = @"^\+375 \((29|33|44|25)\) \d{3}-\d{2}-\d{2}$";
    public const string Email = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
}
