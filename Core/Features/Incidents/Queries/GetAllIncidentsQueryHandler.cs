using Core.Entities;
using Core.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Features.Incidents.Queries
{
    public class GetAllIncidentsQueryHandler : IRequestHandler<GetAllIncidentsQuery, IEnumerable<Incident>>
    {
        private readonly IApplicationDbContext _context;

        public GetAllIncidentsQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Incident>> Handle(GetAllIncidentsQuery request, CancellationToken cancellationToken)
        {
            return await _context.Incidents.GetAllAsync();
        }
    }
}
