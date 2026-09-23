using System.Net;
using Tests.Clients.Support;

namespace Tests.Clients.Integration.Validation;

[TestClass]
public sealed class PersonAndDateApiTests
{
    private static BankTestHost _host = null!;

    [ClassInitialize]
    public static async Task ClassInitialize(TestContext context)
        => _host = await BankTestHost.StartAsync();

    [ClassCleanup]
    public static async Task ClassCleanup()
    {
        if (_host is not null)
        {
            await _host.DisposeAsync();
        }
    }

    [TestMethod]
    public async Task Last_name_1234_is_rejected_and_not_stored()
    {
        var request = ClientSamples.IvanovValid(r => r.LastName = "1234");
        var response = await _host.Client.PostAsync("api/clients", ClientSamples.JsonBody(request));
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        _host.CountByLastName("1234").Should().Be(0);
    }

    [TestMethod]
    public async Task Blank_first_name_is_rejected_and_ivanov_count_stays()
    {
        var before = _host.CountByLastName("Ivanov");
        var request = ClientSamples.IvanovValid(r => r.FirstName = " ");
        var response = await _host.Client.PostAsync("api/clients", ClientSamples.JsonBody(request));
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        _host.CountByLastName("Ivanov").Should().Be(before);
    }

    [TestMethod]
    [DataRow("not-a-date")]
    [DataRow("31.02.2015")]
    [DataRow("29.02.2015")]
    public async Task Invalid_birth_date_is_rejected(string birthDate)
    {
        var before = _host.CountByLastName("Ivanov");
        var request = ClientSamples.IvanovValid(r => r.BirthDate = birthDate);
        var response = await _host.Client.PostAsync("api/clients", ClientSamples.JsonBody(request));
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        _host.CountByLastName("Ivanov").Should().Be(before);
    }
}
