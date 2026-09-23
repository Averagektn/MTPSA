using FluentResults;

namespace Bank.Common.Errors;

public sealed class ValidationError : Error
{
    public string Field { get; }

    public ValidationError(string field, string key) : base(key)
    {
        Field = field;
        Metadata.Add("Field", field);
    }
}
