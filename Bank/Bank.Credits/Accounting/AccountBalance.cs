using Bank.Credits.Models;

namespace Bank.Credits.Accounting;

public static class AccountBalance
{
    public static decimal Saldo(AccountNature nature, decimal debitTurnover, decimal creditTurnover)
        => nature == AccountNature.Active
            ? debitTurnover - creditTurnover
            : creditTurnover - debitTurnover;
}
