using Bank.Common.Cqrs;
using Bank.Deposits.Commands.CloseBankingDay;
using Bank.Deposits.Commands.CreateDepositContract;
using Bank.Common.Dtos.Request;
using Bank.Deposits.Queries.GetAccountsReport;
using Bank.Deposits.Queries.GetAtmDepositBalances;
using Bank.Deposits.Queries.GetBankingDay;
using Bank.Deposits.Queries.GetDepositContract;
using Bank.Deposits.Queries.GetDepositContracts;
using Bank.Deposits.Queries.GetDepositDictionaries;
using Mediator;

namespace Bank.Deposits.Endpoints;

public static class DepositEndpoints
{
    public static void MapDepositEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/api");

        api.MapGet("/deposits/dictionaries", GetDictionaries)
            .WithName("GetDepositDictionaries")
            .WithTags("Deposits");
        api.MapGet("/deposits/contracts", GetContracts)
            .WithName("GetDepositContracts")
            .WithTags("Deposits");
        api.MapGet("/deposits/contracts/{id:int}", GetContract)
            .WithName("GetDepositContract")
            .WithTags("Deposits");
        api.MapPost("/deposits/contracts", CreateContract)
            .WithName("CreateDepositContract")
            .WithTags("Deposits");
        api.MapGet("/accounts", GetAccounts)
            .WithName("GetAccountsReport")
            .WithTags("Deposits");
        api.MapGet("/banking-day", GetBankingDay)
            .WithName("GetBankingDay")
            .WithTags("Deposits");
        api.MapPost("/banking-day/close", CloseBankingDay)
            .WithName("CloseBankingDay")
            .WithTags("Deposits");
        api.MapGet("/deposits/atm/balances", GetAtmBalances)
            .WithName("GetAtmDepositBalances")
            .WithTags("Atm");
    }

    private static async Task<IResult> GetDictionaries(IMediator mediator)
        => (await mediator.Send(new GetDepositDictionariesQuery())).ToHttpResult();

    private static async Task<IResult> GetContracts(int? clientId, IMediator mediator)
        => (await mediator.Send(new GetDepositContractsQuery(clientId))).ToHttpResult();

    private static async Task<IResult> GetContract(int id, IMediator mediator)
        => (await mediator.Send(new GetDepositContractQuery(id))).ToHttpResult();

    private static async Task<IResult> CreateContract(DepositContractRequest request, IMediator mediator)
        => (await mediator.Send(new CreateDepositContractCommand(request)))
            .ToCreatedHttpResult(contract => $"/api/deposits/contracts/{contract.Id}");

    private static async Task<IResult> GetAccounts(IMediator mediator)
        => (await mediator.Send(new GetAccountsReportQuery())).ToHttpResult();

    private static async Task<IResult> GetBankingDay(IMediator mediator)
        => (await mediator.Send(new GetBankingDayQuery())).ToHttpResult();

    private static async Task<IResult> CloseBankingDay(IMediator mediator, int days = 1)
        => (await mediator.Send(new CloseBankingDayCommand(days))).ToHttpResult();

    private static async Task<IResult> GetAtmBalances(int clientId, IMediator mediator)
        => (await mediator.Send(new GetAtmDepositBalancesQuery(clientId))).ToHttpResult();
}
