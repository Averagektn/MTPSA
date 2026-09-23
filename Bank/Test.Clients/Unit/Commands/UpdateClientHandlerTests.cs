using Bank.Clients.Commands.UpdateClient;
using Bank.Common.Errors;
using FluentResults;
using Tests.Clients.Support;

namespace Tests.Clients.Unit.Commands;

[TestClass]
public sealed class UpdateClientHandlerTests
{
    [TestMethod]
    public async Task Updates_an_existing_client()
    {
        using var db = new SqliteTestDb();
        var request = ClientSamples.SeededStudent();
        request.ResidenceAddress = "Updated street 15";
        var result = await db.UpdateClientHandler().Handle(new UpdateClientCommand(1, request));

        result.IsSuccess.Should().BeTrue(string.Join("; ", result.Errors.Select(e => e.Message)));
        result.Value.ResidenceAddress.Should().Be("Updated street 15");
        db.Db.Clients.Single(c => c.Id == 1).ResidenceAddress.Should().Be("Updated street 15");
    }

    [TestMethod]
    public async Task Returns_not_found_for_unknown_id()
    {
        using var db = new SqliteTestDb();
        var result = await db.UpdateClientHandler()
            .Handle(new UpdateClientCommand(999, ClientSamples.RequiredOnly()));

        result.IsFailed.Should().BeTrue();
        result.HasError<NotFoundError>().Should().BeTrue();
    }
}
