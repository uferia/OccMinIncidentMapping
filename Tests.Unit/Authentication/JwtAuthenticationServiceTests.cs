using FluentAssertions;
using Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Tests.Unit.Authentication
{
    [TestClass]
    public class JwtAuthenticationServiceTests
    {
        private Mock<IConfiguration> _mockConfig;
        private Mock<IPasswordHasher> _mockPasswordHasher;
        private Mock<ILogger<JwtAuthenticationService>> _mockLogger;
        private JwtAuthenticationService _service;

        [TestInitialize]
        public void Setup()
        {
            _mockConfig = new Mock<IConfiguration>();
            _mockPasswordHasher = new Mock<IPasswordHasher>();
            _mockLogger = new Mock<ILogger<JwtAuthenticationService>>();
            
            SetupDefaultConfig();
            _service = new JwtAuthenticationService(
                _mockConfig.Object,
                _mockPasswordHasher.Object,
                _mockLogger.Object);
        }

        private void SetupDefaultConfig()
        {
            _mockConfig
                .Setup(x => x["Jwt:SecretKey"])
                .Returns("this-is-a-very-long-secret-key-for-testing-purposes-at-least-32-chars");
            _mockConfig
                .Setup(x => x["Jwt:Issuer"])
                .Returns("TestIssuer");
            _mockConfig
                .Setup(x => x["Jwt:Audience"])
                .Returns("TestAudience");
            _mockConfig
                .Setup(x => x["Jwt:ExpiryMinutes"])
                .Returns("60");
        }

        [TestMethod]
        public async Task GenerateTokenAsync_WithValidCredentials_ReturnsValidToken()
        {
            // Act
            var token = await _service.GenerateTokenAsync("testuser", "Admin");

            // Assert
            token.Should().NotBeNullOrEmpty();
            token.Should().StartWith("eyJ"); // JWT starts with header

            // Verify token structure
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadToken(token) as JwtSecurityToken;
            
            jwtToken.Should().NotBeNull();
            
            // Check for name identifier claim (can be either short or long form)
            var hasNameIdentifier = jwtToken!.Claims.Any(c => 
                (c.Type == ClaimTypes.NameIdentifier || c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier") 
                && c.Value == "testuser");
            hasNameIdentifier.Should().BeTrue();
            
            // Check for role claim (can be either short or long form)
            var hasRole = jwtToken.Claims.Any(c => 
                (c.Type == ClaimTypes.Role || c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/role") 
                && c.Value == "Admin");
            hasRole.Should().BeTrue();
        }

        [TestMethod]
        public async Task GenerateTokenAsync_TokenHasCorrectExpiration()
        {
            // Act
            var token = await _service.GenerateTokenAsync("testuser", "User");

            // Assert
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadToken(token) as JwtSecurityToken;
            
            jwtToken!.ValidTo.Should().BeAfter(DateTime.UtcNow);
            jwtToken.ValidTo.Should().BeBefore(DateTime.UtcNow.AddMinutes(70)); // 60 + buffer
        }

        [TestMethod]
        public async Task GenerateTokenAsync_TokenHasUniqueJti()
        {
            // Act
            var token1 = await _service.GenerateTokenAsync("testuser", "Admin");
            var token2 = await _service.GenerateTokenAsync("testuser", "Admin");

            // Assert
            var handler = new JwtSecurityTokenHandler();
            var jwtToken1 = handler.ReadToken(token1) as JwtSecurityToken;
            var jwtToken2 = handler.ReadToken(token2) as JwtSecurityToken;

            var jti1 = jwtToken1!.Claims.FirstOrDefault(c => c.Type == "jti")?.Value;
            var jti2 = jwtToken2!.Claims.FirstOrDefault(c => c.Type == "jti")?.Value;

            jti1.Should().NotBeNullOrEmpty();
            jti2.Should().NotBeNullOrEmpty();
            jti1.Should().NotBe(jti2);
        }

        [TestMethod]
        public async Task ValidateCredentialsAsync_WithValidCredentials_ReturnsTrue()
        {
            // Act
            var result = await _service.ValidateCredentialsAsync("admin", "admin123");

            // Assert
            result.Should().BeTrue();
        }

        [TestMethod]
        public async Task ValidateCredentialsAsync_WithInvalidPassword_ReturnsFalse()
        {
            // Act
            var result = await _service.ValidateCredentialsAsync("admin", "wrongpassword");

            // Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        public async Task ValidateCredentialsAsync_WithInvalidUsername_ReturnsFalse()
        {
            // Act
            var result = await _service.ValidateCredentialsAsync("nonexistent", "password");

            // Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task GenerateTokenAsync_WithMissingSecretKey_ThrowsException()
        {
            // Arrange
            var config = new Mock<IConfiguration>();
            config.Setup(x => x["Jwt:SecretKey"]).Returns((string?)null);
            var service = new JwtAuthenticationService(
                config.Object,
                _mockPasswordHasher.Object,
                _mockLogger.Object);

            // Act
            await service.GenerateTokenAsync("user", "Admin");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task GenerateTokenAsync_WithShortSecretKey_ThrowsException()
        {
            // Arrange
            var config = new Mock<IConfiguration>();
            config.Setup(x => x["Jwt:SecretKey"]).Returns("tooshort");
            var service = new JwtAuthenticationService(
                config.Object,
                _mockPasswordHasher.Object,
                _mockLogger.Object);

            // Act
            await service.GenerateTokenAsync("user", "Admin");
        }
    }
}
