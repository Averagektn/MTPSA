using Bank.Deposits.Accounting;
using Bank.Deposits.Validation;
using Mediator;

namespace Bank.Deposits.Cqrs;

public static class CqrsServiceCollectionExtensions
{
    public static IServiceCollection AddCqrs(this IServiceCollection services)
    {
        services.AddScoped<DepositWriteValidator>();
        services.AddScoped<DepositLedger>();
        services.AddMediator(options =>
        {
            options.ServiceLifetime = ServiceLifetime.Scoped;
            options.Assemblies = [typeof(CqrsServiceCollectionExtensions)];
        });
        return services;
    }
}
