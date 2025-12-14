using Application.Validators;
using Core.Enums;
using Core.Features.Incidents.Commands;
using FluentAssertions;
using Xunit;

namespace Tests.Unit.Validators
{
    public class CreateIncidentCommandValidatorTests
    {
        private readonly CreateIncidentCommandValidator _validator;

        public CreateIncidentCommandValidatorTests()
        {
            _validator = new CreateIncidentCommandValidator();
        }

        [Fact]
        public async Task Validate_WithValidCommand_ReturnsNoErrors()
        {
            // Arrange
            var command = new CreateIncidentCommand
            {
                Latitude = 40.7128,
                Longitude = -74.0060,
                Hazard = HazardType.Flood,
                Status = StatusType.Ongoing,
                Description = "Flooding in downtown area"
            };

            // Act
            var result = await _validator.ValidateAsync(command);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Theory]
        [InlineData(-91)]
        [InlineData(91)]
        public async Task Validate_WithInvalidLatitude_ReturnsError(double latitude)
        {
            // Arrange
            var command = new CreateIncidentCommand
            {
                Latitude = latitude,
                Longitude = -74.0060,
                Hazard = HazardType.Flood,
                Status = StatusType.Ongoing,
                Description = "Test"
            };

            // Act
            var result = await _validator.ValidateAsync(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Latitude");
        }

        [Theory]
        [InlineData(-181)]
        [InlineData(181)]
        public async Task Validate_WithInvalidLongitude_ReturnsError(double longitude)
        {
            // Arrange
            var command = new CreateIncidentCommand
            {
                Latitude = 40.7128,
                Longitude = longitude,
                Hazard = HazardType.Flood,
                Status = StatusType.Ongoing,
                Description = "Test"
            };

            // Act
            var result = await _validator.ValidateAsync(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Longitude");
        }

        [Fact]
        public async Task Validate_WithDescriptionExceedingLimit_ReturnsError()
        {
            // Arrange
            var command = new CreateIncidentCommand
            {
                Latitude = 40.7128,
                Longitude = -74.0060,
                Hazard = HazardType.Flood,
                Status = StatusType.Ongoing,
                Description = new string('a', 501) // 501 characters (exceeds 500 limit)
            };

            // Act
            var result = await _validator.ValidateAsync(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Description");
        }

        [Fact]
        public async Task Validate_WithBoundaryLatitude_IsValid()
        {
            // Arrange
            var command = new CreateIncidentCommand
            {
                Latitude = 90,
                Longitude = -74.0060,
                Hazard = HazardType.Flood,
                Status = StatusType.Ongoing,
                Description = "Test"
            };

            // Act
            var result = await _validator.ValidateAsync(command);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public async Task Validate_WithBoundaryLongitude_IsValid()
        {
            // Arrange
            var command = new CreateIncidentCommand
            {
                Latitude = 40.7128,
                Longitude = 180,
                Hazard = HazardType.Flood,
                Status = StatusType.Ongoing,
                Description = "Test"
            };

            // Act
            var result = await _validator.ValidateAsync(command);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public async Task Validate_WithMaxDescriptionLength_IsValid()
        {
            // Arrange
            var command = new CreateIncidentCommand
            {
                Latitude = 40.7128,
                Longitude = -74.0060,
                Hazard = HazardType.Flood,
                Status = StatusType.Ongoing,
                Description = new string('a', 500) // Exactly 500 characters
            };

            // Act
            var result = await _validator.ValidateAsync(command);

            // Assert
            result.IsValid.Should().BeTrue();
        }
    }
}
