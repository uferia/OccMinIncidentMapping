namespace Core.Interfaces
{
    public interface IAuthenticationService
    {
        Task<string> GenerateTokenAsync(string username, string role);
        Task<bool> ValidateCredentialsAsync(string username, string password);
    }
}
