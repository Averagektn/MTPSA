using Bank.Credits.Data;
using Bank.Common.Dtos.Response;
using Bank.Credits.Mapping;
using FluentResults;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Bank.Credits.Queries.GetCreditContracts;

public sealed class GetCreditContractsHandler(CreditsDbContext db)
    : IQueryHandler<GetCreditContractsQuery, Result<IReadOnlyList<CreditContractListItem>>>
{
    public async ValueTask<Result<IReadOnlyList<CreditContractListItem>>> Handle(
        GetCreditContractsQuery query,
        CancellationToken cancellationToken = default)
    {
        var contractsQuery = db.CreditContracts
            .AsNoTracking()
            .Include(c => c.Product)
            .Include(c => c.Currency)
            .Include(c => c.PrincipalAccount)
            .Include(c => c.InterestAccount)
            .AsQueryable();

        if (query.ClientId is int clientId)
        {
            contractsQuery = contractsQuery.Where(c => c.ClientId == clientId);
        }

        var contracts = await contractsQuery.OrderBy(c => c.Number).ToListAsync(cancellationToken);
        IReadOnlyList<CreditContractListItem> items = contracts
            .Select(CreditContractMapper.ToListItem)
            .ToList();
        return Result.Ok(items);
    }
}
