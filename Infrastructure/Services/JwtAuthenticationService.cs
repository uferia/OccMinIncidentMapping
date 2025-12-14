using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Services
{
    public class JwtAuthenticationService : IAuthenticationService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<JwtAuthenticationService> _logger;
        private const int MinKeyLength = 32;

        public JwtAuthenticationService(
            IConfiguration configuration,
            ILogger<JwtAuthenticationService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<string> GenerateTokenAsync(string username, string role)
        {
            try
            {
                var key = GetSecureJwtSecret();
                var issuer = _configuration["Jwt:Issuer"];
                var audience = _configuration["Jwt:Audience"];
                var expiryMinutesStr = _configuration["Jwt:ExpiryMinutes"] ?? "60";

                // Validation
                if (string.IsNullOrEmpty(key) || key.Length < MinKeyLength)
                {
                    _logger.LogError("JWT SecretKey is not configured or is too short");
                    throw new InvalidOperationException("JWT configuration is invalid");
                }

                if (!int.TryParse(expiryMinutesStr, out var expiryMinutes) || expiryMinutes <= 0)
                {
                    _logger.LogError("JWT ExpiryMinutes is not a valid positive integer");
                    throw new InvalidOperationException("JWT expiry configuration is invalid");
                }

                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, username),
                    new Claim(ClaimTypes.Name, username),
                    new Claim(ClaimTypes.Role, role),
                    // Standard JWT claims
                    new Claim("iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // Unique token ID
                };

                var token = new JwtSecurityToken(
                    issuer: issuer,
                    audience: audience,
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                    signingCredentials: credentials
                );

                var tokenHandler = new JwtSecurityTokenHandler();
                var tokenString = tokenHandler.WriteToken(token);

                _logger.LogInformation("Token generated successfully for user: {Username}", username);
                return await Task.FromResult(tokenString);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Invalid argument while generating token for user: {Username}. Message: {Message}", 
                    username, ex.Message);
                throw new InvalidOperationException("Token generation failed due to invalid configuration", ex);
            }
            catch (SecurityTokenException ex)
            {
                _logger.LogError(ex, "Security token exception while generating token for user: {Username}", username);
                throw new InvalidOperationException("Failed to generate secure token", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error generating JWT token for user: {Username}. Exception type: {ExceptionType}", 
                    username, ex.GetType().Name);
                throw;
            }
        }

        public async Task<bool> ValidateCredentialsAsync(string username, string password)
        {
            try
            {
                // TODO: Replace with actual user database lookup
                // This is a placeholder implementation for demonstration
                // In production, fetch user from database with hashed passwords
                var validUsers = new Dictionary<string, (string passwordHash, string role)>
                {
                    // Password hashes are pre-computed examples - in production, hash user input and compare
                    { "admin", ("admin_hash_placeholder", "Admin") },
                    { "user", ("user_hash_placeholder", "User") }
                };

                if (!validUsers.TryGetValue(username, out var user))
                {
                    _logger.LogWarning("Login attempt with invalid username: {Username}", username);
                    return false;
                }

                // In production: bool isValid = _passwordHasher.Verify(password, user.passwordHash);
                // For now, we'll use a placeholder comparison
                bool isValid = VerifyPlaceholderPassword(username, password);

                if (!isValid)
                {
                    _logger.LogWarning("Login attempt with invalid password for user: {Username}", username);
                }

                return await Task.FromResult(isValid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating credentials");
                return false;
            }
        }

        /// <summary>
        /// Retrieves JWT secret from secure sources in order of preference:
        /// 1. Environment variable: JWT_SECRET_KEY
        /// 2. Configuration (user secrets in development)
        /// </summary>
        private string GetSecureJwtSecret()
        {
            // First, try to get from environment variable (recommended for production)
            var envSecret = Environment.GetEnvironmentVariable("JWT_SECRET_KEY");
            if (!string.IsNullOrEmpty(envSecret))
                return envSecret;

            // Fall back to configuration (for development/testing only)
            var configSecret = _configuration["Jwt:SecretKey"];
            if (!string.IsNullOrEmpty(configSecret) && !IsPlaceholderKey(configSecret))
                return configSecret;

            throw new InvalidOperationException(
                "JWT SecretKey not found. Please set the 'JWT_SECRET_KEY' environment variable.");
        }

        /// <summary>
        /// Checks if the key is a placeholder value from configuration files.
        /// </summary>
        private static bool IsPlaceholderKey(string key)
        {
            return key.Contains("your-super-secret") ||
                   key.Contains("placeholder") ||
                   key.Contains("change-me") ||
                   key.Contains("example");
        }

        /// <summary>
        /// Temporary placeholder - Replace with actual password comparison using IPasswordHasher
        /// </summary>
        private static bool VerifyPlaceholderPassword(string username, string password)
        {
            var validCredentials = new Dictionary<string, string>
            {
                { "admin", "admin123" },
                { "user", "user123" }
            };

            return validCredentials.TryGetValue(username, out var correctPassword) && 
                   correctPassword == password;
        }
    }
}
