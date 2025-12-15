using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace OccMinIncidentMapping.Extensions
{
    public static class JwtConfigurationExtensions
    {
        /// <summary>
        /// Adds JWT authentication with secure key management.
        /// Keys are retrieved from (in order of preference):
        /// 1. Environment variable: JWT_SECRET_KEY
        /// 2. Google Cloud Secret Manager: jwt-signing-key (in GCP environments)
        /// 3. User Secrets: Jwt:SigningKey (development only)
        /// 4. Configuration file: Jwt:SecretKey (not recommended for production)
        /// 5. Generated temporary key (development only, for local testing)
        /// </summary>
        public static IServiceCollection AddSecureJwtAuthentication(
            this IServiceCollection services,
            IConfiguration configuration,
            IHostEnvironment environment = null)
        {
            var jwtSettings = configuration.GetSection("Jwt");
            var secretKey = GetSecureJwtSecret(configuration, environment);

            if (string.IsNullOrEmpty(secretKey) || secretKey.Length < 32)
                throw new InvalidOperationException(
                    "JWT SecretKey must be provided via 'JWT_SECRET_KEY' environment variable, Google Cloud Secret Manager (jwt-signing-key), " +
                    "or user secrets, and must be at least 32 characters long");

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
        /// 1. Environment variable: JWT_SECRET_KEY (highest priority)
        /// 2. Configuration from Google Cloud Secret Manager: Jwt:SigningKey
        /// 3. User Secrets / Configuration: Jwt:SecretKey or Jwt:SigningKey (development only)
        /// 4. Generated temporary key for local development testing
        /// </summary>
        private static string GetSecureJwtSecret(IConfiguration configuration, IHostEnvironment environment)
        {
            // First, try to get from environment variable (recommended for production)
            var envSecret = Environment.GetEnvironmentVariable("JWT_SECRET_KEY");
            if (!string.IsNullOrEmpty(envSecret) && !IsPlaceholderKey(envSecret))
                return envSecret;

            // Try to get from configuration loaded from GCP Secret Manager
            // Configuration key mapping: jwt-signing-key -> Jwt:SigningKey
            var configSecret = configuration["Jwt:SigningKey"];
            if (!string.IsNullOrEmpty(configSecret) && !IsPlaceholderKey(configSecret))
                return configSecret;

            // Fallback to alternative configuration key (for backward compatibility)
            var legacyConfigSecret = configuration["Jwt:SecretKey"];
            if (!string.IsNullOrEmpty(legacyConfigSecret) && !IsPlaceholderKey(legacyConfigSecret))
                return legacyConfigSecret;

            // For development environments, generate a temporary key for local testing
            if (environment?.IsDevelopment() == true)
            {
                var generatedKey = GenerateDevelopmentKey();
                System.Diagnostics.Debug.WriteLine(
                    "??  WARNING: Using auto-generated JWT secret for development. " +
                    "For persistent configuration, set JWT_SECRET_KEY environment variable or configure user secrets: " +
                    "dotnet user-secrets set \"Jwt:SigningKey\" \"your-key-here\"");
                return generatedKey;
            }

            // If we're not in development, throw an error (production requires explicit key)
            throw new InvalidOperationException(
                "JWT SecretKey not found. Please set the 'JWT_SECRET_KEY' environment variable, " +
                "create 'jwt-signing-key' secret in Google Cloud Secret Manager, " +
                "or configure user secrets for development (Jwt:SigningKey).");
        }

        /// <summary>
        /// Generates a temporary JWT secret key for development purposes only.
        /// This is not suitable for production use.
        /// </summary>
        private static string GenerateDevelopmentKey()
        {
            // Generate a stable key based on machine name and process for development
            // This ensures consistency across restarts while being unique per machine
            var machineKey = Environment.MachineName;
            var keySource = $"OccMinIncidentMapping-Dev-{machineKey}-LocalDevelopment";
            
            using (var sha = System.Security.Cryptography.SHA256.Create())
            {
                var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(keySource));
                return Convert.ToBase64String(hash);
            }
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
