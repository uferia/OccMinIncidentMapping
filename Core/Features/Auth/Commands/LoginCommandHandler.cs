using Core.Interfaces;
using MediatR;

namespace Core.Features.Auth.Commands
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, (string token, string role)>
    {
        private readonly IAuthenticationService _authService;

        public LoginCommandHandler(IAuthenticationService authService)
        {
            _authService = authService;
        }

        public async Task<(string token, string role)> Handle(
            LoginCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                var isValid = await _authService.ValidateCredentialsAsync(
                    request.Username,
                    request.Password);

                if (!isValid)
                {
                    throw new UnauthorizedAccessException("Invalid credentials");
                }

                // TODO: Replace with database lookup
                var role = GetUserRole(request.Username);
                
                var token = await _authService.GenerateTokenAsync(request.Username, role);

                return (token, role);
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new UnauthorizedAccessException("Authentication failed", ex);
            }
        }

        /// <summary>
        /// Temporary placeholder - Replace with actual role lookup from database
        /// </summary>
        private static string GetUserRole(string username)
        {
            // TODO: Fetch from user database
            return username == "admin" ? "Admin" : "User";
        }
    }
}
