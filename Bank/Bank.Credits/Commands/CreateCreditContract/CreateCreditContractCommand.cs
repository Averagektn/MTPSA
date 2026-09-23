using Bank.Common.Dtos.Request;
using Bank.Common.Dtos.Response;
using FluentResults;
using Mediator;

namespace Bank.Credits.Commands.CreateCreditContract;

public sealed record CreateCreditContractCommand(CreditContractRequest Request)
    : ICommand<Result<CreditContractResponse>>;
