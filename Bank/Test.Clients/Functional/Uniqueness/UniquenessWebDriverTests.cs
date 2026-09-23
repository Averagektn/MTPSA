using Tests.Clients.Functional.Infrastructure;
using Tests.Clients.Support;

namespace Tests.Clients.Functional.Uniqueness;

[TestClass]
[DoNotParallelize]
public sealed class UniquenessWebDriverTests : WebDriverTestBase
{
    [ClassInitialize]
    public static Task ClassInitialize(TestContext context)
        => WebDriverSession.EnsureStartedAsync();

    [TestMethod]
    public void Duplicate_student_shows_error_and_is_not_written_twice()
    {
        var before = WebDriverSession.Host.CountByLastName("Glushachenko");
        WebDriverSession.Page.OpenNew(WebDriverSession.Host.BaseUrl);
        WebDriverSession.Page.Fill(ClientSamples.SeededStudent());
        WebDriverSession.Page.SubmitExpectingFieldError();
        WebDriverSession.Page.HasFieldError("lastName").Should().BeTrue();
        WebDriverSession.Host.CountByLastName("Glushachenko").Should().Be(before);
    }

    [TestMethod]
    public void Duplicate_passport_shows_error()
    {
        var request = ClientSamples.UniqueValid(r =>
        {
            r.PassportSeries = "AB";
            r.PassportNumber = "7654321";
        });
        var before = WebDriverSession.Host.CountByPassport("AB", "7654321");
        WebDriverSession.Page.OpenNew(WebDriverSession.Host.BaseUrl);
        WebDriverSession.Page.Fill(request);
        WebDriverSession.Page.SubmitExpectingFieldError();
        WebDriverSession.Page.HasFieldError("passportNumber").Should().BeTrue();
        WebDriverSession.Host.CountByPassport("AB", "7654321").Should().Be(before);
    }

    [TestMethod]
    public void Duplicate_identification_number_shows_error()
    {
        var request = ClientSamples.UniqueValid(r => r.IdentificationNumber = "1505033A015PB7");
        var before = WebDriverSession.Host.CountByIdentification("1505033A015PB7");
        WebDriverSession.Page.OpenNew(WebDriverSession.Host.BaseUrl);
        WebDriverSession.Page.Fill(request);
        WebDriverSession.Page.SubmitExpectingFieldError();
        WebDriverSession.Page.HasFieldError("identificationNumber").Should().BeTrue();
        WebDriverSession.Host.CountByIdentification("1505033A015PB7").Should().Be(before);
    }
}
