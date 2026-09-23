using System.Net;
using System.Net.Http.Json;
using Bank.Common.Dtos.Response;
using Tests.Atm.Support;

namespace Tests.Atm.Integration;

[TestClass]
public sealed class AtmApiTests
{
    private static AtmTestHost _host = null!;
    private static FakeCreditsApi _credits = null!;
    private static FakeDepositsApi _deposits = null!;

    [ClassInitialize]
    public static async Task ClassInitialize(TestContext context)
    {
        _credits = new FakeCreditsApi();
        _deposits = new FakeDepositsApi();
        _host = await AtmTestHost.StartAsync(creditsApi: _credits, depositsApi: _deposits);
    }

    [ClassCleanup]
    public static async Task ClassCleanup()
    {
        if (_host is not null)
        {
            await _host.DisposeAsync();
        }
    }

    [TestMethod]
    public async Task Insert_card_and_pin_opens_the_menu()
    {
        var session = await AuthorizeAsync();
        session.Screen.Should().Be("menu");
        session.Account?.ClientName.Should().Be("Client4 Test X");
    }

    [TestMethod]
    public async Task Three_wrong_pins_lock_the_session()
    {
        var started = await _host.Client.PostAsync("api/atm/sessions", AtmSamples.Insert(AtmSamples.Card));
        var session = await started.Content.ReadFromJsonAsync<AtmSessionResponse>(AtmSamples.JsonOptions);
        session.Should().NotBeNull();
        for (var i = 0; i < 3; i++)
        {
            var pin = await _host.Client.PostAsync($"api/atm/sessions/{session!.Id}/input", AtmSamples.Input("pin", AtmSamples.WrongPin));
            session = await pin.Content.ReadFromJsonAsync<AtmSessionResponse>(AtmSamples.JsonOptions);
            session.Should().NotBeNull();
        }
        session!.Screen.Should().Be("locked");
        session.MessageKey.Should().Be("cardLocked");
    }

    [TestMethod]
    public async Task Withdrawal_sends_the_full_transaction_list_to_the_bank()
    {
        _credits.Balance = 1500m;
        _credits.Executed.Clear();
        var session = await AuthorizeAsync();
        session = await InputAsync(session.Id, "menu", "withdraw");
        session = await InputAsync(session.Id, "amount", "200");
        session.Screen.Should().Be("receiptChoice");
        _credits.Executed.Should().BeEmpty();
        session = await InputAsync(session.Id, "receipt", "yes");
        session.Screen.Should().Be("result");
        _credits.Executed.Should().ContainSingle();
        _credits.Executed[0].Operation.Should().Be("withdraw");
        _credits.Executed[0].Amount.Should().Be(200m);
        _credits.Executed[0].CardNumber.Should().Be(AtmSamples.Card);
        session.Receipt.Should().NotBeNull();
        session.LastResult?.MessageKey.Should().Be("cashDispensed");
    }

    [TestMethod]
    public async Task Insufficient_funds_are_shown_without_printing()
    {
        _credits.Balance = 50m;
        _credits.Executed.Clear();
        var session = await AuthorizeAsync();
        session = await InputAsync(session.Id, "menu", "withdraw");
        session = await InputAsync(session.Id, "amount", "200");
        session.Screen.Should().Be("receiptChoice");
        _credits.Executed.Should().BeEmpty();
        session = await InputAsync(session.Id, "receipt", "no");
        session.Screen.Should().Be("result");
        session.MessageKey.Should().Be("insufficientFunds");
        session.LastResult?.Success.Should().BeFalse();
        session.Receipt.Should().BeNull();
    }

    [TestMethod]
    public async Task Deposit_balance_is_loaded_over_http()
    {
        var session = await AuthorizeAsync();
        session = await InputAsync(session.Id, "menu", "depositBalance");
        session.Screen.Should().Be("receiptChoice");
        session.LastResult.Should().BeNull();
        session = await InputAsync(session.Id, "receipt", "yes");
        session.Screen.Should().Be("result");
        session.LastResult?.MessageKey.Should().Be("depositBalanceShown");
        (session.LastResult?.Deposits ?? []).Should().ContainSingle();
        session.LastResult!.Deposits![0].Amount.Should().Be(1200m);
        session.Receipt.Should().NotBeNull();
    }

    [TestMethod]
    public async Task Payment_prints_a_receipt_automatically()
    {
        _credits.Balance = 1500m;
        var session = await AuthorizeAsync();
        session = await InputAsync(session.Id, "menu", "payment");
        session = await InputAsync(session.Id, "operator", "A1");
        session = await InputAsync(session.Id, "phone", "291112233");
        session.Screen.Should().Be("paymentPhone");
        session = await InputAsync(session.Id, "phone", "2911122330");
        session = await InputAsync(session.Id, "amount", "15");
        session = await InputAsync(session.Id, "confirm", "yes");
        session.Screen.Should().Be("receiptChoice");
        session = await InputAsync(session.Id, "receipt", "yes");
        session.Screen.Should().Be("result");
        session.LastResult?.MessageKey.Should().Be("paymentCompleted");
        session.Receipt.Should().NotBeNull();
        session.Receipt!.OperatorCode.Should().Be("A1");
    }

    [TestMethod]
    public async Task Invalid_card_number_returns_400()
    {
        var response = await _host.Client.PostAsync("api/atm/sessions", AtmSamples.Insert("123"));
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private static async Task<AtmSessionResponse> AuthorizeAsync()
    {
        var started = await _host.Client.PostAsync("api/atm/sessions", AtmSamples.Insert(AtmSamples.Card));
        var session = await started.Content.ReadFromJsonAsync<AtmSessionResponse>(AtmSamples.JsonOptions);
        session.Should().NotBeNull();
        return await InputAsync(session!.Id, "pin", AtmSamples.Pin);
    }

    private static async Task<AtmSessionResponse> InputAsync(Guid id, string kind, string value)
    {
        var response = await _host.Client.PostAsync($"api/atm/sessions/{id}/input", AtmSamples.Input(kind, value));
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var session = await response.Content.ReadFromJsonAsync<AtmSessionResponse>(AtmSamples.JsonOptions);
        session.Should().NotBeNull();
        return session!;
    }
}
