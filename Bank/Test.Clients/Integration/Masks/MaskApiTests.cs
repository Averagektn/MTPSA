using System.Net;
using Tests.Clients.Support;

namespace Tests.Clients.Integration.Masks;

[TestClass]
public sealed class MaskApiTests
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
    [DataRow("passportSeries", "A")]
    [DataRow("passportNumber", "12")]
    [DataRow("identificationNumber", "123")]
    [DataRow("homePhone", "111")]
    [DataRow("mobilePhone", "222")]
    [DataRow("email", "bad")]
    public async Task Invalid_mask_is_rejected(string field, string value)
    {
        var request = ClientSamples.UniqueValid();
        typeof(Bank.Common.Dtos.Request.ClientRequest).GetProperty(HttpAssertions.ToPascal(field))!.SetValue(request, value);
        var response = await _host.Client.PostAsync("api/clients", ClientSamples.JsonBody(request));
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
