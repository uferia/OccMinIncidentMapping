using Core.Features.Auth.Commands;
using FluentValidation;

namespace Application.Validators.Auth
{
    public class LoginCommandValidator : AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty()
                .WithMessage("Username is required")
                .MinimumLength(3)
                .WithMessage("Username must be at least 3 characters")
                .MaximumLength(50)
                .WithMessage("Username cannot exceed 50 characters")
                .Matches(@"^[a-zA-Z0-9._@-]+$")
                .WithMessage("Username contains invalid characters. Only alphanumeric, dots, underscores, @ and hyphens are allowed");

            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage("Password is required")
                .MinimumLength(8)
                .WithMessage("Password must be at least 8 characters")
                .MaximumLength(128)
                .WithMessage("Password cannot exceed 128 characters");
        }
    }
}
