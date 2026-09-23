using Bank.Atm;
using Bank.Atm.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Tests.Atm.Support;

public sealed class AtmTestHost : IAsyncDisposable
{
    public WebApplication App { get; }
    public HttpClient Client { get; }
    public string BaseUrl { get; }

    private AtmTestHost(WebApplication app, HttpClient client, string baseUrl)
    {
        App = app;
        Client = client;
        BaseUrl = baseUrl;
    }

    public static async Task<AtmTestHost> StartAsync(
        bool withFrontend = false,
        ICreditsApi? creditsApi = null,
        IDepositsApi? depositsApi = null,
        string? creditsBaseUrl = null,
        string? depositsBaseUrl = null)
    {
        var database = Guid.NewGuid().ToString("N");
        var serverDir = Path.GetDirectoryName(typeof(AtmWebApplication).Assembly.Location)!;
        var options = new WebApplicationOptions
        {
            EnvironmentName = Environments.Development,
            ApplicationName = typeof(AtmWebApplication).Assembly.GetName().Name,
            ContentRootPath = serverDir,
            WebRootPath = withFrontend ? FrontendDist.EnsureBuilt() : serverDir
        };

        var app = AtmWebApplication.Create(
            options: options,
            configure: builder =>
            {
                builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:Atm"] = $"Data Source={database};Mode=Memory;Cache=Shared",
                    ["CreditsApi:BaseUrl"] = creditsBaseUrl,
                    ["DepositsApi:BaseUrl"] = depositsBaseUrl
                });
            },
            configureServices: builder =>
            {
                if (creditsApi is not null)
                {
                    builder.Services.AddSingleton<ICreditsApi>(creditsApi);
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
        return new AtmTestHost(app, client, url);
    }

    public async ValueTask DisposeAsync()
    {
        Client.Dispose();
        await App.StopAsync();
        await App.DisposeAsync();
    }
}
