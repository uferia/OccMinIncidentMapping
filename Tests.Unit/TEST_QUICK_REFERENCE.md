# Test Quick Reference

## Quick Commands

### Run All Tests
```bash
dotnet test
```

### Run Specific Test Project
```bash
# Unit tests only
dotnet test Tests.Unit

# Feature tests only
dotnet test Tests.Features

# Integration tests only
dotnet test Tests.Integration
```

### Run Single Test Class
```bash
dotnet test --filter "ClassName=LoginCommandHandlerTests"
```

### Run Single Test Method
```bash
dotnet test --filter "FullyQualifiedName~LoginCommandHandlerTests.Handle_WithValidCredentials_ReturnsToken"
```

### Run with Verbose Output
```bash
dotnet test --verbosity detailed
```

### Run and Generate Coverage
```bash
dotnet test /p:CollectCoverage=true
```

### Watch Mode (Auto-run on changes)
```bash
dotnet watch test
```

## Test Projects Overview

```
Tests.Unit/
??? Authentication/
?   ??? LoginCommandHandlerTests.cs (3 tests)
?   ??? JwtAuthenticationServiceTests.cs (6 tests)
??? Validators/
    ??? CreateIncidentCommandValidatorTests.cs (7 tests)
    ??? LoginCommandValidatorTests.cs (8 tests)

Tests.Features/
??? Features/
?   ??? Authentication.feature (4 scenarios)
?   ??? IncidentManagement.feature (7 scenarios)
?   ??? Security.feature (5 scenarios)
??? StepDefinitions/
    ??? AuthenticationStepDefinitions.cs

Tests.Integration/
??? Controllers/
?   ??? AuthControllerIntegrationTests.cs (4 tests)
?   ??? IncidentsControllerIntegrationTests.cs (6 tests)
??? Security/
?   ??? SecurityIntegrationTests.cs (10 tests)
??? TESTING_GUIDE.md
```

## Test Execution by Category

### Authentication Tests (17 tests)
```bash
# Unit tests
dotnet test Tests.Unit --filter "ClassName~Authentication"

# Feature tests
dotnet test Tests.Features

# Integration tests
dotnet test Tests.Integration --filter "ClassName~Auth"
```

### Validation Tests (15 tests)
```bash
dotnet test Tests.Unit --filter "ClassName~Validator"
```

### Security Tests (10 tests)
```bash
dotnet test Tests.Integration --filter "ClassName~Security"
```

## Sample Test Runs

### Verify Authentication Works
```bash
dotnet test Tests.Unit --filter "ClassName~LoginCommandHandler"
# Expected: 3 passed
```

### Check JWT Implementation
```bash
dotnet test Tests.Unit --filter "ClassName~JwtAuthenticationService"
# Expected: 6 passed
```

### Test Incident Validation
```bash
dotnet test Tests.Unit --filter "ClassName~CreateIncidentCommand"
# Expected: 7 passed
```

### Test Login Validation
```bash
dotnet test Tests.Unit --filter "ClassName~LoginCommandValidator"
# Expected: 8 passed
```

### Verify API Security
```bash
dotnet test Tests.Integration --filter "ClassName~Security"
# Expected: 10 passed
```

## Test Credentials for Integration Tests

**Admin User**:
```
Username: admin
Password: admin123
```

**Regular User**:
```
Username: user
Password: user123
```

## Expected Test Data

### Valid Incident
```json
{
  "latitude": 40.7128,
  "longitude": -74.0060,
  "hazard": "Flood",
  "status": "Ongoing",
  "description": "Flooding in downtown area"
}
```

### Invalid Incident (exceeds description limit)
```json
{
  "latitude": 40.7128,
  "longitude": -74.0060,
  "hazard": "Flood",
  "status": "Ongoing",
  "description": "..." // 501+ characters
}
```

## Troubleshooting

### Tests Not Running
```bash
# Rebuild solution
dotnet build

# Clean and rebuild
dotnet clean
dotnet build
```

### Integration Tests Fail
- Ensure app settings are correct
- Check Port 5094 availability
- Verify Firebase credentials

### Slow Test Execution
```bash
# Run tests in parallel
dotnet test --parallel --max-cpu-count 4
```

### See Failed Test Details
```bash
dotnet test --verbosity detailed --logger "console;verbosity=detailed"
```

## CI/CD Integration

### GitHub Actions
```yaml
- name: Run Tests
  run: dotnet test --verbosity detailed
```

### Local Pre-commit Hook
```bash
#!/bin/bash
dotnet test
if [ $? -ne 0 ]; then
  echo "Tests failed!"
  exit 1
fi
```

## Performance Notes

- **Unit Tests**: ~1-2 seconds
- **Feature Tests**: ~3-5 seconds  
- **Integration Tests**: ~5-10 seconds
- **Total Suite**: ~10-15 seconds

## Key Test Metrics

| Metric | Value |
|--------|-------|
| Total Test Cases | 49+ |
| Unit Tests | 24 |
| Feature Scenarios | 11 |
| Integration Tests | 15+ |
| Code Coverage Target | 80%+ |
| All Tests Status | ? Pass |

## Common Test Patterns

### Testing Success Cases
```csharp
[Fact]
public async Task Method_ValidInput_ReturnsExpected()
{
    // Arrange
    var input = GetValidInput();
    
    // Act
    var result = await method.Execute(input);
    
    // Assert
    result.Should().NotBeNull();
}
```

### Testing Error Cases
```csharp
[Fact]
public async Task Method_InvalidInput_ThrowsException()
{
    // Arrange
    var input = GetInvalidInput();
    
    // Act & Assert
    await Assert.ThrowsAsync<ValidationException>(
        () => method.Execute(input)
    );
}
```

### Testing with Mocks
```csharp
var mockService = new Mock<IService>();
mockService
    .Setup(x => x.Method())
    .ReturnsAsync(expectedValue);
```

## Useful Resources

- [xUnit Docs](https://xunit.net/)
- [Moq Docs](https://github.com/moq/moq4)
- [FluentAssertions](https://fluentassertions.com/)
- [SpecFlow Docs](https://specflow.org/)
- [WebApplicationFactory Docs](https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests)

## Next Steps

1. ? Run all tests: `dotnet test`
2. ? Fix any failing tests
3. ? Aim for 80%+ code coverage
4. ? Add tests for new features
5. ? Integrate into CI/CD pipeline
