namespace Contracts.Auth
{
    public class LoginResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string TokenType { get; set; } = "Bearer";
        public int ExpiresIn { get; set; } // in seconds
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
