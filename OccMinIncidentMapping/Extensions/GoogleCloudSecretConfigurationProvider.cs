using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text.Json;

namespace OccMinIncidentMapping.Extensions
{
    /// <summary>
    /// Configuration provider for Google Cloud Secret Manager.
    /// Loads secrets from GCP Secret Manager and makes them available to the application.
    /// </summary>
    public class GoogleCloudSecretConfigurationSource : IConfigurationSource
    {
        private readonly string _projectId;
        private readonly string[]? _secretNames;

        public GoogleCloudSecretConfigurationSource(string projectId, string[]? secretNames = null)
        {
            _projectId = projectId;
            _secretNames = secretNames;
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new GoogleCloudSecretConfigurationProvider(_projectId, _secretNames);
        }
    }

    /// <summary>
    /// Configuration provider implementation for Google Cloud Secret Manager.
    /// Uses REST API with Application Default Credentials for authentication.
    /// </summary>
    public class GoogleCloudSecretConfigurationProvider : ConfigurationProvider
    {
        private readonly string _projectId;
        private readonly string[]? _secretNames;
        private readonly HttpClient _httpClient;

        public GoogleCloudSecretConfigurationProvider(string projectId, string[]? secretNames = null)
        {
            _projectId = projectId;
            _secretNames = secretNames;
            _httpClient = new HttpClient();
        }

        public override void Load()
        {
            try
            {
                // Use provided secret names or default list
                var secretsToLoad = _secretNames ?? new[]
                {
                    "jwt-signing-key",
                    "firebase-credentials-occminproj"
                };

                foreach (var secretName in secretsToLoad)
                {
                    try
                    {
                        var secret = GetSecretAsync(secretName).GetAwaiter().GetResult();
                        if (!string.IsNullOrEmpty(secret))
                        {
                            var configKey = SecretNameToConfigKey(secretName);
                            Data[configKey] = secret;
                        }
                    }
                    catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        Console.WriteLine($"Warning: Secret '{secretName}' not found in Google Cloud Secret Manager.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Warning: Failed to load secret '{secretName}': {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing Google Cloud Secret Manager: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Retrieves a secret from Google Cloud Secret Manager using REST API.
        /// </summary>
        private async Task<string> GetSecretAsync(string secretName)
        {
            try
            {
                // URL to access the secret from Google Cloud Secret Manager
                var url = $"https://secretmanager.googleapis.com/v1/projects/{_projectId}/secrets/{secretName}/versions/latest:access";
                
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("Accept", "application/json");

                var response = await _httpClient.SendAsync(request);

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return string.Empty;
                }

                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                
                // Parse JSON response to extract the secret value
                // Response format: { "payload": { "data": "base64-encoded-secret" } }
                using (JsonDocument doc = JsonDocument.Parse(content))
                {
                    var root = doc.RootElement;
                    
                    if (root.TryGetProperty("payload", out var payloadElement) &&
                        payloadElement.TryGetProperty("data", out var dataElement))
                    {
                        var base64Secret = dataElement.GetString();
                        if (!string.IsNullOrEmpty(base64Secret))
                        {
                            var decodedBytes = Convert.FromBase64String(base64Secret);
                            return System.Text.Encoding.UTF8.GetString(decodedBytes);
                        }
                    }
                }

                return content;
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Converts secret name to configuration key format.
        /// Example: "jwt-signing-key" -> "Jwt:SigningKey"
        /// </summary>
        private static string SecretNameToConfigKey(string secretName)
        {
            // Replace hyphens with colons for nested configuration
            // Convert to PascalCase: jwt-signing-key -> Jwt:SigningKey
            var parts = secretName.Split('-');
            var configParts = parts.Select(part => 
                char.ToUpper(part[0]) + (part.Length > 1 ? part.Substring(1).ToLower() : "")).ToArray();
            return string.Join(":", configParts);
        }
    }

    /// <summary>
    /// Extension method to add Google Cloud Secret Manager configuration.
    /// </summary>
    public static class GoogleCloudConfigurationExtensions
    {
        /// <summary>
        /// Adds Google Cloud Secret Manager as a configuration source.
        /// Requires appropriate GCP permissions and authentication (Application Default Credentials).
        /// </summary>
        public static IConfigurationBuilder AddGoogleCloudSecrets(
            this IConfigurationBuilder builder,
            string projectId,
            string[]? secretNames = null)
        {
            return builder.Add(new GoogleCloudSecretConfigurationSource(projectId, secretNames));
        }
    }
}
