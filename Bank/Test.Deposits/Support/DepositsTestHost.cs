using Bank.Deposits;
using Bank.Deposits.Data;
using Bank.Deposits.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Tests.Deposits.Support;

public sealed class DepositsTestHost : IAsyncDisposable
{
    public WebApplication App { get; }
    public HttpClient Client { get; }
    public string BaseUrl { get; }

    private DepositsTestHost(WebApplication app, HttpClient client, string baseUrl)
    {
        App = app;
        Client = client;
        BaseUrl = baseUrl;
    }

    public static async Task<DepositsTestHost> StartAsync(
        bool withFrontend = false,
        IClientsApi? clientsApi = null,
        string? clientsBaseUrl = null)
    {
        var database = Guid.NewGuid().ToString("N");
        var serverDir = Path.GetDirectoryName(typeof(DepositsWebApplication).Assembly.Location)!;
        var options = new WebApplicationOptions
        {
            EnvironmentName = Environments.Development,
            ApplicationName = typeof(DepositsWebApplication).Assembly.GetName().Name,
            ContentRootPath = serverDir,
            WebRootPath = withFrontend ? FrontendDist.EnsureBuilt() : serverDir
        };

        var app = DepositsWebApplication.Create(
            options: options,
            configure: builder =>
            {
                builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:Deposits"] = $"Data Source={database};Mode=Memory;Cache=Shared",
                    ["ClientsApi:BaseUrl"] = clientsBaseUrl
                });
            },
            configureServices: builder =>
            {
                if (clientsApi is not null)
                {
                    builder.Services.AddSingleton<IClientsApi>(clientsApi);
                }
            });

        app.Urls.Clear();
        app.Urls.Add("http://127.0.0.1:0");
        await app.StartAsync();
        var url = app.Urls.First().TrimEnd('/');
        var client = new HttpClient { BaseAddress = new Uri(url + "/") };
        return new DepositsTestHost(app, client, url);
    }

    public int CountContracts()
    {
        using var scope = App.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DepositsDbContext>();
        return db.DepositContracts.Count();
    }

    public DateOnly BankDate()
    {
        using var scope = App.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DepositsDbContext>();
        return db.BankStates.Select(s => s.CurrentDate).Single();
    }

    public async ValueTask DisposeAsync()
    {
        Client.Dispose();
        await App.StopAsync();
        await App.DisposeAsync();
    }
}
