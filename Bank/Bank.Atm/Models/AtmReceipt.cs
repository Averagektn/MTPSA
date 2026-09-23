namespace Bank.Atm.Models;

public sealed class AtmReceipt
{
    public int Id { get; set; }
    public Guid SessionId { get; set; }
    public DateTimeOffset PrintedAt { get; set; }
    public required string BodyJson { get; set; }
}
