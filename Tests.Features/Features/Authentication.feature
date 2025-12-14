Feature: User Authentication
  As a user
  I want to authenticate with the system
  So that I can access protected resources

  Scenario: Successful login with valid credentials
    Given I have valid login credentials
    When I submit the login request
    Then I should receive a valid JWT token
    And the token should contain my username
    And the token should contain my role

  Scenario: Failed login with invalid password
    Given I have a username "admin"
    And I have an invalid password "wrongpassword"
    When I submit the login request
    Then I should receive an error message
    And I should not receive a token

  Scenario: Failed login with non-existent user
    Given I have a username "nonexistent"
    And I have a password "password123"
    When I submit the login request
    Then I should receive an unauthorized error

  Scenario: Token expiration
    Given I have obtained a valid token
    And the token will expire in 60 minutes
    When I check the token expiration
    Then the expiration should be set correctly
