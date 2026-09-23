using System.Net;
using Bank.Clients;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Tests.Credits.Support;

namespace Tests.Credits.Integration;

[TestClass]
public sealed class ClientsDeleteBlockedTests
{
    [TestMethod]
    public async Task Clients_api_rejects_delete_when_credits_has_a_contract()
    {
        await using var credits = await CreditsTestHost.StartAsync(clientsApi: FakeClientsApi.Seeded());

        var clientsDir = Path.GetDirectoryName(typeof(BankWebApplication).Assembly.Location)!;
        var clientsApp = BankWebApplication.Create(
            options: new Microsoft.AspNetCore.Builder.WebApplicationOptions
            {
                EnvironmentName = Environments.Development,
                ApplicationName = typeof(BankWebApplication).Assembly.GetName().Name,
                ContentRootPath = clientsDir,
                WebRootPath = clientsDir
            },
            configure: builder =>
            {
                builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:Clients"] = $"Data Source={Guid.NewGuid():N};Mode=Memory;Cache=Shared",
                    ["CreditsApi:BaseUrl"] = credits.BaseUrl
                });
            });
        clientsApp.Urls.Clear();
        clientsApp.Urls.Add("http://127.0.0.1:0");
        await clientsApp.StartAsync();

        using var clientsClient = new HttpClient { BaseAddress = new Uri(clientsApp.Urls.First().TrimEnd('/') + "/") };
        var response = await clientsClient.DeleteAsync("api/clients/4");
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        await clientsApp.StopAsync();
        await clientsApp.DisposeAsync();
    }
}
