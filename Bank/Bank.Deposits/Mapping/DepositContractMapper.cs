using Bank.Common.Dtos.Response;
using Bank.Common.Localization;
using Bank.Deposits.Models;

namespace Bank.Deposits.Mapping;

public static class DepositContractMapper
{
    public static string ClientName(ClientResponse client)
        => $"{client.LastName} {client.FirstName} {client.Patronymic}";

    public static DepositContractListItem ToListItem(DepositContract contract, DateOnly bankDate)
        => new(
            contract.Id,
            contract.Number,
            contract.ClientId,
            contract.ClientName,
            RequestLocale.Pick(contract.Product.NameEn, contract.Product.NameRu),
            contract.Currency.Code,
            contract.StartDate,
            contract.EndDate,
            contract.TermMonths,
            contract.Amount,
            contract.AnnualRate,
            Status(contract.Status),
            contract.PrincipalAccount.Number,
            contract.InterestAccount.Number,
            NextPaymentDate(contract, bankDate));

    public static DepositContractResponse ToResponse(DepositContract contract)
        => new(
            contract.Id,
            contract.Number,
            contract.ClientId,
            contract.ClientName,
            contract.ProductId,
            RequestLocale.Pick(contract.Product.NameEn, contract.Product.NameRu),
            contract.CurrencyId,
            contract.Currency.Code,
            contract.StartDate,
            contract.EndDate,
            contract.TermMonths,
            contract.Amount,
            contract.AnnualRate,
            Status(contract.Status),
            contract.PrincipalAccountId,
            contract.PrincipalAccount.Number,
            contract.InterestAccountId,
            contract.InterestAccount.Number);

    private static string Status(DepositContractStatus status)
        => status == DepositContractStatus.Closed ? "closed" : "active";

    public static DateOnly? NextPaymentDate(DepositContract contract, DateOnly bankDate)
    {
        if (contract.Status == DepositContractStatus.Closed || bankDate >= contract.EndDate)
        {
            return null;
        }

        if (contract.Product.InterestSchedule == InterestSchedule.EndOfTerm)
        {
            return contract.EndDate;
        }

        var from = contract.LastInterestPaidOn ?? contract.StartDate;
        var next = from.AddDays(30);
        return next > contract.EndDate ? contract.EndDate : next;
    }
}
