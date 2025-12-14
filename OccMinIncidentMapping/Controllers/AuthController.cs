using Contracts.Auth;
using Core.Features.Auth.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace OccMinIncidentMapping.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IMediator mediator,
            IConfiguration configuration,
            ILogger<AuthController> logger)
        {
            _mediator = mediator;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Login endpoint - Returns JWT token following OAuth2 Bearer token standard
        /// </summary>
        /// <param name="request">Login credentials</param>
        /// <returns>Access token with metadata</returns>
        /// <response code="200">Login successful</response>
        /// <response code="400">Validation failed</response>
        /// <response code="401">Invalid credentials</response>
        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ErrorResponse
                {
                    Error = "invalid_request",
                    ErrorDescription = "Request validation failed",
                    Details = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList()
                });
            }

            try
            {
                var (token, role) = await _mediator.Send(new LoginCommand
                {
                    Username = request.Username,
                    Password = request.Password
                });

                var expiryMinutes = int.TryParse(
                    _configuration["Jwt:ExpiryMinutes"],
                    out var expiry) ? expiry : 60;

                var response = new LoginResponse
                {
                    AccessToken = token,
                    TokenType = "Bearer",
                    ExpiresIn = expiryMinutes * 60, // Convert to seconds
                    Username = request.Username,
                    Role = role
                };

                _logger.LogInformation("User {Username} logged in successfully", request.Username);
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Failed login attempt for user {Username}", request.Username);
                return Unauthorized(new ErrorResponse
                {
                    Error = "invalid_credentials",
                    ErrorDescription = "Invalid username or password"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user {Username}", request.Username);
                return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
                {
                    Error = "server_error",
                    ErrorDescription = "An unexpected error occurred during login"
                });
            }
        }
    }

    /// <summary>
    /// Standard OAuth2 error response format
    /// </summary>
    public class ErrorResponse
    {
        public string Error { get; set; } = string.Empty;
        public string ErrorDescription { get; set; } = string.Empty;
        public List<string> Details { get; set; } = new();
    }
}
