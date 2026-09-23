using Bank.Common.Dtos.Response;
using Bank.Common.Localization;
using Bank.Credits.Models;
using Mapster;

namespace Bank.Credits.Mapping;

public sealed class CreditMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Currency, LookupItem>()
            .Map(item => item.Name, currency => $"{currency.Code} — {RequestLocale.Pick(currency.NameEn, currency.NameRu)}");
    }

    public static TypeAdapterConfig CreateConfig()
    {
        var config = new TypeAdapterConfig();
        config.Scan(typeof(CreditMapping).Assembly);
        return config;
    }
}
