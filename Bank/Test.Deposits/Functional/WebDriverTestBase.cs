namespace Tests.Deposits.Functional;

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
