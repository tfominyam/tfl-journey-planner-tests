using Reqnroll;
using TflJourneyPlanner.UiTests.Pages;
using static Microsoft.Playwright.Assertions;

namespace TflJourneyPlanner.UiTests.StepDefinitions;

[Binding]
public sealed class NavigationSteps
{
    private readonly JourneyPlannerPage _journeyPlanner;

    public NavigationSteps(JourneyPlannerPage journeyPlanner)
    {
        _journeyPlanner = journeyPlanner;
    }

    [Given("I am on the Plan a journey page")]
    public async Task GivenIAmOnThePlanAJourneyPage()
    {
        await _journeyPlanner.OpenAsync();
    }

    [Then("the Plan a journey page is shown")]
    public async Task ThenThePlanAJourneyPageIsShown()
    {
        await Expect(_journeyPlanner.Heading).ToBeVisibleAsync();
    }
}
