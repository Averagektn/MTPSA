using Bank.Common.Errors;
using Tests.Credits.Support;

namespace Tests.Credits.Unit.Validation;

[TestClass]
public sealed class CreditWriteValidatorTests
{
    [TestMethod]
    public async Task Amount_below_cash_loan_minimum_is_rejected()
    {
        using var db = new SqliteTestDb();
        var request = CreditSamples.CashLoan(mutate: r => r.Amount = 999m);
        var result = await db.CreditValidator().ValidateAsync(request);
        result.IsFailed.Should().BeTrue();
        result.Errors.OfType<ValidationError>().Should().Contain(e => e.Field == "amount" && e.Message == "amountBelowMin");
    }

    [TestMethod]
    public async Task Amount_below_online_loan_minimum_is_rejected()
    {
        using var db = new SqliteTestDb();
        var request = CreditSamples.OnlineLoan(mutate: r => r.Amount = 499m);
        var result = await db.CreditValidator().ValidateAsync(request);
        result.IsFailed.Should().BeTrue();
        result.Errors.OfType<ValidationError>().Should().Contain(e => e.Field == "amount" && e.Message == "amountBelowMin");
    }

    [TestMethod]
    public async Task Unknown_client_is_rejected()
    {
        using var db = new SqliteTestDb();
        var request = CreditSamples.CashLoan(clientId: 999);
        var result = await db.CreditValidator().ValidateAsync(request);
        result.IsFailed.Should().BeTrue();
        result.Errors.OfType<ValidationError>().Should().Contain(e => e.Field == "clientId" && e.Message == "clientNotFound");
    }
}
