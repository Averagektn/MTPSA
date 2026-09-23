using Bank.Common.Dtos.Response;
using FluentResults;
using Mediator;

namespace Bank.Clients.Queries.GetClient;

public sealed record GetClientQuery(int Id) : IQuery<Result<ClientResponse>>;
