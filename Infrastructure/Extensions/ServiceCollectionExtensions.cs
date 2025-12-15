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
                        // Try to load from configuration (loaded from GCP Secret Manager)
                        // Secret name: firebase-credentials-occminproj -> FirebaseCredentialsOccminproj
                        string? credentialsFromConfig = configuration["FirebaseCredentialsOccminproj"];
                        if (!string.IsNullOrEmpty(credentialsFromConfig))
                        {
                            // Write the credentials to a temporary file
                            string tempPath = Path.Combine(Path.GetTempPath(), $"firebase-{Guid.NewGuid()}.json");
                            File.WriteAllText(tempPath, credentialsFromConfig);
                            credentialsPath = tempPath;
                        }
                        else
                        {
                            // For GCP environments, use Application Default Credentials (ADC)
                            // This works with Cloud Run, Compute Engine, and other GCP services
                            // No explicit credentials file needed
                            credentialsPath = null;
                            
                            // Only throw if not in a GCP environment with ADC
                            if (!IsGcpEnvironmentWithAdc())
                            {
                                throw new InvalidOperationException(
                                    "Firebase credentials not found. Set GOOGLE_APPLICATION_CREDENTIALS environment variable, " +
                                    "FIREBASE_CREDENTIALS_JSON, or ensure firebase-credentials-occminproj secret is available in GCP Secret Manager, " +
                                    "or ensure Application Default Credentials (ADC) are configured for GCP.");
                            }
                        }
                    }
                }
            }

            // Set environment variable if we have a credentials path
            if (!string.IsNullOrEmpty(credentialsPath))
            {
                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialsPath);
            }

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
            services.AddScoped<IGoogleAuthenticationService, GoogleAuthenticationService>();

            return services;
        }

        /// <summary>
        /// Checks if the application is running in a GCP environment with Application Default Credentials.
        /// </summary>
        private static bool IsGcpEnvironmentWithAdc()
        {
            // Check for common GCP environment indicators
            var gcpProject = Environment.GetEnvironmentVariable("GCP_PROJECT_ID") ?? 
                            Environment.GetEnvironmentVariable("GOOGLE_CLOUD_PROJECT");
            
            // Cloud Run sets GOOGLE_CLOUD_RUN_EXECUTION
            var cloudRunExecution = Environment.GetEnvironmentVariable("GOOGLE_CLOUD_RUN_EXECUTION");
            
            // App Engine sets GAE_INSTANCE
            var appEngineInstance = Environment.GetEnvironmentVariable("GAE_INSTANCE");
            
            // Compute Engine has metadata server
            var hasMetadataServer = HasGcpMetadataServer();

            return !string.IsNullOrEmpty(gcpProject) ||
                   !string.IsNullOrEmpty(cloudRunExecution) ||
                   !string.IsNullOrEmpty(appEngineInstance) ||
                   hasMetadataServer;
        }

        /// <summary>
        /// Checks if GCP metadata server is available (indicates running on GCP compute).
        /// </summary>
        private static bool HasGcpMetadataServer()
        {
            try
            {
                using (var client = new HttpClient { Timeout = TimeSpan.FromSeconds(1) })
                {
                    var response = client.GetAsync("http://metadata.google.internal/computeMetadata/v1/instance/id").Result;
                    return response.IsSuccessStatusCode;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
