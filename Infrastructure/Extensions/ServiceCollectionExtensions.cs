using Core.Interfaces;
using Google.Cloud.Firestore;
using Infrastructure.Data;
using Infrastructure.Data.MappingProfiles;
using Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Set Firebase credentials - prefer environment variable, fallback to file
            string? credentialsPath = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS");
            
            if (string.IsNullOrEmpty(credentialsPath))
            {
                // Check if firebase.json exists in current directory (development only)
                string localFirebaseJsonPath = Path.Combine(Directory.GetCurrentDirectory(), "firebase.json");
                if (File.Exists(localFirebaseJsonPath))
                {
                    credentialsPath = localFirebaseJsonPath;
                }
                else
                {
                    // Try to load credentials from environment variable in JSON format
                    string? credentialsJson = Environment.GetEnvironmentVariable("FIREBASE_CREDENTIALS_JSON");
                    if (!string.IsNullOrEmpty(credentialsJson))
                    {
                        // Write JSON to a temporary file
                        string tempPath = Path.Combine(Path.GetTempPath(), $"firebase-{Guid.NewGuid()}.json");
                        File.WriteAllText(tempPath, credentialsJson);
                        credentialsPath = tempPath;
                    }
                    else
                    {
                        throw new InvalidOperationException(
                            "Firebase credentials not found. Set GOOGLE_APPLICATION_CREDENTIALS environment variable or FIREBASE_CREDENTIALS_JSON.");
                    }
                }
            }

            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialsPath);

            // Initialize Firestore
            var firestore = FirestoreDb.Create(configuration["Firebase:ProjectId"]);
            services.AddSingleton(firestore);

            // Register AutoMapper
            services.AddAutoMapper(cfg => cfg.AddProfile<FirestoreMappingProfile>());

            // Register repositories and context
            services.AddScoped<IIncidentRepository, FirebaseIncidentRepository>();
            services.AddScoped<IApplicationDbContext, ApplicationDbContext>();

            // Register authentication services
            services.AddScoped<IPasswordHasher, Pbkdf2PasswordHasher>();
            services.AddScoped<IAuthenticationService, JwtAuthenticationService>();

            return services;
        }
    }
}
