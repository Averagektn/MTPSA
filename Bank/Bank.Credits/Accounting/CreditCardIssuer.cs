using Bank.Credits.Data;
using Bank.Credits.Models;
using Microsoft.EntityFrameworkCore;

namespace Bank.Credits.Accounting;

public sealed class CreditCardIssuer(CreditsDbContext db, CreditLedger ledger)
{
    public async Task IssueAsync(CreditContract contract, DateOnly bookedOn, CancellationToken cancellationToken)
    {
        var chart = await db.ChartAccounts.FirstAsync(c => c.Code == ChartCodes.CardAccount, cancellationToken);
        var sequence = await db.BankAccounts.CountAsync(a => a.ClientId == contract.ClientId, cancellationToken);
        var clientCode = AccountNumberGenerator.ClientCode(contract.ClientId);
        var cardCount = await db.BankCards.CountAsync(cancellationToken) + 1;
        var account = new BankAccount
        {
            Number = AccountNumberGenerator.Build(chart.Code, clientCode, sequence + 1),
            ChartAccountId = chart.Id,
            ChartAccount = chart,
            ClientId = contract.ClientId,
            NameEn = contract.ClientName,
            NameRu = contract.ClientName,
            CurrencyId = contract.CurrencyId
        };
        db.BankAccounts.Add(account);

        var card = new BankCard
        {
            CardNumber = $"4277{contract.ClientId:D4}{cardCount:D8}",
            Pin = DefaultPin(contract.ClientId),
            CreditContract = contract,
            CardAccount = account
        };
        db.BankCards.Add(card);

        var funding = Math.Min(contract.Amount, DemoCards.Funding);
        await ledger.FundCardAsync(account, contract, bookedOn, funding, cancellationToken);
    }

    public static string DefaultPin(int clientId)
        => clientId == 5 ? DemoCards.SecondPin : DemoCards.FirstPin;
}
