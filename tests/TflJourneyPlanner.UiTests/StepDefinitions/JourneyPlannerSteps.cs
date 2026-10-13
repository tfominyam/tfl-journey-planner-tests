using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Reqnroll;
using TflJourneyPlanner.UiTests.Pages;
using static Microsoft.Playwright.Assertions;

namespace TflJourneyPlanner.UiTests.StepDefinitions;

[Binding]
public sealed class JourneyPlannerSteps
{
    private readonly IPage _page;
    private readonly JourneyPlannerPage _journeyPlanner;

    public JourneyPlannerSteps(IPage page, JourneyPlannerPage journeyPlanner)
    {
        _page = page;
        _journeyPlanner = journeyPlanner;
    }

    [When("I plan the journey without entering a From or To location")]
    public async Task WhenIPlanTheJourneyWithoutLocations()
    {
        // Prove the precondition rather than assume it: both fields start empty.
        await Expect(_journeyPlanner.FromInput).ToBeEmptyAsync();
        await Expect(_journeyPlanner.ToInput).ToBeEmptyAsync();

        await _journeyPlanner.PlanJourneyAsync();
    }

    [Then("I see the message {string}")]
    public async Task ThenISeeTheMessage(string message)
    {
        await Expect(_journeyPlanner.Message(message)).ToBeVisibleAsync();
    }

    [Then("no journey results are shown")]
    public async Task ThenNoJourneyResultsAreShown()
    {
        // REQ-15: validation runs in the browser, so the planner never leaves the page.
        // REQ-14 lands on the results page, so this check is extended when REQ-14 is built.
        await Expect(_page).Not.ToHaveURLAsync(new Regex("/plan-a-journey/results"));
        await Expect(_journeyPlanner.ResultsHeading).ToBeHiddenAsync();
        await Expect(_journeyPlanner.Heading).ToBeVisibleAsync();
    }
}
