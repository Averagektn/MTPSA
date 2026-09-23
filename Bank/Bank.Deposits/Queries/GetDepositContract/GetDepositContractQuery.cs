using Bank.Common.Dtos.Response;
using FluentResults;
using Mediator;

namespace Bank.Deposits.Queries.GetDepositContract;

public sealed record GetDepositContractQuery(int Id) : IQuery<Result<DepositContractResponse>>;
