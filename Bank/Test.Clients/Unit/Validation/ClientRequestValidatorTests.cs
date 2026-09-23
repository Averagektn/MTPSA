using Bank.Common.Dtos.Request;
using Bank.Clients.Validation;
using Tests.Clients.Support;

namespace Tests.Clients.Unit.Validation;

[TestClass]
public sealed class ClientRequestValidatorTests
{
    private readonly ClientRequestValidator _validator = new();

    [TestMethod]
    public async Task Valid_required_only_request_passes()
    {
        var request = ClientSamples.RequiredOnly();
        request.Normalize();
        var result = await _validator.ValidateAsync(request);
        result.IsValid.Should().BeTrue(string.Join("; ", result.Errors.Select(e => e.ErrorMessage)));
    }

    [TestMethod]
    public async Task Last_name_1234_is_rejected()
    {
        var request = ClientSamples.IvanovValid(r => r.LastName = "1234");
        request.Normalize();
        var result = await _validator.ValidateAsync(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "personNameInvalid");
    }

    [TestMethod]
    public async Task Blank_first_name_is_rejected()
    {
        var request = ClientSamples.IvanovValid(r => r.FirstName = " ");
        request.Normalize();
        var result = await _validator.ValidateAsync(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "firstNameRequired");
    }

    [TestMethod]
    [DataRow("not-a-date", "birthDateInvalid")]
    [DataRow("31.02.2015", "birthDateInvalid")]
    [DataRow("29.02.2015", "birthDateInvalid")]
    public async Task Invalid_birth_date_is_rejected(string value, string key)
    {
        var request = ClientSamples.IvanovValid(r => r.BirthDate = value);
        request.Normalize();
        var result = await _validator.ValidateAsync(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == key);
    }

    [TestMethod]
    [DataRow("lastName", "", "lastNameRequired")]
    [DataRow("firstName", "", "firstNameRequired")]
    [DataRow("patronymic", "", "patronymicRequired")]
    [DataRow("birthDate", "", "birthDateRequired")]
    [DataRow("passportSeries", "", "passportSeriesRequired")]
    [DataRow("passportNumber", "", "passportNumberRequired")]
    [DataRow("issuedBy", "", "issuedByRequired")]
    [DataRow("issueDate", "", "issueDateRequired")]
    [DataRow("identificationNumber", "", "identificationNumberRequired")]
    [DataRow("birthPlace", "", "birthPlaceRequired")]
    [DataRow("residenceAddress", "", "residenceAddressRequired")]
    public async Task Missing_required_text_is_rejected(string field, string value, string key)
    {
        var request = ClientSamples.RequiredOnly();
        typeof(ClientRequest).GetProperty(ToPascal(field))!.SetValue(request, value);
        request.Normalize();
        var result = await _validator.ValidateAsync(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == key, string.Join(",", result.Errors.Select(e => e.ErrorMessage)));
    }

    [TestMethod]
    [DataRow("ResidenceCityId", "residenceCityRequired")]
    [DataRow("RegistrationCityId", "registrationCityRequired")]
    [DataRow("MaritalStatusId", "maritalStatusRequired")]
    [DataRow("CitizenshipId", "citizenshipRequired")]
    [DataRow("DisabilityId", "disabilityRequired")]
    public async Task Missing_required_lookup_is_rejected(string field, string key)
    {
        var request = ClientSamples.RequiredOnly();
        typeof(ClientRequest).GetProperty(field)!.SetValue(request, 0);
        request.Normalize();
        var result = await _validator.ValidateAsync(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == key);
    }

    [TestMethod]
    [DataRow("passportSeries", "A", "passportSeriesInvalid")]
    [DataRow("passportNumber", "123", "passportNumberInvalid")]
    [DataRow("identificationNumber", "ABC", "identificationNumberInvalid")]
    [DataRow("homePhone", "12345", "homePhoneInvalid")]
    [DataRow("mobilePhone", "+375 (17) 111-11-11", "mobilePhoneInvalid")]
    [DataRow("email", "not-an-email", "emailInvalid")]
    public async Task Mask_mismatch_is_rejected(string field, string value, string key)
    {
        var request = ClientSamples.UniqueValid();
        typeof(ClientRequest).GetProperty(ToPascal(field))!.SetValue(request, value);
        request.Normalize();
        var result = await _validator.ValidateAsync(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == key, string.Join(",", result.Errors.Select(e => e.ErrorMessage)));
    }

    private static string ToPascal(string field)
        => char.ToUpperInvariant(field[0]) + field[1..];
}
