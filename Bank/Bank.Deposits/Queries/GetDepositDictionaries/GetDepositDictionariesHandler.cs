using Bank.Common.Dtos.Response;
using Bank.Common.Localization;
using Bank.Deposits.Data;
using Bank.Deposits.Http;
using FluentResults;
using MapsterMapper;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Bank.Deposits.Queries.GetDepositDictionaries;

public sealed class GetDepositDictionariesHandler(DepositsDbContext db, IMapper mapper, IClientsApi clientsApi)
    : IQueryHandler<GetDepositDictionariesQuery, Result<DepositDictionariesResponse>>
{
    public async ValueTask<Result<DepositDictionariesResponse>> Handle(
        GetDepositDictionariesQuery _,
        CancellationToken cancellationToken = default)
    {
        var products = await db.DepositProducts
            .AsNoTracking()
            .OrderBy(p => p.Id)
            .ToListAsync(cancellationToken);

        var lookups = products.Select(p => new DepositProductLookup(
            p.Id,
            p.Code,
            RequestLocale.Pick(p.NameEn, p.NameRu),
            p.Revocable,
            p.InterestSchedule.ToString(),
            p.TermMonths,
            p.AnnualRate,
            p.MinAmount,
            p.MaxAmount,
            p.CurrencyId)).ToList();

        var currencies = mapper.Map<List<LookupItem>>(
            await db.Currencies.AsNoTracking().OrderBy(c => c.Id).ToListAsync(cancellationToken));
        var clients = await clientsApi.ListAsync(cancellationToken);
        if (clients.IsFailed)
        {
            return clients.ToResult<DepositDictionariesResponse>();
        }

        return Result.Ok(new DepositDictionariesResponse(lookups, currencies, clients.Value));
    }
}
