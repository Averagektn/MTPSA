using Bank.Clients.Validation;
using Mediator;

namespace Bank.Clients.Cqrs;

public static class CqrsServiceCollectionExtensions
{
    public static IServiceCollection AddCqrs(this IServiceCollection services)
    {
        services.AddScoped<ClientWriteValidator>();
        services.AddMediator(options =>
        {
            options.ServiceLifetime = ServiceLifetime.Scoped;
            options.Assemblies = [typeof(CqrsServiceCollectionExtensions)];
        });
        return services;
    }
}
