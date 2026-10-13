@journey-planner
Feature: Plan a journey with the TfL Journey Planner
  As a TfL customer
  I want to plan a journey between two places
  So that I know how to get there and what to expect along the way

  The scenarios cover the brief for the STA Coding Challenge. Each one is tagged with the
  requirement IDs it covers, which are defined in docs/REQUIREMENTS.md.

  Times shown by the planner come from live service data, so they are checked for format
  ("6 mins") rather than exact values.

  Background:
    Given I am on the Plan a journey page

  @REQ-08 @REQ-09
  Scenario: A valid journey shows walking and cycling times
    When I type "Leicester Sq" in the From field and choose "Leicester Square Underground Station"
    And I type "Covent Gar" in the To field and choose "Covent Garden Underground Station"
    And I plan the journey
    Then journey results are shown from "Leicester Square Underground Station" to "Covent Garden Underground Station"
    And a walking time is shown in minutes
    And a cycling time is shown in minutes

  @REQ-10 @REQ-11
  Scenario: Choosing routes with least walking updates the journey
    Given I have planned a journey from "Leicester Square Underground Station" to "Covent Garden Underground Station"
    When I change my preferences to "Routes with least walking"
    And I update the journey
    Then the results are shown for routes with least walking
    And each journey shows a journey time in minutes

  @REQ-12 @REQ-13
  Scenario: Journey details show complete access information for Covent Garden
    Given I have planned a journey from "Leicester Square Underground Station" to "Covent Garden Underground Station" with routes with least walking
    When I view the details of the first journey
    Then the access information at "Covent Garden Underground Station" includes:
      | Access        |
      | Up stairs     |
      | Up lift       |
      | Level walkway |
    And the accessibility details show the total walking time and the stairs, escalators and walkways on the route

  @REQ-14
  Scenario Outline: A journey with an invalid location is not planned (<Case>)
    When I plan a journey from "<From>" to "<To>"
    Then I am asked which location I meant by "<Unmatched>"
    And no journey results are shown

    Examples:
      | Case         | From                                 | To                                | Unmatched |
      | Invalid From | Xqzpwv123                            | Covent Garden Underground Station | Xqzpwv123 |
      | Invalid To   | Leicester Square Underground Station | Zzqxvt999                         | Zzqxvt999 |
      | Both invalid | Abcxyz111                            | Qwzzpp222                         | Abcxyz111 |

  @REQ-15
  Scenario: A journey cannot be planned without locations
    When I plan the journey without entering a From or To location
    Then I see the message "The From field is required."
    And I see the message "The To field is required."
    And no journey results are shown
