using Microsoft.Playwright;
using Reqnroll;

namespace TflJourneyPlanner.UiTests.Pages;

/// <summary>
/// The cookie consent banner TfL shows to every new visitor. Every scenario starts with a
/// fresh browser context, so the banner appears each time and is dismissed before testing.
/// </summary>
public sealed class CookieBanner
{
    private readonly IPage _page;
    private readonly IReqnrollOutputHelper _output;

    public CookieBanner(IPage page, IReqnrollOutputHelper output)
    {
        _page = page;
        _output = output;
    }

    private ILocator AcceptEssentialButton =>
        _page.GetByRole(AriaRole.Button, new() { Name = "Accept only essential cookies" });

    // The dark layer behind the banner. It has no role, so it is found by its id.
    private ILocator Overlay => _page.Locator("#cb-cookieoverlay");

    public async Task AcceptEssentialCookiesIfShownAsync()
    {
        try
        {
            await AcceptEssentialButton.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 10000 });
        }
        catch (TimeoutException)
        {
            return; // No banner this time: nothing to dismiss.
        }

        await AcceptEssentialButton.ClickAsync();
        await AcceptEssentialButton.WaitForAsync(new() { State = WaitForSelectorState.Hidden });

        // Sometimes the overlay stays on screen after the banner has gone and blocks every
        // click (Issue 5 in EXPLORATORY-NOTES.md). A user would reload the page, so the test
        // does the same, and says so in the output rather than hiding it.
        try
        {
            await Overlay.WaitForAsync(new() { State = WaitForSelectorState.Hidden, Timeout = 10000 });
        }
        catch (TimeoutException)
        {
            _output.WriteLine("Cookie overlay still covering the page 10s after accepting cookies. Reloading the page (Issue 5).");
            await _page.ReloadAsync();
            await Overlay.WaitForAsync(new() { State = WaitForSelectorState.Hidden });
        }
    }
}
