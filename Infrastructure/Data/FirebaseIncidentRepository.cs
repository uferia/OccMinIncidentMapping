using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using Google.Cloud.Firestore;
using Infrastructure.Data.Dtos;

namespace Infrastructure.Data
{
    public class FirebaseIncidentRepository : IIncidentRepository
    {
        private readonly CollectionReference _collection;
        private readonly IMapper _mapper;

        public FirebaseIncidentRepository(FirestoreDb firestore, IMapper mapper)
        {
            _collection = firestore.Collection("incidents");
            _mapper = mapper;
        }

        public async Task<Incident> AddAsync(Incident incident)
        {
            var dto = _mapper.Map<IncidentFirestoreDto>(incident);
            var docRef = await _collection.AddAsync(dto);
            incident.Id = docRef.Id;
            return incident;
        }

        public async Task<IEnumerable<Incident>> GetAllAsync()
        {
            var snapshot = await _collection.GetSnapshotAsync();
            return snapshot.Documents.Select(doc =>
            {
                var dto = doc.ConvertTo<IncidentFirestoreDto>();
                var incident = _mapper.Map<Incident>(dto);
                incident.Id = doc.Id;
                return incident;
            });
        }
    }
}
