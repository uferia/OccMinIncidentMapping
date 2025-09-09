using Core.Interfaces;
using Google.Cloud.Firestore;
using Infrastructure.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), "firebase.json");
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", path);

            var firestore = FirestoreDb.Create(configuration["Firebase:ProjectId"]);
            services.AddSingleton(firestore);
            services.AddScoped<IApplicationDbContext, ApplicationDbContext>();

            return services;
        }
    }
}
