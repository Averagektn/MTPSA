using Bank.Deposits.Models;

namespace Bank.Deposits.Accounting;

public static class AccountBalance
{
    public static decimal Saldo(AccountNature nature, decimal debitTurnover, decimal creditTurnover)
        => nature == AccountNature.Active
            ? debitTurnover - creditTurnover
            : creditTurnover - debitTurnover;
}
