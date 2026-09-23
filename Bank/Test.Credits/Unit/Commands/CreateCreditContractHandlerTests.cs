using Bank.Credits.Accounting;
using Bank.Credits.Commands.CloseBankingDay;
using Bank.Credits.Commands.CreateCreditContract;
using Bank.Credits.Models;
using Microsoft.EntityFrameworkCore;
using Tests.Credits.Support;

namespace Tests.Credits.Unit.Commands;

[TestClass]
public sealed class CreateCreditContractHandlerTests
{
    [TestMethod]
    public async Task Creates_two_accounts_and_opening_postings()
    {
        using var db = new SqliteTestDb();
        var result = await db.CreateCreditContractHandler()
            .Handle(new CreateCreditContractCommand(CreditSamples.CashLoan()));

        result.IsSuccess.Should().BeTrue();
        result.Value.PrincipalAccountNumber.Should().HaveLength(13);
        result.Value.InterestAccountNumber.Should().HaveLength(13);
        db.Db.BankAccounts.Count(a => a.ClientId == 1).Should().Be(3);
        db.Db.BankCards.Count(c => c.CreditContractId == result.Value.Id).Should().Be(1);
        result.Value.Schedule.Should().HaveCount(12);
        db.Db.LedgerEntries.Should().Contain(e => e.Operation == CreditLedger.Allocate);
        db.Db.LedgerEntries.Should().Contain(e => e.Operation == CreditLedger.Disburse);
    }
}

[TestClass]
public sealed class CloseBankingDayHandlerTests
{
    [TestMethod]
    public async Task Cash_loan_collects_annuity_on_the_first_month()
    {
        using var db = new SqliteTestDb();
        var created = await db.CreateCreditContractHandler()
            .Handle(new CreateCreditContractCommand(CreditSamples.CashLoan()));
        created.IsSuccess.Should().BeTrue();

        var closed = await db.CloseBankingDayHandler().Handle(new CloseBankingDayCommand(30));
        closed.IsSuccess.Should().BeTrue();
        closed.Value.CurrentDate.Should().Be(new DateOnly(2026, 10, 20));

        var contract = await db.Db.CreditContracts.FirstAsync(c => c.Id == created.Value.Id);
        contract.PaidPeriods.Should().Be(1);
        contract.LastPaymentOn.Should().Be(new DateOnly(2026, 10, 20));
        contract.Status.Should().Be(CreditContractStatus.Active);
        contract.RemainingPrincipal.Should().BeLessThan(contract.Amount);
        db.Db.LedgerEntries.Should().Contain(e => e.ContractId == contract.Id && e.Operation == CreditLedger.CollectInterest);
        db.Db.LedgerEntries.Should().Contain(e => e.ContractId == contract.Id && e.Operation == CreditLedger.CollectPrincipal);
    }

    [TestMethod]
    public async Task Online_loan_collects_only_interest_before_term_end()
    {
        using var db = new SqliteTestDb();
        var created = await db.CreateCreditContractHandler()
            .Handle(new CreateCreditContractCommand(CreditSamples.OnlineLoan(clientId: 4)));
        created.IsSuccess.Should().BeTrue();

        await db.CloseBankingDayHandler().Handle(new CloseBankingDayCommand(30));

        var afterMonth = await db.Db.CreditContracts.FirstAsync(c => c.Id == created.Value.Id);
        afterMonth.PaidPeriods.Should().Be(1);
        afterMonth.RemainingPrincipal.Should().Be(2000m);
        afterMonth.Status.Should().Be(CreditContractStatus.Active);
        db.Db.LedgerEntries.Should().NotContain(e => e.ContractId == afterMonth.Id && e.Operation == CreditLedger.CollectPrincipal);
        db.Db.LedgerEntries.Should().Contain(e => e.ContractId == afterMonth.Id && e.Operation == CreditLedger.CollectInterest);
    }

    [TestMethod]
    public async Task Online_loan_repays_principal_at_the_end_of_term()
    {
        using var db = new SqliteTestDb();
        var created = await db.CreateCreditContractHandler()
            .Handle(new CreateCreditContractCommand(CreditSamples.OnlineLoan(clientId: 4)));
        created.IsSuccess.Should().BeTrue();

        var closed = await db.CloseBankingDayHandler().Handle(new CloseBankingDayCommand(731));
        closed.IsSuccess.Should().BeTrue();
        closed.Value.CurrentDate.Should().Be(new DateOnly(2028, 9, 20));

        var contract = await db.Db.CreditContracts.FirstAsync(c => c.Id == created.Value.Id);
        contract.Status.Should().Be(CreditContractStatus.Closed);
        contract.RemainingPrincipal.Should().Be(0m);
        contract.PaidPeriods.Should().Be(24);
        db.Db.LedgerEntries.Should().Contain(e => e.ContractId == contract.Id && e.Operation == CreditLedger.CollectPrincipal);
    }
}
