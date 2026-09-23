namespace Bank.Atm.Session;

public static class PinPolicy
{
    public const int MaxAttempts = 3;

    public static int AttemptsLeft(int used) => Math.Max(0, MaxAttempts - used);
}
