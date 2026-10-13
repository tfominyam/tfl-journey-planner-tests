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

    // The From and To inputs have visible labels "From location" and "To location".
    public ILocator FromInput => _page.GetByLabel("From location", new() { Exact = true });

    public ILocator ToInput => _page.GetByLabel("To location", new() { Exact = true });

    // An <input type="submit">, so its accessible name comes from its value.
    public ILocator PlanJourneyButton => _page.GetByRole(AriaRole.Button, new() { Name = "Plan my journey", Exact = true });

    // The results page heading. Shown whenever the results page loads.
    public ILocator ResultsHeading => _page.GetByRole(AriaRole.Heading, new() { Name = "Journey results", Level = 1 });

    public ILocator Message(string text) => _page.GetByText(text, new() { Exact = true });

    public async Task OpenAsync()
    {
        await _page.GotoAsync("/plan-a-journey");
        await _cookieBanner.AcceptEssentialCookiesIfShownAsync();
    }

    public async Task PlanJourneyAsync()
    {
        await PlanJourneyButton.ClickAsync();
    }
}
