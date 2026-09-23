using Bank.Common.Dtos.Response;
using FluentResults;
using Mediator;

namespace Bank.Deposits.Queries.GetDepositDictionaries;

public sealed record GetDepositDictionariesQuery : IQuery<Result<DepositDictionariesResponse>>;
