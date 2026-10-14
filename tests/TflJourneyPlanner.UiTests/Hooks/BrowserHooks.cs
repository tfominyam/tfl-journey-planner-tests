using Microsoft.Playwright;
using Reqnroll;
using Reqnroll.BoDi;
using TflJourneyPlanner.UiTests.Support;

namespace TflJourneyPlanner.UiTests.Hooks;

/// <summary>
/// Browser lifecycle. One browser for the whole run, and a fresh browser context
/// (its own cookies and storage) for every scenario, so no scenario can affect another.
/// </summary>
[Binding]
public sealed class BrowserHooks
{
    private static IPlaywright? _playwright;
    private static IBrowser? _browser;
    private static TestSettings _settings = new();

    private readonly IObjectContainer _container;
    private readonly ScenarioContext _scenarioContext;
    private IBrowserContext? _context;

    public BrowserHooks(IObjectContainer container, ScenarioContext scenarioContext)
    {
        _container = container;
        _scenarioContext = scenarioContext;
    }

    [BeforeTestRun]
    public static async Task StartBrowserAsync()
    {
        _settings = TestSettings.Load();

        // Playwright has two separate timeouts. Actions (click, fill) use the context timeout set
        // per scenario below. Expect(...) assertions have their own default of 5 seconds, which is
        // too short for TfL's results page on a slow day. Keep both on the same configured value.
        Assertions.SetDefaultExpectTimeout(_settings.DefaultTimeoutMs);
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Channel = string.IsNullOrWhiteSpace(_settings.BrowserChannel) ? null : _settings.BrowserChannel,
            Headless = _settings.Headless,
            SlowMo = _settings.SlowMoMs
        });
    }

    [BeforeScenario]
    public async Task StartScenarioAsync()
    {
        _context = await _browser!.NewContextAsync(new BrowserNewContextOptions
        {
            BaseURL = _settings.BaseUrl,
            Locale = "en-GB",
            TimezoneId = "Europe/London",
            ViewportSize = new ViewportSize { Width = 1366, Height = 900 }
        });
        _context.SetDefaultTimeout(_settings.DefaultTimeoutMs);

        if (_settings.TraceOnFailure)
        {
            await _context.Tracing.StartAsync(new TracingStartOptions { Screenshots = true, Snapshots = true });
        }

        var page = await _context.NewPageAsync();

        // Page objects and step classes receive these through their constructors.
        _container.RegisterInstanceAs(_settings);
        _container.RegisterInstanceAs(page);
    }

    [AfterScenario]
    public async Task EndScenarioAsync()
    {
        if (_context is null)
        {
            return;
        }

        var failed = _scenarioContext.TestError is not null;
        var folder = Path.Combine(AppContext.BaseDirectory, "TestResults", "failures");
        var name = $"{SafeFileName(_scenarioContext.ScenarioInfo.Title)}-{DateTime.Now:HHmmss}";

        if (failed)
        {
            Directory.CreateDirectory(folder);
            var page = _context.Pages.FirstOrDefault();
            if (page is not null)
            {
                await page.ScreenshotAsync(new PageScreenshotOptions { Path = Path.Combine(folder, $"{name}.png"), FullPage = true });
            }
        }

        if (_settings.TraceOnFailure)
        {
            await _context.Tracing.StopAsync(failed
                ? new TracingStopOptions { Path = Path.Combine(folder, $"{name}.zip") }
                : new TracingStopOptions());
        }

        await _context.CloseAsync();
    }

    [AfterTestRun]
    public static async Task StopBrowserAsync()
    {
        if (_browser is not null)
        {
            await _browser.CloseAsync();
        }

        _playwright?.Dispose();
    }

    private static string SafeFileName(string title)
    {
        var cleaned = string.Concat(title.Split(Path.GetInvalidFileNameChars()));
        return cleaned.Replace(' ', '-');
    }
}
