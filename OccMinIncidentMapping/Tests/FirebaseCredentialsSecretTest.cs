using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Infrastructure.Services;

namespace OccMinIncidentMapping.Tests
{
    /// <summary>
    /// Test utility to verify firebase-credentials-occminproj retrieval from Google Cloud Secret Manager.
    /// Run this to test if the secret can be accessed.
    /// </summary>
    public static class FirebaseCredentialsSecretTest
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
                if (!GcpSecretExtractor.IsValidJson(secret))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("?? WARNING: Secret retrieved but is NOT valid JSON");
                    Console.ResetColor();
                    Console.WriteLine($"Secret content: {secret}\n");
                    return;
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("? SUCCESS: Secret retrieved and is valid JSON!");
                Console.ResetColor();
                
                Console.WriteLine("\nSecret Preview (first 100 chars):");
                Console.WriteLine(secret.Substring(0, Math.Min(100, secret.Length)) + "...\n");
                
                // Display key fields
                GcpSecretExtractor.DisplaySecretMetadata(secret);
                
                Console.WriteLine("\n? All checks passed!");
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
                return GcpSecretExtractor.ExtractSecretFromJson(content);
            }
        }
    }
}
