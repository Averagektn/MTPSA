namespace Bank.Common.Dtos.Response;

public sealed record PaymentScheduleItem(
    int Period,
    DateOnly Date,
    decimal Principal,
    decimal Interest,
    decimal Payment,
    decimal Remaining);
