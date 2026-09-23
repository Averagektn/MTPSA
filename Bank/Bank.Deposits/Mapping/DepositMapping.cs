using Bank.Common.Dtos.Response;
using Bank.Common.Localization;
using Bank.Deposits.Models;
using Mapster;

namespace Bank.Deposits.Mapping;

public sealed class DepositMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Currency, LookupItem>()
            .Map(item => item.Name, currency => $"{currency.Code} — {RequestLocale.Pick(currency.NameEn, currency.NameRu)}");
    }

    public static TypeAdapterConfig CreateConfig()
    {
        var config = new TypeAdapterConfig();
        config.Scan(typeof(DepositMapping).Assembly);
        return config;
    }
}
