namespace Bank.Clients.Models;

public sealed class Citizenship
{
    public int Id { get; set; }
    public required string NameEn { get; set; }
    public required string NameRu { get; set; }
}
