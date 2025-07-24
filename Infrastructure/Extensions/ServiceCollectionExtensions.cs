using Core.Interfaces;
using Google.Cloud.Firestore;
using Infrastructure.Data;
using Infrastructure.Data.MappingProfiles;
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
            // Set Firebase credentials path
            string credentialsPath = Path.Combine(Directory.GetCurrentDirectory(), "firebase.json");
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialsPath);

            // Initialize Firestore
            var firestore = FirestoreDb.Create(configuration["Firebase:ProjectId"]);
            services.AddSingleton(firestore);

            // Register AutoMapper
            services.AddAutoMapper(cfg => cfg.AddProfile<FirestoreMappingProfile>());

            // Register repositories and context
            services.AddScoped<IIncidentRepository, FirebaseIncidentRepository>();
            services.AddScoped<IApplicationDbContext, ApplicationDbContext>();

            return services;
        }
    }
}
