using Bank.Credits;
using Bank.Credits.Data;
using Bank.Credits.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Tests.Credits.Support;

public sealed class CreditsTestHost : IAsyncDisposable
{
    public WebApplication App { get; }
    public HttpClient Client { get; }
    public string BaseUrl { get; }

    private CreditsTestHost(WebApplication app, HttpClient client, string baseUrl)
    {
        App = app;
        Client = client;
        BaseUrl = baseUrl;
    }

    public static async Task<CreditsTestHost> StartAsync(
        bool withFrontend = false,
        IClientsApi? clientsApi = null,
        IDepositsApi? depositsApi = null,
        string? clientsBaseUrl = null)
    {
        var database = Guid.NewGuid().ToString("N");
        var serverDir = Path.GetDirectoryName(typeof(CreditsWebApplication).Assembly.Location)!;
        var options = new WebApplicationOptions
        {
            EnvironmentName = Environments.Development,
            ApplicationName = typeof(CreditsWebApplication).Assembly.GetName().Name,
            ContentRootPath = serverDir,
            WebRootPath = withFrontend ? FrontendDist.EnsureBuilt() : serverDir
        };

        var app = CreditsWebApplication.Create(
            options: options,
            configure: builder =>
            {
                builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:Credits"] = $"Data Source={database};Mode=Memory;Cache=Shared",
                    ["ClientsApi:BaseUrl"] = clientsBaseUrl
                });
            },
            configureServices: builder =>
            {
                if (clientsApi is not null)
                {
                    builder.Services.AddSingleton<IClientsApi>(clientsApi);
                }

                if (depositsApi is not null)
                {
                    builder.Services.AddSingleton<IDepositsApi>(depositsApi);
                }
            });

        app.Urls.Clear();
        app.Urls.Add("http://127.0.0.1:0");
        await app.StartAsync();
        var url = app.Urls.First().TrimEnd('/');
        var client = new HttpClient { BaseAddress = new Uri(url + "/") };
        return new CreditsTestHost(app, client, url);
    }

    public int CountContracts()
    {
        using var scope = App.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CreditsDbContext>();
        return db.CreditContracts.Count();
    }

    public DateOnly BankDate()
    {
        using var scope = App.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CreditsDbContext>();
        return db.BankStates.Select(s => s.CurrentDate).Single();
    }

    public async ValueTask DisposeAsync()
    {
        Client.Dispose();
        await App.StopAsync();
        await App.DisposeAsync();
    }
}
