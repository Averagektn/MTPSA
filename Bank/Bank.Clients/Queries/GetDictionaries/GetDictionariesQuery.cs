using Bank.Common.Dtos.Response;
using FluentResults;
using Mediator;

namespace Bank.Clients.Queries.GetDictionaries;

public sealed record GetDictionariesQuery : IQuery<Result<DictionariesResponse>>;
