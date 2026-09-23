using Bank.Credits.Accounting;
using Bank.Credits.Data;
using Bank.Common.Dtos.Response;
using Bank.Common.Localization;
using Bank.Credits.Http;
using Bank.Credits.Models;
using FluentResults;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Bank.Credits.Queries.GetAccountsReport;

public sealed class GetAccountsReportHandler(CreditsDbContext db, IDepositsApi depositsApi)
    : IQueryHandler<GetAccountsReportQuery, Result<IReadOnlyList<AccountReportItem>>>
{
    public async ValueTask<Result<IReadOnlyList<AccountReportItem>>> Handle(
        GetAccountsReportQuery _,
        CancellationToken cancellationToken = default)
    {
        var accounts = await db.BankAccounts
            .AsNoTracking()
            .Include(a => a.ChartAccount)
            .Include(a => a.Currency)
            .OrderBy(a => a.Number)
            .ToListAsync(cancellationToken);

        var entries = await db.LedgerEntries.AsNoTracking().ToListAsync(cancellationToken);
        var debitByAccount = entries
            .GroupBy(e => e.DebitAccountId)
            .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount));
        var creditByAccount = entries
            .GroupBy(e => e.CreditAccountId)
            .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount));
        var debitBynByAccount = entries
            .GroupBy(e => e.DebitAccountId)
            .ToDictionary(g => g.Key, g => g.Sum(e => e.AmountByn));
        var creditBynByAccount = entries
            .GroupBy(e => e.CreditAccountId)
            .ToDictionary(g => g.Key, g => g.Sum(e => e.AmountByn));

        var items = accounts.Select(account =>
        {
            var debit = debitByAccount.GetValueOrDefault(account.Id);
            var credit = creditByAccount.GetValueOrDefault(account.Id);
            var debitByn = debitBynByAccount.GetValueOrDefault(account.Id);
            var creditByn = creditBynByAccount.GetValueOrDefault(account.Id);
            return new AccountReportItem(
                account.Number,
                RequestLocale.Pick(account.NameEn, account.NameRu),
                account.ChartAccount.Code,
                RequestLocale.Pick(account.ChartAccount.NameEn, account.ChartAccount.NameRu),
                Nature(account.ChartAccount.Nature),
                debit,
                credit,
                AccountBalance.Saldo(account.ChartAccount.Nature, debit, credit),
                account.Currency.Code,
                debitByn,
                creditByn,
                AccountBalance.Saldo(account.ChartAccount.Nature, debitByn, creditByn));
        }).ToList();

        var knownNumbers = items.Select(item => item.Number).ToHashSet();
        var deposits = await depositsApi.GetAccountsAsync(cancellationToken);
        if (deposits.IsSuccess)
        {
            foreach (var account in deposits.Value)
            {
                if (knownNumbers.Add(account.Number))
                {
                    items.Add(account);
                }
            }
        }

        IReadOnlyList<AccountReportItem> report = items.OrderBy(item => item.Number).ToList();
        return Result.Ok(report);
    }

    private static string Nature(AccountNature nature) => nature switch
    {
        AccountNature.Active => "A",
        AccountNature.Passive => "P",
        _ => "AP"
    };
}
