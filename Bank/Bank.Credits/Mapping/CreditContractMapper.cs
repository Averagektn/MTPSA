using Bank.Common.Dtos.Response;
using Bank.Common.Localization;
using Bank.Credits.Accounting;
using Bank.Credits.Models;

namespace Bank.Credits.Mapping;

public static class CreditContractMapper
{
    public static string ClientName(ClientResponse client)
        => $"{client.LastName} {client.FirstName} {client.Patronymic}";

    public static CreditContractListItem ToListItem(CreditContract contract)
        => new(
            contract.Id,
            contract.Number,
            contract.ClientId,
            contract.ClientName,
            RequestLocale.Pick(contract.Product.NameEn, contract.Product.NameRu),
            Schedule(contract.Product.RepaymentSchedule),
            contract.Currency.Code,
            contract.StartDate,
            contract.EndDate,
            contract.TermMonths,
            contract.Amount,
            contract.RemainingPrincipal,
            contract.AnnualRate,
            Status(contract.Status),
            contract.PrincipalAccount.Number,
            contract.InterestAccount.Number,
            NextPaymentDate(contract));

    public static CreditContractResponse ToResponse(CreditContract contract)
        => new(
            contract.Id,
            contract.Number,
            contract.ClientId,
            contract.ClientName,
            contract.ProductId,
            RequestLocale.Pick(contract.Product.NameEn, contract.Product.NameRu),
            Schedule(contract.Product.RepaymentSchedule),
            contract.CurrencyId,
            contract.Currency.Code,
            contract.StartDate,
            contract.EndDate,
            contract.TermMonths,
            contract.Amount,
            contract.RemainingPrincipal,
            contract.AnnualRate,
            Status(contract.Status),
            contract.PrincipalAccountId,
            contract.PrincipalAccount.Number,
            contract.InterestAccountId,
            contract.InterestAccount.Number,
            PaymentScheduleCalculator.Build(
                contract.Amount,
                contract.AnnualRate,
                contract.StartDate,
                contract.TermMonths,
                contract.Product.RepaymentSchedule));

    private static string Status(CreditContractStatus status)
        => status == CreditContractStatus.Closed ? "closed" : "active";

    private static string Schedule(RepaymentSchedule schedule)
        => schedule == RepaymentSchedule.InterestOnly ? "interestOnly" : "annuity";

    public static DateOnly? NextPaymentDate(CreditContract contract)
    {
        if (contract.Status == CreditContractStatus.Closed || contract.PaidPeriods >= contract.TermMonths)
        {
            return null;
        }

        return contract.StartDate.AddMonths(contract.PaidPeriods + 1);
    }
}
