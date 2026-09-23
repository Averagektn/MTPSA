using Bank.Credits.Commands.CreateCreditContract;
using Bank.Common.Dtos.Request;
using Bank.Credits.Accounting;
using Bank.Credits.Http;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Bank.Credits.Data;

public static class CreditDemoSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CreditsDbContext>();
        if (await db.CreditContracts.AnyAsync())
        {
            return;
        }

        var clientsApi = scope.ServiceProvider.GetRequiredService<IClientsApi>();
        if ((await clientsApi.GetByIdAsync(4)).IsFailed || (await clientsApi.GetByIdAsync(5)).IsFailed)
        {
            return;
        }

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var date = await db.BankStates.Select(s => s.CurrentDate).SingleAsync();
        var start = date.ToString("yyyy-MM-dd");

        await mediator.Send(new CreateCreditContractCommand(new CreditContractRequest
        {
            ClientId = 4,
            ProductId = 1,
            Number = "К-2026-0001",
            CurrencyId = 1,
            StartDate = start,
            EndDate = date.AddMonths(12).ToString("yyyy-MM-dd"),
            TermMonths = 12,
            Amount = 3000m,
            AnnualRate = 18.1m
        }));

        await mediator.Send(new CreateCreditContractCommand(new CreditContractRequest
        {
            ClientId = 5,
            ProductId = 2,
            Number = "К-2026-0002",
            CurrencyId = 1,
            StartDate = start,
            EndDate = date.AddMonths(24).ToString("yyyy-MM-dd"),
            TermMonths = 24,
            Amount = 2000m,
            AnnualRate = 18.1m
        }));

        var cards = await db.BankCards.OrderBy(c => c.Id).ToListAsync();
        if (cards.Count >= 2)
        {
            cards[0].CardNumber = DemoCards.FirstNumber;
            cards[0].Pin = DemoCards.FirstPin;
            cards[1].CardNumber = DemoCards.SecondNumber;
            cards[1].Pin = DemoCards.SecondPin;
            await db.SaveChangesAsync();
        }
    }
}
