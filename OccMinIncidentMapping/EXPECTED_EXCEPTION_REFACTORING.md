# ExpectedException Attribute Removal - SonarQube Compliance

## Overview

Replaced the obsolete `[ExpectedException]` attribute with modern `Assert.ThrowsExceptionAsync` for asynchronous exception testing, addressing SonarQube code quality concerns.

## Why This Change?

The `[ExpectedException]` attribute has several limitations:

### ? Problems with ExpectedException
1. **Poor semantics** - Exception expectation is separated from the action
2. **Less readable** - Difficult to understand what code causes the exception
3. **Limited control** - Cannot verify exception details (message, properties)
4. **Async issues** - Less reliable with async/await patterns
5. **Maintenance burden** - Harder to maintain and debug tests
6. **SonarQube flag** - Flagged as anti-pattern in code quality tools

### ? Benefits of Assert.ThrowsExceptionAsync
1. **Clear intent** - Exception expectation is explicit in the assertion
2. **Better readability** - Action and assertion are together
3. **Rich assertions** - Can verify exception message and properties
4. **Async-friendly** - Designed for async/await patterns
5. **Debuggable** - Easier to understand what went wrong
6. **SonarQube approved** - Follows modern testing best practices

## Changes Made

### File 1: Tests.Unit/Authentication/JwtAuthenticationServiceTests.cs

**Before:**
```csharp
[TestMethod]
[ExpectedException(typeof(InvalidOperationException))]
public async Task GenerateTokenAsync_WithMissingSecretKey_ThrowsException()
{
    // Arrange
    var config = new Mock<IConfiguration>();
    config.Setup(x => x["Jwt:SecretKey"]).Returns((string?)null);
    var service = new JwtAuthenticationService(config.Object, _mockLogger.Object);

    // Act
    await service.GenerateTokenAsync("user", "Admin");
}
```

**After:**
```csharp
[TestMethod]
public async Task GenerateTokenAsync_WithMissingSecretKey_ThrowsException()
{
    // Arrange
    var config = new Mock<IConfiguration>();
    config.Setup(x => x["Jwt:SecretKey"]).Returns((string?)null);
    var service = new JwtAuthenticationService(config.Object, _mockLogger.Object);

    // Act & Assert
    await Assert.ThrowsExceptionAsync<InvalidOperationException>(
        async () => await service.GenerateTokenAsync("user", "Admin"));
}
```

**Tests Updated:** 2
- `GenerateTokenAsync_WithMissingSecretKey_ThrowsException()`
- `GenerateTokenAsync_WithShortSecretKey_ThrowsException()`

### File 2: Tests.Unit/Authentication/LoginCommandHandlerTests.cs

**Before:**
```csharp
[TestMethod]
[ExpectedException(typeof(UnauthorizedAccessException))]
public async Task Handle_WithInvalidCredentials_ThrowsUnauthorizedAccessException()
{
    // Arrange & Act
    // ...
    await _handler.Handle(command, CancellationToken.None);
}
```

**After:**
```csharp
[TestMethod]
public async Task Handle_WithInvalidCredentials_ThrowsUnauthorizedAccessException()
{
    // Arrange
    // ...
    
    // Act & Assert
    await Assert.ThrowsExceptionAsync<UnauthorizedAccessException>(
        async () => await _handler.Handle(command, CancellationToken.None));
}
```

**Tests Updated:** 2
- `Handle_WithInvalidCredentials_ThrowsUnauthorizedAccessException()`
- `Handle_WithNullUsername_ThrowsUnauthorizedAccessException()`

## Comparison

### Code Readability

**ExpectedException Pattern:**
```csharp
[ExpectedException(typeof(InvalidOperationException))]  // Exception expectation at top
public async Task TestMethod()
{
    // Action that throws
    await service.DoSomething();
    // Exception expected but not obvious from reading the code
}
```

**ThrowsExceptionAsync Pattern:**
```csharp
public async Task TestMethod()
{
    // Act & Assert - clearly shows exception is expected
    await Assert.ThrowsExceptionAsync<InvalidOperationException>(
        async () => await service.DoSomething());
}
```

