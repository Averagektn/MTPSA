using Tests.Clients.Functional.Infrastructure;
using Tests.Clients.Support;

namespace Tests.Clients.Functional.Validation;

[TestClass]
[DoNotParallelize]
public sealed class PersonAndDateWebDriverTests : WebDriverTestBase
{
    [ClassInitialize]
    public static Task ClassInitialize(TestContext context)
        => WebDriverSession.EnsureStartedAsync();

    [TestMethod]
    public void Last_name_1234_is_blocked_on_the_client()
    {
        var request = ClientSamples.IvanovValid(r => r.LastName = "1234");
        WebDriverSession.Page.OpenNew(WebDriverSession.Host.BaseUrl);
        WebDriverSession.Page.Fill(request);
        WebDriverSession.Page.SubmitExpectingFieldError();
        WebDriverSession.Page.HasFieldError("lastName").Should().BeTrue();
        WebDriverSession.Host.CountByLastName("1234").Should().Be(0);
    }

    [TestMethod]
    public void Blank_first_name_is_blocked_on_the_client()
    {
        var before = WebDriverSession.Host.CountByLastName("Ivanov");
        var request = ClientSamples.IvanovValid(r => r.FirstName = " ");
        WebDriverSession.Page.OpenNew(WebDriverSession.Host.BaseUrl);
        WebDriverSession.Page.Fill(request);
        WebDriverSession.Page.SubmitExpectingFieldError();
        WebDriverSession.Page.HasFieldError("firstName").Should().BeTrue();
        WebDriverSession.Host.CountByLastName("Ivanov").Should().Be(before);
    }

    [TestMethod]
    [DataRow("not-a-date")]
    [DataRow("31.02.2015")]
    [DataRow("29.02.2015")]
    public void Invalid_birth_date_is_blocked_on_the_client(string birthDate)
    {
        var before = WebDriverSession.Host.CountByLastName("Ivanov");
        var request = ClientSamples.IvanovValid(r => r.BirthDate = birthDate);
        WebDriverSession.Page.OpenNew(WebDriverSession.Host.BaseUrl);
        WebDriverSession.Page.Fill(request);
        WebDriverSession.Page.SubmitExpectingFieldError();
        WebDriverSession.Page.HasFieldError("birthDate").Should().BeTrue();
        WebDriverSession.Host.CountByLastName("Ivanov").Should().Be(before);
    }
}
