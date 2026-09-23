using Bank.Clients.Cqrs;
using Bank.Clients.Data;
using Bank.Clients.Endpoints;
using Bank.Clients.Http;
using Bank.Clients.Mapping;
using Bank.Clients.Validation;
using Bank.Common.Localization;
using FluentValidation;
using MapsterMapper;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Bank.Clients;

public static class BankWebApplication
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

        var sqliteConnectionString = builder.Configuration.GetConnectionString("Clients")
            ?? "Data Source=Clients;Mode=Memory;Cache=Shared";
        var keepAliveConnection = new SqliteConnection(sqliteConnectionString);
        keepAliveConnection.Open();
        builder.Services.AddSingleton(keepAliveConnection);
        builder.Services.AddDbContext<BankDbContext>(options => options.UseSqlite(sqliteConnectionString));
        builder.Services.AddValidatorsFromAssemblyContaining<ClientRequestValidator>();
        builder.Services.AddSingleton(ClientMapping.CreateConfig());
        builder.Services.AddScoped<IMapper, ServiceMapper>();
        builder.Services.AddCqrs();
        builder.Services.AddHttpClient<IDepositsApi, HttpDepositsApi>((sp, client) =>
        {
            var baseUrl = sp.GetRequiredService<IConfiguration>()["DepositsApi:BaseUrl"];
            if (!string.IsNullOrWhiteSpace(baseUrl))
            {
                client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
            }
        });
        builder.Services.AddHttpClient<ICreditsApi, HttpCreditsApi>((sp, client) =>
        {
            var baseUrl = sp.GetRequiredService<IConfiguration>()["CreditsApi:BaseUrl"];
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
            var db = scope.ServiceProvider.GetRequiredService<BankDbContext>();
            db.Database.Migrate();
        }

        app.UseDefaultFiles();
        app.UseStaticFiles();
        app.MapClientEndpoints();
        app.MapDefaultEndpoints();
        app.MapFallbackToFile("index.html");

        return app;
    }
}
