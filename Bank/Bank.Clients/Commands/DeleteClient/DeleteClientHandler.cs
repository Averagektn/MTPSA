using Bank.Clients.Data;
using Bank.Clients.Http;
using Bank.Common.Errors;
using FluentResults;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Bank.Clients.Commands.DeleteClient;

public sealed class DeleteClientHandler(BankDbContext db, IDepositsApi depositsApi, ICreditsApi creditsApi)
    : ICommandHandler<DeleteClientCommand, Result>
{
    public async ValueTask<Result> Handle(DeleteClientCommand command, CancellationToken cancellationToken = default)
    {
        var client = await db.Clients.FirstOrDefaultAsync(c => c.Id == command.Id, cancellationToken);
        if (client is null)
        {
            return Result.Fail(new NotFoundError());
        }

        var deposits = await depositsApi.GetContractsByClientAsync(command.Id, cancellationToken);
        if (deposits.IsFailed)
        {
            return deposits.ToResult();
        }

        if (deposits.Value.Count > 0)
        {
            return Result.Fail(new ValidationError("id", "clientHasDeposits"));
        }

        var credits = await creditsApi.GetContractsByClientAsync(command.Id, cancellationToken);
        if (credits.IsFailed)
        {
            return credits.ToResult();
        }

        if (credits.Value.Count > 0)
        {
            return Result.Fail(new ValidationError("id", "clientHasCredits"));
        }

        db.Clients.Remove(client);
        await db.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}
