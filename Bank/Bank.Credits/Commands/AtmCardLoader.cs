using Bank.Common.Atm;
using Bank.Credits.Accounting;
using Bank.Credits.Data;
using Bank.Credits.Models;
using Bank.Common.Dtos.Response;
using Bank.Common.Errors;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Bank.Credits.Commands;

public sealed record AtmCardPack(BankCard Card, decimal Balance);

public static class AtmCardLoader
{
    public static async Task<Result<AtmCardPack>> LoadAsync(
        CreditsDbContext db,
        string cardNumber,
        CancellationToken cancellationToken)
    {
        var digits = new string(cardNumber.Where(char.IsDigit).ToArray());
        var card = await db.BankCards
            .Include(c => c.CardAccount)
            .Include(c => c.CreditContract)
                .ThenInclude(c => c.PrincipalAccount)
            .Include(c => c.CreditContract)
                .ThenInclude(c => c.Currency)
            .FirstOrDefaultAsync(c => c.CardNumber == digits, cancellationToken);

        if (card is null)
        {
            return Result.Fail(new ValidationError("cardNumber", "cardNotFound"));
        }

        if (card.CreditContract.Status != CreditContractStatus.Active)
        {
            return Result.Fail(new ValidationError("cardNumber", "cardInactive"));
        }

        var debit = await db.LedgerEntries
            .Where(e => e.DebitAccountId == card.CardAccountId)
            .SumAsync(e => (decimal?)e.Amount, cancellationToken) ?? 0m;
        var credit = await db.LedgerEntries
            .Where(e => e.CreditAccountId == card.CardAccountId)
            .SumAsync(e => (decimal?)e.Amount, cancellationToken) ?? 0m;

        return Result.Ok(new AtmCardPack(
            card,
            AccountBalance.Saldo(AccountNature.Passive, debit, credit)));
    }

    public static AtmAuthorizeResponse ToAuthorize(AtmCardPack pack)
        => new(
            pack.Card.CardNumber,
            CardNumberMask.Mask(pack.Card.CardNumber),
            pack.Card.CreditContract.ClientName,
            pack.Card.CreditContract.ClientId,
            pack.Card.CreditContract.Id,
            pack.Card.CreditContract.Number,
            pack.Card.CreditContract.PrincipalAccount.Number,
            pack.Balance,
            pack.Card.CreditContract.Currency.Code);
}
