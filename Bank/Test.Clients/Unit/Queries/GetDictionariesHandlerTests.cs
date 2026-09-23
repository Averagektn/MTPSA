using Bank.Common.Localization;
using Bank.Clients.Queries.GetDictionaries;
using Tests.Clients.Support;

namespace Tests.Clients.Unit.Queries;

[TestClass]
public sealed class GetDictionariesHandlerTests
{
    [TestMethod]
    public async Task Returns_at_least_five_cities_and_all_lookups()
    {
        using var db = new SqliteTestDb();
        RequestLocale.SetFromAcceptLanguage("en");
        var result = await db.GetDictionariesHandler().Handle(new GetDictionariesQuery());

        result.IsSuccess.Should().BeTrue();
        result.Value.Cities.Should().HaveCountGreaterThanOrEqualTo(5);
        result.Value.MaritalStatuses.Should().NotBeEmpty();
        result.Value.Citizenships.Should().NotBeEmpty();
        result.Value.Disabilities.Should().NotBeEmpty();
        result.Value.Cities.Should().Contain(c => c.Name == "Minsk");
        result.Value.Cities.Should().NotContain(c => c.Name == "Минск");
    }

    [TestMethod]
    public async Task Does_not_return_english_names_when_locale_is_russian()
    {
        using var db = new SqliteTestDb();
        RequestLocale.SetFromAcceptLanguage("ru-RU");
        try
        {
            var result = await db.GetDictionariesHandler().Handle(new GetDictionariesQuery());

            result.IsSuccess.Should().BeTrue();
            result.Value.Cities.Should().Contain(c => c.Name == "Минск");
            result.Value.Cities.Should().NotContain(c => c.Name == "Minsk");
        }
        finally
        {
            RequestLocale.SetFromAcceptLanguage("en");
        }
    }
}
