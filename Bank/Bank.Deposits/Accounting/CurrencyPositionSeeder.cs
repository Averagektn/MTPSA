using Bank.Deposits.Data;
using Bank.Deposits.Models;
using Microsoft.EntityFrameworkCore;

namespace Bank.Deposits.Accounting;

public static class CurrencyPositionSeeder
{
    public static async Task EnsureAsync(DepositsDbContext db, CancellationToken cancellationToken = default)
    {
        var chart = await db.ChartAccounts
            .FirstOrDefaultAsync(account => account.Code == ChartCodes.CurrencyPosition, cancellationToken);
        if (chart is null)
        {
            chart = new ChartAccount
            {
                Code = ChartCodes.CurrencyPosition,
                NameEn = "Currency position",
                NameRu = "Валютная позиция",
                Nature = AccountNature.ActivePassive
            };
            db.ChartAccounts.Add(chart);
            await db.SaveChangesAsync(cancellationToken);
        }

        var currencies = await db.Currencies.OrderBy(currency => currency.Id).ToListAsync(cancellationToken);
        foreach (var currency in currencies)
        {
            var number = AccountNumberGenerator.Build(ChartCodes.CurrencyPosition, "00000", currency.Id);
            if (await db.BankAccounts.AnyAsync(account => account.Number == number, cancellationToken))
            {
                continue;
            }

            db.BankAccounts.Add(new BankAccount
            {
                Number = number,
                ChartAccountId = chart.Id,
                CurrencyId = currency.Id,
                NameEn = $"Currency position {currency.Code}",
                NameRu = $"Валютная позиция {currency.Code}"
            });
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
