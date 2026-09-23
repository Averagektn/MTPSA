namespace Bank.Common.Dtos.Response;

public sealed record AccountReportItem(
    string Number,
    string Name,
    string ChartCode,
    string ChartName,
    string Nature,
    decimal Debit,
    decimal Credit,
    decimal Saldo,
    string CurrencyCode = "",
    decimal DebitByn = 0,
    decimal CreditByn = 0,
    decimal SaldoByn = 0);
