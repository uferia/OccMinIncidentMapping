using Core.Entities;

namespace Core.Interfaces
{
    public interface IIncidentRepository
    {
        Task<Incident> AddAsync(Incident incident);
        Task<IEnumerable<Incident>> GetAllAsync();
    }
}
