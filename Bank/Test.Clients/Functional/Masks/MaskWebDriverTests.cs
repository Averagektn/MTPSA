using Tests.Clients.Functional.Infrastructure;
using Tests.Clients.Support;

namespace Tests.Clients.Functional.Masks;

[TestClass]
[DoNotParallelize]
public sealed class MaskWebDriverTests : WebDriverTestBase
{
    [ClassInitialize]
    public static Task ClassInitialize(TestContext context)
        => WebDriverSession.EnsureStartedAsync();

    [TestMethod]
    [DataRow("passportSeries", "A")]
    [DataRow("passportNumber", "123")]
    [DataRow("identificationNumber", "123")]
    [DataRow("homePhone", "111")]
    [DataRow("mobilePhone", "+375 (17) 111-11-11")]
    [DataRow("email", "not-an-email")]
    public void Invalid_mask_is_blocked_on_the_client(string field, string value)
    {
        var request = ClientSamples.IvanovValid();
        typeof(Bank.Common.Dtos.Request.ClientRequest).GetProperty(HttpAssertions.ToPascal(field))!.SetValue(request, value);
        var before = WebDriverSession.Host.CountByLastName("Ivanov");
        WebDriverSession.Page.OpenNew(WebDriverSession.Host.BaseUrl);
        WebDriverSession.Page.Fill(request);
        WebDriverSession.Page.SubmitExpectingFieldError();
        WebDriverSession.Page.HasFieldError(field).Should().BeTrue($"Expected a client-side mask error on {field}");
        WebDriverSession.Host.CountByLastName("Ivanov").Should().Be(before);
    }
}
