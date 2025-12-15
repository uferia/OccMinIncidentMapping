using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

/// <summary>
/// Test utility to verify firebase-credentials-occminproj retrieval from Google Cloud Secret Manager.
/// Run this to test if the secret can be accessed.
/// </summary>
public class FirebaseCredentialsSecretTest
{
    public static async Task Main(string[] args)
    {
        var projectId = Environment.GetEnvironmentVariable("GCP_PROJECT_ID") ?? "your-gcp-project-id";
        var secretName = "firebase-credentials-occminproj";

        Console.WriteLine("=== Firebase Credentials Secret Manager Test ===\n");
        Console.WriteLine($"Project ID: {projectId}");
        Console.WriteLine($"Secret Name: {secretName}\n");

        try
        {
            var secret = await GetSecretAsync(projectId, secretName);
            
            if (string.IsNullOrEmpty(secret))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("? FAILED: Secret not found or returned empty");
                Console.ResetColor();
                return;
            }

            // Verify it's valid JSON
            try
            {
                using (JsonDocument.Parse(secret))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("? SUCCESS: Secret retrieved and is valid JSON!");
                    Console.ResetColor();
                    
                    Console.WriteLine("\nSecret Preview (first 100 chars):");
                    Console.WriteLine(secret.Substring(0, Math.Min(100, secret.Length)) + "...\n");
                    
                    // Parse and show key fields
                    using (JsonDocument doc = JsonDocument.Parse(secret))
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
                    
                    Console.WriteLine("\n? All checks passed!");
                }
            }
            catch (JsonException ex)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("?? WARNING: Secret retrieved but is NOT valid JSON");
                Console.ResetColor();
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Secret content: {secret}\n");
            }
        }
        catch (HttpRequestException ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"? FAILED: HTTP Request Error");
            Console.ResetColor();
            Console.WriteLine($"Status Code: {ex.StatusCode}");
            Console.WriteLine($"Error: {ex.Message}\n");
            
            Console.WriteLine("Troubleshooting:");
            Console.WriteLine("1. Verify secret exists: gcloud secrets describe firebase-credentials-occminproj");
            Console.WriteLine("2. Check service account permission: gcloud secrets get-iam-policy firebase-credentials-occminproj");
            Console.WriteLine("3. Verify GCP_PROJECT_ID is set correctly");
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"? FAILED: {ex.GetType().Name}");
            Console.ResetColor();
            Console.WriteLine($"Error: {ex.Message}\n");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
        }
    }

    private static async Task<string> GetSecretAsync(string projectId, string secretName)
    {
        using (var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(30) })
        {
            // URL to access the secret from Google Cloud Secret Manager
            var url = $"https://secretmanager.googleapis.com/v1/projects/{projectId}/secrets/{secretName}/versions/latest:access";
            
            Console.WriteLine($"Requesting: {url}\n");
            
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Accept", "application/json");

            var response = await httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"HTTP {(int)response.StatusCode}: {response.ReasonPhrase}",
                    null,
                    response.StatusCode);
            }

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
    }
}
