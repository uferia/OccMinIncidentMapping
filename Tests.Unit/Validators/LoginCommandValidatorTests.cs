using Application.Validators.Auth;
using Core.Features.Auth.Commands;
using FluentAssertions;
using Xunit;

namespace Tests.Unit.Validators
{
    public class LoginCommandValidatorTests
    {
        private readonly LoginCommandValidator _validator;

        public LoginCommandValidatorTests()
        {
            _validator = new LoginCommandValidator();
        }

        [Fact]
        public async Task Validate_WithValidCredentials_ReturnsNoErrors()
        {
            // Arrange
            var command = new LoginCommand
            {
                Username = "admin",
                Password = "admin123"
            };

            // Act
            var result = await _validator.ValidateAsync(command);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async Task Validate_WithEmptyUsername_ReturnsError(string? username)
        {
            // Arrange
            var command = new LoginCommand
            {
                Username = username ?? "",
                Password = "password123"
            };

            // Act
            var result = await _validator.ValidateAsync(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Username");
        }

        [Fact]
        public async Task Validate_WithUsernameTooShort_ReturnsError()
        {
            // Arrange
            var command = new LoginCommand
            {
                Username = "ab", // Less than 3 characters
                Password = "password123"
            };

            // Act
            var result = await _validator.ValidateAsync(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Username");
        }

        [Fact]
        public async Task Validate_WithUsernameTooLong_ReturnsError()
        {
            // Arrange
            var command = new LoginCommand
            {
                Username = new string('a', 51), // More than 50 characters
                Password = "password123"
            };

            // Act
            var result = await _validator.ValidateAsync(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Username");
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async Task Validate_WithEmptyPassword_ReturnsError(string? password)
        {
            // Arrange
            var command = new LoginCommand
            {
                Username = "validuser",
                Password = password ?? ""
            };

            // Act
            var result = await _validator.ValidateAsync(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Password");
        }

        [Fact]
        public async Task Validate_WithPasswordTooShort_ReturnsError()
        {
            // Arrange
            var command = new LoginCommand
            {
                Username = "validuser",
                Password = "pass" // Less than 6 characters
            };

            // Act
            var result = await _validator.ValidateAsync(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Password");
        }

        [Fact]
        public async Task Validate_WithBoundaryUsername_IsValid()
        {
            // Arrange
            var command = new LoginCommand
            {
                Username = "abc", // Exactly 3 characters (minimum)
                Password = "password123"
            };

            // Act
            var result = await _validator.ValidateAsync(command);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public async Task Validate_WithBoundaryPassword_IsValid()
        {
            // Arrange
            var command = new LoginCommand
            {
                Username = "validuser",
                Password = "12345678" // Exactly 6 characters (minimum)
            };

            // Act
            var result = await _validator.ValidateAsync(command);

            // Assert
            result.IsValid.Should().BeTrue();
        }
    }
}
