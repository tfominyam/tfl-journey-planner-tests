# Additional Scenarios (not automated)

The brief asks for a list of further scenarios that would be worth testing but don't need automating for this challenge. These are mine, drawn from the exploratory session, the defects I found, and the risks in [`REQUIREMENTS.md`](REQUIREMENTS.md).

Priority uses the same scale as the requirements: **High** means a journey can't be planned or the information is wrong, **Medium** means a degraded experience, **Low** means an edge case with a clear workaround.

## Journey planning

| ID    | Scenario                                      | Expected                                   | Priority |
| ----- | --------------------------------------------- | ------------------------------------------ | -------- |
| AS-01 | Plan an arrive-by journey for later today     | Every journey arrives before that time     | High     |
| AS-02 | Use a postcode as a location (e.g. WC2E 9DD)  | Postcode resolves and a journey is planned | High     |
| AS-03 | Choose "Full step-free access"                | No journey includes stairs or escalators   | High     |
| AS-04 | Swap From and To with the swap button         | Locations swap and results reverse         | Medium   |
| AS-05 | Same place in From and To                     | Clear message, no journey                  | Medium   |
| AS-06 | Fill From only, leave To empty                | Only "The To field is required."           | Medium   |
| AS-07 | Untick every mode except Bus                  | Only bus journeys are shown                | Medium   |
| AS-08 | Set maximum walk time to 5 minutes            | No walking leg longer than 5 minutes       | Medium   |
| AS-09 | "Use my current location", permission allowed | Current location fills From                | Medium   |
| AS-10 | "Use my current location", permission denied  | Clear message, form still usable           | Medium   |
| AS-11 | Leave at a time in the past                   | Warned or moved to now                     | Low      |
| AS-12 | Travel via a third location                   | Every journey passes through it            | Low      |
| AS-13 | Earlier journeys and Later journeys           | Times move back and forward                | Low      |

## Input handling

| ID    | Scenario                                          | Expected                                 | Priority |
| ----- | ------------------------------------------------- | ---------------------------------------- | -------- |
| AS-14 | Text with HTML or script tags, e.g. `<b>Bank</b>` | Shown as plain text, never run           | Medium   |
| AS-15 | More than 100 characters (the field limit)        | Input stops at 100 characters            | Low      |
| AS-16 | Spaces around a name, e.g. "  Covent Gar  "       | Spaces ignored, suggestions still appear | Low      |

## Accessibility

| ID    | Scenario                                        | Expected                                          | Priority |
| ----- | ----------------------------------------------- | ------------------------------------------------- | -------- |
| AS-17 | Plan a journey with the keyboard only           | Every control reachable; Enter picks a suggestion | High     |
| AS-18 | Screen reader on the form and results           | Fields, errors and journeys announced clearly     | High     |
| AS-19 | Automated scan (axe) of the planner and results | No serious WCAG 2.2 AA failures                   | Medium   |
| AS-20 | Page at 200% zoom and 320px wide                | Nothing cut off, no sideways scrolling            | Medium   |

## Browsers, devices and resilience

| ID    | Scenario                                         | Expected                                   | Priority |
| ----- | ------------------------------------------------ | ------------------------------------------ | -------- |
| AS-21 | Journey planner fails or is slow (mocked)        | Clear error, no endless "Fetching results" | High     |
| AS-22 | Same suite in Firefox and WebKit (Safari engine) | Same results as Chrome                     | Medium   |
| AS-23 | Mobile size, e.g. 375 x 812                      | Form and results usable on a phone         | Medium   |
| AS-24 | Time from Plan my journey to results             | Results within an agreed limit             | Low      |

## Regression checks for the defects I found

| ID    | Defect (see [`EXPLORATORY-NOTES.md`](EXPLORATORY-NOTES.md))       | Check once fixed                            |
| ----- | ----------------------------------------------------------------- | ------------------------------------------- |
| AS-25 | Issue 1: lift in the step icons but not the accessibility summary | Summary lists every facility the icons show |
| AS-26 | Issue 2: "more than one location" with a blank entry              | No blank entries in the list                |
| AS-27 | Issue 4: "1 mins"                                                 | "1 min" for a single minute                 |
| AS-28 | Issue 5: cookie overlay stays after accepting                     | Overlay always clears after accepting       |

## What I would automate next, and why

1. **AS-21, journey planner fails or is slow.** Playwright can intercept the request and return an error or a delay, so this is fully repeatable and doesn't depend on live data. It covers a failure users will meet.
2. **AS-17, keyboard only.** It's a High accessibility risk, and Playwright can drive it with key presses.
3. **AS-03, full step-free access.** It builds on the REQ-12/13 steps and checks information that people who rely on it can't easily verify for themselves.
4. **AS-06, one field filled.** A small extension of REQ-15 that closes the obvious gap.
5. **AS-22, other browsers.** Playwright already supports them; the main cost is extra run time.

Some of these aren't good candidates for UI automation at all. AS-18 (screen readers) needs a person using real assistive technology, and AS-24 (timing) belongs in performance testing, where it can be measured properly rather than inferred from UI waits.
