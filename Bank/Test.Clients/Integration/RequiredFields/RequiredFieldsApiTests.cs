using System.Net;
using Tests.Clients.Support;

namespace Tests.Clients.Integration.RequiredFields;

[TestClass]
public sealed class RequiredFieldsApiTests
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
    [DataRow("BirthDate")]
    [DataRow("LastName")]
    [DataRow("IssuedBy")]
    [DataRow("ResidenceAddress")]
    public async Task Missing_required_field_is_rejected(string field)
    {
        var request = ClientSamples.RequiredOnly();
        var property = typeof(Bank.Common.Dtos.Request.ClientRequest).GetProperty(field)!;
        property.SetValue(request, property.PropertyType == typeof(string) ? "" : 0);
        var before = _host.CountByLastName(request.LastName);
        var response = await _host.Client.PostAsync("api/clients", ClientSamples.JsonBody(request));
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        _host.CountByLastName(request.LastName).Should().Be(before);
    }

    [TestMethod]
    public async Task Required_only_payload_is_saved()
    {
        var request = ClientSamples.RequiredOnly();
        var response = await _host.Client.PostAsync("api/clients", ClientSamples.JsonBody(request));
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        _host.CountByPassport(request.PassportSeries, request.PassportNumber).Should().Be(1);
    }
}
