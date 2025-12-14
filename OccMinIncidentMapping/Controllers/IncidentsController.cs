using Core.Features.Incidents.Commands;
using Core.Features.Incidents.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OccMinIncidentMapping.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class IncidentsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<IncidentsController> _logger;

        public IncidentsController(IMediator mediator, ILogger<IncidentsController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateIncidentCommand command)
        {
            var username = User.Identity?.Name ?? "Unknown";
            _logger.LogInformation("User {Username} creating incident", username);

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
