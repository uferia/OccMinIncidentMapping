using Core.Entities;
using MediatR;

namespace Core.Features.Incidents.Queries
{
    public class GetAllIncidentsQuery : IRequest<IEnumerable<Incident>>
    {
    }
}
