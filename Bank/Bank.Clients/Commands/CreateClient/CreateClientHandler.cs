using Bank.Clients.Data;
using Bank.Common.Dtos.Request;
using Bank.Common.Dtos.Response;
using Bank.Clients.Models;
using Bank.Clients.Validation;
using FluentResults;
using MapsterMapper;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Bank.Clients.Commands.CreateClient;

public sealed class CreateClientHandler(BankDbContext db, ClientWriteValidator writeValidator, IMapper mapper)
    : ICommandHandler<CreateClientCommand, Result<ClientResponse>>
{
    public async ValueTask<Result<ClientResponse>> Handle(
        CreateClientCommand command,
        CancellationToken cancellationToken = default)
    {
        var validation = await writeValidator.ValidateAsync(command.Request, excludeId: null, cancellationToken);
        if (validation.IsFailed)
        {
            return validation.ToResult<ClientResponse>();
        }

        var client = mapper.Map<Client>(command.Request);
        db.Clients.Add(client);
        await db.SaveChangesAsync(cancellationToken);

        var created = await db.Clients.WithLookups()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == client.Id, cancellationToken);

        return created is null
            ? Result.Fail<ClientResponse>("createdClientMissing")
            : Result.Ok(mapper.Map<ClientResponse>(created));
    }
}
