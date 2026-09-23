using Bank.Common.Dtos.Response;
using FluentResults;
using Mediator;

namespace Bank.Credits.Queries.GetBankingDay;

public sealed record GetBankingDayQuery : IQuery<Result<BankingDayResponse>>;
