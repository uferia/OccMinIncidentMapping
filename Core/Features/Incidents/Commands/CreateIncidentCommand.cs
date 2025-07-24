using Core.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Features.Incidents.Commands
{
    public record CreateIncidentCommand : IRequest<string>
    {
        public double Latitude { get; init; }
        public double Longitude { get; init; }
        public HazardType Hazard { get; init; }
        public StatusType Status { get; init; }
        public string? Description { get; init; }
    }
}
