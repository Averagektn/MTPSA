using Bank.Deposits.Data;
using Bank.Common.Dtos.Response;
using FluentResults;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Bank.Deposits.Queries.GetBankingDay;

public sealed class GetBankingDayHandler(DepositsDbContext db)
    : IQueryHandler<GetBankingDayQuery, Result<BankingDayResponse>>
{
    public async ValueTask<Result<BankingDayResponse>> Handle(
        GetBankingDayQuery _,
        CancellationToken cancellationToken = default)
    {
        var date = await db.BankStates.Select(s => s.CurrentDate).SingleAsync(cancellationToken);
        return Result.Ok(new BankingDayResponse(date));
    }
}
