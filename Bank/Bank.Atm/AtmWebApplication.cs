using Bank.Atm.Cqrs;
using Bank.Atm.Data;
using Bank.Atm.Endpoints;
using Bank.Atm.Http;
using Bank.Common.Localization;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Bank.Atm;

public static class AtmWebApplication
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

        var sqliteConnectionString = builder.Configuration.GetConnectionString("Atm")
            ?? "Data Source=Atm;Mode=Memory;Cache=Shared";
        var keepAliveConnection = new SqliteConnection(sqliteConnectionString);
        keepAliveConnection.Open();
        builder.Services.AddSingleton(keepAliveConnection);
        builder.Services.AddDbContext<AtmDbContext>(options => options.UseSqlite(sqliteConnectionString));
        builder.Services.AddCqrs();
        builder.Services.AddHttpClient<ICreditsApi, HttpCreditsApi>((sp, client) =>
        {
            var baseUrl = sp.GetRequiredService<IConfiguration>()["CreditsApi:BaseUrl"];
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
            var db = scope.ServiceProvider.GetRequiredService<AtmDbContext>();
            db.Database.Migrate();
        }

        app.UseDefaultFiles();
        app.UseStaticFiles();
        app.MapAtmEndpoints();
        app.MapDefaultEndpoints();
        app.MapFallbackToFile("index.html");

        return app;
    }
}
