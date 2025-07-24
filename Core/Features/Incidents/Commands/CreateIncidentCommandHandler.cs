using Core.Entities;
using Core.Interfaces;
using MediatR;

namespace Core.Features.Incidents.Commands
{
    public class CreateIncidentCommandHandler : IRequestHandler<CreateIncidentCommand, string>
    {
        private readonly IApplicationDbContext _context;

        public CreateIncidentCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<string> Handle(CreateIncidentCommand request, CancellationToken cancellationToken)
        {
            var incident = new Incident
            {
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                Hazard = request.Hazard,
                Status = request.Status,
                Description = request.Description
            };

            await _context.Incidents.AddAsync(incident);
            return incident.Id;
        }
    }
}
