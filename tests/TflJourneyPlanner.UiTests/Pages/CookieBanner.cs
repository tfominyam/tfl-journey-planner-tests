using Microsoft.Playwright;

namespace TflJourneyPlanner.UiTests.Pages;

/// <summary>
/// The cookie consent banner TfL shows to every new visitor. Every scenario starts with a
/// fresh browser context, so the banner appears each time and is dismissed before testing.
/// </summary>
public sealed class CookieBanner
{
    private readonly IPage _page;

    public CookieBanner(IPage page)
    {
        _page = page;
    }

    private ILocator AcceptEssentialButton =>
        _page.GetByRole(AriaRole.Button, new() { Name = "Accept only essential cookies" });

    public async Task AcceptEssentialCookiesIfShownAsync()
    {
        try
        {
            await AcceptEssentialButton.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5000 });
        }
        catch (TimeoutException)
        {
            return; // No banner this time: nothing to dismiss.
        }

        await AcceptEssentialButton.ClickAsync();
        await AcceptEssentialButton.WaitForAsync(new() { State = WaitForSelectorState.Hidden });
    }
}
