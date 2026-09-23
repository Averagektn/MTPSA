using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Tests.Credits.Support;

namespace Tests.Credits.Functional;

[TestClass]
[DoNotParallelize]
public sealed class CreditWebDriverTests : WebDriverTestBase
{
    private static CreditsTestHost _host = null!;
    private static IWebDriver _driver = null!;
    private static WebDriverWait _wait = null!;

    [ClassInitialize]
    public static async Task ClassInitialize(TestContext context)
    {
        _host = await CreditsTestHost.StartAsync(
            withFrontend: true,
            clientsApi: FakeClientsApi.Seeded(),
            depositsApi: FakeDepositsApi.WithClientDepositAccounts());
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
    public void New_contract_form_blocks_missing_required_fields()
    {
        var before = _host.CountContracts();
        OpenNewContract();
        _driver.FindElement(By.CssSelector("[data-testid='save-contract']")).Click();
        _wait.Until(d => d.FindElements(By.CssSelector(".field-error")).Count > 0);
        _driver.FindElements(By.CssSelector(".field-error")).Should().NotBeEmpty();
        _host.CountContracts().Should().Be(before);
    }

    [TestMethod]
    public void Creating_a_contract_shows_it_in_the_list()
    {
        OpenNewContract();
        Select("clientId", "1");
        Select("productId", "1");
        Type("number", "UI-CASH-1");
        Type("amount", "3000");
        _wait.Until(d => d.FindElements(By.CssSelector("[data-testid='schedule-row']")).Count == 12);
        _driver.FindElement(By.CssSelector("[data-testid='save-contract']")).Click();
        _wait.Until(d => d.FindElements(By.CssSelector("[data-testid='contract-row']")).Count >= 3);
        _driver.PageSource.Should().Contain("UI-CASH-1");
    }

    [TestMethod]
    public void Accounts_report_shows_cash_development_fund_and_deposit_accounts()
    {
        _driver.Navigate().GoToUrl($"{_host.BaseUrl}/accounts");
        _wait.Until(d => d.FindElements(By.CssSelector("[data-testid='account-row']")).Count >= 10);
        _driver.PageSource.Should().Contain(Bank.Credits.Accounting.BankAccountNumbers.Cash);
        _driver.PageSource.Should().Contain(Bank.Credits.Accounting.BankAccountNumbers.DevelopmentFund);
        _driver.PageSource.Should().Contain("3404");
    }

    [TestMethod]
    public void Closing_the_banking_day_changes_the_displayed_date()
    {
        _driver.Navigate().GoToUrl(_host.BaseUrl);
        var date = _wait.Until(d => d.FindElement(By.CssSelector("[data-testid='bank-date']")));
        var before = date.Text;
        _driver.FindElement(By.CssSelector("[data-testid='close-day']")).Click();
        _wait.Until(d => d.FindElement(By.CssSelector("[data-testid='bank-date']")).Text != before);
        _driver.FindElement(By.CssSelector("[data-testid='bank-date']")).Text.Should().NotBe(before);
    }

    private void OpenNewContract()
    {
        _driver.Navigate().GoToUrl($"{_host.BaseUrl}/contracts/new");
        _wait.Until(d => d.FindElements(By.CssSelector("form.client-form")).Count > 0);
        _wait.Until(d => d.FindElement(By.Name("clientId")).FindElements(By.TagName("option")).Count > 1);
        _wait.Until(d => d.FindElement(By.Name("productId")).FindElements(By.TagName("option")).Count > 1);
    }

    private void Type(string name, string value)
    {
        var element = _driver.FindElement(By.Name(name));
        element.Clear();
        element.SendKeys(value);
    }

    private void Select(string name, string value)
        => _driver.FindElement(By.Name(name)).FindElement(By.CssSelector($"option[value='{value}']")).Click();
}
