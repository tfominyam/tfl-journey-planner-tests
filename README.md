# TfL Journey Planner UI Tests

UI test automation for the [TfL Journey Planner](https://tfl.gov.uk/plan-a-journey), written for the STA Coding Challenge (Senior Test Analyst, ref 5541).

C# . .NET 10 . Reqnroll (Gherkin) . NUnit . Playwright for .NET . Google Chrome

[![UI tests](https://github.com/tfominyam/tfl-journey-planner-tests/actions/workflows/ui-tests.yml/badge.svg)](https://github.com/tfominyam/tfl-journey-planner-tests/actions/workflows/ui-tests.yml)

## At a glance

| What                 | Result                                                                      |
| -------------------- | --------------------------------------------------------------------------- |
| Required tests       | All 6 areas in the brief automated: 7 tests plus a smoke test               |
| Stability            | 63 of 64 passed over 8 full runs, headed and headless (details below)       |
| Defects found        | 5 defects and observations, written up with steps and screenshots           |
| Additional scenarios | 28 listed in [`docs/ADDITIONAL-SCENARIOS.md`](docs/ADDITIONAL-SCENARIOS.md) |
| CI                   | GitHub Actions builds and lists the tests on every pull request             |

## What is tested

Every scenario is tagged with the requirement it covers. The requirements, the test data and the reasons for them are in [`docs/REQUIREMENTS.md`](docs/REQUIREMENTS.md).

| Brief                                             | Requirement    | Scenario                                                           |
| ------------------------------------------------- | -------------- | ------------------------------------------------------------------ |
| Valid journey using the autocomplete              | REQ-08         | A valid journey shows walking and cycling times                    |
| Walking and cycling times                         | REQ-09         | A valid journey shows walking and cycling times                    |
| Edit preferences, least walking, journey time     | REQ-10, REQ-11 | Choosing routes with least walking updates the journey             |
| View Details, access information at Covent Garden | REQ-12, REQ-13 | Journey details show complete access information for Covent Garden |
| Invalid locations                                 | REQ-14         | A journey with an invalid location is not planned (3 examples)     |
| No locations                                      | REQ-15         | A journey cannot be planned without locations                      |
| Framework check                                   | -              | The Plan a journey page opens (`@smoke`)                           |

The feature files are in [`tests/TflJourneyPlanner.UiTests/Features`](tests/TflJourneyPlanner.UiTests/Features).

## How to run

### Prerequisites

| Tool          | Version             | Check with         |
| ------------- | ------------------- | ------------------ |
| .NET SDK      | 10.0                | `dotnet --version` |
| Google Chrome | Any current version | `chrome://version` |
| Git           | Any                 | `git --version`    |

Node.js is not needed. The Playwright NuGet package carries its own driver, and the tests use the Chrome already installed on the machine, so there is no browser download.

### Command line

```
git clone https://github.com/tfominyam/tfl-journey-planner-tests.git
cd tfl-journey-planner-tests
dotnet test
```

A full run takes about three and a half minutes. Some useful variations:

| To run                        | Command                                             |
| ----------------------------- | --------------------------------------------------- |
| Only the smoke test           | `dotnet test --filter "Category=smoke"`             |
| One requirement               | `dotnet test --filter "Category=REQ-14"`            |
| With every step in the output | `dotnet test --logger "console;verbosity=detailed"` |

Headless (no browser window), in PowerShell:

```
$env:TFLTESTS_Headless = "true"
dotnet test
Remove-Item Env:TFLTESTS_Headless
```

### Visual Studio 2026

Open `TflJourneyPlanner.slnx`, build with Ctrl+Shift+B, then open Test Explorer (Ctrl+E, T) and run all, or filter with `Trait:smoke`. The Reqnroll extension (Extensions > Manage Extensions, search "Reqnroll") adds Gherkin colouring and go-to-step, but isn't needed to run the tests.

### Settings

Settings live in [`appsettings.json`](tests/TflJourneyPlanner.UiTests/appsettings.json). Any of them can be overridden with an environment variable starting `TFLTESTS_`, so nothing needs editing for a different machine or CI.

| Setting            | Default              | Purpose                                                 |
| ------------------ | -------------------- | ------------------------------------------------------- |
| `BaseUrl`          | `https://tfl.gov.uk` | Site under test                                         |
| `BrowserChannel`   | `chrome`             | Installed Chrome; leave empty for Playwright's Chromium |
| `Headless`         | `false`              | `true` hides the browser                                |
| `SlowMoMs`         | `0`                  | Pause between actions, handy when watching a run        |
| `DefaultTimeoutMs` | `15000`              | Used for both actions and assertions                    |
| `TraceOnFailure`   | `true`               | Keep a screenshot and trace when a scenario fails       |

### When a test fails

The failing scenario's screenshot and Playwright trace are saved to `tests/TflJourneyPlanner.UiTests/bin/Debug/net10.0/TestResults/failures`. Drag the `.zip` onto [trace.playwright.dev](https://trace.playwright.dev) to step through every action with a snapshot of the page before and after.

## Project structure

```
TflJourneyPlanner.slnx
.github/workflows/ui-tests.yml     CI
docs/
  REQUIREMENTS.md                  Requirements, test data, ambiguities, risks
  EXPLORATORY-NOTES.md             Exploratory session, defects, screenshots
  ADDITIONAL-SCENARIOS.md          Scenarios listed but not automated
tests/TflJourneyPlanner.UiTests/
  Features/                        Gherkin: JourneyPlanner.feature, Smoke.feature
  StepDefinitions/                 Step bindings, grouped Given / When / Then
  Pages/                           Page objects: JourneyPlannerPage, CookieBanner
  Hooks/BrowserHooks.cs            Browser lifecycle and failure evidence
  Support/TestSettings.cs          Settings from appsettings.json and environment
```

## How I worked

1. **Test basis first.** I broke the brief into numbered requirements, noted where it was open to interpretation and what I decided, and listed the risks before writing any code ([`REQUIREMENTS.md`](docs/REQUIREMENTS.md)).
2. **Explored the site by hand.** A time-boxed session against the live planner, with screenshots. It changed my test data and found defects ([`EXPLORATORY-NOTES.md`](docs/EXPLORATORY-NOTES.md)).
3. **Wrote the feature file before the code,** so the scenarios read as the requirements, not as the implementation.
4. **Built the framework and proved it with a smoke test** before automating any requirement.
5. **One requirement per pull request,** each checked against the live page with Playwright's recorder (codegen) and run with the earlier tests before merging.
6. **Hardened it.** Eight full runs, headed and headless, investigating every failure rather than re-running until green.

The full history is in the [pull requests](https://github.com/tfominyam/tfl-journey-planner-tests/pulls?q=is%3Apr).

## Design decisions

**Reqnroll with Playwright for .NET.** The brief asks for C# and Gherkin. Reqnroll is the maintained successor to SpecFlow. For the browser I chose Playwright over Selenium and ChromeDriver: it talks to Chrome directly, waits for elements automatically, gives each test an isolated browser context cheaply, and records traces. Using both would mean two drivers doing one job.

**Branded Chrome, not bundled Chromium.** The tests drive the Chrome a TfL customer would use, and there's nothing extra to download. Leaving `BrowserChannel` empty switches to Playwright's Chromium.

**Locators a user would recognise.** Elements are found by role and visible name (`GetByRole(Button, "Plan my journey")`, `GetByRole(Combobox, "From location")`), not by CSS ids or XPath. They survive markup changes and double as a light accessibility check. The one exception is the results summary box, which has no role or label, and the code says so.

**Page objects and context injection.** Locators live only in page objects. Step classes ask for a page object in their constructor, and Reqnroll creates and injects it, so no step builds its own browser or shares state through string keys.

**A fresh browser context per scenario.** One Chrome instance for the run, but every scenario gets new cookies and storage. Scenarios can't affect each other and can run in any order.

**Live data is checked for format and range, never exact values.** Walking, cycling and journey times must read as minutes between 1 and 60. Access details must show a number of stairs, escalators and walkways, whatever the number.

**Check what is there before checking what isn't.** "No journey results" passes instantly if the page hasn't loaded, so every scenario first asserts something that is shown (an error, a message) and only then that the results are absent.

**The stuck cookie overlay is handled, logged and reported, not hidden.** TfL's cookie overlay sometimes stays on screen after cookies are accepted and blocks every click (Issue 5). It happened in between a fifth and a half of scenarios, depending on the run. A Playwright locator handler waits up to ten seconds and, if the overlay is still there, reloads the page as a user would, and writes the page address to the test output. Each occurrence is visible in the logs, and the defect is written up.

**Both of Playwright's timeouts are configured.** Actions use the context timeout; `Expect` assertions have their own default of five seconds. Both are set from `DefaultTimeoutMs`, after one slow TfL response showed the difference.

**Evidence on failure.** Every scenario records a Playwright trace. It's kept, with a full-page screenshot, only when the scenario fails, so passing runs leave nothing behind.

**Clicking what a user clicks.** TfL draws its own radio buttons over the real input. The tests click the visible label, as a person would, and then assert the radio is ticked, rather than forcing a click on a hidden element.

## Results

Each full run is 8 tests and takes 3 to 3.5 minutes.

| Run | Mode     | Passed | Notes                                                  |
| --- | -------- | ------ | ------------------------------------------------------ |
| 1-3 | Headed   | 24/24  |                                                        |
| 4   | Headless | 7/8    | Cookie overlay stuck on the results page (Issue 5)     |
| 5   | Headless | 8/8    |                                                        |
| 6-7 | Headless | 16/16  | After the overlay handler was added                    |
| 8   | Headed   | 8/8    | Logged in detail: overlay cleared 4 times, all handled |

The one failure led to the fix: the overlay could stick on any page, not just the first.

## Defects and observations

Found during exploratory testing and automation. Full write-ups with steps, expected and actual results and screenshots are in [`EXPLORATORY-NOTES.md`](docs/EXPLORATORY-NOTES.md).

| #   | Finding                                                                 | Severity |
| --- | ----------------------------------------------------------------------- | -------- |
| 1   | Lift shown in the step icons but missing from the accessibility summary | Medium   |
| 2   | "More than one location" message with a blank entry                     | Medium   |
| 3   | Nonsense input is matched to real places                                | Low      |
| 4   | Cycling time reads "1 mins"                                             | Low      |
| 5   | Cookie overlay can stay on screen after accepting cookies               | Medium   |

The tests check what the requirements ask for. They don't bend to fit a defect: REQ-13 checks the access icons and the accessibility details, and the lift gap stays a reported defect.

## Continuous integration

[`.github/workflows/ui-tests.yml`](.github/workflows/ui-tests.yml) has two jobs.

| Job        | Runs                          | Does                                                              |
| ---------- | ----------------------------- | ----------------------------------------------------------------- |
| `build`    | Every push and pull request   | Restore, build, list the tests (8 expected)                       |
| `ui-tests` | By hand, from the Actions tab | Smoke test first, then the full suite; uploads results and traces |

Why the UI tests aren't automatic: when they first ran on GitHub's servers, TfL's bot protection showed a "Verification required" page instead of the planner. The smoke test caught it in 29 seconds and the journey tests were skipped, which is what it's there for. The tests now recognise that page and stop with a plain message, for example in [this manual run](https://github.com/tfominyam/tfl-journey-planner-tests/actions/runs/38597697001). They don't try to get round the check: it's TfL saying no to automated traffic from data centres, and that should be respected. In a team setting the UI job would run on an agent TfL allows, such as a self-hosted runner inside its network.

## Additional scenarios

The brief asks for further scenarios to be listed rather than automated. There are 28 in [`docs/ADDITIONAL-SCENARIOS.md`](docs/ADDITIONAL-SCENARIOS.md), grouped by area and prioritised, with the five I'd automate next and why.

## Use of AI

I used Claude, Anthropic's AI assistant, throughout this project, as a pair programmer and mentor.

- **Claude:** drafted most of the C# (hooks, page objects, step definitions), the CI workflow and first drafts of these documents and suggested causes and fixes when tests failed.
- **Me:** analysed the brief and decided the requirements and test data; did the exploratory testing and took the screenshots; recorded the live pages with Playwright's codegen; ran every test locally, headed and headless; checked each failure's screenshot, trace and call log; reviewed and merged every change through a pull request; and decided what went in, what didn't, and why.

I've read and understood every line in this repository.

## Author

Tangi Fominyam . ISTQB Certified Tester Foundation Level (CTFL v4.0)
