using System.Net;
using System.Net.Http.Json;
using Bank.Deposits.Accounting;
using Bank.Common.Dtos.Response;
using Tests.Deposits.Support;

namespace Tests.Deposits.Integration;

[TestClass]
public sealed class DepositApiTests
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
    public async Task Seeded_accounts_report_has_at_least_six_accounts_and_sfrb_capital()
    {
        var response = await _host.Client.GetAsync("api/accounts");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var accounts = await response.Content.ReadFromJsonAsync<List<AccountReportItem>>(DepositSamples.JsonOptions);
        accounts.Should().NotBeNull();
        accounts.Should().HaveCountGreaterThanOrEqualTo(6);

        var sfrb = accounts!.Single(a => a.Number == BankAccountNumbers.DevelopmentFund);
        sfrb.Credit.Should().BeGreaterThanOrEqualTo(1_000_000m);
        sfrb.Saldo.Should().BeGreaterThanOrEqualTo(1_000_000m);
    }

    [TestMethod]
    public async Task Create_contract_returns_201_and_two_accounts()
    {
        var day = await _host.Client.GetFromJsonAsync<BankingDayResponse>("api/banking-day", DepositSamples.JsonOptions);
        day.Should().NotBeNull();
        var request = DepositSamples.AlfaSafe(4, r =>
        {
            r.Number = "API-4";
            r.StartDate = day!.CurrentDate.ToString("yyyy-MM-dd");
            r.EndDate = day.CurrentDate.AddMonths(13).ToString("yyyy-MM-dd");
        });
        var before = _host.CountContracts();
        var response = await _host.Client.PostAsync("api/deposits/contracts", DepositSamples.JsonBody(request));
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        _host.CountContracts().Should().Be(before + 1);

        var created = await response.Content.ReadFromJsonAsync<DepositContractResponse>(DepositSamples.JsonOptions);
        created.Should().NotBeNull();
        created!.PrincipalAccountNumber.Should().HaveLength(13);
        created.InterestAccountNumber.Should().HaveLength(13);
        created.PrincipalAccountNumber.Should().NotBe(created.InterestAccountNumber);
    }

    [TestMethod]
    public async Task Invalid_amount_returns_400()
    {
        var request = DepositSamples.AlfaSafe(clientId: 1, r => r.Amount = 10m);
        var response = await _host.Client.PostAsync("api/deposits/contracts", DepositSamples.JsonBody(request));
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await HttpAssertions.ReadProblem(response);
        problem.Errors.Should().ContainKey("amount");
    }

    [TestMethod]
    public async Task Close_banking_day_moves_the_date_and_can_change_turnovers()
    {
        var before = _host.BankDate();
        var accountsBefore = await _host.Client.GetFromJsonAsync<List<AccountReportItem>>("api/accounts", DepositSamples.JsonOptions);
        accountsBefore.Should().NotBeNull();
        var sfrbDebit = accountsBefore!.Single(a => a.Number == BankAccountNumbers.DevelopmentFund).Debit;

        var response = await _host.Client.PostAsync("api/banking-day/close?days=30", null);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var day = await response.Content.ReadFromJsonAsync<BankingDayResponse>(DepositSamples.JsonOptions);
        day.Should().NotBeNull();
        day!.CurrentDate.Should().Be(before.AddDays(30));

        var accountsAfter = await _host.Client.GetFromJsonAsync<List<AccountReportItem>>("api/accounts", DepositSamples.JsonOptions);
        accountsAfter.Should().NotBeNull();
        var sfrbAfter = accountsAfter!.Single(a => a.Number == BankAccountNumbers.DevelopmentFund);
        sfrbAfter.Debit.Should().BeGreaterThan(sfrbDebit);
    }

    [TestMethod]
    public async Task Dictionaries_include_clients_from_the_clients_api()
    {
        var response = await _host.Client.GetAsync("api/deposits/dictionaries");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var dictionaries = await response.Content.ReadFromJsonAsync<DepositDictionariesResponse>(DepositSamples.JsonOptions);
        dictionaries.Should().NotBeNull();
        dictionaries!.Clients.Should().HaveCountGreaterThanOrEqualTo(6);
        dictionaries.Products.Should().HaveCountGreaterThanOrEqualTo(2);
    }
}
