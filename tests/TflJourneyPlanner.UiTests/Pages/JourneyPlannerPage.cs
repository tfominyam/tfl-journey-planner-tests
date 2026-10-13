using Microsoft.Playwright;

namespace TflJourneyPlanner.UiTests.Pages;

/// <summary>The Plan a journey page, its widget and the results page it leads to.</summary>
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

    // From and To are autocomplete comboboxes labelled "From location" and "To location".
    public ILocator FromInput => _page.GetByRole(AriaRole.Combobox, new() { Name = "From location", Exact = true });

    public ILocator ToInput => _page.GetByRole(AriaRole.Combobox, new() { Name = "To location", Exact = true });

    // An <input type="submit">, so its accessible name comes from its value.
    public ILocator PlanJourneyButton => _page.GetByRole(AriaRole.Button, new() { Name = "Plan my journey", Exact = true });

    // The results page heading. Shown whenever the results page loads, even with no journeys.
    public ILocator ResultsHeading => _page.GetByRole(AriaRole.Heading, new() { Name = "Journey results", Level = 1 });

    // Walking and cycling options only appear when a journey has been planned.
    public ILocator WalkingOption => _page.GetByRole(AriaRole.Link, new() { Name = "Walking speed:" });

    public ILocator CyclingOption => _page.GetByRole(AriaRole.Link, new() { Name = "Cycling Route:" });

    // "We found more than one location matching 'X'". The page can hold one of these per
    // field (both invalid gives two), so always ask for the one about a specific location.
    public ILocator DisambiguationMessage(string location) =>
        _page.GetByText($"We found more than one location matching '{location}'", new() { Exact = true });

    public ILocator Message(string text) => _page.GetByText(text, new() { Exact = true });

    private ILocator Suggestion(string name) => _page.GetByRole(AriaRole.Option, new() { Name = name });

    public async Task OpenAsync()
    {
        await _page.GotoAsync("/plan-a-journey");
        await _cookieBanner.AcceptEssentialCookiesIfShownAsync();
    }

    /// <summary>
    /// Types a location the way a user does. If the autocomplete offers that exact place it is
    /// chosen; if not (an invalid location), the list is closed and the typed text is left as it is.
    /// </summary>
    public async Task EnterLocationAsync(ILocator field, string location)
    {
        await field.ClickAsync();
        await field.FillAsync(string.Empty);
        await field.PressSequentiallyAsync(location, new() { Delay = 50 });

        var match = Suggestion(location).First;
        try
        {
            await match.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5000 });
        }
        catch (TimeoutException)
        {
            await field.PressAsync("Escape");
            return;
        }

        await match.ClickAsync();
    }

    public async Task PlanJourneyAsync()
    {
        await PlanJourneyButton.ClickAsync();
    }
}
