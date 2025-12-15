using Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Core.Features.Auth.Commands
{
    /// <summary>
    /// Handler for Google SSO authentication
    /// Verifies the Google ID token and generates a JWT token
    /// </summary>
    public class GoogleSsoCommandHandler : IRequestHandler<GoogleSsoCommand, (string token, string role)>
    {
        private readonly IAuthenticationService _authService;
        private readonly IGoogleAuthenticationService _googleAuthService;
        private readonly ILogger<GoogleSsoCommandHandler> _logger;

        public GoogleSsoCommandHandler(
            IAuthenticationService authService,
            IGoogleAuthenticationService googleAuthService,
            ILogger<GoogleSsoCommandHandler> logger)
        {
            _authService = authService;
            _googleAuthService = googleAuthService;
            _logger = logger;
        }

        public async Task<(string token, string role)> Handle(
            GoogleSsoCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrEmpty(request.IdToken))
                {
                    throw new ArgumentException("ID token is required");
                }

                // Verify Google ID token and get user information
                var googleUser = await _googleAuthService.VerifyIdTokenAsync(request.IdToken);

                if (googleUser == null)
                {
                    _logger.LogWarning("Invalid or expired Google ID token");
                    throw new UnauthorizedAccessException("Invalid Google ID token");
                }

                _logger.LogInformation("User {Email} authenticated via Google SSO", googleUser.Email);

                // TODO: Replace with database lookup or create user if not exists
                var role = GetUserRole(googleUser.Email);

                // Generate JWT token
                var token = await _authService.GenerateTokenAsync(googleUser.Email, role);

                return (token, role);
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Google SSO authentication");
                throw new UnauthorizedAccessException("Google SSO authentication failed", ex);
            }
        }

        /// <summary>
        /// Temporary placeholder - Replace with actual role lookup from database
        /// </summary>
        private static string GetUserRole(string email)
        {
            // TODO: Fetch from user database based on email
            // For now, all Google SSO users get User role
            return "User";
        }
    }
}
