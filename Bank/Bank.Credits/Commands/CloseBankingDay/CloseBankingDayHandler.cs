using Bank.Credits.Accounting;
using Bank.Credits.Data;
using Bank.Common.Dtos.Response;
using Bank.Common.Errors;
using Bank.Credits.Models;
using FluentResults;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Bank.Credits.Commands.CloseBankingDay;

public sealed class CloseBankingDayHandler(CreditsDbContext db, CreditLedger ledger)
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
        var contracts = await db.CreditContracts
            .Include(c => c.Product)
            .Include(c => c.PrincipalAccount)
            .Include(c => c.InterestAccount)
            .Where(c => c.Status == CreditContractStatus.Active)
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
        List<CreditContract> contracts,
        CancellationToken cancellationToken)
    {
        foreach (var contract in contracts)
        {
            if (contract.Status != CreditContractStatus.Active)
            {
                continue;
            }

            var period = contract.PaidPeriods + 1;
            if (period > contract.TermMonths)
            {
                continue;
            }

            if (contract.StartDate.AddMonths(period) != date)
            {
                continue;
            }

            var schedule = PaymentScheduleCalculator.Build(
                contract.Amount,
                contract.AnnualRate,
                contract.StartDate,
                contract.TermMonths,
                contract.Product.RepaymentSchedule);
            var item = schedule[period - 1];
            await ledger.CollectInterestAsync(contract, date, item.Interest, cancellationToken);
            await ledger.CollectPrincipalAsync(contract, date, item.Principal, cancellationToken);
            contract.PaidPeriods = period;
            contract.RemainingPrincipal = item.Remaining;
            contract.LastPaymentOn = date;
            if (item.Remaining == 0 || period == contract.TermMonths)
            {
                contract.Status = CreditContractStatus.Closed;
                contract.RemainingPrincipal = 0;
            }
        }
    }
}
