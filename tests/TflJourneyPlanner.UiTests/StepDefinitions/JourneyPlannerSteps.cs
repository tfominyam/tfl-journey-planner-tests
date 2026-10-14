using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Reqnroll;
using Shouldly;
using TflJourneyPlanner.UiTests.Pages;
using static Microsoft.Playwright.Assertions;

namespace TflJourneyPlanner.UiTests.StepDefinitions;

[Binding]
public sealed class JourneyPlannerSteps
{
    private static readonly Regex ResultsUrl = new("/plan-a-journey/results");

    // A number followed by "min", "mins", "minute" or "minutes", with or without a space.
    private static readonly Regex MinutesPattern = new(@"\d+\s*min(ute)?s?\b", RegexOptions.IgnoreCase);

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
        await PlanAndWaitForResultsAsync();
    }

    [When("I type {string} in the {word} field and choose {string}")]
    public async Task WhenITypeInTheFieldAndChoose(string typed, string field, string suggestion)
    {
        var input = field switch
        {
            "From" => _journeyPlanner.FromInput,
            "To" => _journeyPlanner.ToInput,
            _ => throw new ArgumentException($"Unknown field '{field}'. Use From or To.")
        };

        await _journeyPlanner.TypeAndChooseAsync(input, typed, suggestion);

        // Prove the choice took: the field now holds the full place name, not what was typed.
        await Expect(input).ToHaveValueAsync(new Regex(Regex.Escape(suggestion)));
    }

    [Given("I have planned a journey from {string} to {string}")]
    public async Task GivenIHavePlannedAJourneyFromTo(string from, string to)
    {
        await WhenIPlanAJourneyFromTo(from, to);

        // A Given sets up state, so prove the state is there before the scenario carries on.
        // With the default "fastest routes" this short trip only offers walking and cycling,
        // so walking is the sign the results have loaded. Public transport options only
        // appear once "Routes with least walking" is chosen.
        await Expect(_journeyPlanner.WalkingOption).ToBeVisibleAsync();
    }

    [When("I change my preferences to {string}")]
    public async Task WhenIChangeMyPreferencesTo(string preference)
    {
        await _journeyPlanner.ChoosePreferenceAsync(preference);
        await Expect(_journeyPlanner.Preference(preference)).ToBeCheckedAsync();
    }

    [When("I update the journey")]
    public async Task WhenIUpdateTheJourney()
    {
        await _journeyPlanner.UpdateJourneyButton.ClickAsync();
    }

    [Then("the results are shown for routes with least walking")]
    public async Task ThenTheResultsAreShownForRoutesWithLeastWalking()
    {
        // Only appears once the updated results have loaded, so this also waits for the update.
        await Expect(_journeyPlanner.PreferenceSummary("Showing routes with least walking")).ToBeVisibleAsync();
    }

    [Then("each journey shows a journey time in minutes")]
    public async Task ThenEachJourneyShowsAJourneyTimeInMinutes()
    {
        var journeys = _journeyPlanner.JourneyOptions;
        await Expect(journeys.First).ToBeVisibleAsync();

        var count = await journeys.CountAsync();
        count.ShouldBeGreaterThan(0, "the results should list at least one journey");

        for (var i = 0; i < count; i++)
        {
            await Expect(journeys.Nth(i)).ToContainTextAsync(MinutesPattern);
        }
    }

    [When("I plan the journey")]
    public async Task WhenIPlanTheJourney()
    {
        await PlanAndWaitForResultsAsync();
    }

    [Then("journey results are shown from {string} to {string}")]
    public async Task ThenJourneyResultsAreShownFromTo(string from, string to)
    {
        await Expect(_journeyPlanner.ResultsHeading).ToBeVisibleAsync();
        await Expect(_journeyPlanner.ResultsSummary).ToContainTextAsync(from);
        await Expect(_journeyPlanner.ResultsSummary).ToContainTextAsync(to);
    }

    [Then("a walking time is shown in minutes")]
    public async Task ThenAWalkingTimeIsShownInMinutes()
    {
        // Live data, so check the format ("6 mins", "1 mins"), not the value.
        await Expect(_journeyPlanner.WalkingOption).ToBeVisibleAsync();
        await Expect(_journeyPlanner.WalkingOption).ToContainTextAsync(MinutesPattern);
    }

    [Then("a cycling time is shown in minutes")]
    public async Task ThenACyclingTimeIsShownInMinutes()
    {
        await Expect(_journeyPlanner.CyclingOption).ToBeVisibleAsync();
        await Expect(_journeyPlanner.CyclingOption).ToContainTextAsync(MinutesPattern);
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

    private async Task PlanAndWaitForResultsAsync()
    {
        await _journeyPlanner.PlanJourneyAsync();
        await _page.WaitForURLAsync(ResultsUrl);
    }
}
