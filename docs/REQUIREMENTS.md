# Requirements and Test Basis Analysis

|                |                                                                                                              |
| -------------- | ------------------------------------------------------------------------------------------------------------ |
| **Project**    | TfL Journey Planner UI test automation (STA Coding Challenge)                                                |
| **Author**     | Tangi Fominyam                                                                                               |
| **Version**    | 0.3 (draft)                                                                                                  |
| **Date**       | 12 October 2026                                                                                              |
| **Test basis** | *STA Coding Challenge - Web* brief (9 Oct 2026); the live Journey Planner; exploratory session (12 Oct 2026) |

## 1. Purpose

Before writing any automation I wanted the brief broken down into requirements I could test and trace.
This document lists them, records where the brief is open to interpretation and what I decided, and
notes the risks that could make the tests unreliable.

Every scenario in the feature files carries the tags of the requirements it covers (for example `@REQ-08`),
so a reviewer can go from any test back to the brief, or from any requirement to its tests.

Version 0.3 reflects what I found in the exploratory session. The detail and screenshots are in
[`EXPLORATORY-NOTES.md`](EXPLORATORY-NOTES.md).

## 2. Requirements

Priority reflects how much a real user would be affected if the behaviour broke: **High** means a
journey can't be planned or the information is wrong, **Medium** means a degraded experience, **Low** means
an edge case with a clear workaround.

### 2.1 Functional requirements (what the tests must verify)

| ID     | Requirement                                                            | Priority | Scenario                     |
| ------ | ---------------------------------------------------------------------- | -------- | ---------------------------- |
| REQ-08 | Plan Leicester Square to Covent Garden, picking both from autocomplete | High     | Plan a valid journey         |
| REQ-09 | Results show a walking time and a cycling time                         | High     | Plan a valid journey         |
| REQ-10 | Edit preferences, choose least walking, update the journey             | Medium   | Least walking                |
| REQ-11 | The updated journey shows a valid journey time                         | Medium   | Least walking                |
| REQ-12 | View Details opens the full journey details                            | Medium   | Access information           |
| REQ-13 | Complete access information is shown for Covent Garden                 | High     | Access information           |
| REQ-14 | Invalid locations give no journey results                              | Medium   | Invalid journey (3 examples) |
| REQ-15 | No journey can be planned with both locations empty                    | Low      | No locations                 |

**Expected behaviour, confirmed by exploratory testing.** This is what the tests will check.

- **REQ-08:** suggestions appear after 2 or 3 characters. The results URL carries the stop IDs `FromId=1000135` and `ToId=1000056`.
- **REQ-09:** the "Walking and cycling" section shows both times as a number followed by "mins", for example "6 mins" and "1 mins".
- **REQ-10:** the option is labelled "Routes with least walking". After "Update journey" the URL includes `JourneyPreference=leastwalking`.
- **REQ-11:** each journey shows its time on the right, for example "11 mins".
- **REQ-12 and REQ-13:** "complete" access information is the step icons at Covent Garden ("Up stairs", "Up lift", "Level walkway") plus the Accessibility details list (total walking time, stairs, escalator and walkway counts).
- **REQ-14:** no journey results appear. The page says "We found more than one location matching '...'" and offers suggestions, and the stop ID for the invalid location is empty in the URL.
- **REQ-15:** the messages "The From field is required." and "The To field is required." appear.

### 2.2 Technical constraints

| ID     | Constraint                                   | How it is met                                          |
| ------ | -------------------------------------------- | ------------------------------------------------------ |
| REQ-01 | Tests written in C#                          | .NET 10 test project with NUnit                        |
| REQ-02 | Tests written in Gherkin                     | Reqnroll feature files                                 |
| REQ-03 | Playwright and "Chromedriver" .NET libraries | Microsoft.Playwright against Google Chrome (see A1)    |
| REQ-07 | At least 5 tests                             | 7 tests: 4 scenarios plus a 3-example Scenario Outline |

### 2.3 Deliverables

| ID     | Deliverable                                    | Where                               |
| ------ | ---------------------------------------------- | ----------------------------------- |
| REQ-04 | Code in a public Git repository                | This repository                     |
| REQ-05 | README explaining development decisions        | `README.md`, with detail in `docs/` |
| REQ-06 | AI disclaimer if AI was used to write code     | "Use of AI" section in `README.md`  |
| REQ-16 | Additional scenarios, listed but not automated | `docs/ADDITIONAL-SCENARIOS.md`      |
| REQ-17 | Submission within 5 working days               | Due Friday 16 October 2026          |

### 2.4 Test data

The brief gives the valid locations but not the invalid ones, so I chose them myself.

