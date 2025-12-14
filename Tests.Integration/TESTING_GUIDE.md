# Testing Guide

## Overview

The OccMin Incident Mapping application includes comprehensive testing across three levels:
1. **Unit Tests** - Test individual components in isolation
2. **Feature Tests** - Test business scenarios using BDD/SpecFlow
3. **Integration Tests** - Test API endpoints and system interactions

## Test Projects

### Tests.Unit
- **Purpose**: Test business logic and individual components
- **Framework**: xUnit + Moq + FluentAssertions
- **Coverage**: Authentication, Validation, Services

### Tests.Features
- **Purpose**: Test user-facing features using BDD approach
- **Framework**: SpecFlow + xUnit + FluentAssertions
- **Coverage**: Authentication flows, Incident management, Security

### Tests.Integration
- **Purpose**: Test API endpoints and full request/response cycles
- **Framework**: xUnit + WebApplicationFactory + FluentAssertions
- **Coverage**: Controllers, Security headers, CORS, Authorization

## Running Tests

### Run All Tests
```bash
dotnet test
```

### Run Specific Test Project
```bash
dotnet test Tests.Unit
dotnet test Tests.Features
dotnet test Tests.Integration
```

### Run Specific Test Class
```bash
dotnet test Tests.Unit --filter "ClassName=LoginCommandHandlerTests"
```

### Run with Verbose Output
```bash
dotnet test --verbosity detailed
```

### Generate Coverage Report
```bash
dotnet test /p:CollectCoverage=true
```

## Test Structure

### Unit Tests

#### Authentication Tests
**File**: `Tests.Unit\Authentication\LoginCommandHandlerTests.cs`
- Valid credentials return token
- Invalid credentials throw exception
- Edge cases handling

**File**: `Tests.Unit\Authentication\JwtAuthenticationServiceTests.cs`
- Token generation with valid claims
- Token expiration validation
- Credential validation logic

#### Validator Tests
**File**: `Tests.Unit\Validators\CreateIncidentCommandValidatorTests.cs`
- Valid incident data passes validation
- Invalid latitude/longitude rejected
- Description length constraints
- Boundary value testing

**File**: `Tests.Unit\Validators\LoginCommandValidatorTests.cs`
- Username/password validation
- Length constraints
- Empty value handling

### Feature Tests

#### Authentication Feature
**File**: `Tests.Features\Features\Authentication.feature`
- Successful login with valid credentials
- Failed login scenarios
- Token expiration verification

#### Incident Management Feature
**File**: `Tests.Features\Features\IncidentManagement.feature`
- Create incident with valid/invalid data
- Unauthorized access prevention
- Description length constraints
- Retrieve incidents

#### Security Feature
**File**: `Tests.Features\Features\Security.feature`
- Security headers verification
- CORS policy enforcement
- Request size limits
- Token validation
- Error message sanitization

### Integration Tests

#### Auth Controller Integration Tests
**File**: `Tests.Integration\Controllers\AuthControllerIntegrationTests.cs`
- Login endpoint with valid/invalid credentials
- Input validation at API level
- Response format validation

#### Incidents Controller Integration Tests
**File**: `Tests.Integration\Controllers\IncidentsControllerIntegrationTests.cs`
- Create incident via API
- Get all incidents
- Authentication requirement
- Validation error handling

#### Security Integration Tests
**File**: `Tests.Integration\Security\SecurityIntegrationTests.cs`
- Security headers in responses
- CORS validation
- Authentication enforcement
- Authorization checks
- Error handling

## Test Scenarios

### Authentication Flow
```
1. User submits credentials
2. Credentials validated
3. JWT token generated
4. Token returned to client
5. Token used for subsequent requests
6. Server validates token signature
7. Request allowed if valid
```

### Incident Creation Flow
```
1. User authenticates and gets token
2. User submits incident data
3. Data validated (latitude, longitude, etc.)
4. Incident created in database
5. Incident ID returned
6. Response includes location header
```

### Security Validation
```
1. Request includes security headers
2. CORS origin validated
3. Request size checked
4. Authentication verified
5. Authorization confirmed
6. Business logic executed
7. Response includes security headers
```

## Test Data

### Valid Test Credentials
```
Admin User:
  Username: admin
  Password: admin123
  Role: Admin

Regular User:
  Username: user
  Password: user123
  Role: User
```

