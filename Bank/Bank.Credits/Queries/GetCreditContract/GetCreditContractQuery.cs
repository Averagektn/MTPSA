using Bank.Common.Dtos.Response;
using FluentResults;
using Mediator;

namespace Bank.Credits.Queries.GetCreditContract;

public sealed record GetCreditContractQuery(int Id) : IQuery<Result<CreditContractResponse>>;
