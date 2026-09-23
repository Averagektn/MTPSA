using Bank.Common.Atm;
using Bank.Credits.Accounting;
using Bank.Credits.Commands;
using Bank.Credits.Data;
using Bank.Common.Dtos.Request;
using Bank.Common.Dtos.Response;
using Bank.Common.Errors;
using FluentResults;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Bank.Credits.Commands.ExecuteAtmTransaction;

public sealed record ExecuteAtmTransactionCommand(AtmTransactionRequest Request)
    : ICommand<Result<AtmTransactionResponse>>;

public sealed class ExecuteAtmTransactionHandler(CreditsDbContext db, CreditLedger ledger)
    : ICommandHandler<ExecuteAtmTransactionCommand, Result<AtmTransactionResponse>>
{
    public async ValueTask<Result<AtmTransactionResponse>> Handle(
        ExecuteAtmTransactionCommand command,
        CancellationToken cancellationToken = default)
    {
        var request = command.Request;
        request.Normalize();
        if (request.CardNumber.Length != 16)
        {
            return Result.Fail(new ValidationError("cardNumber", "cardNotFound"));
        }

        if (request.Pin.Length != 4)
        {
            return Result.Fail(new ValidationError("pin", "invalidPin"));
        }

        var loaded = await AtmCardLoader.LoadAsync(db, request.CardNumber, cancellationToken);
        if (loaded.IsFailed)
        {
            return loaded.ToResult<AtmTransactionResponse>();
        }

        var pack = loaded.Value;
        if (pack.Card.Pin != request.Pin)
        {
            return Result.Fail(new ValidationError("pin", "invalidPin"));
        }

        if (!AtmEnum.TryParse<AtmOperation>(request.Operation, out var operation))
        {
            return Result.Fail(new ValidationError("operation", "unknownOperation"));
        }

        return operation switch
        {
            AtmOperation.Balance => Result.Ok(Balance(pack)),
            AtmOperation.Withdraw => await WithdrawAsync(pack, request.Amount, cancellationToken),
            AtmOperation.Payment => await PayAsync(pack, request, cancellationToken),
            _ => Result.Fail(new ValidationError("operation", "unknownOperation"))
        };
    }

    private static AtmTransactionResponse Balance(AtmCardPack pack)
    {
        var receipt = Receipt("receiptBalance", pack, AtmOperation.Balance.ToWire(), null, pack.Balance, null, null);
        return Ok("balanceShown", AtmOperation.Balance.ToWire(), pack, receipt);
    }

    private async Task<Result<AtmTransactionResponse>> WithdrawAsync(
        AtmCardPack pack,
        decimal? amount,
        CancellationToken cancellationToken)
    {
        var parsed = ValidateAmount(amount);
        if (parsed.IsFailed)
        {
            return parsed.ToResult<AtmTransactionResponse>();
        }

        if (pack.Balance < parsed.Value)
        {
            return Result.Fail(new ValidationError("amount", "insufficientFunds"));
        }

        var date = await db.BankStates.Select(s => s.CurrentDate).SingleAsync(cancellationToken);
        await ledger.AtmWithdrawAsync(pack.Card.CardAccount, pack.Card.CreditContract, date, parsed.Value, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        pack = pack with { Balance = pack.Balance - parsed.Value };
        var receipt = Receipt("receiptWithdraw", pack, AtmOperation.Withdraw.ToWire(), parsed.Value, pack.Balance, null, null);
        return Result.Ok(Ok("cashDispensed", AtmOperation.Withdraw.ToWire(), pack, receipt));
    }

    private async Task<Result<AtmTransactionResponse>> PayAsync(
        AtmCardPack pack,
        AtmTransactionRequest request,
        CancellationToken cancellationToken)
    {
        var parsed = ValidateAmount(request.Amount);
        if (parsed.IsFailed)
        {
            return parsed.ToResult<AtmTransactionResponse>();
        }

        if (string.IsNullOrWhiteSpace(request.OperatorCode) || !AtmOperatorCatalog.IsKnown(request.OperatorCode))
        {
            return Result.Fail(new ValidationError("operatorCode", "operatorRequired"));
        }

        if (request.Phone is null || request.Phone.Length != 10)
        {
            return Result.Fail(new ValidationError("phone", "phoneInvalid"));
        }

        if (pack.Balance < parsed.Value)
        {
            return Result.Fail(new ValidationError("amount", "insufficientFunds"));
        }

        var date = await db.BankStates.Select(s => s.CurrentDate).SingleAsync(cancellationToken);
        await ledger.AtmPaymentAsync(pack.Card.CardAccount, pack.Card.CreditContract, date, parsed.Value, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        pack = pack with { Balance = pack.Balance - parsed.Value };
        var receipt = Receipt("receiptPayment", pack, AtmOperation.Payment.ToWire(), parsed.Value, pack.Balance, request.OperatorCode, request.Phone);
        return Result.Ok(Ok("paymentCompleted", AtmOperation.Payment.ToWire(), pack, receipt));
    }

    private static Result<decimal> ValidateAmount(decimal? amount)
    {
        if (amount is null)
        {
            return Result.Fail(new ValidationError("amount", "amountRequired"));
        }

        if (amount <= 0 || amount != decimal.Truncate(amount.Value))
        {
            return Result.Fail(new ValidationError("amount", "amountNotInteger"));
        }

        return Result.Ok(amount.Value);
    }

    private static AtmTransactionResponse Ok(
        string message,
        string operation,
        AtmCardPack pack,
        AtmReceiptDto receipt)
        => new(
            true,
            message,
            operation,
            pack.Balance,
            pack.Card.CreditContract.PrincipalAccount.Number,
            pack.Card.CreditContract.ClientName,
            pack.Card.CreditContract.Currency.Code,
            receipt);

    private static AtmReceiptDto Receipt(
        string title,
        AtmCardPack pack,
        string operation,
        decimal? amount,
        decimal? balance,
        string? operatorCode,
        string? phone)
        => new(
            title,
            DateTimeOffset.UtcNow,
            CardNumberMask.Mask(pack.Card.CardNumber),
            operation,
            amount,
            balance,
            operatorCode,
            phone,
            pack.Card.CreditContract.PrincipalAccount.Number,
            pack.Card.CreditContract.ClientName);
}
