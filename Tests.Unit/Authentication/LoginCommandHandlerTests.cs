using Core.Features.Auth.Commands;
using Core.Interfaces;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Tests.Unit.Authentication
{
    [TestClass]
    public class LoginCommandHandlerTests
    {
        private Mock<IAuthenticationService> _mockAuthService;
        private LoginCommandHandler _handler;

        [TestInitialize]
        public void Setup()
        {
            _mockAuthService = new Mock<IAuthenticationService>();
            _handler = new LoginCommandHandler(_mockAuthService.Object);
        }

        [TestMethod]
        public async Task Handle_WithValidCredentials_ReturnsTokenAndRole()
        {
            // Arrange
            var command = new LoginCommand
            {
                Username = "admin",
                Password = "admin123"
            };

            var expectedToken = "valid-jwt-token";
            _mockAuthService
                .Setup(x => x.ValidateCredentialsAsync("admin", "admin123"))
                .ReturnsAsync(true);
            _mockAuthService
                .Setup(x => x.GenerateTokenAsync("admin", "Admin"))
                .ReturnsAsync(expectedToken);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.token.Should().NotBeNullOrEmpty();
            result.token.Should().Be(expectedToken);
            result.role.Should().Be("Admin");
            _mockAuthService.Verify(x => x.ValidateCredentialsAsync("admin", "admin123"), Times.Once);
            _mockAuthService.Verify(x => x.GenerateTokenAsync("admin", "Admin"), Times.Once);
        }

        [TestMethod]
        public async Task Handle_WithInvalidCredentials_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var command = new LoginCommand
            {
                Username = "admin",
                Password = "wrongpassword"
            };

            _mockAuthService
                .Setup(x => x.ValidateCredentialsAsync("admin", "wrongpassword"))
                .ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<UnauthorizedAccessException>(
                async () => await _handler.Handle(command, CancellationToken.None));
        }

        [TestMethod]
        public async Task Handle_WithNullUsername_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var command = new LoginCommand
            {
                Username = "",
                Password = "admin123"
            };

            _mockAuthService
                .Setup(x => x.ValidateCredentialsAsync("", "admin123"))
                .ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<UnauthorizedAccessException>(
                async () => await _handler.Handle(command, CancellationToken.None));
        }
    }
}
