using AutoMapper;
using Core.Interfaces;
using Google.Cloud.Firestore;

namespace Infrastructure.Data
{
    public class ApplicationDbContext : IApplicationDbContext
    {
        public ApplicationDbContext(FirestoreDb firestore, IMapper mapper)
        {
            Incidents = new FirebaseIncidentRepository(firestore, mapper);
        }

        public IIncidentRepository Incidents { get; }
    }
}
