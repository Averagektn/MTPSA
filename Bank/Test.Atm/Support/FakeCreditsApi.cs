using Bank.Atm.Http;
using Bank.Common.Atm;
using Bank.Common.Dtos.Request;
using Bank.Common.Dtos.Response;
using Bank.Common.Errors;
using FluentResults;

namespace Tests.Atm.Support;

public sealed class FakeCreditsApi : ICreditsApi
{
    public decimal Balance { get; set; } = 1500m;
    public string Pin { get; set; } = AtmSamples.Pin;
    public string CardNumber { get; set; } = AtmSamples.Card;
    public List<AtmTransactionRequest> Executed { get; } = [];

    public Task<Result<AtmAuthorizeResponse>> AuthorizeAsync(
        AtmAuthorizeRequest request,
        CancellationToken cancellationToken = default)
    {
        var card = CardNumberMask.Digits(request.CardNumber);
        if (card != CardNumber)
        {
            return Task.FromResult(Result.Fail<AtmAuthorizeResponse>(new ValidationError("cardNumber", "cardNotFound")));
        }

        if (CardNumberMask.Digits(request.Pin) != Pin)
        {
            return Task.FromResult(Result.Fail<AtmAuthorizeResponse>(new ValidationError("pin", "invalidPin")));
        }

        return Task.FromResult(Result.Ok(Account()));
    }

    public Task<Result<AtmTransactionResponse>> ExecuteAsync(
        AtmTransactionRequest request,
        CancellationToken cancellationToken = default)
    {
        Executed.Add(request);
        request.Normalize();
        if (CardNumberMask.Digits(request.Pin) != Pin)
        {
            return Task.FromResult(Result.Fail<AtmTransactionResponse>(new ValidationError("pin", "invalidPin")));
        }

        if (!AtmEnum.TryParse<AtmOperation>(request.Operation, out var operation)
            || operation is not (AtmOperation.Withdraw or AtmOperation.Payment or AtmOperation.Balance))
        {
            return Task.FromResult(Result.Fail<AtmTransactionResponse>(new ValidationError("operation", "unknownOperation")));
        }

        if (operation is AtmOperation.Withdraw or AtmOperation.Payment)
        {
            if (request.Amount is null or <= 0)
            {
                return Task.FromResult(Result.Fail<AtmTransactionResponse>(new ValidationError("amount", "amountNotInteger")));
            }

            if (request.Amount > Balance)
            {
                return Task.FromResult(Result.Fail<AtmTransactionResponse>(new ValidationError("amount", "insufficientFunds")));
            }

            Balance -= request.Amount.Value;
        }

        var receipt = new AtmReceiptDto(
            operation switch
            {
                AtmOperation.Payment => "receiptPayment",
                AtmOperation.Withdraw => "receiptWithdraw",
                _ => "receiptBalance"
            },
            DateTimeOffset.UtcNow,
            CardNumberMask.Mask(CardNumber),
            operation.ToWire(),
            request.Amount,
            Balance,
            request.OperatorCode,
            request.Phone,
            "2410100040010",
            "Client4 Test X");

        return Task.FromResult(Result.Ok(new AtmTransactionResponse(
            true,
            operation switch
            {
                AtmOperation.Payment => "paymentCompleted",
                AtmOperation.Withdraw => "cashDispensed",
                _ => "balanceShown"
            },
            operation.ToWire(),
            Balance,
            "2410100040010",
            "Client4 Test X",
            "BYN",
            receipt)));
    }

    private AtmAuthorizeResponse Account()
        => new(CardNumber, CardNumberMask.Mask(CardNumber), "Client4 Test X", 4, 1, "К-2026-0001", "2410100040010", Balance, "BYN");
}
