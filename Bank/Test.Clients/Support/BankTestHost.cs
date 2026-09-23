using Bank.Clients;
using Bank.Clients.Data;
using Bank.Clients.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Tests.Clients.Support;

public sealed class BankTestHost : IAsyncDisposable
{
    public WebApplication App { get; }
    public HttpClient Client { get; }
    public string BaseUrl { get; }

    private BankTestHost(WebApplication app, HttpClient client, string baseUrl)
    {
        App = app;
        Client = client;
        BaseUrl = baseUrl;
    }

    public static async Task<BankTestHost> StartAsync(
        bool withFrontend = false,
        string frontendProject = "frontend",
        IDepositsApi? depositsApi = null,
        ICreditsApi? creditsApi = null)
    {
        var database = Guid.NewGuid().ToString("N");
        var serverDir = Path.GetDirectoryName(typeof(BankWebApplication).Assembly.Location)!;
        var options = new WebApplicationOptions
        {
            EnvironmentName = Environments.Development,
            ApplicationName = typeof(BankWebApplication).Assembly.GetName().Name,
            ContentRootPath = serverDir,
            WebRootPath = withFrontend ? FrontendDist.EnsureBuilt(frontendProject) : serverDir
        };

        var app = BankWebApplication.Create(
            options: options,
            configure: builder =>
            {
                builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:Clients"] = $"Data Source={database};Mode=Memory;Cache=Shared"
                });
            },
            configureServices: builder =>
            {
                if (depositsApi is not null)
                {
                    builder.Services.AddSingleton<IDepositsApi>(depositsApi);
                }

                if (creditsApi is not null)
                {
                    builder.Services.AddSingleton<ICreditsApi>(creditsApi);
                }
            });

        app.Urls.Clear();
        app.Urls.Add("http://127.0.0.1:0");
        await app.StartAsync();
        var url = app.Urls.First().TrimEnd('/');
        var client = new HttpClient { BaseAddress = new Uri(url + "/") };
        return new BankTestHost(app, client, url);
    }

    public int CountByLastName(string lastName)
    {
        using var scope = App.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BankDbContext>();
        return db.Clients.Count(c => c.LastName == lastName);
    }

    public int CountByPassport(string series, string number)
    {
        using var scope = App.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BankDbContext>();
        return db.Clients.Count(c => c.PassportSeries == series && c.PassportNumber == number);
    }

    public int CountByIdentification(string identificationNumber)
    {
        using var scope = App.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BankDbContext>();
        return db.Clients.Count(c => c.IdentificationNumber == identificationNumber);
    }

    public async ValueTask DisposeAsync()
    {
        Client.Dispose();
        await App.StopAsync();
        await App.DisposeAsync();
    }
}
