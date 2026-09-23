using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Tests.Clients.Functional.Infrastructure;

namespace Tests.Clients.Functional.Atm;

[TestClass]
[DoNotParallelize]
public sealed class HomeAtmLinkWebDriverTests : WebDriverTestBase
{
    [ClassInitialize]
    public static Task ClassInitialize(TestContext context)
        => WebDriverSession.EnsureStartedAsync();

    [TestMethod]
    public void Home_page_has_go_to_atm_button()
    {
        var driver = WebDriverSession.Driver;
        driver.Navigate().GoToUrl(WebDriverSession.Host.BaseUrl);
        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));
        var link = wait.Until(d => d.FindElement(By.CssSelector("[data-testid='go-to-atm']")));
        link.Displayed.Should().BeTrue();
        link.Text.ToLowerInvariant().Should().Contain("atm");
    }
}
