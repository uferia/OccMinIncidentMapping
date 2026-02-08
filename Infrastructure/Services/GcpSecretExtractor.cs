using System.Text.Json;

namespace Infrastructure.Services
{
    /// <summary>
    /// Utility class for extracting and parsing secrets from Google Cloud Secret Manager.
    /// Provides common functionality for secret parsing to avoid code duplication.
    /// </summary>
    public static class GcpSecretExtractor
    {
        /// <summary>
        /// Parses a Google Cloud Secret Manager JSON response and extracts the base64-encoded secret.
        /// Expected response format: { "payload": { "data": "base64-encoded-secret" } }
        /// </summary>
        /// <param name="jsonContent">The JSON content from GCP Secret Manager API response</param>
        /// <returns>The decoded secret string, or the original content if extraction fails</returns>
        public static string ExtractSecretFromJson(string jsonContent)
        {
            try
            {
                // Parse JSON response to extract the secret value
                // Response format: { "payload": { "data": "base64-encoded-secret" } }
                using (JsonDocument doc = JsonDocument.Parse(jsonContent))
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

                return jsonContent;
            }
            catch (JsonException)
            {
                // If parsing fails, return the original content
                return jsonContent;
            }
        }

        /// <summary>
        /// Displays secret metadata (project_id, client_email, type) from a JSON string.
        /// </summary>
        /// <param name="jsonContent">The JSON content to parse</param>
        public static void DisplaySecretMetadata(string jsonContent)
        {
            try
            {
                using (JsonDocument doc = JsonDocument.Parse(jsonContent))
                {
                    var root = doc.RootElement;
                    
                    if (root.TryGetProperty("project_id", out var projectIdElement))
                    {
                        Console.WriteLine($"Firebase Project ID: {projectIdElement.GetString()}");
                    }
                    
                    if (root.TryGetProperty("client_email", out var emailElement))
                    {
                        Console.WriteLine($"Service Account Email: {emailElement.GetString()}");
                    }
                    
                    if (root.TryGetProperty("type", out var typeElement))
                    {
                        Console.WriteLine($"Key Type: {typeElement.GetString()}");
                    }
                }
            }
            catch
            {
                // Silently fail if metadata display fails
            }
        }

        /// <summary>
        /// Validates if the given content is valid JSON.
        /// </summary>
        /// <param name="content">The content to validate</param>
        /// <returns>True if content is valid JSON, false otherwise</returns>
        public static bool IsValidJson(string content)
        {
            try
            {
                JsonDocument.Parse(content);
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }
    }
}
