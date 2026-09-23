using System.Net;
using System.Net.Http.Json;
using Bank.Credits.Accounting;
using Bank.Common.Dtos.Response;
using Tests.Credits.Support;

namespace Tests.Credits.Integration;

[TestClass]
public sealed class CreditApiTests
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
    public async Task Seeded_accounts_report_has_at_least_ten_accounts_and_sfrb_capital()
    {
        var response = await _host.Client.GetAsync("api/accounts");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var accounts = await response.Content.ReadFromJsonAsync<List<AccountReportItem>>(CreditSamples.JsonOptions);
        accounts.Should().NotBeNull();
        accounts.Should().HaveCountGreaterThanOrEqualTo(10);

        var sfrb = accounts!.Single(a => a.Number == BankAccountNumbers.DevelopmentFund);
        sfrb.Credit.Should().BeGreaterThanOrEqualTo(1_000_000m);
        sfrb.Saldo.Should().BeGreaterThanOrEqualTo(1_000_000m - 5000m);
    }

    [TestMethod]
    public async Task Create_contract_returns_201_two_accounts_and_schedule()
    {
        var day = await _host.Client.GetFromJsonAsync<BankingDayResponse>("api/banking-day", CreditSamples.JsonOptions);
        day.Should().NotBeNull();
        var request = CreditSamples.CashLoan(6, r =>
        {
            r.Number = "API-6";
            r.StartDate = day!.CurrentDate.ToString("yyyy-MM-dd");
            r.EndDate = day.CurrentDate.AddMonths(12).ToString("yyyy-MM-dd");
        });
        var before = _host.CountContracts();
        var response = await _host.Client.PostAsync("api/credits/contracts", CreditSamples.JsonBody(request));
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        _host.CountContracts().Should().Be(before + 1);

        var created = await response.Content.ReadFromJsonAsync<CreditContractResponse>(CreditSamples.JsonOptions);
        created.Should().NotBeNull();
        created!.PrincipalAccountNumber.Should().HaveLength(13);
        created.InterestAccountNumber.Should().HaveLength(13);
        created.PrincipalAccountNumber.Should().NotBe(created.InterestAccountNumber);
        created.Schedule.Should().HaveCount(12);
        created.RepaymentSchedule.Should().Be("annuity");
    }

    [TestMethod]
    public async Task Invalid_amount_returns_400()
    {
        var request = CreditSamples.CashLoan(clientId: 1, r => r.Amount = 10m);
        var response = await _host.Client.PostAsync("api/credits/contracts", CreditSamples.JsonBody(request));
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await HttpAssertions.ReadProblem(response);
        problem.Errors.Should().ContainKey("amount");
    }

    [TestMethod]
    public async Task Close_banking_day_moves_the_date_and_can_change_turnovers()
    {
        var before = _host.BankDate();
        var accountsBefore = await _host.Client.GetFromJsonAsync<List<AccountReportItem>>("api/accounts", CreditSamples.JsonOptions);
        accountsBefore.Should().NotBeNull();
        var cashDebit = accountsBefore!.Single(a => a.Number == BankAccountNumbers.Cash).Debit;

        var response = await _host.Client.PostAsync("api/banking-day/close?days=30", null);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var day = await response.Content.ReadFromJsonAsync<BankingDayResponse>(CreditSamples.JsonOptions);
        day.Should().NotBeNull();
        day!.CurrentDate.Should().Be(before.AddDays(30));

        var accountsAfter = await _host.Client.GetFromJsonAsync<List<AccountReportItem>>("api/accounts", CreditSamples.JsonOptions);
        accountsAfter.Should().NotBeNull();
        var cashAfter = accountsAfter!.Single(a => a.Number == BankAccountNumbers.Cash);
        cashAfter.Debit.Should().BeGreaterThan(cashDebit);
    }

    [TestMethod]
    public async Task Dictionaries_include_clients_from_the_clients_api()
    {
        var response = await _host.Client.GetAsync("api/credits/dictionaries");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var dictionaries = await response.Content.ReadFromJsonAsync<CreditDictionariesResponse>(CreditSamples.JsonOptions);
        dictionaries.Should().NotBeNull();
        dictionaries!.Clients.Should().HaveCountGreaterThanOrEqualTo(6);
        dictionaries.Products.Should().HaveCountGreaterThanOrEqualTo(2);
    }
}
