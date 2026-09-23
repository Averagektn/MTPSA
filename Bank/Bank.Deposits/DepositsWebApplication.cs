using Bank.Common.Accounting;
using Bank.Deposits.Accounting;
using Bank.Deposits.Cqrs;
using Bank.Deposits.Data;
using Bank.Deposits.Endpoints;
using Bank.Deposits.Http;
using Bank.Deposits.Mapping;
using Bank.Deposits.Validation;
using Bank.Common.Localization;
using FluentValidation;
using MapsterMapper;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Bank.Deposits;

public static class DepositsWebApplication
{
    public static WebApplication Create(
        string[]? args = null,
        Action<WebApplicationBuilder>? configure = null,
        Action<WebApplicationBuilder>? configureServices = null,
        WebApplicationOptions? options = null)
    {
        var builder = options is null
            ? WebApplication.CreateBuilder(args ?? [])
            : WebApplication.CreateBuilder(options);
        configure?.Invoke(builder);

        builder.AddServiceDefaults();
        builder.Services.AddProblemDetails();
        builder.Services.AddOpenApi();

        var sqliteConnectionString = builder.Configuration.GetConnectionString("Deposits")
            ?? "Data Source=Deposits;Mode=Memory;Cache=Shared";
        var keepAliveConnection = new SqliteConnection(sqliteConnectionString);
        keepAliveConnection.Open();
        builder.Services.AddSingleton(keepAliveConnection);
        builder.Services.AddDbContext<DepositsDbContext>(options => options.UseSqlite(sqliteConnectionString));
        builder.Services.AddValidatorsFromAssemblyContaining<DepositContractRequestValidator>();
        builder.Services.AddSingleton(DepositMapping.CreateConfig());
        builder.Services.AddScoped<IMapper, ServiceMapper>();
        builder.Services.AddCqrs();
        builder.Services.AddExchangeRates(builder.Configuration);
        builder.Services.AddHttpClient<IClientsApi, HttpClientsApi>((sp, client) =>
        {
            var baseUrl = sp.GetRequiredService<IConfiguration>()["ClientsApi:BaseUrl"];
            if (!string.IsNullOrWhiteSpace(baseUrl))
            {
                client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
            }
        });
        configureServices?.Invoke(builder);

        var app = builder.Build();

        app.Lifetime.ApplicationStopping.Register(keepAliveConnection.Dispose);

        app.Use(async (context, next) =>
        {
            RequestLocale.SetFromAcceptLanguage(context.Request.Headers.AcceptLanguage);
            await next();
        });

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<DepositsDbContext>();
            db.Database.Migrate();
            CurrencyPositionSeeder.EnsureAsync(db).GetAwaiter().GetResult();
        }

        DepositDemoSeeder.SeedAsync(app.Services).GetAwaiter().GetResult();

        app.UseDefaultFiles();
        app.UseStaticFiles();
        app.MapDepositEndpoints();
        app.MapDefaultEndpoints();
        app.MapFallbackToFile("index.html");

        return app;
    }
}
