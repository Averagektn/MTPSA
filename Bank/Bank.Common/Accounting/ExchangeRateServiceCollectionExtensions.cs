using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bank.Common.Accounting;

public static class ExchangeRateServiceCollectionExtensions
{
    public static IServiceCollection AddExchangeRates(this IServiceCollection services, IConfiguration configuration)
    {
        var provider = configuration["ExchangeRates:Provider"];
        if (string.Equals(provider, "Nbrb", StringComparison.OrdinalIgnoreCase))
        {
            services.AddHttpClient<IExchangeRateProvider, NbrbExchangeRateProvider>(client =>
            {
                client.BaseAddress = new Uri("https://api.nbrb.by/");
            });
            return services;
        }

        services.AddSingleton<IExchangeRateProvider, HardcodedExchangeRateProvider>();
        return services;
    }
}
