using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace OccMinIncidentMapping.Extensions
{
    public static class JwtConfigurationExtensions
    {
        /// <summary>
        /// Adds JWT authentication with secure key management.
        /// Keys should be provided via environment variables or Azure Key Vault, not in configuration files.
        /// </summary>
        public static IServiceCollection AddSecureJwtAuthentication(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var jwtSettings = configuration.GetSection("Jwt");
            var secretKey = GetSecureJwtSecret(configuration);

            if (string.IsNullOrEmpty(secretKey) || secretKey.Length < 32)
                throw new InvalidOperationException(
                    "JWT SecretKey must be provided via environment variable 'JWT_SECRET_KEY' and must be at least 32 characters long");

            var key = Encoding.ASCII.GetBytes(secretKey);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidateAudience = true,
                    ValidAudience = jwtSettings["Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

            return services;
        }

        /// <summary>
        /// Retrieves JWT secret from secure sources in order of preference:
        /// 1. Environment variable: JWT_SECRET_KEY
        /// 2. User Secrets (development only)
        /// 3. Azure Key Vault (if configured)
        /// </summary>
        private static string GetSecureJwtSecret(IConfiguration configuration)
        {
            // First, try to get from environment variable (recommended for production)
            var envSecret = Environment.GetEnvironmentVariable("JWT_SECRET_KEY");
            if (!string.IsNullOrEmpty(envSecret))
                return envSecret;

            // Fall back to user secrets (development only)
            var userSecretsKey = configuration["Jwt:SecretKey"];
            if (!string.IsNullOrEmpty(userSecretsKey) && !IsPlaceholderKey(userSecretsKey))
                return userSecretsKey;

            // If we're in development and no secure key is found, throw an error
            throw new InvalidOperationException(
                "JWT SecretKey not found. Please set the 'JWT_SECRET_KEY' environment variable or configure user secrets for development.");
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
    }
}
