using Bank.Common.Dtos.Request;
using Bank.Common.Dtos.Response;
using FluentResults;
using Mediator;

namespace Bank.Clients.Commands.CreateClient;

public sealed record CreateClientCommand(ClientRequest Request) : ICommand<Result<ClientResponse>>;
