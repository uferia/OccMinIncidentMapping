using Contracts.Auth;
using Core.Enums;
using Core.Features.Incidents.Commands;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using OccMinIncidentMapping;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Tests.Integration.Controllers
{
    public class IncidentsControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private string? _authToken;

        public IncidentsControllerIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        private async Task AuthenticateAsync()
        {
            if (!string.IsNullOrEmpty(_authToken))
                return;

            var loginRequest = new LoginRequest
            {
                Username = "admin",
                Password = "admin123"
            };

            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
            var result = await loginResponse.Content.ReadAsAsync<LoginResponse>();
            _authToken = result!.Token;
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_authToken}");
        }

        [Fact]
        public async Task CreateIncident_WithValidData_ReturnsCreated()
        {
            // Arrange
            await AuthenticateAsync();
            var command = new CreateIncidentCommand
            {
                Latitude = 40.7128,
                Longitude = -74.0060,
                Hazard = HazardType.Flood,
                Status = StatusType.Ongoing,
                Description = "Flooding in downtown area"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/incidents", command);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var location = response.Headers.Location;
            location.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateIncident_WithInvalidLatitude_ReturnsBadRequest()
        {
            // Arrange
            await AuthenticateAsync();
            var command = new CreateIncidentCommand
            {
                Latitude = 95, // Invalid
                Longitude = -74.0060,
                Hazard = HazardType.Flood,
                Status = StatusType.Ongoing,
                Description = "Test"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/incidents", command);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateIncident_WithoutAuthentication_ReturnsUnauthorized()
        {
            // Arrange
            var command = new CreateIncidentCommand
            {
                Latitude = 40.7128,
                Longitude = -74.0060,
                Hazard = HazardType.Flood,
                Status = StatusType.Ongoing,
                Description = "Test"
            };

            // Remove auth header if present
            _client.DefaultRequestHeaders.Remove("Authorization");

            // Act
            var response = await _client.PostAsJsonAsync("/api/incidents", command);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetAllIncidents_WithAuthentication_ReturnsOk()
        {
            // Arrange
            await AuthenticateAsync();

            // Act
            var response = await _client.GetAsync("/api/incidents");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Should().NotBeNull();
        }

        [Fact]
        public async Task GetAllIncidents_WithoutAuthentication_ReturnsUnauthorized()
        {
            // Arrange
            _client.DefaultRequestHeaders.Remove("Authorization");

            // Act
            var response = await _client.GetAsync("/api/incidents");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task CreateIncident_WithExceededDescriptionLength_ReturnsBadRequest()
        {
            // Arrange
            await AuthenticateAsync();
            var command = new CreateIncidentCommand
            {
                Latitude = 40.7128,
                Longitude = -74.0060,
                Hazard = HazardType.Flood,
                Status = StatusType.Ongoing,
                Description = new string('a', 501) // Exceeds 500 character limit
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/incidents", command);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}
