using System.Net;
using Tests.Clients.Support;

namespace Tests.Clients.Integration.Uniqueness;

[TestClass]
public sealed class UniquenessApiTests
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
    public async Task Duplicate_student_is_rejected_and_not_written_twice()
    {
        var before = _host.CountByLastName("Glushachenko");
        var response = await _host.Client.PostAsync("api/clients", ClientSamples.JsonBody(ClientSamples.SeededStudent()));
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await HttpAssertions.ReadProblem(response);
        problem.Errors.Should().ContainKey("lastName");
        _host.CountByLastName("Glushachenko").Should().Be(before);
    }

    [TestMethod]
    public async Task Duplicate_passport_is_rejected()
    {
        var request = ClientSamples.UniqueValid(r =>
        {
            r.PassportSeries = "AB";
            r.PassportNumber = "7654321";
        });
        var before = _host.CountByPassport("AB", "7654321");
        var response = await _host.Client.PostAsync("api/clients", ClientSamples.JsonBody(request));
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await HttpAssertions.ReadProblem(response);
        problem.Errors.Should().ContainKey("passportNumber");
        _host.CountByPassport("AB", "7654321").Should().Be(before);
    }

    [TestMethod]
    public async Task Duplicate_identification_number_is_rejected()
    {
        var request = ClientSamples.UniqueValid(r => r.IdentificationNumber = "1505033A015PB7");
        var before = _host.CountByIdentification("1505033A015PB7");
        var response = await _host.Client.PostAsync("api/clients", ClientSamples.JsonBody(request));
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await HttpAssertions.ReadProblem(response);
        problem.Errors.Should().ContainKey("identificationNumber");
        _host.CountByIdentification("1505033A015PB7").Should().Be(before);
    }
}
