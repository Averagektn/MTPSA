using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Tests.Clients.Functional.Infrastructure;

namespace Tests.Clients.Functional.Credits;

[TestClass]
[DoNotParallelize]
public sealed class HomeCreditsLinkWebDriverTests : WebDriverTestBase
{
    [ClassInitialize]
    public static Task ClassInitialize(TestContext context)
        => WebDriverSession.EnsureStartedAsync();

    [TestMethod]
    public void Home_page_has_go_to_credits_button()
    {
        var driver = WebDriverSession.Driver;
        driver.Navigate().GoToUrl(WebDriverSession.Host.BaseUrl);
        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));
        var link = wait.Until(d => d.FindElement(By.CssSelector("[data-testid='go-to-credits']")));
        link.Displayed.Should().BeTrue();
        link.Text.ToLowerInvariant().Should().Contain("credit");
    }
}
