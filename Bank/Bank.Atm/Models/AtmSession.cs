using Bank.Common.Atm;

namespace Bank.Atm.Models;

public sealed class AtmSession
{
    public Guid Id { get; set; }
    public AtmScreen Screen { get; set; }
    public string? CardNumber { get; set; }
    public string? Pin { get; set; }
    public int PinAttempts { get; set; }
    public bool Authorized { get; set; }
    public string FieldsJson { get; set; } = "[]";
    public string? AccountJson { get; set; }
    public string? LastResultJson { get; set; }
    public string? ReceiptJson { get; set; }
    public string? MessageKey { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
