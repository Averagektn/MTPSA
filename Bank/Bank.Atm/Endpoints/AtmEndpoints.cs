using Bank.Common.Cqrs;
using Bank.Atm.Commands;
using Bank.Common.Dtos.Request;
using Mediator;

namespace Bank.Atm.Endpoints;

public static class AtmEndpoints
{
    public static void MapAtmEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/api/atm");
        api.MapPost("/sessions", Start)
            .WithName("StartAtmSession")
            .WithTags("Atm");
        api.MapGet("/sessions/{id:guid}", Get)
            .WithName("GetAtmSession")
            .WithTags("Atm");
        api.MapPost("/sessions/{id:guid}/input", Input)
            .WithName("SubmitAtmInput")
            .WithTags("Atm");
    }

    private static async Task<IResult> Start(AtmInsertCardRequest request, IMediator mediator)
        => (await mediator.Send(new StartAtmSessionCommand(request.CardNumber))).ToHttpResult();

    private static async Task<IResult> Get(Guid id, IMediator mediator)
        => (await mediator.Send(new GetAtmSessionQuery(id))).ToHttpResult();

    private static async Task<IResult> Input(Guid id, AtmInputRequest request, IMediator mediator)
        => (await mediator.Send(new SubmitAtmInputCommand(id, request.Kind, request.Value))).ToHttpResult();
}
