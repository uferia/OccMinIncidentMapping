using Contracts.Auth;
using Core.Features.Auth.Commands;
using Core.Interfaces;
using FluentAssertions;
using Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using System.IdentityModel.Tokens.Jwt;

namespace Tests.Features.StepDefinitions
{
    [Binding]
    public class AuthenticationStepDefinitions
    {
        private string? _username;
        private string? _password;
        private string? _token;
        private Exception? _exception;
        private readonly IAuthenticationService _authService;

        public AuthenticationStepDefinitions()
        {
            var mockConfig = new Mock<IConfiguration>();
            mockConfig.Setup(x => x["Jwt:SecretKey"])
                .Returns("this-is-a-very-long-secret-key-for-testing-purposes-at-least-32-chars");
            mockConfig.Setup(x => x["Jwt:Issuer"]).Returns("TestIssuer");
            mockConfig.Setup(x => x["Jwt:Audience"]).Returns("TestAudience");
            mockConfig.Setup(x => x["Jwt:ExpiryMinutes"]).Returns("60");
            
            _authService = new JwtAuthenticationService(mockConfig.Object);
        }

        [Given("I have valid login credentials")]
        public void GivenIHaveValidLoginCredentials()
        {
            _username = "admin";
            _password = "admin123";
        }

        [Given("I have a username \"([^\"]*)\"")]
        public void GivenIHaveAUsername(string username)
        {
            _username = username;
        }

        [Given("I have an invalid password \"([^\"]*)\"")]
        public void GivenIHaveAnInvalidPassword(string password)
        {
            _password = password;
        }

        [Given("I have a password \"([^\"]*)\"")]
        public void GivenIHaveAPassword(string password)
        {
            _password = password;
        }

        [When("I submit the login request")]
        public async Task WhenISubmitTheLoginRequest()
        {
            try
            {
                var isValid = await _authService.ValidateCredentialsAsync(_username!, _password!);
                if (!isValid)
                {
                    _exception = new UnauthorizedAccessException("Invalid credentials");
                }
                else
                {
                    var role = _username == "admin" ? "Admin" : "User";
                    _token = await _authService.GenerateTokenAsync(_username, role);
                }
            }
            catch (Exception ex)
            {
                _exception = ex;
            }
        }

        [Then("I should receive a valid JWT token")]
        public void ThenIShouldReceiveAValidJwtToken()
        {
            _token.Should().NotBeNullOrEmpty();
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadToken(_token!);
            jwtToken.Should().NotBeNull();
        }

        [Then("the token should contain my username")]
        public void ThenTheTokenShouldContainMyUsername()
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadToken(_token!) as JwtSecurityToken;
            jwtToken!.Claims.Should()
                .Contain(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier" && c.Value == _username);
        }

        [Then("the token should contain my role")]
        public void ThenTheTokenShouldContainMyRole()
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadToken(_token!) as JwtSecurityToken;
            jwtToken!.Claims.Should()
                .Contain(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/role");
        }

        [Then("I should receive an error message")]
        public void ThenIShouldReceiveAnErrorMessage()
        {
            _exception.Should().NotBeNull();
        }

        [Then("I should not receive a token")]
        public void ThenIShouldNotReceiveAToken()
        {
            _token.Should().BeNullOrEmpty();
        }

        [Then("I should receive an unauthorized error")]
        public void ThenIShouldReceiveAnUnauthorizedError()
        {
            _exception.Should().BeOfType<UnauthorizedAccessException>();
        }

        [Given("I have obtained a valid token")]
        public async Task GivenIHaveObtainedAValidToken()
        {
            _username = "admin";
            _password = "admin123";
            _token = await _authService.GenerateTokenAsync("admin", "Admin");
        }

        [Given("the token will expire in 60 minutes")]
        public void GivenTheTokenWillExpireIn60Minutes()
        {
            // Token is already generated with 60 minute expiry
        }

        [When("I check the token expiration")]
        public void WhenICheckTheTokenExpiration()
        {
            // Token already validated in Given step
        }

        [Then("the expiration should be set correctly")]
        public void ThenTheExpirationShouldBeSetCorrectly()
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadToken(_token!) as JwtSecurityToken;
            jwtToken!.ValidTo.Should().BeAfter(DateTime.UtcNow);
            jwtToken.ValidTo.Should().BeBefore(DateTime.UtcNow.AddMinutes(70));
        }
    }
}
