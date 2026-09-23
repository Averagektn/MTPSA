using Bank.Deposits.Data;
using Bank.Common.Dtos.Response;
using Bank.Deposits.Mapping;
using FluentResults;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Bank.Deposits.Queries.GetDepositContracts;

public sealed class GetDepositContractsHandler(DepositsDbContext db)
    : IQueryHandler<GetDepositContractsQuery, Result<IReadOnlyList<DepositContractListItem>>>
{
    public async ValueTask<Result<IReadOnlyList<DepositContractListItem>>> Handle(
        GetDepositContractsQuery query,
        CancellationToken cancellationToken = default)
    {
        var contractsQuery = db.DepositContracts
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
        var bankDate = await db.BankStates.Select(s => s.CurrentDate).SingleAsync(cancellationToken);
        IReadOnlyList<DepositContractListItem> items = contracts
            .Select(contract => DepositContractMapper.ToListItem(contract, bankDate))
            .ToList();
        return Result.Ok(items);
    }
}
