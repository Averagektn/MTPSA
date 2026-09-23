using Bank.Deposits.Accounting;
using Bank.Deposits.Data;
using Bank.Common.Dtos.Response;
using Bank.Deposits.Http;
using Bank.Deposits.Mapping;
using Bank.Deposits.Models;
using Bank.Deposits.Validation;
using FluentResults;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Bank.Deposits.Commands.CreateDepositContract;

public sealed class CreateDepositContractHandler(
    DepositsDbContext db,
    DepositWriteValidator writeValidator,
    DepositLedger ledger,
    IClientsApi clientsApi)
    : ICommandHandler<CreateDepositContractCommand, Result<DepositContractResponse>>
{
    public async ValueTask<Result<DepositContractResponse>> Handle(
        CreateDepositContractCommand command,
        CancellationToken cancellationToken = default)
    {
        var validation = await writeValidator.ValidateAsync(command.Request, cancellationToken);
        if (validation.IsFailed)
        {
            return validation.ToResult<DepositContractResponse>();
        }

        var request = command.Request;
        var client = await clientsApi.GetByIdAsync(request.ClientId, cancellationToken);
        if (client.IsFailed)
        {
            return client.ToResult<DepositContractResponse>();
        }

        var product = await db.DepositProducts
            .Include(p => p.PrincipalChartAccount)
            .Include(p => p.InterestChartAccount)
            .FirstAsync(p => p.Id == request.ProductId, cancellationToken);
        var bankDate = await db.BankStates.Select(s => s.CurrentDate).SingleAsync(cancellationToken);
        var sequence = await db.BankAccounts.CountAsync(a => a.ClientId == client.Value.Id, cancellationToken);
        var clientCode = AccountNumberGenerator.ClientCode(client.Value.Id);
        var fullName = DepositContractMapper.ClientName(client.Value);

        var principal = new BankAccount
        {
            Number = AccountNumberGenerator.Build(product.PrincipalChartAccount.Code, clientCode, sequence + 1),
            ChartAccountId = product.PrincipalChartAccountId,
            ChartAccount = product.PrincipalChartAccount,
            ClientId = client.Value.Id,
            NameEn = fullName,
            NameRu = fullName,
            CurrencyId = request.CurrencyId
        };
        var interest = new BankAccount
        {
            Number = AccountNumberGenerator.Build(product.InterestChartAccount.Code, clientCode, sequence + 2),
            ChartAccountId = product.InterestChartAccountId,
            ChartAccount = product.InterestChartAccount,
            ClientId = client.Value.Id,
            NameEn = fullName,
            NameRu = fullName,
            CurrencyId = request.CurrencyId
        };
        db.BankAccounts.AddRange(principal, interest);

        var contract = new DepositContract
        {
            Number = string.IsNullOrWhiteSpace(request.Number)
                ? await NextContractNumberAsync(bankDate.Year, cancellationToken)
                : request.Number,
            ClientId = client.Value.Id,
            ClientName = fullName,
            ProductId = product.Id,
            Product = product,
            CurrencyId = request.CurrencyId,
            StartDate = request.ParsedStartDate!.Value,
            EndDate = request.ParsedEndDate!.Value,
            TermMonths = request.TermMonths,
            Amount = request.Amount,
            AnnualRate = request.AnnualRate,
            Status = DepositContractStatus.Active,
            PrincipalAccount = principal,
            InterestAccount = interest
        };
        db.DepositContracts.Add(contract);
        await ledger.OpenDepositAsync(contract, bankDate, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        var created = await db.DepositContracts
            .AsNoTracking()
            .Include(c => c.Product)
            .Include(c => c.Currency)
            .Include(c => c.PrincipalAccount)
            .Include(c => c.InterestAccount)
            .FirstAsync(c => c.Id == contract.Id, cancellationToken);

        return Result.Ok(DepositContractMapper.ToResponse(created));
    }

    private async Task<string> NextContractNumberAsync(int year, CancellationToken cancellationToken)
    {
        var prefix = $"Д-{year}-";
        var numbers = await db.DepositContracts
            .Where(c => c.Number.StartsWith(prefix))
            .Select(c => c.Number)
            .ToListAsync(cancellationToken);
        var max = 0;
        foreach (var number in numbers)
        {
            var parts = number.Split('-');
            if (parts.Length == 3 && int.TryParse(parts[2], out var value) && value > max)
            {
                max = value;
            }
        }

        return $"{prefix}{max + 1:D4}";
    }
}
