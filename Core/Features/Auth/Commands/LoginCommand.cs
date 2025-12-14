using MediatR;

namespace Core.Features.Auth.Commands
{
    public record LoginCommand : IRequest<(string token, string role)>
    {
        public string Username { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
    }
}
