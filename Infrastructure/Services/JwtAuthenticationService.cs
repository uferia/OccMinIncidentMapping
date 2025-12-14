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
        private readonly IPasswordHasher _passwordHasher;
        private readonly ILogger<JwtAuthenticationService> _logger;
        private const int MinKeyLength = 32;

        public JwtAuthenticationService(
            IConfiguration configuration,
            IPasswordHasher passwordHasher,
            ILogger<JwtAuthenticationService> logger)
        {
            _configuration = configuration;
            _passwordHasher = passwordHasher;
            _logger = logger;
        }

        public async Task<string> GenerateTokenAsync(string username, string role)
        {
            try
            {
                var key = _configuration["Jwt:SecretKey"];
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating JWT token");
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
