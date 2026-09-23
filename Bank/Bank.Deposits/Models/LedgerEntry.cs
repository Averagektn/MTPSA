namespace Bank.Deposits.Models;

public sealed class LedgerEntry
{
    public int Id { get; set; }
    public DateOnly BookedOn { get; set; }
    public int DebitAccountId { get; set; }
    public BankAccount DebitAccount { get; set; } = null!;
    public int CreditAccountId { get; set; }
    public BankAccount CreditAccount { get; set; } = null!;
    public decimal Amount { get; set; }
    public decimal AmountByn { get; set; }
    public decimal Rate { get; set; }
    public required string Operation { get; set; }
    public int? ContractId { get; set; }
    public DepositContract? Contract { get; set; }
}
