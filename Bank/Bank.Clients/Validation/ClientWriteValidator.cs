using System.Text.Json;
using Bank.Clients.Data;
using Bank.Common.Dtos.Request;
using Bank.Common.Errors;
using FluentResults;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Bank.Clients.Validation;

public sealed class ClientWriteValidator(BankDbContext db, IValidator<ClientRequest> validator)
{
    public async Task<Result> ValidateAsync(
        ClientRequest request,
        int? excludeId,
        CancellationToken cancellationToken = default)
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

        await AddLookupErrors(request, errors, cancellationToken);
        await AddUniquenessErrors(request, excludeId, errors, cancellationToken);
        return errors.Count > 0 ? Result.Fail(errors) : Result.Ok();
    }

    private async Task AddLookupErrors(
        ClientRequest request,
        List<IError> errors,
        CancellationToken cancellationToken)
    {
        if (!await db.Cities.AnyAsync(c => c.Id == request.ResidenceCityId, cancellationToken))
        {
            errors.Add(new ValidationError("residenceCityId", "residenceCityNotFound"));
        }

        if (!await db.Cities.AnyAsync(c => c.Id == request.RegistrationCityId, cancellationToken))
        {
            errors.Add(new ValidationError("registrationCityId", "registrationCityNotFound"));
        }

        if (!await db.MaritalStatuses.AnyAsync(c => c.Id == request.MaritalStatusId, cancellationToken))
        {
            errors.Add(new ValidationError("maritalStatusId", "maritalStatusNotFound"));
        }

        if (!await db.Citizenships.AnyAsync(c => c.Id == request.CitizenshipId, cancellationToken))
        {
            errors.Add(new ValidationError("citizenshipId", "citizenshipNotFound"));
        }

        if (!await db.Disabilities.AnyAsync(c => c.Id == request.DisabilityId, cancellationToken))
        {
            errors.Add(new ValidationError("disabilityId", "disabilityNotFound"));
        }
    }

    private async Task AddUniquenessErrors(
        ClientRequest request,
        int? excludeId,
        List<IError> errors,
        CancellationToken cancellationToken)
    {
        var duplicatePerson = await db.Clients.AnyAsync(c =>
            c.LastName == request.LastName
            && c.FirstName == request.FirstName
            && c.Patronymic == request.Patronymic
            && c.BirthDate == request.ParsedBirthDate!.Value
            && (excludeId == null || c.Id != excludeId), cancellationToken);

        if (duplicatePerson)
        {
            errors.Add(new ValidationError("lastName", "duplicatePerson"));
        }

        var duplicatePassport = await db.Clients.AnyAsync(c =>
            c.PassportSeries == request.PassportSeries
            && c.PassportNumber == request.PassportNumber
            && (excludeId == null || c.Id != excludeId), cancellationToken);

        if (duplicatePassport)
        {
            errors.Add(new ValidationError("passportNumber", "duplicatePassport"));
        }

        var duplicateId = await db.Clients.AnyAsync(c =>
            c.IdentificationNumber == request.IdentificationNumber
            && (excludeId == null || c.Id != excludeId), cancellationToken);

        if (duplicateId)
        {
            errors.Add(new ValidationError("identificationNumber", "duplicateIdentificationNumber"));
        }
    }
}
