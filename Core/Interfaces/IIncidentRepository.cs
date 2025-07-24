using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IIncidentRepository
    {
        Task<Incident> AddAsync(Incident incident);
        Task<IEnumerable<Incident>> GetAllAsync();
    }
}
