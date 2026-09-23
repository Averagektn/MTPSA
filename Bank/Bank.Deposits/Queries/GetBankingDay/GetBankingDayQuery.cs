using Bank.Common.Dtos.Response;
using FluentResults;
using Mediator;

namespace Bank.Deposits.Queries.GetBankingDay;

public sealed record GetBankingDayQuery : IQuery<Result<BankingDayResponse>>;
