namespace Bank.Credits.Models;

public sealed class BankAccount
{
    public int Id { get; set; }
    public required string Number { get; set; }
    public int ChartAccountId { get; set; }
    public ChartAccount ChartAccount { get; set; } = null!;
    public int? ClientId { get; set; }
    public required string NameEn { get; set; }
    public required string NameRu { get; set; }
    public int CurrencyId { get; set; }
    public Currency Currency { get; set; } = null!;
}
