# Requirements and Test Basis Analysis

| | |
| --- | --- |
| **Project** | TfL Journey Planner UI test automation (STA Coding Challenge) |
| **Author** | Tangi Fominyam |
| **Version** | 0.1 (draft) |
| **Date** | 9 October 2026 |
| **Test basis** | *STA Coding Challenge – Web* brief, received 9 October 2026; the live Journey Planner at https://tfl.gov.uk/plan-a-journey |

## 1. Purpose

Before writing any automation I wanted the brief broken down into requirements I could test and trace.
This document lists them, records where the brief is open to interpretation and what I decided, and
notes the risks that could make the tests unreliable.

Every scenario in the feature files carries the tags of the requirements it covers (for example `@REQ-08`),
so a reviewer can go from any test back to the brief, or from any requirement to its tests.

## 2. Requirements

Priority reflects how much a real user would be affected if the behaviour broke: **High** means a
journey can't be planned or the information is wrong, **Medium** means a degraded experience, **Low** means
an edge case with a clear workaround.

### 2.1 Functional requirements (what the tests must verify)

| ID | Requirement | Priority | Covered by |
| --- | --- | --- | --- |
| REQ-08 | A valid journey can be planned from Leicester Square Underground Station to Covent Garden Underground Station, with both locations selected from the autocomplete suggestions rather than typed in full | High | Scenario: plan a valid journey |
| REQ-09 | The results show both a walking time and a cycling time | High | Scenario: plan a valid journey |
| REQ-10 | After planning, "Edit preferences" can be used to select routes with least walking and update the journey | Medium | Scenario: least walking |
| REQ-11 | The updated journey shows a valid journey time | Medium | Scenario: least walking |
| REQ-12 | "View Details" opens the full journey details | Medium | Scenario: access information |
| REQ-13 | Complete access information is shown for Covent Garden Underground Station | High | Scenario: access information |
| REQ-14 | No results are given when one or more locations are invalid | Medium | Scenario Outline: invalid journey (3 examples) |
| REQ-15 | A journey cannot be planned when no locations are entered | Low | Scenario: no locations |

### 2.2 Technical constraints

| ID | Constraint | How it is met |
| --- | --- | --- |
| REQ-01 | Tests written in C# | .NET 10 test project with NUnit |
| REQ-02 | Tests written in Gherkin | Reqnroll feature files |
| REQ-03 | Playwright and "Chromedriver" .NET libraries used for UI automation | Microsoft.Playwright against Google Chrome (see A1) |
| REQ-07 | At least 5 tests | 7 tests: 4 scenarios plus a Scenario Outline with 3 examples |

### 2.3 Deliverables

| ID | Deliverable | Where |
| --- | --- | --- |
| REQ-04 | Code in a public Git repository | This repository |
| REQ-05 | README explaining development decisions | `README.md`, with detail in `docs/` |
| REQ-06 | AI disclaimer if AI was used to write code | "Use of AI" section in `README.md` |
| REQ-16 | Additional functional and non-functional scenarios, listed but not automated | `docs/ADDITIONAL-SCENARIOS.md` |
| REQ-17 | Submission within 5 working days | Due Friday 16 October 2026 |

## 3. Ambiguities and my interpretation

| ID | What the brief leaves open | How I've read it | Status |
| --- | --- | --- | --- |
| A1 | It names both Playwright and "Chromedriver". Playwright drives the browser itself and doesn't use ChromeDriver, which is Selenium's driver. | Use Playwright for .NET and run it against Google Chrome. Adding Selenium as well would mean two drivers doing the same job. | Clarification requested by email, 9 Oct |
| A2 | It doesn't say where the walking and cycling times appear. | Find them during exploratory testing and assert them wherever the results page shows them. | Open, resolved in exploration |
| A3 | "Validate the journey time" gives no expected value, and the real value changes with live service data. | Check the time is shown, correctly formatted and within a sensible range. An exact value would make the test flaky. | Decided |
| A4 | "Complete" access information isn't defined. | List the access fields the live page shows for Covent Garden in the exploratory notes, then assert every one of them. | Open, resolved in exploration |
| A5 | It doesn't say what "does not provide results" looks like. | Record the actual behaviour during exploration (error message, "did you mean" page, or empty results) and assert exactly that. | Open, resolved in exploration |

## 4. Risks to the tests

| Risk | Effect | Mitigation |
| --- | --- | --- |
| Live service data (times, disruptions) changes from run to run | Exact-value assertions fail at random | Assert format and range, not exact values (A3) |
| Cookie consent banner covers the page | Clicks land on the banner instead of the widget | Dismiss it in a hook before every scenario |
| TfL changes the page markup | Selectors break | Prefer user-facing locators (role, label, text) over CSS and XPath; keep all selectors in page objects |
| Slow responses from the journey planner | Timeouts | Rely on Playwright's built-in waiting; no fixed sleeps |
| State leaking between scenarios | Tests pass alone but fail together | Fresh browser context for every scenario; no scenario depends on another |
| Bot detection when running headless or in CI | Requests blocked | Run against branded Chrome; record behaviour in CI and document any limits found |

## 5. Out of scope

- Automating the additional scenarios in REQ-16. The brief asks for them to be listed only.
- Browsers other than Chrome. Cross-browser testing is listed as an additional scenario.
- Performance and load testing of the journey planner.

## 6. Revision history

| Version | Date | Change |
| --- | --- | --- |
| 0.1 | 9 Oct 2026 | First draft from the brief, before exploratory testing |
