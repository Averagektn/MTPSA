using Bank.Common.Cqrs;
using Bank.Credits.Commands.AuthorizeAtm;
using Bank.Credits.Commands.CloseBankingDay;
using Bank.Credits.Commands.CreateCreditContract;
using Bank.Credits.Commands.ExecuteAtmTransaction;
using Bank.Common.Dtos.Request;
using Bank.Credits.Queries.GetAccountsReport;
using Bank.Credits.Queries.GetBankingDay;
using Bank.Credits.Queries.GetCreditContract;
using Bank.Credits.Queries.GetCreditContracts;
using Bank.Credits.Queries.GetCreditDictionaries;
using Mediator;

namespace Bank.Credits.Endpoints;

public static class CreditEndpoints
{
    public static void MapCreditEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/api");

        api.MapGet("/credits/dictionaries", GetDictionaries)
            .WithName("GetCreditDictionaries")
            .WithTags("Credits");
        api.MapGet("/credits/contracts", GetContracts)
            .WithName("GetCreditContracts")
            .WithTags("Credits");
        api.MapGet("/credits/contracts/{id:int}", GetContract)
            .WithName("GetCreditContract")
            .WithTags("Credits");
        api.MapPost("/credits/contracts", CreateContract)
            .WithName("CreateCreditContract")
            .WithTags("Credits");
        api.MapGet("/accounts", GetAccounts)
            .WithName("GetAccountsReport")
            .WithTags("Credits");
        api.MapGet("/banking-day", GetBankingDay)
            .WithName("GetBankingDay")
            .WithTags("Credits");
        api.MapPost("/banking-day/close", CloseBankingDay)
            .WithName("CloseBankingDay")
            .WithTags("Credits");
        api.MapPost("/credits/atm/authorize", AuthorizeAtm)
            .WithName("AuthorizeAtm")
            .WithTags("Atm");
        api.MapPost("/credits/atm/transactions", ExecuteAtm)
            .WithName("ExecuteAtmTransaction")
            .WithTags("Atm");
    }

    private static async Task<IResult> GetDictionaries(IMediator mediator)
        => (await mediator.Send(new GetCreditDictionariesQuery())).ToHttpResult();

    private static async Task<IResult> GetContracts(int? clientId, IMediator mediator)
        => (await mediator.Send(new GetCreditContractsQuery(clientId))).ToHttpResult();

    private static async Task<IResult> GetContract(int id, IMediator mediator)
        => (await mediator.Send(new GetCreditContractQuery(id))).ToHttpResult();

    private static async Task<IResult> CreateContract(CreditContractRequest request, IMediator mediator)
        => (await mediator.Send(new CreateCreditContractCommand(request)))
            .ToCreatedHttpResult(contract => $"/api/credits/contracts/{contract.Id}");

    private static async Task<IResult> GetAccounts(IMediator mediator)
        => (await mediator.Send(new GetAccountsReportQuery())).ToHttpResult();

    private static async Task<IResult> GetBankingDay(IMediator mediator)
        => (await mediator.Send(new GetBankingDayQuery())).ToHttpResult();

    private static async Task<IResult> CloseBankingDay(IMediator mediator, int days = 1)
        => (await mediator.Send(new CloseBankingDayCommand(days))).ToHttpResult();

    private static async Task<IResult> AuthorizeAtm(AtmAuthorizeRequest request, IMediator mediator)
        => (await mediator.Send(new AuthorizeAtmCommand(request))).ToHttpResult();

    private static async Task<IResult> ExecuteAtm(AtmTransactionRequest request, IMediator mediator)
        => (await mediator.Send(new ExecuteAtmTransactionCommand(request))).ToHttpResult();
}
