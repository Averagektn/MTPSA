using Bank.Clients.Commands.CreateClient;
using Bank.Clients.Commands.DeleteClient;
using Bank.Clients.Commands.UpdateClient;
using Bank.Common.Cqrs;
using Bank.Common.Dtos.Request;
using Bank.Clients.Queries.GetClient;
using Bank.Clients.Queries.GetClients;
using Bank.Clients.Queries.GetDictionaries;
using Mediator;

namespace Bank.Clients.Endpoints;

public static class ClientEndpoints
{
    public static void MapClientEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/api");

        api.MapGet("/dictionaries", GetDictionaries).WithName("GetDictionaries").WithTags("Dictionaries");
        api.MapGet("/clients", GetClients).WithName("GetClients").WithTags("Clients");
        api.MapGet("/clients/{id:int}", GetClient).WithName("GetClient").WithTags("Clients");
        api.MapPost("/clients", CreateClient).WithName("CreateClient").WithTags("Clients");
        api.MapPut("/clients/{id:int}", UpdateClient).WithName("UpdateClient").WithTags("Clients");
        api.MapDelete("/clients/{id:int}", DeleteClient).WithName("DeleteClient").WithTags("Clients");
    }

    private static async Task<IResult> GetDictionaries(IMediator mediator)
        => (await mediator.Send(new GetDictionariesQuery())).ToHttpResult();

    private static async Task<IResult> GetClients(IMediator mediator)
        => (await mediator.Send(new GetClientsQuery())).ToHttpResult();

    private static async Task<IResult> GetClient(int id, IMediator mediator)
        => (await mediator.Send(new GetClientQuery(id))).ToHttpResult();

    private static async Task<IResult> CreateClient(ClientRequest request, IMediator mediator)
        => (await mediator.Send(new CreateClientCommand(request)))
            .ToCreatedHttpResult(client => $"/api/clients/{client.Id}");

    private static async Task<IResult> UpdateClient(int id, ClientRequest request, IMediator mediator)
        => (await mediator.Send(new UpdateClientCommand(id, request))).ToHttpResult();

    private static async Task<IResult> DeleteClient(int id, IMediator mediator)
        => (await mediator.Send(new DeleteClientCommand(id))).ToHttpResult();
}
