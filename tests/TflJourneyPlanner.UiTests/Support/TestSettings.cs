using Microsoft.Extensions.Configuration;

namespace TflJourneyPlanner.UiTests.Support;

/// <summary>
/// Settings for a test run, read from appsettings.json.
/// Any value can be overridden with an environment variable prefixed TFLTESTS_,
/// for example TFLTESTS_Headless=true in CI.
/// </summary>
public sealed class TestSettings
{
    public string BaseUrl { get; set; } = "https://tfl.gov.uk";

    /// <summary>"chrome" runs branded Google Chrome; leave empty to use Playwright's bundled Chromium.</summary>
    public string BrowserChannel { get; set; } = "chrome";

    public bool Headless { get; set; } = true;

    public float SlowMoMs { get; set; }

    public float DefaultTimeoutMs { get; set; } = 15000;

    /// <summary>Keep a Playwright trace and a screenshot for any scenario that fails.</summary>
    public bool TraceOnFailure { get; set; } = true;

    public static TestSettings Load()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables(prefix: "TFLTESTS_")
            .Build();

        return configuration.Get<TestSettings>() ?? new TestSettings();
    }
}
