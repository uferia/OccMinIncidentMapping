using MediatR;

namespace Core.Features.Auth.Commands
{
    /// <summary>
    /// Command to handle Google SSO authentication
    /// The IdToken is verified and a JWT token is generated
    /// </summary>
    public record GoogleSsoCommand : IRequest<(string token, string role)>
    {
        public string IdToken { get; init; } = string.Empty;
    }
}
