using Bank.Common.Dtos.Request;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace Tests.Clients.Functional.Infrastructure;

public sealed class ClientFormPage
{
    private readonly IWebDriver _driver;
    private readonly WebDriverWait _wait;

    public ClientFormPage(IWebDriver driver)
    {
        _driver = driver;
        _wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));
    }

    public void OpenNew(string baseUrl)
    {
        _driver.Navigate().GoToUrl($"{baseUrl.TrimEnd('/')}/clients/new");
        _wait.Until(d => d.FindElements(By.CssSelector("form.client-form")).Count > 0);
        _wait.Until(d => d.FindElement(By.Name("residenceCityId")).FindElements(By.TagName("option")).Count > 1);
    }

    public void Fill(ClientRequest request, bool includeOptional = true)
    {
        Type("lastName", request.LastName);
        Type("firstName", request.FirstName);
        Type("patronymic", request.Patronymic);
        Type("birthDate", request.BirthDate);
        Type("birthPlace", request.BirthPlace);
        Type("passportSeries", request.PassportSeries);
        Type("passportNumber", request.PassportNumber);
        Type("issuedBy", request.IssuedBy);
        Type("issueDate", request.IssueDate);
        Type("identificationNumber", request.IdentificationNumber);
        if (request.ResidenceCityId > 0)
        {
            Select("residenceCityId", request.ResidenceCityId.ToString());
        }

        Type("residenceAddress", request.ResidenceAddress);
        if (request.RegistrationCityId > 0)
        {
            Select("registrationCityId", request.RegistrationCityId.ToString());
        }

        if (request.MaritalStatusId > 0)
        {
            Select("maritalStatusId", request.MaritalStatusId.ToString());
        }

        if (request.CitizenshipId > 0)
        {
            Select("citizenshipId", request.CitizenshipId.ToString());
        }

        if (request.DisabilityId > 0)
        {
            Select("disabilityId", request.DisabilityId.ToString());
        }

        if (includeOptional)
        {
            TypePhone("homePhone", request.HomePhone ?? "");
            TypePhone("mobilePhone", request.MobilePhone ?? "");
            Type("email", request.Email ?? "");
            Type("workplace", request.Workplace ?? "");
            Type("position", request.Position ?? "");
            Type("monthlyIncome", request.MonthlyIncome?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "");
        }

        var pensioner = _driver.FindElement(By.Name("pensioner"));
        if (request.Pensioner != pensioner.Selected)
        {
            pensioner.Click();
        }
    }

    public void Submit()
        => _driver.FindElement(By.CssSelector("[data-testid='save-client']")).Click();

    public void SubmitExpectingFieldError()
    {
        Submit();
        _wait.Until(d => d.FindElements(By.CssSelector(".field-error")).Count > 0);
    }

    public void SubmitExpectingList()
    {
        Submit();
        _wait.Until(d => d.Url.Contains("/clients", StringComparison.Ordinal)
            && !d.Url.Contains("/new", StringComparison.Ordinal)
            && !d.Url.Contains("/edit", StringComparison.Ordinal));
    }

    public bool HasFieldError(string field)
    {
        var input = _driver.FindElement(By.Name(field));
        var label = input.FindElement(By.XPath("./ancestor::label[contains(@class,'field')]"));
        return label.FindElements(By.CssSelector(".field-error")).Count > 0;
    }

    private void Type(string name, string value)
    {
        var element = _driver.FindElement(By.Name(name));
        element.Clear();
        if (value.Length > 0)
        {
            element.SendKeys(value);
        }
    }

    private void TypePhone(string name, string value)
    {
        var element = _driver.FindElement(By.Name(name));
        element.Click();
        element.SendKeys(Keys.Control + "a");

        var digits = new string(value.Where(char.IsDigit).ToArray());
        if (digits.StartsWith("375", StringComparison.Ordinal) && digits.Length > 3)
        {
            digits = digits[3..];
        }

        element.SendKeys(digits.Length == 0 ? Keys.Backspace : digits);
    }

    private void Select(string name, string value)
        => _driver.FindElement(By.Name(name)).FindElement(By.CssSelector($"option[value='{value}']")).Click();
}
