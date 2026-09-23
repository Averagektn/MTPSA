using Bank.Common.Validation;

namespace Bank.Common.Dtos.Request;

public sealed class ClientRequest
{
    public string LastName { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string Patronymic { get; set; } = "";
    public string BirthDate { get; set; } = "";
    public string PassportSeries { get; set; } = "";
    public string PassportNumber { get; set; } = "";
    public string IssuedBy { get; set; } = "";
    public string IssueDate { get; set; } = "";
    public string IdentificationNumber { get; set; } = "";
    public string BirthPlace { get; set; } = "";
    public int ResidenceCityId { get; set; }
    public string ResidenceAddress { get; set; } = "";
    public int RegistrationCityId { get; set; }
    public string? HomePhone { get; set; }
    public string? MobilePhone { get; set; }
    public string? Email { get; set; }
    public string? Workplace { get; set; }
    public string? Position { get; set; }
    public int MaritalStatusId { get; set; }
    public int CitizenshipId { get; set; }
    public int DisabilityId { get; set; }
    public bool Pensioner { get; set; }
    public decimal? MonthlyIncome { get; set; }

    public DateOnly? ParsedBirthDate { get; private set; }
    public DateOnly? ParsedIssueDate { get; private set; }

    public void Normalize()
    {
        LastName = LastName.Trim();
        FirstName = FirstName.Trim();
        Patronymic = Patronymic.Trim();
        BirthDate = BirthDate.Trim();
        PassportSeries = PassportSeries.Trim().ToUpperInvariant();
        PassportNumber = PassportNumber.Trim();
        IssuedBy = IssuedBy.Trim();
        IssueDate = IssueDate.Trim();
        IdentificationNumber = IdentificationNumber.Trim().ToUpperInvariant();
        BirthPlace = BirthPlace.Trim();
        ResidenceAddress = ResidenceAddress.Trim();
        HomePhone = EmptyToNull(HomePhone);
        MobilePhone = EmptyToNull(MobilePhone);
        Email = EmptyToNull(Email);
        Workplace = EmptyToNull(Workplace);
        Position = EmptyToNull(Position);

        var birth = DateParsing.Parse(BirthDate);
        ParsedBirthDate = birth.IsSuccess ? birth.Value : null;
        var issue = DateParsing.Parse(IssueDate);
        ParsedIssueDate = issue.IsSuccess ? issue.Value : null;
    }

    private static string? EmptyToNull(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
