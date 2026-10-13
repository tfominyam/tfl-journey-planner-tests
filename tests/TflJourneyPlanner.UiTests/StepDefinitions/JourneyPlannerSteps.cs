using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Reqnroll;
using TflJourneyPlanner.UiTests.Pages;
using static Microsoft.Playwright.Assertions;

namespace TflJourneyPlanner.UiTests.StepDefinitions;

[Binding]
public sealed class JourneyPlannerSteps
{
    private static readonly Regex ResultsUrl = new("/plan-a-journey/results");

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

    [When("I plan a journey from {string} to {string}")]
    public async Task WhenIPlanAJourneyFromTo(string from, string to)
    {
        await _journeyPlanner.EnterLocationAsync(_journeyPlanner.FromInput, from);
        await _journeyPlanner.EnterLocationAsync(_journeyPlanner.ToInput, to);
        await _journeyPlanner.PlanJourneyAsync();

        await _page.WaitForURLAsync(ResultsUrl);
    }

    [Then("I see the message {string}")]
    public async Task ThenISeeTheMessage(string message)
    {
        await Expect(_journeyPlanner.Message(message)).ToBeVisibleAsync();
    }

    [Then("I am asked which location I meant by {string}")]
    public async Task ThenIAmAskedWhichLocationIMeantBy(string location)
    {
        await Expect(_journeyPlanner.DisambiguationMessage(location)).ToBeVisibleAsync();
    }

    [Then("no journey results are shown")]
    public async Task ThenNoJourneyResultsAreShown()
    {
        // Scenarios assert what the page does show first (a message), so by the time this
        // runs the page has settled and a count of zero means "none", not "not loaded yet".
        await Expect(_journeyPlanner.WalkingOption).ToHaveCountAsync(0);
        await Expect(_journeyPlanner.CyclingOption).ToHaveCountAsync(0);
    }
}
