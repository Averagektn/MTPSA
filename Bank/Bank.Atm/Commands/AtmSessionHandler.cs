using System.Text.Json;
using Bank.Atm.Data;
using Bank.Atm.Http;
using Bank.Atm.Models;
using Bank.Atm.Session;
using Bank.Common.Atm;
using Bank.Common.Dtos.Request;
using Bank.Common.Dtos.Response;
using Bank.Common.Errors;
using FluentResults;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Bank.Atm.Commands;

public sealed record StartAtmSessionCommand(string CardNumber) : ICommand<Result<AtmSessionResponse>>;

public sealed record SubmitAtmInputCommand(Guid SessionId, string Kind, string Value)
    : ICommand<Result<AtmSessionResponse>>;

public sealed record GetAtmSessionQuery(Guid SessionId) : IQuery<Result<AtmSessionResponse>>;

public sealed class AtmSessionHandler(AtmDbContext db, ICreditsApi credits, IDepositsApi deposits)
    : ICommandHandler<StartAtmSessionCommand, Result<AtmSessionResponse>>,
      ICommandHandler<SubmitAtmInputCommand, Result<AtmSessionResponse>>,
      IQueryHandler<GetAtmSessionQuery, Result<AtmSessionResponse>>
{
    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public async ValueTask<Result<AtmSessionResponse>> Handle(
        StartAtmSessionCommand command,
        CancellationToken cancellationToken = default)
    {
        var card = CardNumberMask.Digits(command.CardNumber);
        if (card.Length != 16)
        {
            return Result.Fail(new ValidationError("cardNumber", "cardNumberInvalid"));
        }

        var session = new AtmSession
        {
            Id = Guid.NewGuid(),
            Screen = AtmScreen.Pin,
            CardNumber = card,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        var buffer = new TransactionBuffer();
        buffer.Set("cardNumber", card);
        session.FieldsJson = buffer.ToJson();
        db.Sessions.Add(session);
        await db.SaveChangesAsync(cancellationToken);
        return Result.Ok(Map(session, buffer));
    }

    public async ValueTask<Result<AtmSessionResponse>> Handle(
        GetAtmSessionQuery query,
        CancellationToken cancellationToken = default)
    {
        var session = await LoadAsync(query.SessionId, cancellationToken);
        return session is null
            ? Result.Fail(new NotFoundError())
            : Result.Ok(Map(session));
    }

    public async ValueTask<Result<AtmSessionResponse>> Handle(
        SubmitAtmInputCommand command,
        CancellationToken cancellationToken = default)
    {
        var session = await LoadAsync(command.SessionId, cancellationToken);
        if (session is null)
        {
            return Result.Fail(new NotFoundError());
        }

        if (!AtmEnum.TryParse<AtmInputKind>(command.Kind, out var kind))
        {
            return Result.Fail(new ValidationError("kind", "unknownOperation"));
        }

        if (session.Screen == AtmScreen.Locked && kind != AtmInputKind.Eject)
        {
            return Result.Ok(Map(session));
        }

        var buffer = TransactionBuffer.Parse(session.FieldsJson);
        var result = kind switch
        {
            AtmInputKind.Pin => await SubmitPinAsync(session, buffer, command.Value, cancellationToken),
            AtmInputKind.Menu => SelectMenu(session, buffer, command.Value),
            AtmInputKind.Amount => SubmitAmount(session, buffer, command.Value),
            AtmInputKind.Operator => SelectOperator(session, buffer, command.Value),
            AtmInputKind.Phone => SubmitPhone(session, buffer, command.Value),
            AtmInputKind.Confirm => ConfirmPayment(session, buffer, command.Value),
            AtmInputKind.Receipt => await ChooseReceiptAsync(session, buffer, command.Value, cancellationToken),
            AtmInputKind.Continue => Continue(session, buffer),
            AtmInputKind.Eject => Eject(session),
            _ => Result.Fail(new ValidationError("kind", "unknownOperation"))
        };

        if (result.IsFailed)
        {
            return result;
        }

        if (session.Screen != AtmScreen.Card)
        {
            session.FieldsJson = buffer.ToJson();
        }

        session.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return Result.Ok(Map(session, buffer));
    }

    private async Task<Result> SubmitPinAsync(
        AtmSession session,
        TransactionBuffer buffer,
        string pin,
        CancellationToken cancellationToken)
    {
        if (session.Screen != AtmScreen.Pin)
        {
            return Result.Fail(new ValidationError("kind", "unknownOperation"));
        }

        var digits = CardNumberMask.Digits(pin);
        if (digits.Length != 4)
        {
            session.MessageKey = "invalidPin";
            return Result.Ok();
        }

        buffer.Set("pin", digits);
        var auth = await credits.AuthorizeAsync(
            new AtmAuthorizeRequest { CardNumber = session.CardNumber ?? "", Pin = digits },
            cancellationToken);
        if (auth.IsFailed)
        {
            session.PinAttempts++;
            session.MessageKey = FirstKey(auth) ?? "invalidPin";
            if (session.PinAttempts >= PinPolicy.MaxAttempts)
            {
                session.Screen = AtmScreen.Locked;
                session.Authorized = false;
                session.MessageKey = "cardLocked";
            }

            return Result.Ok();
        }

        session.Pin = digits;
        session.PinAttempts = 0;
        session.Authorized = true;
        session.AccountJson = JsonSerializer.Serialize(auth.Value, Json);
        session.MessageKey = null;
        session.LastResultJson = null;
        session.ReceiptJson = null;
        session.Screen = AtmScreen.Menu;
        return Result.Ok();
    }

    private static Result SelectMenu(
        AtmSession session,
        TransactionBuffer buffer,
        string operation)
    {
        if (session.Screen != AtmScreen.Menu || !session.Authorized)
        {
            return Result.Fail(new ValidationError("kind", "unknownOperation"));
        }

        if (!AtmEnum.TryParse<AtmOperation>(operation, out var op))
        {
            return Result.Fail(new ValidationError("operation", "unknownOperation"));
        }

        if (op == AtmOperation.Eject)
        {
            return Eject(session);
        }

        buffer.Keep("cardNumber", "pin");
        buffer.Set("operation", op.ToWire());
        session.MessageKey = null;
        session.LastResultJson = null;
        session.ReceiptJson = null;

        return op switch
        {
            AtmOperation.Withdraw => SetScreen(session, AtmScreen.WithdrawAmount),
            AtmOperation.Balance => SetScreen(session, AtmScreen.ReceiptChoice),
            AtmOperation.DepositBalance => SetScreen(session, AtmScreen.ReceiptChoice),
            AtmOperation.Payment => SetScreen(session, AtmScreen.PaymentOperator),
            _ => Result.Fail(new ValidationError("operation", "unknownOperation"))
        };
    }

    private static Result SubmitAmount(
        AtmSession session,
        TransactionBuffer buffer,
        string value)
    {
        if (session.Screen is not (AtmScreen.WithdrawAmount or AtmScreen.PaymentAmount))
        {
            return Result.Fail(new ValidationError("kind", "unknownOperation"));
        }

        if (!AmountParser.TryParseNonNegativeInteger(value, out var amount) || amount <= 0)
        {
            session.MessageKey = "amountNotInteger";
            return Result.Ok();
        }

        buffer.Set("amount", amount.ToString());
        session.MessageKey = null;
        session.Screen = session.Screen == AtmScreen.PaymentAmount
            ? AtmScreen.PaymentConfirm
            : AtmScreen.ReceiptChoice;
        return Result.Ok();
    }

    private static Result SelectOperator(AtmSession session, TransactionBuffer buffer, string code)
    {
        if (session.Screen != AtmScreen.PaymentOperator)
        {
            return Result.Fail(new ValidationError("kind", "unknownOperation"));
        }

        var normalized = code.Trim().ToUpperInvariant();
        if (!AtmOperatorCatalog.IsKnown(normalized))
        {
            session.MessageKey = "operatorRequired";
            return Result.Ok();
        }

        buffer.Set("operatorCode", normalized);
        session.MessageKey = null;
        session.Screen = AtmScreen.PaymentPhone;
        return Result.Ok();
    }

    private static Result SubmitPhone(AtmSession session, TransactionBuffer buffer, string phone)
    {
        if (session.Screen != AtmScreen.PaymentPhone)
        {
            return Result.Fail(new ValidationError("kind", "unknownOperation"));
        }

        var digits = CardNumberMask.Digits(phone);
        if (digits.Length != 10)
        {
            session.MessageKey = "phoneInvalid";
            return Result.Ok();
        }

        buffer.Set("phone", digits);
        session.MessageKey = null;
        session.Screen = AtmScreen.PaymentAmount;
        return Result.Ok();
    }

    private static Result ConfirmPayment(
        AtmSession session,
        TransactionBuffer buffer,
        string value)
    {
        if (session.Screen != AtmScreen.PaymentConfirm)
        {
            return Result.Fail(new ValidationError("kind", "unknownOperation"));
        }

        if (!AtmEnum.TryParse<AtmChoice>(value, out var choice))
        {
            return Result.Fail(new ValidationError("kind", "unknownOperation"));
        }

        if (choice == AtmChoice.No)
        {
            buffer.Keep("cardNumber", "pin", "operation");
            session.MessageKey = null;
            session.Screen = AtmScreen.PaymentOperator;
            return Result.Ok();
        }

        session.MessageKey = null;
        session.Screen = AtmScreen.ReceiptChoice;
        return Result.Ok();
    }

    private async Task<Result> ChooseReceiptAsync(
        AtmSession session,
        TransactionBuffer buffer,
        string value,
        CancellationToken cancellationToken)
    {
        if (session.Screen != AtmScreen.ReceiptChoice)
        {
            return Result.Fail(new ValidationError("kind", "unknownOperation"));
        }

        if (!AtmEnum.TryParse<AtmChoice>(value, out var choice))
        {
            return Result.Fail(new ValidationError("kind", "unknownOperation"));
        }

        if (!AtmEnum.TryParse<AtmOperation>(buffer.Get("operation"), out var operation))
        {
            return Result.Fail(new ValidationError("operation", "unknownOperation"));
        }

        var executed = operation == AtmOperation.DepositBalance
            ? await SubmitDepositBalanceAsync(session, buffer, cancellationToken)
            : await SubmitToBankAsync(session, buffer, operation, cancellationToken);
        if (executed.IsFailed)
        {
            return executed;
        }

        var result = ReadResult(session);
        if (choice == AtmChoice.Yes && result?.Success == true && result.Receipt is not null)
        {
            await PrintAsync(session, result.Receipt, cancellationToken);
        }
        else
        {
            session.ReceiptJson = null;
        }

        session.Screen = AtmScreen.Result;
        if (result?.MessageKey is not null)
        {
            session.MessageKey = result.MessageKey;
        }

        return Result.Ok();
    }

    private static Result Continue(AtmSession session, TransactionBuffer buffer)
    {
        if (session.Screen != AtmScreen.Result || !session.Authorized)
        {
            return Result.Fail(new ValidationError("kind", "unknownOperation"));
        }

        buffer.Keep("cardNumber", "pin");
        session.LastResultJson = null;
        session.ReceiptJson = null;
        session.MessageKey = null;
        session.Screen = AtmScreen.Menu;
        return Result.Ok();
    }

    private static Result Eject(AtmSession session)
    {
        session.Screen = AtmScreen.Card;
        session.Authorized = false;
        session.Pin = null;
        session.CardNumber = null;
        session.AccountJson = null;
        session.LastResultJson = null;
        session.ReceiptJson = null;
        session.MessageKey = "cardReturned";
        session.FieldsJson = "[]";
        return Result.Ok();
    }

    private async Task<Result> SubmitToBankAsync(
        AtmSession session,
        TransactionBuffer buffer,
        AtmOperation operation,
        CancellationToken cancellationToken)
    {
        var executed = await credits.ExecuteAsync(buffer.ToRequest(operation), cancellationToken);
        if (executed.IsFailed)
        {
            session.MessageKey = FirstKey(executed) ?? "requestFailed";
            session.LastResultJson = JsonSerializer.Serialize(
                new AtmTransactionResponse(false, session.MessageKey, operation.ToWire(), null, null, null, null, null),
                Json);
            session.Screen = AtmScreen.Result;
            return Result.Ok();
        }

        session.LastResultJson = JsonSerializer.Serialize(executed.Value, Json);
        session.AccountJson = UpdateAccount(session.AccountJson, executed.Value);
        session.MessageKey = executed.Value.MessageKey;
        return Result.Ok();
    }

    private async Task<Result> SubmitDepositBalanceAsync(
        AtmSession session,
        TransactionBuffer buffer,
        CancellationToken cancellationToken)
    {
        var account = ReadAccount(session);
        if (account is null)
        {
            session.MessageKey = "requestFailed";
            session.Screen = AtmScreen.Result;
            return Result.Ok();
        }

        var depositsResult = await deposits.GetBalancesAsync(account.ClientId, cancellationToken);
        if (depositsResult.IsFailed)
        {
            session.MessageKey = FirstKey(depositsResult) ?? "depositsUnavailable";
            session.LastResultJson = JsonSerializer.Serialize(
                new AtmTransactionResponse(false, session.MessageKey, AtmOperation.DepositBalance.ToWire(), null, null, account.ClientName, null, null),
                Json);
            session.Screen = AtmScreen.Result;
            return Result.Ok();
        }

        if (depositsResult.Value.Items.Count == 0)
        {
            session.MessageKey = "noDepositAccount";
            session.LastResultJson = JsonSerializer.Serialize(
                new AtmTransactionResponse(false, "noDepositAccount", AtmOperation.DepositBalance.ToWire(), null, null, account.ClientName, account.CurrencyCode, null, depositsResult.Value.Items),
                Json);
            session.Screen = AtmScreen.Result;
            return Result.Ok();
        }

        var first = depositsResult.Value.Items[0];
        var receipt = new AtmReceiptDto(
            "receiptDepositBalance",
            DateTimeOffset.UtcNow,
            account.CardNumberMasked,
            AtmOperation.DepositBalance.ToWire(),
            first.Amount,
            first.Amount,
            null,
            null,
            null,
            account.ClientName,
            first.AccountNumber);
        var response = new AtmTransactionResponse(
            true,
            "depositBalanceShown",
            AtmOperation.DepositBalance.ToWire(),
            first.Amount,
            first.AccountNumber,
            account.ClientName,
            first.CurrencyCode,
            receipt,
            depositsResult.Value.Items);
        session.LastResultJson = JsonSerializer.Serialize(response, Json);
        session.MessageKey = response.MessageKey;
        _ = buffer;
        return Result.Ok();
    }

    private async Task PrintAsync(AtmSession session, AtmReceiptDto receipt, CancellationToken cancellationToken)
    {
        db.Receipts.Add(new AtmReceipt
        {
            SessionId = session.Id,
            PrintedAt = receipt.PrintedAt,
            BodyJson = JsonSerializer.Serialize(receipt, Json)
        });
        await db.SaveChangesAsync(cancellationToken);
        session.ReceiptJson = JsonSerializer.Serialize(receipt, Json);
    }

    private async Task<AtmSession?> LoadAsync(Guid id, CancellationToken cancellationToken)
        => await db.Sessions.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    private static Result SetScreen(AtmSession session, AtmScreen screen)
    {
        session.Screen = screen;
        return Result.Ok();
    }

    private static string? FirstKey(ResultBase result)
        => result.Errors.OfType<ValidationError>().FirstOrDefault()?.Message
           ?? result.Errors.FirstOrDefault()?.Message;

    private static AtmAuthorizeResponse? ReadAccount(AtmSession session)
        => string.IsNullOrWhiteSpace(session.AccountJson)
            ? null
            : JsonSerializer.Deserialize<AtmAuthorizeResponse>(session.AccountJson, Json);

    private static AtmTransactionResponse? ReadResult(AtmSession session)
        => string.IsNullOrWhiteSpace(session.LastResultJson)
            ? null
            : JsonSerializer.Deserialize<AtmTransactionResponse>(session.LastResultJson, Json);

    private static AtmReceiptDto? ReadReceipt(AtmSession session)
        => string.IsNullOrWhiteSpace(session.ReceiptJson)
            ? null
            : JsonSerializer.Deserialize<AtmReceiptDto>(session.ReceiptJson, Json);

    private static string UpdateAccount(string? json, AtmTransactionResponse result)
    {
        if (string.IsNullOrWhiteSpace(json) || result.AvailableBalance is null)
        {
            return json ?? "";
        }

        var account = JsonSerializer.Deserialize<AtmAuthorizeResponse>(json, Json);
        if (account is null)
        {
            return json;
        }

        var updated = account with { AvailableBalance = result.AvailableBalance.Value };
        return JsonSerializer.Serialize(updated, Json);
    }

    private static AtmSessionResponse Map(AtmSession session, TransactionBuffer? buffer = null)
    {
        buffer ??= TransactionBuffer.Parse(session.FieldsJson);
        return new AtmSessionResponse(
            session.Id,
            session.Screen.ToWire(),
            session.CardNumber is null ? null : CardNumberMask.Mask(session.CardNumber),
            PinPolicy.AttemptsLeft(session.PinAttempts),
            session.MessageKey,
            buffer.Visible(),
            ReadAccount(session),
            ReadResult(session),
            ReadReceipt(session),
            AtmOperatorCatalog.All);
    }
}
