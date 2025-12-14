Feature: Incident Management
  As a system user
  I want to manage incidents
  So that I can track hazard events

  Scenario: Create incident with valid data
    Given I am authenticated as "admin"
    And I have incident data with:
      | Field       | Value      |
      | Latitude    | 40.7128    |
      | Longitude   | -74.0060   |
      | Hazard      | Flood      |
      | Status      | Ongoing    |
      | Description | Test flood |
    When I submit the incident creation request
    Then the incident should be created successfully
    And I should receive the incident ID
    And the response status should be 201

  Scenario: Create incident with invalid latitude
    Given I am authenticated as "admin"
    And I have incident data with invalid latitude 95
    When I submit the incident creation request
    Then I should receive a validation error
    And the error should mention latitude

  Scenario: Create incident with invalid longitude
    Given I am authenticated as "admin"
    And I have incident data with invalid longitude 200
    When I submit the incident creation request
    Then I should receive a validation error
    And the error should mention longitude

  Scenario: Create incident without authentication
    Given I am not authenticated
    And I have valid incident data
    When I submit the incident creation request
    Then I should receive a 401 Unauthorized error

  Scenario: Retrieve all incidents
    Given I am authenticated as "admin"
    When I request all incidents
    Then I should receive a list of incidents
    And the response status should be 200

  Scenario: Incident with maximum description length
    Given I am authenticated as "admin"
    And I have an incident with 500 character description
    When I submit the incident creation request
    Then the incident should be created successfully

  Scenario: Incident with exceeded description length
    Given I am authenticated as "admin"
    And I have an incident with 501 character description
    When I submit the incident creation request
    Then I should receive a validation error
    And the error should mention description length
