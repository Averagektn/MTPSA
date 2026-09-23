using Bank.Common.Errors;
using Bank.Deposits.Data;
using Bank.Common.Dtos.Response;
using Bank.Deposits.Mapping;
using FluentResults;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Bank.Deposits.Queries.GetDepositContract;

public sealed class GetDepositContractHandler(DepositsDbContext db)
    : IQueryHandler<GetDepositContractQuery, Result<DepositContractResponse>>
{
    public async ValueTask<Result<DepositContractResponse>> Handle(
        GetDepositContractQuery query,
        CancellationToken cancellationToken = default)
    {
        var contract = await db.DepositContracts
            .AsNoTracking()
            .Include(c => c.Product)
            .Include(c => c.Currency)
            .Include(c => c.PrincipalAccount)
            .Include(c => c.InterestAccount)
            .FirstOrDefaultAsync(c => c.Id == query.Id, cancellationToken);

        return contract is null
            ? Result.Fail<DepositContractResponse>(new NotFoundError())
            : Result.Ok(DepositContractMapper.ToResponse(contract));
    }
}
