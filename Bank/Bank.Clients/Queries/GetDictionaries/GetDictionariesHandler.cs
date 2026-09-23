using Bank.Clients.Data;
using Bank.Common.Dtos.Response;
using FluentResults;
using MapsterMapper;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Bank.Clients.Queries.GetDictionaries;

public sealed class GetDictionariesHandler(BankDbContext db, IMapper mapper)
    : IQueryHandler<GetDictionariesQuery, Result<DictionariesResponse>>
{
    public async ValueTask<Result<DictionariesResponse>> Handle(
        GetDictionariesQuery _,
        CancellationToken cancellationToken = default)
    {
        var cities = mapper.Map<List<LookupItem>>(await db.Cities.AsNoTracking().ToListAsync(cancellationToken));
        cities.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.CurrentCulture));

        var maritalStatuses = mapper.Map<List<LookupItem>>(
            await db.MaritalStatuses.AsNoTracking().OrderBy(c => c.Id).ToListAsync(cancellationToken));
        var citizenships = mapper.Map<List<LookupItem>>(
            await db.Citizenships.AsNoTracking().OrderBy(c => c.Id).ToListAsync(cancellationToken));
        var disabilities = mapper.Map<List<LookupItem>>(
            await db.Disabilities.AsNoTracking().OrderBy(c => c.Id).ToListAsync(cancellationToken));

        return Result.Ok(new DictionariesResponse(cities, maritalStatuses, citizenships, disabilities));
    }
}
