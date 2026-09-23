using Bank.Credits.Commands;
using Bank.Credits.Data;
using Bank.Common.Dtos.Request;
using Bank.Common.Dtos.Response;
using Bank.Common.Errors;
using FluentResults;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Bank.Credits.Commands.AuthorizeAtm;

public sealed record AuthorizeAtmCommand(AtmAuthorizeRequest Request) : ICommand<Result<AtmAuthorizeResponse>>;

public sealed class AuthorizeAtmHandler(CreditsDbContext db)
    : ICommandHandler<AuthorizeAtmCommand, Result<AtmAuthorizeResponse>>
{
    public async ValueTask<Result<AtmAuthorizeResponse>> Handle(
        AuthorizeAtmCommand command,
        CancellationToken cancellationToken = default)
    {
        var request = command.Request;
        request.CardNumber = new string(request.CardNumber.Where(char.IsDigit).ToArray());
        request.Pin = new string(request.Pin.Where(char.IsDigit).ToArray());

        var loaded = await AtmCardLoader.LoadAsync(db, request.CardNumber, cancellationToken);
        if (loaded.IsFailed)
        {
            return loaded.ToResult<AtmAuthorizeResponse>();
        }

        if (loaded.Value.Card.Pin != request.Pin)
        {
            return Result.Fail(new ValidationError("pin", "invalidPin"));
        }

        return Result.Ok(AtmCardLoader.ToAuthorize(loaded.Value));
    }
}