**Valid locations (REQ-08 to REQ-13)**

| Field | Typed          | Selected from suggestions            |
| ----- | -------------- | ------------------------------------ |
| From  | `Leicester Sq` | Leicester Square Underground Station |
| To    | `Covent Gar`   | Covent Garden Underground Station    |

Only part of each name is typed, because the brief says to select from the autocomplete rather than enter the full text.

**Invalid locations (REQ-14)**

| Case         | From                                 | To                                |
| ------------ | ------------------------------------ | --------------------------------- |
| Invalid From | `Xqzpwv123`                          | Covent Garden Underground Station |
| Invalid To   | Leicester Square Underground Station | `Zzqxvt999`                       |
| Both invalid | `Abcxyz111`                          | `Qwzzpp222`                       |

Why these values:

- **One example per field position.** From and To could be validated separately, so each is tested on its own and then both together. This is equivalence partitioning: each row stands for one class of input.
- **Random letters and digits, not misspellings.** A near miss like "Leicester Sqare" would probably resolve to the real station and wouldn't be invalid at all.
- **Typed, not selected.** A user who mistypes has no suggestion to pick, so the values are typed in full and submitted.
- **Each value used only once,** so a failing row points straight at its value.

**What exploration changed.** In version 0.2 I assumed nonsense strings couldn't match anything. They can: TfL's
fuzzy matching turned `Xqzpwv123` into "123Vegan" and "V12 Motorsport Store", and `Abcxyz111` into "ABC Pharmacy".
The values are still invalid for this purpose, because none of them resolves to a stop ID and no journey is planned.
So the REQ-14 test checks for the "more than one location" message and the absence of journey results. It doesn't
check for a "no results" message (there isn't one), or for the suggestions themselves, which can change.

**No locations (REQ-15):** both fields left empty, with nothing typed.

## 3. Ambiguities and my interpretation

| ID  | Question                                       | Status                                    |
| --- | ---------------------------------------------- | ----------------------------------------- |
| A1  | Playwright or "Chromedriver"?                  | Decided                                   |
| A2  | Where do the walking and cycling times appear? | Resolved by exploration                   |
| A3  | What does "validate the journey time" mean?    | Decided, confirmed by exploration         |
| A4  | What counts as "complete" access information?  | Resolved by exploration                   |
| A5  | What does "no results" look like?              | Resolved by exploration; approach changed |

- **A1.** Playwright drives the browser itself and doesn't use ChromeDriver, which is Selenium's driver. I'm using Playwright for .NET against Google Chrome. Adding Selenium as well would mean two drivers doing the same job.
- **A2.** They're in the "Walking and cycling" section of the results page.
- **A3.** The brief gives no expected value, and the real one changes with live data ("11 mins" during exploration). The test checks the time is shown, correctly formatted and within a sensible range. An exact value would make it flaky.
- **A4.** The step icons at Covent Garden plus the Accessibility details list (see REQ-13 above). Exploration also found that the two don't agree about the lift, which is logged as Issue 1 in the exploratory notes.
- **A5.** There's no "no results" message. The page asks "did you mean" instead. See section 2.4 for how this changed the test.

## 4. Risks to the tests

| Risk                                           | Mitigation                                               |
| ---------------------------------------------- | -------------------------------------------------------- |
| Live data (times, delays) changes between runs | Assert format and range, not exact values                |
| Cookie banner covers the page                  | Dismiss it in a hook before every scenario               |
| TfL changes the page markup                    | User-facing locators; all selectors kept in page objects |
| Slow journey planner responses                 | Playwright's built-in waiting; no fixed sleeps           |
| State leaks between scenarios                  | Fresh browser context per scenario; no shared state      |
| Bot detection in headless runs or CI           | Use branded Chrome; document any limits found            |
| Suggestions for invalid input change over time | Assert the message and no results, not the suggestions   |

## 5. Out of scope

- Automating the additional scenarios in REQ-16. The brief asks for them to be listed only.
- Browsers other than Chrome. Cross-browser testing is listed as an additional scenario.
- Performance and load testing of the journey planner.
- Testing for the defects found during exploration. They're reported in `EXPLORATORY-NOTES.md` instead.

## 6. Revision history

| Version | Date        | Change                                                                                                                         |
| ------- | ----------- | ------------------------------------------------------------------------------------------------------------------------------ |
| 0.3     | 12 Oct 2026 | Updated after exploratory testing: ambiguities resolved, expected behaviour added, test data reasoning corrected, one new risk |
| 0.2     | 12 Oct 2026 | Added test data (section 2.4)                                                                                                  |
| 0.1     | 9 Oct 2026  | First draft from the brief, before exploratory testing                                                                         |
