using Bank.Common.Dtos.Request;
using Bank.Common.Dtos.Response;
using FluentResults;
using Mediator;

namespace Bank.Deposits.Commands.CreateDepositContract;

public sealed record CreateDepositContractCommand(DepositContractRequest Request)
    : ICommand<Result<DepositContractResponse>>;
