using Bank.Common.Accounting;
using Bank.Credits.Data;
using Bank.Credits.Models;
using Microsoft.EntityFrameworkCore;

namespace Bank.Credits.Accounting;

public sealed class CreditLedger(CreditsDbContext db, IExchangeRateProvider rates)
{
    public const string Allocate = "allocateCredit";
    public const string Disburse = "disburseCash";
    public const string AccrueInterest = "accrueInterest";
    public const string CollectInterest = "collectInterest";
    public const string CollectPrincipal = "collectPrincipal";
    public const string ReturnToFund = "returnToFund";
    public const string FundCard = "fundCard";
    public const string AtmWithdraw = "atmWithdraw";
    public const string AtmPayment = "atmPayment";

    public async Task<BankAccount> CashAsync(CancellationToken cancellationToken)
        => await RequiredByNumberAsync(BankAccountNumbers.Cash, cancellationToken);

    public async Task<BankAccount> DevelopmentFundAsync(CancellationToken cancellationToken)
        => await RequiredByNumberAsync(BankAccountNumbers.DevelopmentFund, cancellationToken);

    public async Task<BankAccount> MobileSettlementsAsync(CancellationToken cancellationToken)
        => await RequiredByNumberAsync(BankAccountNumbers.MobileSettlements, cancellationToken);

    public async Task OpenCreditAsync(
        CreditContract contract,
        DateOnly bookedOn,
        CancellationToken cancellationToken)
    {
        var cash = await CashAsync(cancellationToken);
        var developmentFund = await DevelopmentFundAsync(cancellationToken);
        var currency = await CurrencyCodeAsync(contract.CurrencyId, cancellationToken);
        await PostAsync(bookedOn, developmentFund, contract.PrincipalAccount, contract.Amount, currency, Allocate, contract, cancellationToken);
        await PostAsync(bookedOn, contract.PrincipalAccount, cash, contract.Amount, currency, Disburse, contract, cancellationToken);
    }

    public async Task CollectInterestAsync(
        CreditContract contract,
        DateOnly bookedOn,
        decimal amount,
        CancellationToken cancellationToken)
    {
        if (amount <= 0)
        {
            return;
        }

        var cash = await CashAsync(cancellationToken);
        var developmentFund = await DevelopmentFundAsync(cancellationToken);
        var currency = await CurrencyCodeAsync(contract.CurrencyId, cancellationToken);
        await PostAsync(bookedOn, contract.InterestAccount, developmentFund, amount, currency, AccrueInterest, contract, cancellationToken);
        await PostAsync(bookedOn, cash, contract.InterestAccount, amount, currency, CollectInterest, contract, cancellationToken);
    }

    public async Task CollectPrincipalAsync(
        CreditContract contract,
        DateOnly bookedOn,
        decimal amount,
        CancellationToken cancellationToken)
    {
        if (amount <= 0)
        {
            return;
        }

        var cash = await CashAsync(cancellationToken);
        var developmentFund = await DevelopmentFundAsync(cancellationToken);
        var currency = await CurrencyCodeAsync(contract.CurrencyId, cancellationToken);
        await PostAsync(bookedOn, cash, contract.PrincipalAccount, amount, currency, CollectPrincipal, contract, cancellationToken);
        await PostAsync(bookedOn, contract.PrincipalAccount, developmentFund, amount, currency, ReturnToFund, contract, cancellationToken);
    }

    public async Task FundCardAsync(
        BankAccount cardAccount,
        CreditContract contract,
        DateOnly bookedOn,
        decimal amount,
        CancellationToken cancellationToken)
    {
        if (amount <= 0)
        {
            return;
        }

        var cash = await CashAsync(cancellationToken);
        var currency = await CurrencyCodeAsync(contract.CurrencyId, cancellationToken);
        await PostAsync(bookedOn, cash, cardAccount, amount, currency, FundCard, contract, cancellationToken);
    }

