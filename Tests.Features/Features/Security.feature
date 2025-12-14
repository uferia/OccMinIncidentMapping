Feature: API Security
  As a security-conscious user
  I want the API to enforce security measures
  So that my data and access are protected

  Scenario: Security headers are present in responses
    Given I make a request to the API
    When I receive the response
    Then the response should have X-Frame-Options header
    And the response should have X-Content-Type-Options header
    And the response should have X-XSS-Protection header
    And the response should have Content-Security-Policy header

  Scenario: CORS policy is enforced
    Given I make a request from an allowed origin
    When I include the request
    Then the response should allow the request
    And I should receive the data

  Scenario: CORS policy blocks unauthorized origins
    Given I make a request from an unauthorized origin
    When I include the request
    Then the response should block the request
    And I should not receive the data

  Scenario: Large request payloads are rejected
    Given I prepare a request with 11 MB payload
    When I submit the request
    Then I should receive a 413 Payload Too Large error

  Scenario: Invalid JWT tokens are rejected
    Given I have an invalid JWT token
    When I include the token in the Authorization header
    And I make a request to a protected endpoint
    Then I should receive a 401 Unauthorized error

  Scenario: Expired tokens are rejected
    Given I have an expired JWT token
    When I include the expired token in the Authorization header
    And I make a request to a protected endpoint
    Then I should receive a 401 Unauthorized error

  Scenario: Error messages do not expose sensitive information
    Given I trigger an unhandled exception
    When I receive the error response
    Then the response should not contain stack traces
    And the response should contain a generic error message
