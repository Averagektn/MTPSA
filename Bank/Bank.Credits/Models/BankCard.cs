namespace Bank.Credits.Models;

public sealed class BankCard
{
    public int Id { get; set; }
    public required string CardNumber { get; set; }
    public required string Pin { get; set; }
    public int CreditContractId { get; set; }
    public CreditContract CreditContract { get; set; } = null!;
    public int CardAccountId { get; set; }
    public BankAccount CardAccount { get; set; } = null!;
}
