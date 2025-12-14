using Contracts.Auth;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using OccMinIncidentMapping;
using System.Net.Http.Json;
using Xunit;

namespace Tests.Integration.Security
{
    public class SecurityHeadersIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public SecurityHeadersIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Response_ContainsXFrameOptionsHeader()
        {
            // Act
            var response = await _client.GetAsync("/api/auth/login");

            // Assert
            response.Headers.Should().Contain(h => h.Key == "X-Frame-Options");
            response.Headers.GetValues("X-Frame-Options").First().Should().Be("DENY");
        }

        [Fact]
        public async Task Response_ContainsXContentTypeOptionsHeader()
        {
            // Act
            var response = await _client.GetAsync("/api/incidents");

            // Assert
            response.Headers.Should().Contain(h => h.Key == "X-Content-Type-Options");
            response.Headers.GetValues("X-Content-Type-Options").First().Should().Be("nosniff");
        }

        [Fact]
        public async Task Response_ContainsXXssProtectionHeader()
        {
            // Act
            var response = await _client.GetAsync("/");

            // Assert
            response.Headers.Should().Contain(h => h.Key == "X-XSS-Protection");
            response.Headers.GetValues("X-XSS-Protection").First().Should().Contain("1");
        }

        [Fact]
        public async Task Response_ContainsContentSecurityPolicyHeader()
        {
            // Act
            var response = await _client.GetAsync("/api/auth/login");

            // Assert
            response.Headers.Should().Contain(h => h.Key == "Content-Security-Policy");
            var cspValue = response.Headers.GetValues("Content-Security-Policy").First();
            cspValue.Should().Contain("default-src");
        }

        [Fact]
        public async Task Response_ContainsReferrerPolicyHeader()
        {
            // Act
            var response = await _client.GetAsync("/api/incidents");

            // Assert
            response.Headers.Should().Contain(h => h.Key == "Referrer-Policy");
        }
    }

    public class CorsIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public CorsIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Request_FromAllowedOrigin_IsSuccessful()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Username = "admin",
                Password = "admin123"
            };

            _client.DefaultRequestHeaders.Add("Origin", "http://localhost:4200");

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

            // Assert
            response.IsSuccessStatusCode.Should().BeTrue();
        }
    }

    public class AuthenticationIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public AuthenticationIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task ProtectedEndpoint_WithoutToken_ReturnsUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("/api/incidents");

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task ProtectedEndpoint_WithInvalidToken_ReturnsUnauthorized()
        {
            // Arrange
            _client.DefaultRequestHeaders.Add("Authorization", "Bearer invalid-token");

            // Act
            var response = await _client.GetAsync("/api/incidents");

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task ProtectedEndpoint_WithValidToken_ReturnsSuccess()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Username = "admin",
                Password = "admin123"
            };

            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
            var loginResult = await loginResponse.Content.ReadAsAsync<LoginResponse>();
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {loginResult!.Token}");

            // Act
            var response = await _client.GetAsync("/api/incidents");

            // Assert
            response.IsSuccessStatusCode.Should().BeTrue();
        }
    }

    public class ErrorHandlingIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public ErrorHandlingIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task ValidationError_ReturnsStructuredErrorResponse()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Username = "",
                Password = ""
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
            var content = await response.Content.ReadAsAsync<dynamic>();
            content.Should().NotBeNull();
        }
    }
}
