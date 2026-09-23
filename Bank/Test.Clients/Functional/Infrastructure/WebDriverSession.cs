using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Tests.Clients.Support;

namespace Tests.Clients.Functional.Infrastructure;

public static class WebDriverSession
{
    private static readonly SemaphoreSlim Gate = new(1, 1);
    private static readonly SemaphoreSlim RunGate = new(1, 1);

    public static BankTestHost Host { get; private set; } = null!;
    public static IWebDriver Driver { get; private set; } = null!;
    public static ClientFormPage Page { get; private set; } = null!;

    public static async Task EnsureStartedAsync()
    {
        if (Host is not null)
        {
            return;
        }

        await Gate.WaitAsync();
        try
        {
            if (Host is not null)
            {
                return;
            }

            Host = await BankTestHost.StartAsync(withFrontend: true, frontendProject: "clients-frontend");
            var options = new ChromeOptions();
            options.AddArgument("--headless=new");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-gpu");
            options.AddArgument("--disable-dev-shm-usage");
            options.AddArgument("--window-size=1400,1000");
            Driver = new ChromeDriver(options);
            Page = new ClientFormPage(Driver);
        }
        finally
        {
            Gate.Release();
        }
    }

    public static void Acquire()
        => RunGate.Wait();

    public static void Release()
        => RunGate.Release();

    public static async Task DisposeAsync()
    {
        Driver?.Quit();
        Driver?.Dispose();
        Driver = null!;
        if (Host is not null)
        {
            await Host.DisposeAsync();
            Host = null!;
        }
    }
}

[TestClass]
public sealed class WebDriverAssemblyHooks
{
    [AssemblyCleanup]
    public static async Task AssemblyCleanup()
        => await WebDriverSession.DisposeAsync();
}
