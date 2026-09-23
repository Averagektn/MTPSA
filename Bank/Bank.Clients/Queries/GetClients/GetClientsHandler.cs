using Bank.Clients.Data;
using Bank.Common.Dtos.Response;
using FluentResults;
using MapsterMapper;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Bank.Clients.Queries.GetClients;

public sealed class GetClientsHandler(BankDbContext db, IMapper mapper)
    : IQueryHandler<GetClientsQuery, Result<IReadOnlyList<ClientListItem>>>
{
    public async ValueTask<Result<IReadOnlyList<ClientListItem>>> Handle(
        GetClientsQuery _,
        CancellationToken cancellationToken = default)
    {
        var clients = await db.Clients.WithLookups()
            .AsNoTracking()
            .OrderBy(c => c.LastName)
            .ThenBy(c => c.FirstName)
            .ThenBy(c => c.Patronymic)
            .ToListAsync(cancellationToken);

        IReadOnlyList<ClientListItem> items = mapper.Map<List<ClientListItem>>(clients);
        return Result.Ok(items);
    }
}
