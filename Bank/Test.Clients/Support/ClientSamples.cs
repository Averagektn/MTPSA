using System.Text.Json;
using Bank.Common.Dtos.Request;

namespace Tests.Clients.Support;

public static class ClientSamples
{
    private static int UniqueNumber = 9000000;

    public static ClientRequest SeededStudent() => new()
    {
        LastName = "Glushachenko",
        FirstName = "Nikita",
        Patronymic = "Sergeevich",
        BirthDate = "15.05.2003",
        PassportSeries = "AB",
        PassportNumber = "7654321",
        IssuedBy = "Central District Police Department, Minsk",
        IssueDate = "20.06.2019",
        IdentificationNumber = "1505033A015PB7",
        BirthPlace = "Minsk",
        ResidenceCityId = 1,
        ResidenceAddress = "Independence Ave. 10, apt. 5",
        RegistrationCityId = 1,
        HomePhone = "+375 (17) 234-56-78",
        MobilePhone = "+375 (29) 111-22-33",
        Email = "n.glushachenko@example.com",
        Workplace = "BSUIR",
        Position = "Student",
        MaritalStatusId = 1,
        CitizenshipId = 1,
        DisabilityId = 1,
        Pensioner = false,
        MonthlyIncome = 900
    };

    public static ClientRequest UniqueValid(Action<ClientRequest>? mutate = null)
    {
        var n = Interlocked.Increment(ref UniqueNumber);
        var passport = n.ToString("0000000");
        var request = RequiredOnly();
        request.LastName = "Smirnov";
        request.FirstName = "Oleg";
        request.Patronymic = "Igorevich";
        request.BirthDate = "21.03.1992";
        request.PassportSeries = "ZZ";
        request.PassportNumber = passport;
        request.IdentificationNumber = $"{passport}A111PB1";
        request.HomePhone = "+375 (17) 200-00-01";
        request.MobilePhone = "+375 (29) 200-00-02";
        request.Email = $"o.smirnov.{n}@example.com";
        request.Workplace = "Bank";
        request.Position = "Analyst";
        request.MonthlyIncome = 1200;
        mutate?.Invoke(request);
        return request;
    }

    public static ClientRequest RequiredOnly(Action<ClientRequest>? mutate = null)
    {
        var n = Interlocked.Increment(ref UniqueNumber);
        var passport = n.ToString("0000000");
        var request = new ClientRequest
        {
            LastName = "Kovalev",
            FirstName = "Ivan",
            Patronymic = "Petrovich",
            BirthDate = "10.10.1990",
            PassportSeries = "ZZ",
            PassportNumber = passport,
            IssuedBy = "Minsk City Police Department",
            IssueDate = "11.11.2011",
            IdentificationNumber = $"{passport}B222PB2",
            BirthPlace = "Minsk",
            ResidenceCityId = 1,
            ResidenceAddress = "Lenin St. 1",
            RegistrationCityId = 1,
            MaritalStatusId = 1,
            CitizenshipId = 1,
            DisabilityId = 1,
            Pensioner = false
        };
        mutate?.Invoke(request);
        return request;
    }

    public static ClientRequest IvanovValid(Action<ClientRequest>? mutate = null)
    {
        var request = UniqueValid();
        request.LastName = "Ivanov";
        request.FirstName = "Ivan";
        request.Patronymic = "Ivanovich";
        mutate?.Invoke(request);
        return request;
    }

    public static void ClearField(ClientRequest request, string field)
    {
        switch (field)
        {
            case "lastName": request.LastName = ""; break;
            case "firstName": request.FirstName = ""; break;
            case "patronymic": request.Patronymic = ""; break;
            case "birthDate": request.BirthDate = ""; break;
            case "passportSeries": request.PassportSeries = ""; break;
            case "passportNumber": request.PassportNumber = ""; break;
            case "issuedBy": request.IssuedBy = ""; break;
            case "issueDate": request.IssueDate = ""; break;
            case "identificationNumber": request.IdentificationNumber = ""; break;
            case "birthPlace": request.BirthPlace = ""; break;
            case "residenceAddress": request.ResidenceAddress = ""; break;
            case "residenceCityId": request.ResidenceCityId = 0; break;
            case "registrationCityId": request.RegistrationCityId = 0; break;
            case "maritalStatusId": request.MaritalStatusId = 0; break;
            case "citizenshipId": request.CitizenshipId = 0; break;
            case "disabilityId": request.DisabilityId = 0; break;
            default: throw new ArgumentOutOfRangeException(nameof(field), field, "Unknown form field.");
        }
    }

    public static StringContent JsonBody(ClientRequest request)
        => new(JsonSerializer.Serialize(request, JsonOptions), System.Text.Encoding.UTF8, "application/json");

    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };
}

public sealed class ValidationProblemDto
{
    public Dictionary<string, string[]> Errors { get; set; } = [];
}
