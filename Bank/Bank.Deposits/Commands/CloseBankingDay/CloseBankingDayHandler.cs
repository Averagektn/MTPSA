using Bank.Deposits.Accounting;
using Bank.Deposits.Data;
using Bank.Common.Dtos.Response;
using Bank.Common.Errors;
using Bank.Deposits.Models;
using FluentResults;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Bank.Deposits.Commands.CloseBankingDay;

public sealed class CloseBankingDayHandler(DepositsDbContext db, DepositLedger ledger)
    : ICommandHandler<CloseBankingDayCommand, Result<BankingDayResponse>>
{
    public const int MaxDays = 800;

    public async ValueTask<Result<BankingDayResponse>> Handle(
        CloseBankingDayCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.Days is < 1 or > MaxDays)
        {
            return Result.Fail<BankingDayResponse>(new ValidationError("days", "daysOutOfRange"));
        }

        var state = await db.BankStates.SingleAsync(cancellationToken);
        var contracts = await db.DepositContracts
            .Include(c => c.Product)
            .Include(c => c.PrincipalAccount)
            .Include(c => c.InterestAccount)
            .Where(c => c.Status == DepositContractStatus.Active)
            .ToListAsync(cancellationToken);

        for (var i = 0; i < command.Days; i++)
        {
            state.CurrentDate = state.CurrentDate.AddDays(1);
            await ApplyForDateAsync(state.CurrentDate, contracts, cancellationToken);
        }

        await db.SaveChangesAsync(cancellationToken);
        return Result.Ok(new BankingDayResponse(state.CurrentDate));
    }

    private async Task ApplyForDateAsync(
        DateOnly date,
        List<DepositContract> contracts,
        CancellationToken cancellationToken)
    {
        foreach (var contract in contracts)
        {
            if (contract.Status != DepositContractStatus.Active)
            {
                continue;
            }

            if (date >= contract.EndDate)
            {
                var from = contract.LastInterestPaidOn ?? contract.StartDate;
                var days = date.DayNumber - from.DayNumber;
                var interest = InterestCalculator.ForDays(contract.Amount, contract.AnnualRate, days);
                await ledger.PayInterestAsync(contract, date, interest, cancellationToken);
                await ledger.CloseDepositAsync(contract, date, cancellationToken);
                continue;
            }

            if (contract.Product.InterestSchedule != InterestSchedule.Monthly)
            {
                continue;
            }

            var elapsed = date.DayNumber - contract.StartDate.DayNumber;
            if (elapsed > 0 && elapsed % 30 == 0)
            {
                var interest = InterestCalculator.ForDays(contract.Amount, contract.AnnualRate, 30);
                await ledger.PayInterestAsync(contract, date, interest, cancellationToken);
            }
        }
    }
}
