using Core.Features.Incidents.Commands;
using Core.Features.Incidents.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace OccMinIncidentMapping.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IncidentsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public IncidentsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateIncidentCommand command)
        {
            var id = await _mediator.Send(command);
            return CreatedAtAction(nameof(Get), new { id }, command);
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var incidents = await _mediator.Send(new GetAllIncidentsQuery());
            return Ok(incidents);
        }
    }
}
