namespace Core.Interfaces
{
    /// <summary>
    /// Interface for Google authentication service
    /// Handles verification of Google ID tokens
    /// </summary>
    public interface IGoogleAuthenticationService
    {
        /// <summary>
        /// Verifies a Google ID token and extracts user information
        /// </summary>
        /// <param name="idToken">The Google ID token to verify</param>
        /// <returns>Google user information if valid, null if invalid or expired</returns>
        Task<GoogleUserInfo?> VerifyIdTokenAsync(string idToken);
    }

    /// <summary>
    /// Google user information extracted from ID token
    /// </summary>
    public class GoogleUserInfo
    {
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Picture { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty; // Google's unique user ID
    }
}
