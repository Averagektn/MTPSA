using Bank.Common.Dtos.Response;
using FluentResults;
using Mediator;

namespace Bank.Credits.Queries.GetCreditContracts;

public sealed record GetCreditContractsQuery(int? ClientId) : IQuery<Result<IReadOnlyList<CreditContractListItem>>>;
