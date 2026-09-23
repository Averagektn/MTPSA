using Bank.Common.Dtos.Request;
using Bank.Common.Dtos.Response;
using FluentResults;
using Mediator;

namespace Bank.Clients.Commands.UpdateClient;

public sealed record UpdateClientCommand(int Id, ClientRequest Request) : ICommand<Result<ClientResponse>>;
