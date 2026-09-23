using Bank.Credits.Accounting;
using Bank.Credits.Validation;
using Mediator;

namespace Bank.Credits.Cqrs;

public static class CqrsServiceCollectionExtensions
{
    public static IServiceCollection AddCqrs(this IServiceCollection services)
    {
        services.AddScoped<CreditWriteValidator>();
        services.AddScoped<CreditLedger>();
        services.AddScoped<CreditCardIssuer>();
        services.AddMediator(options =>
        {
            options.ServiceLifetime = ServiceLifetime.Scoped;
            options.Assemblies = [typeof(CqrsServiceCollectionExtensions)];
        });
        return services;
    }
}
