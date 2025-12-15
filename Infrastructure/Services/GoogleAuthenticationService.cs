using Core.Interfaces;
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services
{
    /// <summary>
    /// Service for verifying Google ID tokens and extracting user information
    /// </summary>
    public class GoogleAuthenticationService : IGoogleAuthenticationService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<GoogleAuthenticationService> _logger;
        private readonly string _googleClientId;

        public GoogleAuthenticationService(
            IConfiguration configuration,
            ILogger<GoogleAuthenticationService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _googleClientId = configuration["Google:ClientId"] ?? string.Empty;

            if (string.IsNullOrEmpty(_googleClientId))
            {
                _logger.LogWarning("Google:ClientId is not configured. Google SSO will not work.");
            }
        }

        /// <summary>
        /// Verifies a Google ID token and extracts user information
        /// </summary>
        public async Task<GoogleUserInfo?> VerifyIdTokenAsync(string idToken)
        {
            try
            {
                if (string.IsNullOrEmpty(_googleClientId))
                {
                    _logger.LogError("Google ClientId is not configured");
                    return null;
                }

                // Verify the token with Google's servers
                var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { _googleClientId }
                });

                if (payload == null)
                {
                    _logger.LogWarning("Google token validation returned null payload");
                    return null;
                }

                _logger.LogInformation("Google ID token verified successfully for user: {Email}", payload.Email);

                return new GoogleUserInfo
                {
                    Email = payload.Email,
                    Name = payload.Name,
                    Picture = payload.Picture,
                    Subject = payload.Subject
                };
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid Google ID token: {Message}", ex.Message);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying Google ID token");
                return null;
            }
        }
    }
}
