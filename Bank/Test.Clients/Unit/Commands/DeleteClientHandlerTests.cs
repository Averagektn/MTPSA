using Bank.Clients.Commands.DeleteClient;
using Bank.Common.Errors;
using FluentResults;
using Tests.Clients.Support;

namespace Tests.Clients.Unit.Commands;

[TestClass]
public sealed class DeleteClientHandlerTests
{
    [TestMethod]
    public async Task Deletes_an_existing_client()
    {
        using var db = new SqliteTestDb();
        var result = await db.DeleteClientHandler().Handle(new DeleteClientCommand(1));

        result.IsSuccess.Should().BeTrue();
        db.Db.Clients.Should().NotContain(c => c.Id == 1);
    }

    [TestMethod]
    public async Task Returns_not_found_for_unknown_id()
    {
        using var db = new SqliteTestDb();
        var result = await db.DeleteClientHandler().Handle(new DeleteClientCommand(999));

        result.IsFailed.Should().BeTrue();
        result.HasError<NotFoundError>().Should().BeTrue();
        db.Db.Clients.Should().HaveCount(6);
    }

    [TestMethod]
    public async Task Client_with_a_contract_cannot_be_deleted()
    {
        using var db = new SqliteTestDb();
        db.DepositsApi.ClientIdsWithDeposits.Add(1);

        var result = await db.DeleteClientHandler().Handle(new DeleteClientCommand(1));

        result.IsFailed.Should().BeTrue();
        result.Errors.OfType<ValidationError>().Should().Contain(e => e.Message == "clientHasDeposits");
        db.Db.Clients.Should().Contain(c => c.Id == 1);
    }

    [TestMethod]
    public async Task Does_not_delete_when_deposits_api_is_unavailable()
    {
        using var db = new SqliteTestDb();
        db.DepositsApi.Unavailable = true;

        var result = await db.DeleteClientHandler().Handle(new DeleteClientCommand(1));

        result.IsFailed.Should().BeTrue();
        result.Errors.OfType<ValidationError>().Should().Contain(e => e.Message == "depositsUnavailable");
        db.Db.Clients.Should().Contain(c => c.Id == 1);
    }

    [TestMethod]
    public async Task Client_with_a_credit_contract_cannot_be_deleted()
    {
        using var db = new SqliteTestDb();
        db.CreditsApi.ClientIdsWithCredits.Add(1);

        var result = await db.DeleteClientHandler().Handle(new DeleteClientCommand(1));

        result.IsFailed.Should().BeTrue();
        result.Errors.OfType<ValidationError>().Should().Contain(e => e.Message == "clientHasCredits");
        db.Db.Clients.Should().Contain(c => c.Id == 1);
    }

    [TestMethod]
    public async Task Does_not_delete_when_credits_api_is_unavailable()
    {
        using var db = new SqliteTestDb();
        db.CreditsApi.Unavailable = true;

        var result = await db.DeleteClientHandler().Handle(new DeleteClientCommand(1));

        result.IsFailed.Should().BeTrue();
        result.Errors.OfType<ValidationError>().Should().Contain(e => e.Message == "creditsUnavailable");
        db.Db.Clients.Should().Contain(c => c.Id == 1);
    }
}
