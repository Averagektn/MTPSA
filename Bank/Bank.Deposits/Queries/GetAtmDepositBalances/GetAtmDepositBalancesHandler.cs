using Bank.Deposits.Data;
using Bank.Deposits.Models;
using Bank.Common.Dtos.Response;
using Bank.Common.Errors;
using Bank.Common.Localization;
using FluentResults;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Bank.Deposits.Queries.GetAtmDepositBalances;

public sealed record GetAtmDepositBalancesQuery(int ClientId)
    : IQuery<Result<AtmDepositBalanceResponse>>;

public sealed class GetAtmDepositBalancesHandler(DepositsDbContext db)
    : IQueryHandler<GetAtmDepositBalancesQuery, Result<AtmDepositBalanceResponse>>
{
    public async ValueTask<Result<AtmDepositBalanceResponse>> Handle(
        GetAtmDepositBalancesQuery query,
        CancellationToken cancellationToken = default)
    {
        if (query.ClientId <= 0)
        {
            return Result.Fail(new ValidationError("clientId", "clientNotFound"));
        }

        var contracts = await db.DepositContracts
            .AsNoTracking()
            .Include(c => c.Product)
            .Include(c => c.Currency)
            .Include(c => c.PrincipalAccount)
            .Where(c => c.ClientId == query.ClientId && c.Status == DepositContractStatus.Active)
            .OrderBy(c => c.Number)
            .ToListAsync(cancellationToken);

        var clientName = contracts.FirstOrDefault()?.ClientName ?? "";
        IReadOnlyList<AtmDepositBalanceItem> items = contracts
            .Select(c => new AtmDepositBalanceItem(
                c.Number,
                c.PrincipalAccount.Number,
                RequestLocale.Pick(c.Product.NameEn, c.Product.NameRu),
                c.Amount,
                c.Currency.Code))
            .ToList();

        return Result.Ok(new AtmDepositBalanceResponse(query.ClientId, clientName, items));
    }
}
