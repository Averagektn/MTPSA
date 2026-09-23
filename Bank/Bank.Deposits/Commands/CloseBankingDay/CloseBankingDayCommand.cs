using Bank.Common.Dtos.Response;
using FluentResults;
using Mediator;

namespace Bank.Deposits.Commands.CloseBankingDay;

public sealed record CloseBankingDayCommand(int Days = 1) : ICommand<Result<BankingDayResponse>>;
