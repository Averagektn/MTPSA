using Bank.Common.Dtos.Response;
using Bank.Common.Localization;
using Bank.Credits.Data;
using Bank.Credits.Http;
using Bank.Credits.Models;
using FluentResults;
using MapsterMapper;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Bank.Credits.Queries.GetCreditDictionaries;

public sealed class GetCreditDictionariesHandler(CreditsDbContext db, IMapper mapper, IClientsApi clientsApi)
    : IQueryHandler<GetCreditDictionariesQuery, Result<CreditDictionariesResponse>>
{
    public async ValueTask<Result<CreditDictionariesResponse>> Handle(
        GetCreditDictionariesQuery _,
        CancellationToken cancellationToken = default)
    {
        var products = await db.CreditProducts
            .AsNoTracking()
            .OrderBy(p => p.Id)
            .ToListAsync(cancellationToken);

        var lookups = products.Select(p => new CreditProductLookup(
            p.Id,
            p.Code,
            RequestLocale.Pick(p.NameEn, p.NameRu),
            p.RepaymentSchedule == RepaymentSchedule.InterestOnly ? "interestOnly" : "annuity",
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
            return clients.ToResult<CreditDictionariesResponse>();
        }

        return Result.Ok(new CreditDictionariesResponse(lookups, currencies, clients.Value));
    }
}
