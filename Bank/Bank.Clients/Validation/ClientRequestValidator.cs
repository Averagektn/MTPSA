using System.Text.RegularExpressions;
using Bank.Common.Dtos.Request;
using Bank.Common.Validation;
using FluentValidation;

namespace Bank.Clients.Validation;

public sealed class ClientRequestValidator : AbstractValidator<ClientRequest>
{
    public ClientRequestValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Continue;

        PersonName(x => x.LastName, "lastNameRequired", "lastNameTooLong");
        PersonName(x => x.FirstName, "firstNameRequired", "firstNameTooLong");
        PersonName(x => x.Patronymic, "patronymicRequired", "patronymicTooLong");

        RuleFor(x => x.BirthDate)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("birthDateRequired")
            .Must(value => DateParsing.Parse(value).IsSuccess)
            .WithMessage("birthDateInvalid");

        RuleFor(x => x.ParsedBirthDate)
            .Must(date => date <= DateOnly.FromDateTime(DateTime.Today))
            .When(x => x.ParsedBirthDate is not null)
            .WithMessage("birthDateFuture")
            .OverridePropertyName(nameof(ClientRequest.BirthDate));

        RuleFor(x => x.ParsedBirthDate)
            .Must(date => date!.Value >= new DateOnly(1900, 1, 1))
            .When(x => x.ParsedBirthDate is not null)
            .WithMessage("birthDateTooEarly")
            .OverridePropertyName(nameof(ClientRequest.BirthDate));

        RuleFor(x => x.PassportSeries)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("passportSeriesRequired")
            .Matches(ValidationPatterns.PassportSeries).WithMessage("passportSeriesInvalid");

        RuleFor(x => x.PassportNumber)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("passportNumberRequired")
            .Matches(ValidationPatterns.PassportNumber).WithMessage("passportNumberInvalid");

        RuleFor(x => x.IssuedBy)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("issuedByRequired")
            .MaximumLength(200).WithMessage("issuedByInvalid");

        RuleFor(x => x.IssueDate)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("issueDateRequired")
            .Must(value => DateParsing.Parse(value).IsSuccess)
            .WithMessage("issueDateInvalid");

        RuleFor(x => x.ParsedIssueDate)
            .Must(date => date <= DateOnly.FromDateTime(DateTime.Today))
            .When(x => x.ParsedIssueDate is not null)
            .WithMessage("issueDateFuture")
            .OverridePropertyName(nameof(ClientRequest.IssueDate));

        RuleFor(x => x)
            .Must(x => x.ParsedIssueDate > x.ParsedBirthDate)
            .When(x => x.ParsedBirthDate is not null && x.ParsedIssueDate is not null)
            .WithMessage("issueDateBeforeBirth")
            .OverridePropertyName(nameof(ClientRequest.IssueDate));

        RuleFor(x => x.IdentificationNumber)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("identificationNumberRequired")
            .Matches(ValidationPatterns.IdentificationNumber).WithMessage("identificationNumberInvalid");

        RuleFor(x => x.BirthPlace)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("birthPlaceRequired")
            .MaximumLength(200).WithMessage("birthPlaceInvalid");

        RuleFor(x => x.ResidenceCityId)
            .GreaterThan(0).WithMessage("residenceCityRequired");

        RuleFor(x => x.ResidenceAddress)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("residenceAddressRequired")
            .MaximumLength(300).WithMessage("residenceAddressInvalid");

        RuleFor(x => x.RegistrationCityId)
            .GreaterThan(0).WithMessage("registrationCityRequired");

        RuleFor(x => x.HomePhone)
            .Matches(ValidationPatterns.HomePhone)
            .When(x => !string.IsNullOrWhiteSpace(x.HomePhone))
            .WithMessage("homePhoneInvalid");

        RuleFor(x => x.MobilePhone)
            .Matches(ValidationPatterns.MobilePhone)
            .When(x => !string.IsNullOrWhiteSpace(x.MobilePhone))
            .WithMessage("mobilePhoneInvalid");

        RuleFor(x => x.Email)
            .Matches(ValidationPatterns.Email, RegexOptions.IgnoreCase)
            .When(x => !string.IsNullOrWhiteSpace(x.Email))
            .WithMessage("emailInvalid");

        RuleFor(x => x.Workplace)
            .MaximumLength(200).WithMessage("workplaceTooLong");

        RuleFor(x => x.Position)
            .MaximumLength(200).WithMessage("positionTooLong");

        RuleFor(x => x.MaritalStatusId)
            .GreaterThan(0).WithMessage("maritalStatusRequired");

        RuleFor(x => x.CitizenshipId)
            .GreaterThan(0).WithMessage("citizenshipRequired");

        RuleFor(x => x.DisabilityId)
            .GreaterThan(0).WithMessage("disabilityRequired");

        RuleFor(x => x.MonthlyIncome)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MonthlyIncome is not null)
            .WithMessage("incomeNegative");
    }

    private void PersonName(
        System.Linq.Expressions.Expression<Func<ClientRequest, string>> field,
        string requiredKey,
        string tooLongKey)
    {
        RuleFor(field)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(requiredKey)
            .MaximumLength(100).WithMessage(tooLongKey)
            .Matches(ValidationPatterns.PersonName).WithMessage("personNameInvalid");
    }
}
