using Bank.Common.Dtos.Response;
using FluentResults;
using Mediator;

namespace Bank.Deposits.Queries.GetDepositContracts;

public sealed record GetDepositContractsQuery(int? ClientId) : IQuery<Result<IReadOnlyList<DepositContractListItem>>>;
