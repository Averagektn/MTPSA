using System.Text.Json;
using Bank.Common.Errors;
using Bank.Deposits.Data;
using Bank.Common.Dtos.Request;
using Bank.Deposits.Http;
using FluentResults;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Bank.Deposits.Validation;

public sealed class DepositWriteValidator(
    DepositsDbContext db,
    IValidator<DepositContractRequest> validator,
    IClientsApi clientsApi)
{
    public async Task<Result> ValidateAsync(DepositContractRequest request, CancellationToken cancellationToken = default)
    {
        request.Normalize();

        var result = await validator.ValidateAsync(request, cancellationToken);
        var errors = new List<IError>();
        foreach (var error in result.Errors)
        {
            var field = JsonNamingPolicy.CamelCase.ConvertName(error.PropertyName);
            errors.Add(new ValidationError(field, error.ErrorMessage));
        }

        if (errors.Count > 0)
        {
            return Result.Fail(errors);
        }

        var product = await db.DepositProducts
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);
        if (product is null)
        {
            errors.Add(new ValidationError("productId", "productNotFound"));
            return Result.Fail(errors);
        }

        var client = await clientsApi.GetByIdAsync(request.ClientId, cancellationToken);
        if (client.IsFailed)
        {
            errors.AddRange(client.Errors);
        }

        if (!await db.Currencies.AnyAsync(c => c.Id == request.CurrencyId, cancellationToken))
        {
            errors.Add(new ValidationError("currencyId", "currencyNotFound"));
        }

        if (request.TermMonths != product.TermMonths)
        {
            errors.Add(new ValidationError("termMonths", "termMismatch"));
        }

        if (request.AnnualRate != product.AnnualRate)
        {
            errors.Add(new ValidationError("annualRate", "rateMismatch"));
        }

        if (request.Amount < product.MinAmount)
        {
            errors.Add(new ValidationError("amount", "amountBelowMin"));
        }

        if (request.Amount > product.MaxAmount)
        {
            errors.Add(new ValidationError("amount", "amountAboveMax"));
        }

        var bankDate = await db.BankStates.Select(s => s.CurrentDate).SingleAsync(cancellationToken);
        if (request.ParsedStartDate < bankDate)
        {
            errors.Add(new ValidationError("startDate", "startDateBeforeBankDay"));
        }

        var expectedEnd = request.ParsedStartDate!.Value.AddMonths(product.TermMonths);
        if (request.ParsedEndDate != expectedEnd)
        {
            errors.Add(new ValidationError("endDate", "endDateMismatch"));
        }

        if (!string.IsNullOrWhiteSpace(request.Number)
            && await db.DepositContracts.AnyAsync(c => c.Number == request.Number, cancellationToken))
        {
            errors.Add(new ValidationError("number", "duplicateContractNumber"));
        }

        return errors.Count > 0 ? Result.Fail(errors) : Result.Ok();
    }
}
