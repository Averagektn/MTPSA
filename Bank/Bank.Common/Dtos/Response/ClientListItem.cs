namespace Bank.Common.Dtos.Response;

public sealed record ClientListItem(
    int Id,
    string LastName,
    string FirstName,
    string Patronymic,
    DateOnly BirthDate,
    string PassportSeries,
    string PassportNumber,
    string IdentificationNumber,
    string ResidenceCity,
    string? MobilePhone,
    string? Email);
