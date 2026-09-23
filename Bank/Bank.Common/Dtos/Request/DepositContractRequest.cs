using Bank.Common.Validation;

namespace Bank.Common.Dtos.Request;

public sealed class DepositContractRequest
{
    public int ClientId { get; set; }
    public int ProductId { get; set; }
    public string Number { get; set; } = "";
    public int CurrencyId { get; set; }
    public string StartDate { get; set; } = "";
    public string EndDate { get; set; } = "";
    public int TermMonths { get; set; }
    public decimal Amount { get; set; }
    public decimal AnnualRate { get; set; }

    public DateOnly? ParsedStartDate { get; private set; }
    public DateOnly? ParsedEndDate { get; private set; }

    public void Normalize()
    {
        Number = Number.Trim();
        StartDate = StartDate.Trim();
        EndDate = EndDate.Trim();

        var start = DateParsing.Parse(StartDate);
        ParsedStartDate = start.IsSuccess ? start.Value : null;
        var end = DateParsing.Parse(EndDate);
        ParsedEndDate = end.IsSuccess ? end.Value : null;
    }
}
