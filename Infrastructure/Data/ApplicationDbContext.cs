using AutoMapper;
using Core.Interfaces;
using Google.Cloud.Firestore;

namespace Infrastructure.Data
{
    public class ApplicationDbContext : IApplicationDbContext
    {
        private readonly FirestoreDb _firestore;

        public ApplicationDbContext(FirestoreDb firestore, IMapper mapper)
        {
            _firestore = firestore;
            Incidents = new FirebaseIncidentRepository(firestore, mapper);
        }

        public IIncidentRepository Incidents { get; }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Firebase handles persistence within repository operations,
            // but we return a placeholder value for compatibility
            await Task.CompletedTask;
            return 0;
        }
    }
}
