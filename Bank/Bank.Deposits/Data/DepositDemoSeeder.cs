using Bank.Deposits.Commands.CreateDepositContract;
using Bank.Common.Dtos.Request;
using Bank.Deposits.Http;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Bank.Deposits.Data;

public static class DepositDemoSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DepositsDbContext>();
        if (await db.DepositContracts.AnyAsync())
        {
            return;
        }

        var clientsApi = scope.ServiceProvider.GetRequiredService<IClientsApi>();
        if ((await clientsApi.GetByIdAsync(2)).IsFailed || (await clientsApi.GetByIdAsync(3)).IsFailed)
        {
            return;
        }

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var date = await db.BankStates.Select(s => s.CurrentDate).SingleAsync();
        var start = date.ToString("yyyy-MM-dd");
        var end = date.AddMonths(13).ToString("yyyy-MM-dd");

        await mediator.Send(new CreateDepositContractCommand(new DepositContractRequest
        {
            ClientId = 2,
            ProductId = 1,
            Number = "Д-2026-0001",
            CurrencyId = 1,
            StartDate = start,
            EndDate = end,
            TermMonths = 13,
            Amount = 1000m,
            AnnualRate = 5.5m
        }));

        await mediator.Send(new CreateDepositContractCommand(new DepositContractRequest
        {
            ClientId = 3,
            ProductId = 2,
            Number = "Д-2026-0002",
            CurrencyId = 1,
            StartDate = start,
            EndDate = end,
            TermMonths = 13,
            Amount = 5000m,
            AnnualRate = 12m
        }));

        if (!(await clientsApi.GetByIdAsync(4)).IsFailed)
        {
            await mediator.Send(new CreateDepositContractCommand(new DepositContractRequest
            {
                ClientId = 4,
                ProductId = 1,
                Number = "Д-2026-0003",
                CurrencyId = 1,
                StartDate = start,
                EndDate = end,
                TermMonths = 13,
                Amount = 1200m,
                AnnualRate = 5.5m
            }));
        }
    }
}