### Valid Test Incident
```
{
  "latitude": 40.7128,
  "longitude": -74.0060,
  "hazard": "Flood",
  "status": "Ongoing",
  "description": "Flooding in downtown area"
}
```

### Invalid Test Cases
```
// Invalid latitude (out of range)
{ "latitude": 95, ... }

// Invalid longitude (out of range)
{ "longitude": 200, ... }

// Exceeded description length
{ "description": new string('a', 501) }

// Empty username
{ "username": "", "password": "password" }

// Too short password
{ "username": "admin", "password": "123" }
```

## Assertions Used

### FluentAssertions Examples
```csharp
// String assertions
result.Should().NotBeNullOrEmpty();
result.Should().StartWith("Bearer ");
result.Should().Contain("token");

// Numeric assertions
latitude.Should().BeInRange(-90, 90);
longitude.Should().BeInRange(-180, 180);

// Collection assertions
errors.Should().BeEmpty();
errors.Should().Contain(e => e.PropertyName == "Username");

// HTTP status assertions
response.StatusCode.Should().Be(HttpStatusCode.OK);
response.IsSuccessStatusCode.Should().BeTrue();

// Header assertions
response.Headers.Should().Contain(h => h.Key == "X-Frame-Options");
```

## Mocking Patterns

### Mock Authentication Service
```csharp
var mockAuthService = new Mock<IAuthenticationService>();
mockAuthService
    .Setup(x => x.ValidateCredentialsAsync("admin", "admin123"))
    .ReturnsAsync(true);
mockAuthService
    .Setup(x => x.GenerateTokenAsync("admin", "Admin"))
    .ReturnsAsync("valid-token");
```

### Mock Configuration
```csharp
var mockConfig = new Mock<IConfiguration>();
mockConfig.Setup(x => x["Jwt:SecretKey"])
    .Returns("secret-key-here");
```

## Debugging Tests

### Enable Detailed Logging
```bash
dotnet test --verbosity detailed
```

### Run Single Test
```bash
dotnet test --filter "FullyQualifiedName~LoginCommandHandlerTests.Handle_WithValidCredentials"
```

### Debug in VS Code
```json
{
  "name": ".NET Core Launch (console)",
  "type": "csharp",
  "request": "launch",
  "preLaunchTask": "build",
  "program": "${workspaceFolder}/bin/Debug/net8.0/Tests.Unit.dll",
}
```

## Best Practices

### Naming Conventions
```csharp
[MethodUnderTest]_[Scenario]_[ExpectedResult]

// Examples:
Handle_WithValidCredentials_ReturnsToken
Validate_WithInvalidLatitude_ReturnsError
CreateIncident_WithoutAuthentication_ReturnsUnauthorized
```

### Arrange-Act-Assert Pattern
```csharp
[Fact]
public async Task TestMethod()
{
    // Arrange - Set up test data and mocks
    var input = new TestData();
    
    // Act - Execute the method under test
    var result = await method.Execute(input);
    
    // Assert - Verify the result
    result.Should().Be(expected);
}
```

### Test Isolation
- Each test should be independent
- No shared state between tests
- Clean up resources after each test
- Use fresh mocks for each test

### Coverage Targets
- Authentication/Authorization: >90%
- Validation: >85%
- Controllers: >80%
- Services: >85%

## Continuous Integration

Tests should run automatically:
- On every commit (pre-commit hook)
- On pull requests
- On scheduled builds
- Before production deployment

### GitHub Actions Example
```yaml
- name: Run Tests
  run: dotnet test --verbosity detailed
```

## Troubleshooting

### Tests Timeout
- Increase timeout in test settings
- Check for deadlocks in async code
- Verify database connectivity

### Mock Issues
- Verify mock setup matches method signature
- Check return type matches expected
- Use `Times.Once()` to verify calls

### Integration Test Failures
- Check appsettings.json configuration
- Verify database is available
- Check port availability for web server

## Additional Resources

- [xUnit Documentation](https://xunit.net/)
- [Moq Documentation](https://github.com/moq/moq4)
- [FluentAssertions Documentation](https://fluentassertions.com/)
- [SpecFlow Documentation](https://specflow.org/)
- [WebApplicationFactory Docs](https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests)

## Test Maintenance

### Regular Updates
- Update test dependencies monthly
- Review and refactor old tests
- Add tests for new features
- Remove obsolete tests

### Documentation
- Keep this guide updated
- Document new test scenarios
- Comment complex test logic
- Maintain test data documentation
