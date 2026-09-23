using Bank.Clients.Commands.CreateClient;
using FluentResults;
using Tests.Clients.Support;

namespace Tests.Clients.Unit.Commands;

[TestClass]
public sealed class CreateClientHandlerTests
{
    [TestMethod]
    public async Task Creates_a_client_with_required_fields_only()
    {
        using var db = new SqliteTestDb();
        var request = ClientSamples.RequiredOnly();
        var result = await db.CreateClientHandler().Handle(new CreateClientCommand(request));

        result.IsSuccess.Should().BeTrue(string.Join("; ", result.Errors.Select(e => e.Message)));
        result.Value.LastName.Should().Be(request.LastName);
        result.Value.PassportNumber.Should().Be(request.PassportNumber);
        db.Db.Clients.Count(c => c.PassportNumber == request.PassportNumber).Should().Be(1);
    }

    [TestMethod]
    public async Task Rejects_a_duplicate_student()
    {
        using var db = new SqliteTestDb();
        var result = await db.CreateClientHandler().Handle(new CreateClientCommand(ClientSamples.SeededStudent()));

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message == "duplicatePerson");
        db.Db.Clients.Count(c => c.LastName == "Glushachenko").Should().Be(1);
    }
}
