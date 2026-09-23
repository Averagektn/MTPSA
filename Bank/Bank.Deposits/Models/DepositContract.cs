namespace Bank.Deposits.Models;

public sealed class DepositContract
{
    public int Id { get; set; }
    public required string Number { get; set; }
    public int ClientId { get; set; }
    public required string ClientName { get; set; }
    public int ProductId { get; set; }
    public DepositProduct Product { get; set; } = null!;
    public int CurrencyId { get; set; }
    public Currency Currency { get; set; } = null!;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public int TermMonths { get; set; }
    public decimal Amount { get; set; }
    public decimal AnnualRate { get; set; }
    public DepositContractStatus Status { get; set; }
    public int PrincipalAccountId { get; set; }
    public BankAccount PrincipalAccount { get; set; } = null!;
    public int InterestAccountId { get; set; }
    public BankAccount InterestAccount { get; set; } = null!;
    public DateOnly? LastInterestPaidOn { get; set; }
}
