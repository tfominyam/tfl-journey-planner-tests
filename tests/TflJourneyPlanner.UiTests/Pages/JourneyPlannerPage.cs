using System.Text.RegularExpressions;
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

    // TfL's bot protection page. Seen when the tests run from GitHub's servers instead of a home connection.
    private ILocator VerificationHeading => _page.GetByRole(AriaRole.Heading, new() { Name = "Verification required" });

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

    // The grey box at the top of the results page: "From: ... To: ... Leaving: ...".
    // It has no role or label of its own, so this is the one place a CSS class is used.
    public ILocator ResultsSummary => _page.Locator(".journey-result-summary");

    // "We found more than one location matching 'X'". The page can hold one of these per
    // field (both invalid gives two), so always ask for the one about a specific location.
    public ILocator DisambiguationMessage(string location) =>
        _page.GetByText($"We found more than one location matching '{location}'", new() { Exact = true });

    public ILocator Message(string text) => _page.GetByText(text, new() { Exact = true });

    // Results page: travel preferences.
    public ILocator EditPreferencesButton => _page.GetByRole(AriaRole.Button, new() { Name = "Edit preferences" });

    public ILocator UpdateJourneyButton => _page.GetByRole(AriaRole.Button, new() { Name = "Update journey" });

    public ILocator Preference(string name) => _page.GetByRole(AriaRole.Radio, new() { Name = name, Exact = true });

    // TfL draws its own radio buttons: the real <input> sits under a styled label box that
    // takes the click. A user clicks the label, so the test does too.
    private ILocator PreferenceLabel(string name) => _page.GetByText(name, new() { Exact = true });

    // The summary line under "Travel preferences & accessibility", e.g. "Showing routes with least walking".
    public ILocator PreferenceSummary(string text) => _page.GetByText(text);

    // Each public transport journey has a header button named "Option 1: walking, Piccadilly line ...".
    public ILocator JourneyOptions => _page.GetByRole(AriaRole.Button, new() { NameRegex = new Regex(@"^Option \d+:") });

    // The content of the first journey is a region labelled by its "Option 1: ..." button.
    // Scoping to it means later checks can't pick up text from another journey.
    public ILocator FirstJourney => _page.GetByRole(AriaRole.Region, new() { NameRegex = new Regex(@"^Option 1:") });

    public ILocator ViewDetails => FirstJourney.GetByText("View details", new() { Exact = true });

    public ILocator AccessibilityDetailsToggle => FirstJourney.GetByText("Accessibility details", new() { Exact = true });

    // Step-by-step access icons, e.g. "Up stairs", "Up lift", "Level walkway". They are links.
    public ILocator AccessIcon(string name) => FirstJourney.GetByRole(AriaRole.Link, new() { Name = name, Exact = true });

    // Text inside the expanded accessibility details, e.g. "2 sets of stairs down".
    public ILocator FirstJourneyText(Regex pattern) => FirstJourney.GetByText(pattern);

    private ILocator Suggestion(string name) => _page.GetByRole(AriaRole.Option, new() { Name = name });

    public async Task OpenAsync()
    {
        await _page.GotoAsync("/plan-a-journey");

        // Wait for whichever page TfL serves. If it is the bot check, stop straight away with a clear
        // reason rather than timing out later on a missing element. The tests never try to get round it.
        await Heading.Or(VerificationHeading).WaitForAsync();
        if (await VerificationHeading.IsVisibleAsync())
        {
            throw new InvalidOperationException(
                "TfL showed a 'Verification required' page instead of Plan a journey. Its bot protection is " +
                "blocking this machine or network (for example a cloud CI runner), so the UI tests can't run here.");
        }

        await _cookieBanner.AcceptEssentialCookiesIfShownAsync();
    }

    /// <summary>
    /// Types a location the way a user does. If the autocomplete offers that exact place it is
    /// chosen; if not (an invalid location), the list is closed and the typed text is left as it is.
    /// </summary>
    public async Task EnterLocationAsync(ILocator field, string location)
    {
        await TypeLikeAUserAsync(field, location);

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

    /// <summary>
    /// Types part of a place name, waits for the autocomplete and picks the named suggestion.
    /// This is the path the brief asks for: "using the widget's autocomplete".
    /// </summary>
    public async Task TypeAndChooseAsync(ILocator field, string typed, string suggestion)
    {
        await TypeLikeAUserAsync(field, typed);

        // The list can show the same station more than once (stop and station entries),
        // so take the first match, as a user would.
        await Suggestion(suggestion).First.ClickAsync();
    }

    public async Task ChoosePreferenceAsync(string preference)
    {
        await EditPreferencesButton.ClickAsync();
        await PreferenceLabel(preference).ClickAsync();
    }

    // One key at a time, so the autocomplete reacts as it would to a person typing.
    private static async Task TypeLikeAUserAsync(ILocator field, string text)
    {
        await field.ClickAsync();
        await field.FillAsync(string.Empty);
        await field.PressSequentiallyAsync(text, new() { Delay = 50 });
    }

    public async Task PlanJourneyAsync()
    {
        await PlanJourneyButton.ClickAsync();
    }
}
