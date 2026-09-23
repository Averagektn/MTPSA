using Bank.Common.Dtos.Response;
using Bank.Credits.Http;
using FluentResults;

namespace Tests.Credits.Support;

public sealed class FakeDepositsApi : IDepositsApi
{
    public List<AccountReportItem> Accounts { get; } = [];
    public bool Unavailable { get; set; }

    public static FakeDepositsApi WithClientDepositAccounts()
    {
        var api = new FakeDepositsApi();
        api.Accounts.AddRange(
        [
            new("3404100020010", "Client2 Test X", "3404", "Demand deposits", "P", 1000m, 1000m, 0m),
            new("3470100020027", "Client2 Test X", "3470", "Demand interest", "P", 4.52m, 4.52m, 0m),
            new("3414100030016", "Client3 Test X", "3414", "Term deposits", "P", 5000m, 5000m, 0m),
            new("3471100030023", "Client3 Test X", "3471", "Term interest", "P", 0m, 0m, 0m)
        ]);
        return api;
    }

    public Task<Result<IReadOnlyList<AccountReportItem>>> GetAccountsAsync(
        CancellationToken cancellationToken = default)
    {
        if (Unavailable)
        {
            return Task.FromResult(Result.Fail<IReadOnlyList<AccountReportItem>>(
                new Bank.Common.Errors.ValidationError("accounts", "depositsUnavailable")));
        }

        return Task.FromResult(Result.Ok<IReadOnlyList<AccountReportItem>>(Accounts));
    }
}
