using Bank.Clients.Data;
using Bank.Common.Dtos.Request;
using Bank.Common.Dtos.Response;
using Bank.Common.Errors;
using Bank.Clients.Models;
using Bank.Clients.Validation;
using FluentResults;
using MapsterMapper;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Bank.Clients.Commands.UpdateClient;

public sealed class UpdateClientHandler(BankDbContext db, ClientWriteValidator writeValidator, IMapper mapper)
    : ICommandHandler<UpdateClientCommand, Result<ClientResponse>>
{
    public async ValueTask<Result<ClientResponse>> Handle(
        UpdateClientCommand command,
        CancellationToken cancellationToken = default)
    {
        var client = await db.Clients.FirstOrDefaultAsync(c => c.Id == command.Id, cancellationToken);
        if (client is null)
        {
            return Result.Fail<ClientResponse>(new NotFoundError());
        }

        var validation = await writeValidator.ValidateAsync(command.Request, excludeId: command.Id, cancellationToken);
        if (validation.IsFailed)
        {
            return validation.ToResult<ClientResponse>();
        }

        mapper.Map<ClientRequest, Client>(command.Request, client);
        await db.SaveChangesAsync(cancellationToken);

        var updated = await db.Clients.WithLookups()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == command.Id, cancellationToken);

        return updated is null
            ? Result.Fail<ClientResponse>(new NotFoundError())
            : Result.Ok(mapper.Map<ClientResponse>(updated));
    }
}
