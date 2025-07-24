using Core.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Features.Incidents.Queries
{
    public class GetAllIncidentsQuery : IRequest<IEnumerable<Incident>>, IBaseRequest
    {
        
    }
}
