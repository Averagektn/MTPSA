namespace Bank.Deposits.Models;

public sealed class BankState
{
    public int Id { get; set; }
    public DateOnly CurrentDate { get; set; }
}
