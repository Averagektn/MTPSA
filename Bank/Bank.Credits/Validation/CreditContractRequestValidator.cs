using Bank.Common.Validation;
using Bank.Common.Dtos.Request;
using FluentValidation;

namespace Bank.Credits.Validation;

public sealed class CreditContractRequestValidator : AbstractValidator<CreditContractRequest>
{
    public CreditContractRequestValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Continue;

        RuleFor(x => x.ClientId)
            .GreaterThan(0).WithMessage("clientRequired");

        RuleFor(x => x.ProductId)
            .GreaterThan(0).WithMessage("productRequired");

        RuleFor(x => x.Number)
            .MaximumLength(32).WithMessage("contractNumberTooLong");

        RuleFor(x => x.CurrencyId)
            .GreaterThan(0).WithMessage("currencyRequired");

        RuleFor(x => x.StartDate)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("startDateRequired")
            .Must(value => DateParsing.Parse(value).IsSuccess)
            .WithMessage("startDateInvalid");

        RuleFor(x => x.EndDate)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("endDateRequired")
            .Must(value => DateParsing.Parse(value).IsSuccess)
            .WithMessage("endDateInvalid");

        RuleFor(x => x.TermMonths)
            .GreaterThan(0).WithMessage("termRequired");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("amountRequired");

        RuleFor(x => x.AnnualRate)
            .GreaterThan(0).WithMessage("rateRequired");
    }
}