### Advanced Assertions

**ExpectedException:** Limited to exception type only
```csharp
[ExpectedException(typeof(InvalidOperationException))]
public void TestMethod()
{
    // Can only verify exception type
}
```

**ThrowsExceptionAsync:** Can verify exception details
```csharp
[TestMethod]
public async Task TestMethod()
{
    var ex = await Assert.ThrowsExceptionAsync<InvalidOperationException>(
        async () => await service.DoSomething());
    
    // Can verify exception details
    Assert.AreEqual("Expected error message", ex.Message);
    Assert.IsNotNull(ex.InnerException);
}
```

## Test Results

? **Build Status:** Successful
? **Unit Tests:** 30/30 Passing
? **Test Duration:** 228 ms
? **Code Quality:** Improved

```
Test summary: total: 30, failed: 0, succeeded: 30, skipped: 0
```

## Modified Files

| File | Tests Updated | Exception Types |
|------|---|---|
| `Tests.Unit/Authentication/JwtAuthenticationServiceTests.cs` | 2 | InvalidOperationException |
| `Tests.Unit/Authentication/LoginCommandHandlerTests.cs` | 2 | UnauthorizedAccessException |
| **Total** | **4** | **2 types** |

## Best Practices Applied

### ? Pattern: Assert.ThrowsExceptionAsync

For async methods that should throw exceptions:
```csharp
[TestMethod]
public async Task MethodUnderTest_WithInvalidInput_ThrowsException()
{
    // Act & Assert
    var ex = await Assert.ThrowsExceptionAsync<SpecificException>(
        async () => await MethodUnderTest());
    
    // Optional: Verify exception details
    Assert.AreEqual("Expected message", ex.Message);
}
```

### ? Pattern: Assert.ThrowsException

For synchronous methods that should throw exceptions:
```csharp
[TestMethod]
public void MethodUnderTest_WithInvalidInput_ThrowsException()
{
    // Act & Assert
    var ex = Assert.ThrowsException<SpecificException>(
        () => MethodUnderTest());
    
    // Optional: Verify exception details
    Assert.AreEqual("Expected message", ex.Message);
}
```

## Migration Guide

If you have other tests using `ExpectedException`:

### Step 1: Identify all uses
```bash
grep -r "ExpectedException" --include="*.cs" .
```

### Step 2: Replace pattern

For **async methods:**
```csharp
// Before
[ExpectedException(typeof(MyException))]
public async Task TestAsync() { ... }

// After
public async Task TestAsync()
{
    await Assert.ThrowsExceptionAsync<MyException>(
        async () => { ... });
}
```

For **synchronous methods:**
```csharp
// Before
[ExpectedException(typeof(MyException))]
public void Test() { ... }

// After
public void Test()
{
    Assert.ThrowsException<MyException>(() => { ... });
}
```

### Step 3: Verify tests still pass
```bash
dotnet test
```

## Compliance

This change addresses:
- ? **SonarQube** - Code smell S1989 (ExpectedException anti-pattern)
- ? **Microsoft** - Recommended testing patterns
- ? **Industry Standards** - Modern unit testing best practices
- ? **Code Quality** - Improved readability and maintainability

## Further Reading

- [Microsoft Docs: Assert.ThrowsExceptionAsync](https://docs.microsoft.com/en-us/dotnet/api/microsoft.visualstudio.testtools.unittesting.assert.throwsexceptionasync)
- [SonarQube: ExpectedException Anti-Pattern](https://rules.sonarsource.com/csharp/RSPEC-3415)
- [xUnit Best Practices](https://xunit.net/docs/comparisons)
- [NUnit Best Practices](https://docs.nunit.org/articles/nunit/getting-started/assertions.html)

## Summary

? **Status:** COMPLETE
? **Tests:** ALL PASSING (30/30)
? **Code Quality:** IMPROVED
? **SonarQube:** COMPLIANT

All `ExpectedException` attributes have been replaced with `Assert.ThrowsExceptionAsync` for modern, readable, and maintainable exception testing.
