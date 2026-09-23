using Bank.Common.Errors;
using Bank.Credits.Data;
using Bank.Common.Dtos.Response;
using Bank.Credits.Mapping;
using FluentResults;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Bank.Credits.Queries.GetCreditContract;

public sealed class GetCreditContractHandler(CreditsDbContext db)
    : IQueryHandler<GetCreditContractQuery, Result<CreditContractResponse>>
{
    public async ValueTask<Result<CreditContractResponse>> Handle(
        GetCreditContractQuery query,
        CancellationToken cancellationToken = default)
    {
        var contract = await db.CreditContracts
            .AsNoTracking()
            .Include(c => c.Product)
            .Include(c => c.Currency)
            .Include(c => c.PrincipalAccount)
            .Include(c => c.InterestAccount)
            .FirstOrDefaultAsync(c => c.Id == query.Id, cancellationToken);

        return contract is null
            ? Result.Fail<CreditContractResponse>(new NotFoundError())
            : Result.Ok(CreditContractMapper.ToResponse(contract));
    }
}
