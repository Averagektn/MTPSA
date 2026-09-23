namespace Bank.Deposits.Models;

public sealed class ChartAccount
{
    public int Id { get; set; }
    public required string Code { get; set; }
    public required string NameEn { get; set; }
    public required string NameRu { get; set; }
    public AccountNature Nature { get; set; }
}
