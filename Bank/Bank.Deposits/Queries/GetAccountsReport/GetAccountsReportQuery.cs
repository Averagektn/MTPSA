using Bank.Common.Dtos.Response;
using FluentResults;
using Mediator;

namespace Bank.Deposits.Queries.GetAccountsReport;

public sealed record GetAccountsReportQuery : IQuery<Result<IReadOnlyList<AccountReportItem>>>;
