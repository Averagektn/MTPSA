using Bank.Deposits.Accounting;
using Bank.Deposits.Commands.CloseBankingDay;
using Bank.Deposits.Commands.CreateDepositContract;
using Bank.Deposits.Models;
using Microsoft.EntityFrameworkCore;
using Tests.Deposits.Support;

namespace Tests.Deposits.Unit.Commands;

[TestClass]
public sealed class CreateDepositContractHandlerTests
{
    [TestMethod]
    public async Task Creates_two_accounts_and_opening_postings()
    {
        using var db = new SqliteTestDb();
        var result = await db.CreateDepositContractHandler()
            .Handle(new CreateDepositContractCommand(DepositSamples.AlfaSafe()));

        result.IsSuccess.Should().BeTrue();
        result.Value.PrincipalAccountNumber.Should().HaveLength(13);
        result.Value.InterestAccountNumber.Should().HaveLength(13);
        db.Db.BankAccounts.Count(a => a.ClientId == 1).Should().Be(2);
        db.Db.LedgerEntries.Should().Contain(e => e.Operation == DepositLedger.OpenCashIn);
        db.Db.LedgerEntries.Should().Contain(e => e.Operation == DepositLedger.OpenBankUse);
    }

    [TestMethod]
    public async Task Usd_opening_converts_cash_through_the_currency_position()
    {
        using var db = new SqliteTestDb();
        var result = await db.CreateDepositContractHandler()
            .Handle(new CreateDepositContractCommand(DepositSamples.AlfaSafe(mutate: request =>
            {
                request.CurrencyId = 2;
                request.Amount = 1000m;
            })));

        result.IsSuccess.Should().BeTrue();
        var entries = await db.Db.LedgerEntries
            .Include(entry => entry.DebitAccount)
            .Include(entry => entry.CreditAccount)
            .Where(entry => entry.Operation == DepositLedger.OpenCashIn)
            .ToListAsync();

        entries.Should().HaveCount(2);
        entries.Sum(entry => entry.AmountByn).Should().Be(6500m);
        entries.Should().Contain(entry =>
            entry.DebitAccount.Number == BankAccountNumbers.Cash && entry.Amount == 3250m && entry.AmountByn == 3250m);
        entries.Should().Contain(entry =>
            entry.CreditAccount.CurrencyId == 2 && entry.Amount == 1000m && entry.AmountByn == 3250m);
    }
}

[TestClass]
public sealed class CloseBankingDayHandlerTests
{
    [TestMethod]
    public async Task Alfa_safe_pays_interest_every_30_days()
    {
        using var db = new SqliteTestDb();
        var created = await db.CreateDepositContractHandler()
            .Handle(new CreateDepositContractCommand(DepositSamples.AlfaSafe()));
        created.IsSuccess.Should().BeTrue();

        var state = await db.Db.BankStates.SingleAsync();
        state.CurrentDate = new DateOnly(2026, 10, 19);
        await db.Db.SaveChangesAsync();

        var closed = await db.CloseBankingDayHandler().Handle(new CloseBankingDayCommand());
        closed.IsSuccess.Should().BeTrue();
        closed.Value.CurrentDate.Should().Be(new DateOnly(2026, 10, 20));

        var contract = await db.Db.DepositContracts.FirstAsync(c => c.Id == created.Value.Id);
        contract.LastInterestPaidOn.Should().Be(new DateOnly(2026, 10, 20));
        contract.Status.Should().Be(DepositContractStatus.Active);
        db.Db.LedgerEntries.Should().Contain(e => e.ContractId == contract.Id && e.Operation == DepositLedger.PayInterest);
    }

    [TestMethod]
    public async Task Alfa_vklad_pays_only_at_the_end_of_term()
    {
        using var db = new SqliteTestDb();
        var created = await db.CreateDepositContractHandler()
            .Handle(new CreateDepositContractCommand(DepositSamples.AlfaVklad(clientId: 4)));
        created.IsSuccess.Should().BeTrue();

        var state = await db.Db.BankStates.SingleAsync();
        state.CurrentDate = new DateOnly(2026, 10, 19);
        await db.Db.SaveChangesAsync();
        await db.CloseBankingDayHandler().Handle(new CloseBankingDayCommand());

        var afterMonth = await db.Db.DepositContracts.FirstAsync(c => c.Id == created.Value.Id);
        afterMonth.LastInterestPaidOn.Should().BeNull();
        afterMonth.Status.Should().Be(DepositContractStatus.Active);

        state = await db.Db.BankStates.SingleAsync();
        state.CurrentDate = new DateOnly(2027, 10, 19);
        await db.Db.SaveChangesAsync();
        await db.CloseBankingDayHandler().Handle(new CloseBankingDayCommand());

        var closed = await db.Db.DepositContracts.FirstAsync(c => c.Id == created.Value.Id);
        closed.Status.Should().Be(DepositContractStatus.Closed);
        closed.LastInterestPaidOn.Should().Be(new DateOnly(2027, 10, 20));
        db.Db.LedgerEntries.Should().Contain(e => e.ContractId == closed.Id && e.Operation == DepositLedger.PayPrincipal);
    }

    [TestMethod]
    public async Task Closing_30_days_at_once_pays_alfa_safe_interest()
    {
        using var db = new SqliteTestDb();
        var created = await db.CreateDepositContractHandler()
            .Handle(new CreateDepositContractCommand(DepositSamples.AlfaSafe()));
        created.IsSuccess.Should().BeTrue();

        var closed = await db.CloseBankingDayHandler().Handle(new CloseBankingDayCommand(30));
        closed.IsSuccess.Should().BeTrue();
        closed.Value.CurrentDate.Should().Be(new DateOnly(2026, 10, 20));

        var contract = await db.Db.DepositContracts.FirstAsync(c => c.Id == created.Value.Id);
        contract.LastInterestPaidOn.Should().Be(new DateOnly(2026, 10, 20));
        db.Db.LedgerEntries.Should().Contain(e => e.ContractId == contract.Id && e.Operation == DepositLedger.PayInterest);
    }
}
