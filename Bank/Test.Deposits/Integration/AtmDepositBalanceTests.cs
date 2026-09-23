using System.Net;
using System.Net.Http.Json;
using Bank.Common.Dtos.Response;
using Tests.Deposits.Support;

namespace Tests.Deposits.Integration;

[TestClass]
public sealed class AtmDepositBalanceTests
{
    private static DepositsTestHost _host = null!;

    [ClassInitialize]
    public static async Task ClassInitialize(TestContext context)
        => _host = await DepositsTestHost.StartAsync(clientsApi: FakeClientsApi.Seeded());

    [ClassCleanup]
    public static async Task ClassCleanup()
    {
        if (_host is not null)
        {
            await _host.DisposeAsync();
        }
    }

    [TestMethod]
    public async Task Client_with_a_seeded_deposit_gets_the_principal_amount()
    {
        var response = await _host.Client.GetAsync("api/deposits/atm/balances?clientId=4");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<AtmDepositBalanceResponse>(DepositSamples.JsonOptions);
        body.Should().NotBeNull();
        body!.Items.Should().ContainSingle();
        body.Items[0].Amount.Should().Be(1200m);
        body.Items[0].ContractNumber.Should().Be("Д-2026-0003");
    }

    [TestMethod]
    public async Task Client_without_deposits_gets_an_empty_list()
    {
        var response = await _host.Client.GetAsync("api/deposits/atm/balances?clientId=1");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<AtmDepositBalanceResponse>(DepositSamples.JsonOptions);
        body.Should().NotBeNull();
        body!.Items.Should().BeEmpty();
    }
}
