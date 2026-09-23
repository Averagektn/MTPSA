using Bank.Common.Errors;
using FluentResults;
using Microsoft.AspNetCore.Http;
using HttpResult = Microsoft.AspNetCore.Http.IResult;

namespace Bank.Common.Cqrs;

public static class ResultHttpExtensions
{
    public static HttpResult ToHttpResult(this Result result)
        => result.IsSuccess ? Results.NoContent() : ToFailedHttpResult(result);

    public static HttpResult ToHttpResult<T>(this Result<T> result)
        => result.IsSuccess ? Results.Ok(result.Value) : ToFailedHttpResult(result);

    public static HttpResult ToCreatedHttpResult<T>(this Result<T> result, Func<T, string> location)
        => result.IsSuccess
            ? Results.Created(location(result.Value), result.Value)
            : ToFailedHttpResult(result);

    private static HttpResult ToFailedHttpResult(ResultBase result)
    {
        if (result.HasError<NotFoundError>())
        {
            return Results.NotFound();
        }

        if (result.HasError<ValidationError>())
        {
            return Results.ValidationProblem(ToFieldErrors(result));
        }

        var message = result.Errors.Count > 0 ? result.Errors[0].Message : "requestFailed";
        return Results.Problem(detail: message);
    }

    private static Dictionary<string, string[]> ToFieldErrors(ResultBase result)
    {
        var errors = new Dictionary<string, string[]>();
        foreach (var error in result.Errors.OfType<ValidationError>())
        {
            if (errors.TryGetValue(error.Field, out var existing))
            {
                if (!existing.Contains(error.Message))
                {
                    errors[error.Field] = [.. existing, error.Message];
                }
            }
            else
            {
                errors[error.Field] = [error.Message];
            }
        }

        return errors;
    }
}
