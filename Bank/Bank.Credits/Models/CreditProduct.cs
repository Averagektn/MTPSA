namespace Bank.Credits.Models;

public sealed class CreditProduct
{
    public int Id { get; set; }
    public required string Code { get; set; }
    public required string NameEn { get; set; }
    public required string NameRu { get; set; }
    public RepaymentSchedule RepaymentSchedule { get; set; }
    public int TermMonths { get; set; }
    public decimal AnnualRate { get; set; }
    public decimal MinAmount { get; set; }
    public decimal MaxAmount { get; set; }
    public int CurrencyId { get; set; }
    public Currency Currency { get; set; } = null!;
    public int PrincipalChartAccountId { get; set; }
    public ChartAccount PrincipalChartAccount { get; set; } = null!;
    public int InterestChartAccountId { get; set; }
    public ChartAccount InterestChartAccount { get; set; } = null!;
}
