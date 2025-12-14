using Core.Features.Incidents.Commands;
using FluentValidation;

namespace Application.Validators
{
    public class CreateIncidentCommandValidator : AbstractValidator<CreateIncidentCommand>
    {
        public CreateIncidentCommandValidator()
        {
            RuleFor(x => x.Latitude)
                .InclusiveBetween(-90, 90)
                .WithMessage("Invalid latitude value");

            RuleFor(x => x.Longitude)
                .InclusiveBetween(-180, 180)
                .WithMessage("Invalid longitude value");

            RuleFor(x => x.Hazard)
                .IsInEnum()
                .WithMessage("Invalid hazard type");

            RuleFor(x => x.Status)
                .IsInEnum()
                .WithMessage("Invalid status value");

            RuleFor(x => x.Description)
                .MaximumLength(500)
                .WithMessage("Description cannot exceed 500 characters");
        }
    }
}
