namespace Tests.Credits.Functional;

public static class WebDriverSession
{
    private static readonly SemaphoreSlim RunGate = new(1, 1);

    public static void Acquire() => RunGate.Wait();

    public static void Release() => RunGate.Release();
}

[TestClass]
public sealed class WebDriverAssemblyHooks
{
    [AssemblyCleanup]
    public static Task AssemblyCleanup() => Task.CompletedTask;
}
