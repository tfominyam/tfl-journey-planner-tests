using Microsoft.Playwright;

namespace TflJourneyPlanner.UiTests.Pages;

/// <summary>The Plan a journey page and its widget.</summary>
public sealed class JourneyPlannerPage
{
    private readonly IPage _page;
    private readonly CookieBanner _cookieBanner;

    public JourneyPlannerPage(IPage page, CookieBanner cookieBanner)
    {
        _page = page;
        _cookieBanner = cookieBanner;
    }

    public ILocator Heading => _page.GetByRole(AriaRole.Heading, new() { Name = "Plan a journey", Level = 1 });

    public async Task OpenAsync()
    {
        await _page.GotoAsync("/plan-a-journey");
        await _cookieBanner.AcceptEssentialCookiesIfShownAsync();
    }
}
