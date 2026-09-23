using System.Net;
using System.Net.Http.Json;
using Bank.Credits.Accounting;
using Bank.Common.Dtos.Request;
using Bank.Common.Dtos.Response;
using Tests.Credits.Support;

namespace Tests.Credits.Integration;

[TestClass]
public sealed class AtmBankApiTests
{
    private static CreditsTestHost _host = null!;

    [ClassInitialize]
    public static async Task ClassInitialize(TestContext context)
        => _host = await CreditsTestHost.StartAsync(
            clientsApi: FakeClientsApi.Seeded(),
            depositsApi: FakeDepositsApi.WithClientDepositAccounts());

    [ClassCleanup]
    public static async Task ClassCleanup()
    {
        if (_host is not null)
        {
            await _host.DisposeAsync();
        }
    }

    [TestMethod]
    public async Task Authorize_accepts_the_demo_card()
    {
        var response = await _host.Client.PostAsJsonAsync("api/credits/atm/authorize", new AtmAuthorizeRequest
        {
            CardNumber = DemoCards.FirstNumber,
            Pin = DemoCards.FirstPin
        });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<AtmAuthorizeResponse>(CreditSamples.JsonOptions);
        body.Should().NotBeNull();
        body!.ClientId.Should().Be(4);
        body.AvailableBalance.Should().Be(1500m);
    }

    [TestMethod]
    public async Task Withdraw_decreases_the_card_balance()
    {
        var before = await _host.Client.PostAsJsonAsync("api/credits/atm/authorize", new AtmAuthorizeRequest
        {
            CardNumber = DemoCards.FirstNumber,
            Pin = DemoCards.FirstPin
        });
        var account = await before.Content.ReadFromJsonAsync<AtmAuthorizeResponse>(CreditSamples.JsonOptions);
        account.Should().NotBeNull();

        var withdraw = await _host.Client.PostAsJsonAsync("api/credits/atm/transactions", new AtmTransactionRequest
        {
            CardNumber = DemoCards.FirstNumber,
            Pin = DemoCards.FirstPin,
            Operation = "withdraw",
            Amount = 100m
        });
        withdraw.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await withdraw.Content.ReadFromJsonAsync<AtmTransactionResponse>(CreditSamples.JsonOptions);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.AvailableBalance.Should().Be(account!.AvailableBalance - 100m);
    }

    [TestMethod]
    public async Task Withdraw_over_balance_returns_400()
    {
        var response = await _host.Client.PostAsJsonAsync("api/credits/atm/transactions", new AtmTransactionRequest
        {
            CardNumber = DemoCards.FirstNumber,
            Pin = DemoCards.FirstPin,
            Operation = "withdraw",
            Amount = 1_000_000m
        });
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
