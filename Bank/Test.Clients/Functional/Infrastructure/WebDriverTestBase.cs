namespace Tests.Clients.Functional.Infrastructure;

[DoNotParallelize]
public abstract class WebDriverTestBase
{
    [TestInitialize]
    public void LockSession()
        => WebDriverSession.Acquire();

    [TestCleanup]
    public void UnlockSession()
        => WebDriverSession.Release();
}
