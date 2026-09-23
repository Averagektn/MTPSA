using FluentResults;

namespace Bank.Common.Errors;

public sealed class NotFoundError : Error
{
    public NotFoundError() : base("notFound")
    {
    }
}
