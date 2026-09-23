using Bank.Common.Accounting;
using Bank.Credits.Accounting;
using Bank.Credits.Cqrs;
using Bank.Credits.Data;
using Bank.Credits.Endpoints;
using Bank.Credits.Http;
using Bank.Credits.Mapping;
using Bank.Credits.Validation;
using Bank.Common.Localization;
using FluentValidation;
using MapsterMapper;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Bank.Credits;

public static class CreditsWebApplication
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

        var sqliteConnectionString = builder.Configuration.GetConnectionString("Credits")
            ?? "Data Source=Credits;Mode=Memory;Cache=Shared";
        var keepAliveConnection = new SqliteConnection(sqliteConnectionString);
        keepAliveConnection.Open();
        builder.Services.AddSingleton(keepAliveConnection);
        builder.Services.AddDbContext<CreditsDbContext>(options => options.UseSqlite(sqliteConnectionString));
        builder.Services.AddValidatorsFromAssemblyContaining<CreditContractRequestValidator>();
        builder.Services.AddSingleton(CreditMapping.CreateConfig());
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
        builder.Services.AddHttpClient<IDepositsApi, HttpDepositsApi>((sp, client) =>
        {
            var baseUrl = sp.GetRequiredService<IConfiguration>()["DepositsApi:BaseUrl"];
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
            var db = scope.ServiceProvider.GetRequiredService<CreditsDbContext>();
            db.Database.Migrate();
            CurrencyPositionSeeder.EnsureAsync(db).GetAwaiter().GetResult();
        }

        CreditDemoSeeder.SeedAsync(app.Services).GetAwaiter().GetResult();

        app.UseDefaultFiles();
        app.UseStaticFiles();
        app.MapCreditEndpoints();
        app.MapDefaultEndpoints();
        app.MapFallbackToFile("index.html");

        return app;
    }
}
