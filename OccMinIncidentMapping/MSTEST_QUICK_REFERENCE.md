# MSTest Quick Reference for OccMinIncidentMapping

## Test Class Structure

```csharp
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using FluentAssertions;

[TestClass]  // Mark class as containing tests
public class MyServiceTests
{
    private MyService _service;
    private Mock<IDependency> _mockDependency;

    [TestInitialize]  // Runs before each test
    public void Setup()
    {
        _mockDependency = new Mock<IDependency>();
        _service = new MyService(_mockDependency.Object);
    }

    [TestCleanup]  // Runs after each test (optional)
    public void Cleanup()
    {
        // Cleanup code here
    }

    [TestMethod]  // Marks a test method
    public async Task MyTest_WithCondition_ExpectsResult()
    {
        // Arrange
        var input = "test";

        // Act
        var result = await _service.DoSomethingAsync(input);

        // Assert
        result.Should().NotBeNull();
    }
}
```

---

## Attributes Comparison

### Basic Test Attributes

| Scenario | Code |
|----------|------|
| **Basic Test** | `[TestMethod] public void Test() { }` |
| **Async Test** | `[TestMethod] public async Task TestAsync() { }` |
| **Expected Exception** | `[TestMethod] [ExpectedException(typeof(ArgumentException))]` |
| **Test Initialization** | `[TestInitialize] public void Setup() { }` |
| **Test Cleanup** | `[TestCleanup] public void Cleanup() { }` |
| **Parameterized Test** | `[TestMethod] [DataRow(1)] [DataRow(2)]` |
| **Test Category** | `[TestMethod] [TestCategory("Authentication")]` |
| **Ignore Test** | `[TestMethod] [Ignore]` |

---

## Common Patterns

### 1. Mocking with Moq

```csharp
[TestMethod]
public async Task LoginAsync_WithValidCredentials_ReturnsToken()
{
    // Arrange
    var mockConfig = new Mock<IConfiguration>();
    mockConfig.Setup(x => x["Jwt:SecretKey"])
        .Returns("very-long-secret-key-at-least-32-chars");
    
    var service = new JwtAuthenticationService(mockConfig.Object);

    // Act
    var result = await service.GenerateTokenAsync("user", "Admin");

    // Assert
    result.Should().NotBeNullOrEmpty();
    mockConfig.Verify(x => x["Jwt:SecretKey"], Times.Once);
}
```

### 2. Parameterized Tests

```csharp
[TestMethod]
[DataRow("")]
[DataRow(null)]
[DataRow("   ")]
public async Task ValidateUsername_WithInvalidInput_ReturnsFalse(string username)
{
    var result = await _validator.ValidateAsync(username);
    result.Should().BeFalse();
}
```

### 3. Exception Testing

```csharp
[TestMethod]
[ExpectedException(typeof(ArgumentNullException))]
public async Task Constructor_WithNullDependency_ThrowsException()
{
    // This will pass if ArgumentNullException is thrown
    new MyService(null);
}
```

### 4. Fluent Assertions

```csharp
[TestMethod]
public void StringTests()
{
    var value = "hello";
    
    value.Should().NotBeNullOrEmpty();
    value.Should().Be("hello");
    value.Should().Contain("ell");
    value.Should().StartWith("hel");
    value.Should().HaveLength(5);
}

[TestMethod]
public void CollectionTests()
{
    var list = new[] { 1, 2, 3, 4, 5 };
    
    list.Should().NotBeEmpty();
    list.Should().HaveCount(5);
    list.Should().Contain(3);
    list.Should().AllSatisfy(x => x > 0);
}

[TestMethod]
public void BooleanTests()
{
    var result = true;
    
    result.Should().BeTrue();
    (1 == 1).Should().BeTrue();
}
```

---

## Running Tests

### Visual Studio Test Explorer
```
View ? Test Explorer (Ctrl+E, T)
- Run All Tests
- Run Selected Tests
- Debug Tests
- View Test Results
```

