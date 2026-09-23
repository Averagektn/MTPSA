using Bank.Clients.Data;
using Bank.Common.Dtos.Response;
using Bank.Common.Errors;
using FluentResults;
using MapsterMapper;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Bank.Clients.Queries.GetClient;

public sealed class GetClientHandler(BankDbContext db, IMapper mapper)
    : IQueryHandler<GetClientQuery, Result<ClientResponse>>
{
    public async ValueTask<Result<ClientResponse>> Handle(GetClientQuery query, CancellationToken cancellationToken = default)
    {
        var client = await db.Clients.WithLookups()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == query.Id, cancellationToken);

        return client is null
            ? Result.Fail<ClientResponse>(new NotFoundError())
            : Result.Ok(mapper.Map<ClientResponse>(client));
    }
}
