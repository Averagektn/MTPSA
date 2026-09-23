using Bank.Clients.Validation;
using Tests.Clients.Support;

namespace Tests.Clients.Unit.Validation;

[TestClass]
public sealed class ClientWriteValidatorTests
{
    [TestMethod]
    public async Task Duplicate_student_fio_and_birth_date_fails()
    {
        using var db = new SqliteTestDb();
        var validator = new ClientWriteValidator(db.Db, new ClientRequestValidator());
        var result = await validator.ValidateAsync(ClientSamples.SeededStudent(), excludeId: null);
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message == "duplicatePerson");
    }

    [TestMethod]
    public async Task Duplicate_passport_fails()
    {
        using var db = new SqliteTestDb();
        var validator = new ClientWriteValidator(db.Db, new ClientRequestValidator());
        var request = ClientSamples.UniqueValid(r =>
        {
            r.PassportSeries = "AB";
            r.PassportNumber = "7654321";
        });
        var result = await validator.ValidateAsync(request, excludeId: null);
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message == "duplicatePassport");
    }

    [TestMethod]
    public async Task Duplicate_identification_number_fails()
    {
        using var db = new SqliteTestDb();
        var validator = new ClientWriteValidator(db.Db, new ClientRequestValidator());
        var request = ClientSamples.UniqueValid(r => r.IdentificationNumber = "1505033A015PB7");
        var result = await validator.ValidateAsync(request, excludeId: null);
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message == "duplicateIdentificationNumber");
    }

    [TestMethod]
    public async Task Unique_required_only_client_passes()
    {
        using var db = new SqliteTestDb();
        var validator = new ClientWriteValidator(db.Db, new ClientRequestValidator());
        var result = await validator.ValidateAsync(ClientSamples.RequiredOnly(), excludeId: null);
        result.IsSuccess.Should().BeTrue(string.Join("; ", result.Errors.Select(e => e.Message)));
    }
}
