using Tests.Clients.Functional.Infrastructure;
using Tests.Clients.Support;

namespace Tests.Clients.Functional.RequiredFields;

[TestClass]
[DoNotParallelize]
public sealed class RequiredFieldsWebDriverTests : WebDriverTestBase
{
    [ClassInitialize]
    public static Task ClassInitialize(TestContext context)
        => WebDriverSession.EnsureStartedAsync();

    [TestMethod]
    [DataRow("lastName")]
    [DataRow("firstName")]
    [DataRow("patronymic")]
    [DataRow("birthDate")]
    [DataRow("passportSeries")]
    [DataRow("passportNumber")]
    [DataRow("issuedBy")]
    [DataRow("issueDate")]
    [DataRow("identificationNumber")]
    [DataRow("birthPlace")]
    [DataRow("residenceAddress")]
    [DataRow("residenceCityId")]
    [DataRow("registrationCityId")]
    [DataRow("maritalStatusId")]
    [DataRow("citizenshipId")]
    [DataRow("disabilityId")]
    public void Missing_required_field_is_blocked_on_the_client(string field)
    {
        var request = ClientSamples.RequiredOnly();
        ClientSamples.ClearField(request, field);
        WebDriverSession.Page.OpenNew(WebDriverSession.Host.BaseUrl);
        WebDriverSession.Page.Fill(request, includeOptional: false);
        WebDriverSession.Page.SubmitExpectingFieldError();
        WebDriverSession.Page.HasFieldError(field).Should().BeTrue($"Expected a client-side error on {field}");
        WebDriverSession.Host.CountByPassport(request.PassportSeries, request.PassportNumber).Should().Be(0);
    }

    [TestMethod]
    public void Required_only_form_is_saved()
    {
        var request = ClientSamples.RequiredOnly();
        WebDriverSession.Page.OpenNew(WebDriverSession.Host.BaseUrl);
        WebDriverSession.Page.Fill(request, includeOptional: false);
        WebDriverSession.Page.SubmitExpectingList();
        WebDriverSession.Host.CountByPassport(request.PassportSeries, request.PassportNumber).Should().Be(1);
    }
}