    public async Task AtmWithdrawAsync(
        BankAccount cardAccount,
        CreditContract contract,
        DateOnly bookedOn,
        decimal amount,
        CancellationToken cancellationToken)
    {
        var cash = await CashAsync(cancellationToken);
        await PostAsync(bookedOn, cardAccount, cash, amount, "BYN", AtmWithdraw, contract, cancellationToken);
    }

    public async Task AtmPaymentAsync(
        BankAccount cardAccount,
        CreditContract contract,
        DateOnly bookedOn,
        decimal amount,
        CancellationToken cancellationToken)
    {
        var mobile = await MobileSettlementsAsync(cancellationToken);
        await PostAsync(bookedOn, cardAccount, mobile, amount, "BYN", AtmPayment, contract, cancellationToken);
    }

    private async Task PostAsync(
        DateOnly bookedOn,
        BankAccount debit,
        BankAccount credit,
        decimal amount,
        string amountCurrencyCode,
        string operation,
        CreditContract? contract,
        CancellationToken cancellationToken)
    {
        var debitCode = await CurrencyCodeAsync(debit.CurrencyId, cancellationToken);
        var creditCode = await CurrencyCodeAsync(credit.CurrencyId, cancellationToken);
        var amountRate = await rates.GetBynPerUnitAsync(amountCurrencyCode, bookedOn, cancellationToken);
        var amountByn = ExchangeConversion.ToByn(amount, amountRate);
        var debitLeg = ExchangeConversion.ForSide(
            debitCode,
            amount,
            amountCurrencyCode,
            amountByn,
            await rates.GetBynPerUnitAsync(debitCode, bookedOn, cancellationToken));
        var creditLeg = ExchangeConversion.ForSide(
            creditCode,
            amount,
            amountCurrencyCode,
            amountByn,
            await rates.GetBynPerUnitAsync(creditCode, bookedOn, cancellationToken));

        if (debitCode.Equals(creditCode, StringComparison.OrdinalIgnoreCase))
        {
            Add(bookedOn, debit, credit, debitLeg, operation, contract);
            return;
        }

        var debitPosition = await PositionAsync(debit.CurrencyId, cancellationToken);
        var creditPosition = await PositionAsync(credit.CurrencyId, cancellationToken);
        Add(bookedOn, debit, debitPosition, debitLeg, operation, contract);
        Add(bookedOn, creditPosition, credit, creditLeg, operation, contract);
    }

    private void Add(
        DateOnly bookedOn,
        BankAccount debit,
        BankAccount credit,
        ExchangeConversion.MoneyLeg leg,
        string operation,
        CreditContract? contract)
    {
        db.LedgerEntries.Add(new LedgerEntry
        {
            BookedOn = bookedOn,
            DebitAccount = debit,
            CreditAccount = credit,
            Amount = leg.Amount,
            AmountByn = leg.AmountByn,
            Rate = leg.BynPerUnit,
            Operation = operation,
            Contract = contract
        });
    }

    private async Task<BankAccount> PositionAsync(int currencyId, CancellationToken cancellationToken)
    {
        var chartId = await db.ChartAccounts
            .Where(account => account.Code == ChartCodes.CurrencyPosition)
            .Select(account => account.Id)
            .SingleAsync(cancellationToken);
        return await db.BankAccounts.SingleAsync(
            account => account.ChartAccountId == chartId && account.CurrencyId == currencyId,
            cancellationToken);
    }

    private async Task<string> CurrencyCodeAsync(int currencyId, CancellationToken cancellationToken)
        => await db.Currencies
            .Where(currency => currency.Id == currencyId)
            .Select(currency => currency.Code)
            .SingleAsync(cancellationToken);

    private async Task<BankAccount> RequiredByNumberAsync(string number, CancellationToken cancellationToken)
        => await db.BankAccounts.FirstAsync(a => a.Number == number, cancellationToken);
}
