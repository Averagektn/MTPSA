using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Tests.Atm.Support;

namespace Tests.Atm.Functional;

[DoNotParallelize]
public abstract class WebDriverTestBase
{
    [TestInitialize]
    public void LockSession() => WebDriverSession.Acquire();

    [TestCleanup]
    public void UnlockSession() => WebDriverSession.Release();
}

public static class WebDriverSession
{
    private static readonly SemaphoreSlim RunGate = new(1, 1);
    public static void Acquire() => RunGate.Wait();
    public static void Release() => RunGate.Release();
}

[TestClass]
[DoNotParallelize]
public sealed class AtmWebDriverTests : WebDriverTestBase
{
    private static AtmTestHost _host = null!;
    private static IWebDriver _driver = null!;
    private static WebDriverWait _wait = null!;

    [ClassInitialize]
    public static async Task ClassInitialize(TestContext context)
    {
        _host = await AtmTestHost.StartAsync(
            withFrontend: true,
            creditsApi: new FakeCreditsApi(),
            depositsApi: new FakeDepositsApi());
        var options = new ChromeOptions();
        options.AddArgument("--headless=new");
        options.AddArgument("--no-sandbox");
        options.AddArgument("--disable-gpu");
        options.AddArgument("--disable-dev-shm-usage");
        options.AddArgument("--window-size=1400,1000");
        _driver = new ChromeDriver(options);
        _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(20));
    }

    [ClassCleanup]
    public static async Task ClassCleanup()
    {
        _driver?.Quit();
        _driver?.Dispose();
        if (_host is not null)
        {
            await _host.DisposeAsync();
        }
    }

    [TestMethod]
    public void Withdrawal_flow_prints_a_receipt()
    {
        OpenMenu();
        _driver.FindElement(By.CssSelector("[data-testid='menu-withdraw']")).Click();
        TypeAmount("200");
        _wait.Until(d => d.FindElements(By.CssSelector("[data-testid='receipt-yes']")).Count > 0);
        _driver.FindElement(By.CssSelector("[data-testid='receipt-yes']")).Click();
        _wait.Until(d => d.FindElements(By.CssSelector("[data-testid='atm-receipt']")).Count > 0);
        _driver.FindElement(By.CssSelector("[data-testid='close-receipt']")).Click();
        _wait.Until(d => d.FindElements(By.CssSelector("[data-testid='atm-success']")).Count > 0);
        _wait.Until(d => d.FindElements(By.CssSelector("[data-testid='menu-withdraw']")).Count > 0);
    }

    [TestMethod]
    public void Credit_and_deposit_balances_are_available_from_the_menu()
    {
        OpenMenu();
        _driver.FindElement(By.CssSelector("[data-testid='menu-balance']")).Click();
        _wait.Until(d => d.FindElements(By.CssSelector("[data-testid='receipt-no']")).Count > 0);
        _driver.FindElement(By.CssSelector("[data-testid='receipt-no']")).Click();
        _wait.Until(d => d.FindElements(By.CssSelector("[data-testid='atm-success']")).Count > 0);
        _wait.Until(d => d.FindElements(By.CssSelector("[data-testid='menu-deposit']")).Count > 0);
        _driver.FindElement(By.CssSelector("[data-testid='menu-deposit']")).Click();
        _wait.Until(d => d.FindElements(By.CssSelector("[data-testid='receipt-yes']")).Count > 0);
        _driver.FindElement(By.CssSelector("[data-testid='receipt-yes']")).Click();
        _wait.Until(d => d.FindElements(By.CssSelector("[data-testid='deposit-item']")).Count > 0);
    }

    [TestMethod]
    public void Mobile_payment_requires_confirmation_and_prints_a_receipt()
    {
        OpenMenu();
        _driver.FindElement(By.CssSelector("[data-testid='menu-payment']")).Click();
        _wait.Until(d => d.FindElements(By.CssSelector("[data-testid='operator-A1']")).Count > 0);
        _driver.FindElement(By.CssSelector("[data-testid='operator-A1']")).Click();
        _wait.Until(d => d.FindElements(By.CssSelector("[data-testid='submit-phone']")).Count > 0);
        ClickKeys("2911122330");
        _driver.FindElement(By.CssSelector("[data-testid='submit-phone']")).Click();
        _wait.Until(d => d.FindElements(By.CssSelector("[data-testid='submit-amount']")).Count > 0);
        TypeAmount("15");
        _wait.Until(d => d.FindElements(By.CssSelector("[data-testid='confirm-payment']")).Count > 0);
        _driver.FindElement(By.CssSelector("[data-testid='confirm-payment']")).Click();
        _wait.Until(d => d.FindElements(By.CssSelector("[data-testid='receipt-yes']")).Count > 0);
        _driver.FindElement(By.CssSelector("[data-testid='receipt-yes']")).Click();
        _wait.Until(d => d.FindElements(By.CssSelector("[data-testid='atm-receipt']")).Count > 0);
    }

    private void OpenMenu()
    {
        _driver.Navigate().GoToUrl(_host.BaseUrl);
        _wait.Until(d => d.FindElements(By.CssSelector("[data-testid='demo-card']")).Count > 0);
        _driver.FindElement(By.CssSelector("[data-testid='demo-card']")).Click();
        _driver.FindElement(By.CssSelector("[data-testid='insert-card']")).Click();
        _wait.Until(d => d.FindElements(By.CssSelector("[data-testid='submit-pin']")).Count > 0);
        ClickKeys("1111");
        _driver.FindElement(By.CssSelector("[data-testid='submit-pin']")).Click();
        _wait.Until(d => d.FindElements(By.CssSelector("[data-testid='menu-withdraw']")).Count > 0);
    }

    private void TypeAmount(string amount)
    {
        _wait.Until(d => d.FindElements(By.CssSelector("[data-testid='submit-amount']")).Count > 0);
        ClickKeys(amount);
        _driver.FindElement(By.CssSelector("[data-testid='submit-amount']")).Click();
    }

    private void ClickKeys(string digits)
    {
        foreach (var digit in digits)
        {
            _driver.FindElement(By.CssSelector($"[data-testid='key-{digit}']")).Click();
        }
    }
}
