using Bank.Common.Accounting;
using Bank.Deposits.Data;
using Bank.Deposits.Models;
using Microsoft.EntityFrameworkCore;

namespace Bank.Deposits.Accounting;

public sealed class DepositLedger(DepositsDbContext db, IExchangeRateProvider rates)
{
    public const string OpenCashIn = "openCashIn";
    public const string OpenBankUse = "openBankUse";
    public const string AccrueInterest = "accrueInterest";
    public const string PayInterest = "payInterest";
    public const string ReturnPrincipal = "returnPrincipal";
    public const string PayPrincipal = "payPrincipal";

    public async Task<BankAccount> CashAsync(CancellationToken cancellationToken)
        => await RequiredByNumberAsync(BankAccountNumbers.Cash, cancellationToken);

    public async Task<BankAccount> DevelopmentFundAsync(CancellationToken cancellationToken)
        => await RequiredByNumberAsync(BankAccountNumbers.DevelopmentFund, cancellationToken);

    public async Task OpenDepositAsync(
        DepositContract contract,
        DateOnly bookedOn,
        CancellationToken cancellationToken)
    {
        var cash = await CashAsync(cancellationToken);
        var developmentFund = await DevelopmentFundAsync(cancellationToken);
        var currency = await CurrencyCodeAsync(contract.CurrencyId, cancellationToken);
        await PostAsync(bookedOn, cash, contract.PrincipalAccount, contract.Amount, currency, OpenCashIn, contract, cancellationToken);
        await PostAsync(bookedOn, contract.PrincipalAccount, developmentFund, contract.Amount, currency, OpenBankUse, contract, cancellationToken);
    }

    public async Task PayInterestAsync(
        DepositContract contract,
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
        await PostAsync(bookedOn, developmentFund, contract.InterestAccount, amount, currency, AccrueInterest, contract, cancellationToken);
        await PostAsync(bookedOn, contract.InterestAccount, cash, amount, currency, PayInterest, contract, cancellationToken);
        contract.LastInterestPaidOn = bookedOn;
    }

    public async Task CloseDepositAsync(
        DepositContract contract,
        DateOnly bookedOn,
        CancellationToken cancellationToken)
    {
        var cash = await CashAsync(cancellationToken);
        var developmentFund = await DevelopmentFundAsync(cancellationToken);
        var currency = await CurrencyCodeAsync(contract.CurrencyId, cancellationToken);
        await PostAsync(bookedOn, developmentFund, contract.PrincipalAccount, contract.Amount, currency, ReturnPrincipal, contract, cancellationToken);
        await PostAsync(bookedOn, contract.PrincipalAccount, cash, contract.Amount, currency, PayPrincipal, contract, cancellationToken);
        contract.Status = DepositContractStatus.Closed;
    }

    private async Task PostAsync(
        DateOnly bookedOn,
        BankAccount debit,
        BankAccount credit,
        decimal amount,
        string amountCurrencyCode,
        string operation,
        DepositContract? contract,
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
        DepositContract? contract)
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
