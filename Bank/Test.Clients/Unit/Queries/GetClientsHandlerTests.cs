using Bank.Clients.Queries.GetClients;
using Tests.Clients.Support;

namespace Tests.Clients.Unit.Queries;

[TestClass]
public sealed class GetClientsHandlerTests
{
    [TestMethod]
    public async Task Returns_seeded_clients_sorted_by_last_name()
    {
        using var db = new SqliteTestDb();
        var result = await db.GetClientsHandler().Handle(new GetClientsQuery());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCountGreaterThanOrEqualTo(5);
        var lastNames = result.Value.Select(c => c.LastName).ToList();
        lastNames.Should().BeInAscendingOrder(StringComparer.Ordinal);
        lastNames[0].Should().Be("Glushachenko");
    }

    [TestMethod]
    public async Task Returns_empty_list_when_no_clients_exist()
    {
        using var db = new SqliteTestDb();
        db.Db.Clients.RemoveRange(db.Db.Clients);
        await db.Db.SaveChangesAsync();

        var result = await db.GetClientsHandler().Handle(new GetClientsQuery());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }
}