### Command Line
```bash
# Run all tests
dotnet test

# Run specific class
dotnet test --filter "ClassName=JwtAuthenticationServiceTests"

# Run with verbosity
dotnet test --verbosity detailed

# Run and generate coverage (requires coverage tool)
dotnet test /p:CollectCoverageMetrics=true
```

### Test Settings
```bash
# Run tests in parallel
dotnet test --parallel

# Run with detailed output
dotnet test --verbosity normal

# Stop on first failure
dotnet test -- RunConfiguration.StopOnFirstFailure=true
```

---

## Assertion Examples

### Object Assertions
```csharp
obj.Should().NotBeNull();
obj.Should().BeOfType<MyClass>();
obj.Should().Be(expectedObject);
```

### String Assertions
```csharp
str.Should().NotBeNullOrEmpty();
str.Should().Be("expected");
str.Should().Contain("substring");
str.Should().StartWith("prefix");
str.Should().EndWith("suffix");
str.Should().MatchRegex(@"^\d+$");
```

### Numeric Assertions
```csharp
num.Should().Be(5);
num.Should().BeGreaterThan(0);
num.Should().BeLessThan(10);
num.Should().BeInRange(0, 10);
```

### Collection Assertions
```csharp
collection.Should().NotBeEmpty();
collection.Should().HaveCount(5);
collection.Should().Contain(item);
collection.Should().OnlyContain(x => x > 0);
collection.Should().BeInAscendingOrder();
```

### DateTime Assertions
```csharp
date.Should().BeAfter(DateTime.Now);
date.Should().BeBefore(DateTime.Now.AddDays(1));
date.Should().BeCloseTo(expectedDate, TimeSpan.FromSeconds(1));
```

---

## Mock Verification Examples

```csharp
// Verify method was called
mock.Verify(x => x.Method(), Times.Once);

// Verify it was called multiple times
mock.Verify(x => x.Method(), Times.Exactly(3));

// Verify it was never called
mock.Verify(x => x.Method(), Times.Never);

// Verify call with specific argument
mock.Verify(x => x.Method(It.Is<string>(s => s == "value")));

// Verify call with any argument
mock.Verify(x => x.Method(It.IsAny<string>()));
```

---

## Test Naming Convention

**Format**: `MethodUnderTest_Scenario_ExpectedOutcome`

**Examples**:
- ? `GenerateTokenAsync_WithValidCredentials_ReturnsValidToken`
- ? `ValidateCredentialsAsync_WithInvalidPassword_ReturnsFalse`
- ? `Constructor_WithNullDependency_ThrowsArgumentNullException`
- ? `Test1` (not descriptive)
- ? `MyTest` (not specific enough)

---

## Common MSTest Issues & Solutions

| Issue | Solution |
|-------|----------|
| **Test doesn't run** | Make sure class has `[TestClass]` and method has `[TestMethod]` |
| **Setup code not running** | Use `[TestInitialize]` instead of constructor |
| **ExpectedException not working** | Ensure exception is thrown synchronously (not in Task) |
| **DataRow not working** | Make sure test method has parameters matching `[DataRow]` values |
| **Tests run slowly** | Consider running in parallel with `dotnet test --parallel` |

---

## Best Practices

1. ? **One assertion per test** (or logically related assertions)
2. ? **Descriptive test names** (use the naming convention)
3. ? **AAA Pattern**: Arrange, Act, Assert
4. ? **Mock external dependencies** (don't test integration in unit tests)
5. ? **Test edge cases** (null, empty, boundary values)
6. ? **Keep tests independent** (no test depends on another)
7. ? **Use TestInitialize for setup** (instead of constructors)
8. ? **Verify critical calls** (use mock verification)

---

## References

- [MSTest v2 Documentation](https://github.com/microsoft/testfx)
- [Moq Quick Start](https://github.com/moq/moq4/wiki/Quickstart)
- [FluentAssertions Documentation](https://fluentassertions.com/)
- [.NET Testing Best Practices](https://docs.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices)

---

**Last Updated**: 2024
**Version**: MSTest 3.1.1
**Status**: Ready for Use
