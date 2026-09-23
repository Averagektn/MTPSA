namespace Bank.Clients.Models;

public sealed class Client
{
    public int Id { get; set; }

    public required string LastName { get; set; }
    public required string FirstName { get; set; }
    public required string Patronymic { get; set; }
    public DateOnly BirthDate { get; set; }

    public required string PassportSeries { get; set; }
    public required string PassportNumber { get; set; }
    public required string IssuedBy { get; set; }
    public DateOnly IssueDate { get; set; }
    public required string IdentificationNumber { get; set; }

    public required string BirthPlace { get; set; }
    public int ResidenceCityId { get; set; }
    public City ResidenceCity { get; set; } = null!;
    public required string ResidenceAddress { get; set; }
    public int RegistrationCityId { get; set; }
    public City RegistrationCity { get; set; } = null!;

    public string? HomePhone { get; set; }
    public string? MobilePhone { get; set; }
    public string? Email { get; set; }
    public string? Workplace { get; set; }
    public string? Position { get; set; }

    public int MaritalStatusId { get; set; }
    public MaritalStatus MaritalStatus { get; set; } = null!;
    public int CitizenshipId { get; set; }
    public Citizenship Citizenship { get; set; } = null!;
    public int DisabilityId { get; set; }
    public Disability Disability { get; set; } = null!;

    public bool Pensioner { get; set; }
    public decimal? MonthlyIncome { get; set; }
}
