@smoke
Feature: Framework smoke check
  Confirms the browser, configuration and cookie handling all work before any journey is planned.
  If this fails, look at the setup (browser, settings, network), not the journey scenarios.

  Scenario: The Plan a journey page opens
    Given I am on the Plan a journey page
    Then the Plan a journey page is shown
