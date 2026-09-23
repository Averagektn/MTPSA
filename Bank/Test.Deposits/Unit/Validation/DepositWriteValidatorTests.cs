using Bank.Common.Errors;
using Tests.Deposits.Support;

namespace Tests.Deposits.Unit.Validation;

[TestClass]
public sealed class DepositWriteValidatorTests
{
    [TestMethod]
    public async Task Amount_below_safe_minimum_is_rejected()
    {
        using var db = new SqliteTestDb();
        var request = DepositSamples.AlfaSafe(mutate: r => r.Amount = 199m);
        var result = await db.DepositValidator().ValidateAsync(request);
        result.IsFailed.Should().BeTrue();
        result.Errors.OfType<ValidationError>().Should().Contain(e => e.Field == "amount" && e.Message == "amountBelowMin");
    }

    [TestMethod]
    public async Task Amount_below_vklad_minimum_is_rejected()
    {
        using var db = new SqliteTestDb();
        var request = DepositSamples.AlfaVklad(mutate: r => r.Amount = 49m);
        var result = await db.DepositValidator().ValidateAsync(request);
        result.IsFailed.Should().BeTrue();
        result.Errors.OfType<ValidationError>().Should().Contain(e => e.Field == "amount" && e.Message == "amountBelowMin");
    }

    [TestMethod]
    public async Task Unknown_client_is_rejected()
    {
        using var db = new SqliteTestDb();
        var request = DepositSamples.AlfaSafe(clientId: 999);
        var result = await db.DepositValidator().ValidateAsync(request);
        result.IsFailed.Should().BeTrue();
        result.Errors.OfType<ValidationError>().Should().Contain(e => e.Field == "clientId" && e.Message == "clientNotFound");
    }
}
