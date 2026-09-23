using Bank.Common.Dtos.Response;
using FluentResults;
using Mediator;

namespace Bank.Credits.Queries.GetCreditDictionaries;

public sealed record GetCreditDictionariesQuery : IQuery<Result<CreditDictionariesResponse>>;
