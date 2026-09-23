using Bank.Common.Errors;
using Bank.Clients.Queries.GetClient;
using FluentResults;
using Tests.Clients.Support;

namespace Tests.Clients.Unit.Queries;

[TestClass]
public sealed class GetClientHandlerTests
{
    [TestMethod]
    public async Task Returns_the_seeded_student()
    {
        using var db = new SqliteTestDb();
        var result = await db.GetClientHandler().Handle(new GetClientQuery(1));

        result.IsSuccess.Should().BeTrue();
        result.Value.LastName.Should().Be("Glushachenko");
        result.Value.FirstName.Should().Be("Nikita");
    }

    [TestMethod]
    public async Task Returns_not_found_for_unknown_id()
    {
        using var db = new SqliteTestDb();
        var result = await db.GetClientHandler().Handle(new GetClientQuery(999));

        result.IsFailed.Should().BeTrue();
        result.HasError<NotFoundError>().Should().BeTrue();
    }
}
