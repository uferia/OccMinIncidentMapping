namespace Contracts.Auth
{
    /// <summary>
    /// Request for Google SSO authentication
    /// </summary>
    public class GoogleSsoRequest
    {
        /// <summary>
        /// Google ID Token received from frontend after user authenticates with Google
        /// </summary>
        public string IdToken { get; set; } = string.Empty;
    }
}
