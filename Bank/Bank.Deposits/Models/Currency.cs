namespace Bank.Deposits.Models;

public sealed class Currency
{
    public int Id { get; set; }
    public required string Code { get; set; }
    public required string NameEn { get; set; }
    public required string NameRu { get; set; }
}
