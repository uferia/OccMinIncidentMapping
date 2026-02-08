namespace Infrastructure.Constants
{
    /// <summary>
    /// Constants for Google Cloud Platform (GCP) integration.
    /// </summary>
    public static class GcpConstants
    {
        /// <summary>
        /// GCP metadata server URL for retrieving instance metadata.
        /// Used to detect if the application is running on GCP Compute Engine.
        /// </summary>
        public const string MetadataServerUrl = "https://metadata.google.internal/computeMetadata/v1/instance/id";

        /// <summary>
        /// Metadata server connection timeout in seconds.
        /// </summary>
        public const int MetadataServerTimeoutSeconds = 1;
    }
}
