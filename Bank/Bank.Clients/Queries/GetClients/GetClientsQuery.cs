using Bank.Common.Dtos.Response;
using FluentResults;
using Mediator;

namespace Bank.Clients.Queries.GetClients;

public sealed record GetClientsQuery : IQuery<Result<IReadOnlyList<ClientListItem>>>;
